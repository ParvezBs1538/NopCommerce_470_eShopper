using Newtonsoft.Json;
using NopStation.Plugin.SMS.BulkSms.Domains;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace NopStation.Plugin.SMS.BulkSms.Services
{
    public class SmsSender : ISmsSender
    {
        private readonly BulkSmsSettings _bulkSmsSettings;

        public SmsSender(BulkSmsSettings bulkSmsSettings)
        {
            _bulkSmsSettings = bulkSmsSettings;
        }

        public void SendNotification(QueuedSms queuedSms)
        {
            var parameter = new SmsParameter() { To = queuedSms.PhoneNumber, Body = queuedSms.Body };
            SendNotification(new List<SmsParameter> { parameter });
        }

        public void SendNotification(IList<SmsParameter> smsParameters)
        {
            var request = WebRequest.Create("https://api.bulksms.com/v1/messages");

            request.Credentials = new NetworkCredential(_bulkSmsSettings.Username, _bulkSmsSettings.Password);
            request.PreAuthenticate = true;
            request.Method = "POST";
            request.ContentType = "application/json";

            var encoding = new UnicodeEncoding();
            var encodedData = encoding.GetBytes(JsonConvert.SerializeObject(smsParameters));

            var stream = request.GetRequestStream();
            stream.Write(encodedData, 0, encodedData.Length);
            stream.Close();

            var response = request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            reader.ReadToEnd();
        }
    }
}
