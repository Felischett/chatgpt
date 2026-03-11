using Microsoft.AspNetCore.DataProtection;

namespace WebApi_Server.Services
{
    public class MessageCryptoService
    {
        private readonly IDataProtector _protector;

        public MessageCryptoService(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("messanger.messages.v1");
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText))
                return "";

            return _protector.Protect(plainText);
        }

        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrWhiteSpace(encryptedText))
                return "";

            return _protector.Unprotect(encryptedText);
        }
    }
}
