using System.Security.Cryptography.X509Certificates;
namespace ContosoAPI.Services
{
    public class CertificateProviderService : ICertificateProviderService, IDisposable
    {
        private readonly X509Store store;
        private bool disposedValue;

        public CertificateProviderService()
        {
            store = new X509Store(StoreName.My, StoreLocation.CurrentUser, OpenFlags.ReadOnly);
        }

        public X509Certificate2 FindCertificate(string thumbprint)
        {
            if (string.IsNullOrWhiteSpace(thumbprint))
            {
                return null;
            }

            var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, true);

            return certificates.FirstOrDefault();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    store?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
