using System;
using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.bKash
{
    public class BkashPaymentSettings : ISettings
    {
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFeePercentage { get; set; }
        public bool UseSandbox { get; set; }

        public string IdToken { get; set; }
        public int ExpiresInSec { get; set; }
        public string RefreshToken { get; set; }
        public string TokenType { get; set; }
        public DateTime TokenCreateTime { get; set; }
    }
}
