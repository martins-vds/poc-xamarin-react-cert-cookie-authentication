using System.Security.Cryptography.X509Certificates;
namespace ContosoAPI.Services
{
    public class CertificateValidationService : ICertificateValidationService
    {
        private readonly string[] validThumbprints = new[]
        {
            "E7AEDF2D0B94D1F3B7E9C8FED01A2B82818F04C5",
            "6B376ADD497F832DD2CDFCC8D0EF19E73E0C187C",
            "0A6FD7288A66002E0CD7740383304CC5C25FE341"
        };

        public bool ValidateCertificate(X509Certificate2 clientCertificate)
        {
            return validThumbprints.Contains(clientCertificate.Thumbprint);
        }
    }
}
