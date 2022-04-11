using ContosoAPI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContosoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ICertificateValidationService certificateValidationService;
        private readonly ICertificateProviderService certificateProviderService;

        public AuthController(ICertificateValidationService certificateValidationService, ICertificateProviderService certificateProviderService)
        {
            this.certificateValidationService = certificateValidationService;
            this.certificateProviderService = certificateProviderService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] string thumbprint)
        {
            var certificate = certificateProviderService.FindCertificate(thumbprint);

            if(certificate == null)
            {
                return Unauthorized();
            }

            if (certificateValidationService.ValidateCertificate(certificate))
            {
                var claims = new[]
                     {
                        new Claim(
                            ClaimTypes.NameIdentifier,
                            certificate.Subject),
                        new Claim(
                            ClaimTypes.Name,
                            certificate.Subject)
                     };

                var principal = new ClaimsPrincipal(
                    new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

                await Request.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return Ok();
            }
            else
            {
                return Unauthorized();
            }
        }

        [AllowAnonymous]
        [HttpGet("logout")]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
