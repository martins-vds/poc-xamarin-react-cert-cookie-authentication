using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ContosoMobile
{
    public partial class MainPage : ContentPage
    {
        private static string backendUrl = DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5021" : "http://localhost:5021";

        private CookieContainer cookieContainer = new CookieContainer();

        public bool _isAuthenticated = false;
        public bool IsAuthenticated
        {
            get
            {
                return _isAuthenticated;
            }
            set
            {
                if(value != _isAuthenticated)
                {
                    _isAuthenticated = value;
                    OnPropertyChanged(nameof(IsAuthenticated));
                }
            }
        }

        public MainPage()
        {            
            InitializeComponent();
            
            Content.BindingContext = this;

            webView.Cookies = cookieContainer;
            webView.Source = new UrlWebViewSource()
            {
                Url = $"{backendUrl}/"
            };
            webView.HorizontalOptions = LayoutOptions.FillAndExpand;
            webView.VerticalOptions = LayoutOptions.FillAndExpand;
        }

        private async void LoginButton_Clicked(object sender, EventArgs e)
        {
            await LoginAsync();
        }

        protected override async void OnAppearing()
        {
            await LoginAsync();

            RefreshWebView();

            base.OnAppearing();
        }

        private async Task LoginAsync()
        {
            if (IsAuthenticated is false)
            {
                await AuthenticateAsync();
            }
        }

        private async Task AuthenticateAsync()
        {
            using var handler = new HttpClientHandler()
            {
                CookieContainer = cookieContainer,
            };

#if DEBUG
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (cert.Issuer.Equals("CN=localhost"))
                    return true;
                return errors == System.Net.Security.SslPolicyErrors.None;
            };
#endif
            using var httpClient = new HttpClient(handler);

            var thumbprint = "0A6FD7288A66002E0CD7740383304CC5C25FE341";

            using var response = await httpClient.PostAsJsonAsync($"{backendUrl}/api/auth/login", thumbprint);

            if (response.IsSuccessStatusCode)
            {
                var cookie = GetAuthCookie();

                IsAuthenticated = cookie is not null && cookie.Expired is false;
            }
            else
            {
                IsAuthenticated = false;
            }
        }

        private async void LogoutButton_Clicked(object sender, EventArgs e)
        {
            using var handler = new HttpClientHandler()
            {
                CookieContainer = cookieContainer,
            };

            using var httpClient = new HttpClient(handler);
            using var response = await httpClient.GetAsync($"{backendUrl}/api/auth/logout");

            var cookie = GetAuthCookie();

            if (cookie is not null)
            {
                cookie.Expired = true;
            }

            IsAuthenticated = false;
        }

        private Cookie GetAuthCookie()
        {
            return cookieContainer.GetCookies(new Uri(backendUrl))[".AspNetCore.Cookies"];
        }

        private void RefreshButton_Clicked(object sender, EventArgs e)
        {
            RefreshWebView();
        }

        private void RefreshWebView()
        {
            webView.Reload();
        }
    }
}
