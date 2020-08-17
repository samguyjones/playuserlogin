using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace DotNetCoreSqlDb.Models
{
    public class MemberToken
    {
        public int ID { get; set; }
        public int MemberID { get; set; }
        public string TokenHash { get; set; }

        public void setRandomHash()
        {
            var bytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }
            this.TokenHash = BitConverter.ToString(bytes).Replace("-",
                string.Empty);
        }
    }


}

