using System;

namespace DotNetCoreSqlDb.Models
{
    public class ResetPassModel
    {
        public int TokenId { get; set; }

        public string ResetCode { get; set; }
    }
}