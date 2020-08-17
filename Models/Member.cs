using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace DotNetCoreSqlDb.Models
{
    public class Member
    {
        public int ID { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required]
        public string Password { get; set; }

        public void setPassword(string rawPass)
        {
            this.Password = Member.getHash(rawPass);
        }
        
        public static string getHash(string rawPass) {
            byte[] bytePass = new UTF8Encoding().GetBytes(rawPass);
            byte[] hash = ((HashAlgorithm) CryptoConfig.CreateFromName("MD5")).ComputeHash(bytePass);
            return BitConverter.ToString(hash)
                .Replace("-", string.Empty)
                .ToLower();
        }
    }
}

