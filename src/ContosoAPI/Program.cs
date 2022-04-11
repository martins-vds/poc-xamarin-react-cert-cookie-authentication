using ContosoAPI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.FileProviders;
using System.Net;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.ConfigureHttpsDefaults(options => options.ClientCertificateMode = ClientCertificateMode.AllowCertificate);
});

builder.Services
    .AddControllers();

builder.Services.AddScoped<ICertificateValidationService, CertificateValidationService>();
builder.Services.AddScoped<ICertificateProviderService, CertificateProviderService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Events = new CookieAuthenticationEvents()
        {
            OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
        };
    })
    .AddCertificate(CertificateAuthenticationDefaults.AuthenticationScheme, options =>
     {
         options.RevocationMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.NoCheck;
         options.Events = new CertificateAuthenticationEvents
         {
             OnCertificateValidated = async context =>
             {
                 var validationService = context.HttpContext.RequestServices
                     .GetRequiredService<ICertificateValidationService>();

                 if (validationService.ValidateCertificate(context.ClientCertificate))
                 {
                     var claims = new[]
                     {
                        new Claim(
                            ClaimTypes.NameIdentifier,
                            context.ClientCertificate.Subject,
                            ClaimValueTypes.String, context.Options.ClaimsIssuer),
                        new Claim(
                            ClaimTypes.Name,
                            context.ClientCertificate.Subject,
                            ClaimValueTypes.String, context.Options.ClaimsIssuer)
                     };

                     context.Principal = new ClaimsPrincipal(
                         new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

                     await context.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, context.Principal);

                     context.Success();
                 }
             }
         };
     });

builder.Services.AddAuthorization(options =>
{
    var policy = new AuthorizationPolicyBuilder(CookieAuthenticationDefaults.AuthenticationScheme, CertificateAuthenticationDefaults.AuthenticationScheme)
                            .RequireAuthenticatedUser()
                            .Build();
    options.FallbackPolicy = policy;
    options.DefaultPolicy = policy;
});

builder.Services.AddHttpLogging(options => options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All);

var app = builder.Build();

app.UseHttpLogging();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();  
}

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
    RequestPath = ""
});

app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();
