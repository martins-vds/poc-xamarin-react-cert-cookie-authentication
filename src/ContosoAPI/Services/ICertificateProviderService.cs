using System.Security.Cryptography.X509Certificates;
namespace ContosoAPI.Services
{
    public interface ICertificateProviderService
    {
        X509Certificate2 FindCertificate(string thumbprint);
    }
}
