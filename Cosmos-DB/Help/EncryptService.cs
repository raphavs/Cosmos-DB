using System.Text;
using System.Security.Cryptography;

namespace Cosmos_DB.Help
{
    public class EncryptService
    {
        public string GenerateHash(HashAlgorithm hashAlgorithm, string value)
        {
            // Convert the input string to a byte array and compute the hash
            var hashedBytes = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
            
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string
            foreach (var data in hashedBytes)
            {
                sBuilder.Append(data.ToString("x2"));
            }

            // Return the hexadecimal string
            return sBuilder.ToString();
        }
    }
}