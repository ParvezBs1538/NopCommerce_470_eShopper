using System;
using System.Text;
using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.BlueSnapHosted
{
    public class BlueSnapSettings : ISettings
    {
        public bool AdditionalFeePercentage { get; set; }

        public bool IsSandBox { get; set; }

        public string AuthorizationKey
        {
            get
            {
                var encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                                   .GetBytes(Username + ":" + Password));
                return $"Basic {encoded}";
            }
        }

        public string Username { get; set; }

        public string Password { get; set; }

        public decimal AdditionalFee { get; set; }
    }
}
