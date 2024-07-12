using NopStation.Plugin.SMS.Afilnet.Domains;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NopStation.Plugin.SMS.Afilnet.Services
{
    public class SmsSender : ISmsSender
    {
        private readonly AfilnetSettings _afilnetSettings;

        public SmsSender(AfilnetSettings afilnetSettings)
        {
            _afilnetSettings = afilnetSettings;
        }

        public async Task SendNotificationAsync(QueuedSms queuedSms)
        {
            await SendNotificationAsync(queuedSms.PhoneNumber, queuedSms.Body);
        }

        public async Task SendNotificationAsync(string phoneNumber, string body)
        {
            var data = new Dictionary<string, string>
            {
                ["class"] = "sms",
                ["method"] = "sendsms",
                ["from"] = _afilnetSettings.From,
                ["to"] = phoneNumber,
                ["sms"] = body,
                ["scheduledatetime"] = "",
                ["output"] = ""
            };

            var credentials = new NetworkCredential(_afilnetSettings.Username, _afilnetSettings.Password);
            var handler = new HttpClientHandler { Credentials = credentials };
            var client = new HttpClient(handler);
            var postContent = new FormUrlEncodedContent(data);
            var response = await client.PostAsync("https://www.afilnet.com/api/http/", postContent);
            response.EnsureSuccessStatusCode(); 
            await response.Content.ReadAsStringAsync();
        }
    }
}
