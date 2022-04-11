using System.Security.Cryptography.X509Certificates;
namespace ContosoAPI.Services
{
    public interface ICertificateValidationService
    {
        bool ValidateCertificate(X509Certificate2 clientCertificate);
    }
}
