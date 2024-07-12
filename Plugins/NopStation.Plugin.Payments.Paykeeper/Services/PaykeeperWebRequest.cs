using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Payments;
using NopStation.Plugin.Payments.Paykeeper.Services.Responses;
using RestSharp;

namespace NopStation.Plugin.Payments.Paykeeper.Services
{
    public class PaykeeperWebRequest : IPaykeeperWebRequest
    {
        #region Fields

        private readonly PaykeeperPaymentSettings _paykeeperPaymentSettings;
        private readonly ICustomerService _customerService;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public PaykeeperWebRequest(PaykeeperPaymentSettings paykeeperPaymentSettings,
            ICustomerService customerService,
            ILogger logger)
        {
            _paykeeperPaymentSettings = paykeeperPaymentSettings;
            _customerService = customerService;
            _logger = logger;
        }

        #endregion

        #region Methods

        public TokenResponse GetToken()
        {
            var uri = new Uri(_paykeeperPaymentSettings.GatewayUrl).Concat("/info/settings/token/");
            var request = CreateWebRequest(Method.GET);

            var client = new RestClient(uri);
            var response = client.Execute(request);

            return JsonConvert.DeserializeObject<TokenResponse>(response.Content);
        }

        public async Task<InvoiceInformationResponse> GetInvoiceInformationAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var uri = new Uri(_paykeeperPaymentSettings.GatewayUrl).Concat("/change/invoice/preview");
            var request = CreateWebRequest(Method.POST);

            var order = postProcessPaymentRequest.Order;
            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

            var tokenResponse = GetToken();
            var data = $"&token={tokenResponse.Token}&pay_amount={order.OrderTotal}&clientid={customer.Id}&orderid={order.Id}&client_email={customer.Email}";
            var client = new RestClient(uri);
            request.AddParameter("application/x-www-form-urlencoded", data, ParameterType.RequestBody);

            var response = client.Execute(request);
            return JsonConvert.DeserializeObject<InvoiceInformationResponse>(response.Content);
        }

        public string Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var uri = new Uri(_paykeeperPaymentSettings.GatewayUrl).Concat("/change/payment/reverse/");
            var request = CreateWebRequest(Method.POST);

            var data = $"&token={GetToken().Token}&id={refundPaymentRequest.Order.CaptureTransactionId}&amount={refundPaymentRequest.AmountToRefund}&partial={refundPaymentRequest.IsPartialRefund}&refund_cart=[]";
            var client = new RestClient(uri);
            request.AddParameter("application/x-www-form-urlencoded", data, ParameterType.RequestBody);

            var response = client.Execute(request);
            dynamic jObject = JObject.Parse(response.Content);

            return jObject.msg.ToString();
        }

        #endregion

        #region Utilities

        protected RestRequest CreateWebRequest(Method method = Method.POST)
        {
            var request = new RestRequest(method)
            {
                RequestFormat = DataFormat.None
            };
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("Authorization", GetHeaderToken());

            return request;
        }

        protected string GetHeaderToken()
        {
            var buffer = Encoding.UTF8.GetBytes($"{_paykeeperPaymentSettings.Login}:{_paykeeperPaymentSettings.Password}");
            var hexString = BitConverter.ToString(buffer);
            hexString = hexString.Replace("-", "");

            var bytesArray = new byte[hexString.Length / 2];
            for (var i = 0; i < bytesArray.Length; i++)
            {
                bytesArray[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            var headerToken = $"Basic {Convert.ToBase64String(bytesArray)}";
            return headerToken;
        }

        #endregion
    }

}
