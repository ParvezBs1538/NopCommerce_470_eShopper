using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NopStation.Plugin.Misc.Core.Helpers;

namespace NopStation.Plugin.Payments.Dmoney.Infrastructure
{
    public class PostMiddleware
    {
        private readonly RequestDelegate _next;

        public PostMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var route = context.GetRouteData();
            if (context.Request.QueryString.ToString().Contains("simple"))
            {
                await ReturnIndexPage(context);
                return;
            }
            await _next.Invoke(context);
        }

        private static async Task ReturnIndexPage(HttpContext context)
        {
            var dmoneyPaymentSettings = NopInstance.Load<DmoneyPaymentSettings>();

            var sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
            sb.AppendFormat("<form name='form' action='{0}' method='post'>", dmoneyPaymentSettings.GatewayUrl);

            sb.AppendFormat("<input type=\"hidden\" name=\"orgCode\" value=\"" + dmoneyPaymentSettings.OrganizationCode + "\"/>");
            sb.AppendFormat("<input type=\"hidden\" name=\"password\" value=\"" + dmoneyPaymentSettings.Password + "\"/>");
            sb.AppendFormat("<input type=\"hidden\" name=\"secretKey\" value=\"" + dmoneyPaymentSettings.SecretKey + "\"/>");
            sb.AppendFormat("<input type=\"hidden\" name=\"billerCode\" value=\"" + dmoneyPaymentSettings.BillerCode + "\"/>");
            sb.AppendFormat("<input type=\"hidden\" name=\"billOrInvoiceNo\" value=\"" + 100 + "\"/>");
            sb.AppendFormat("<input type=\"hidden\" name=\"transactionTrackingNo\" value=\"" + Guid.NewGuid() + "\"/>");
            sb.AppendFormat("<input type=\"hidden\" name=\"amount\" value=\"" + 1000 + "\"/>");
            sb.AppendFormat("<input type=\"hidden\" name=\"language\" value=\"EN\"/>");
            sb.AppendFormat("<input type=\"hidden\" name=\"orderType\" value=\"merchantPayment\"/>");
            sb.AppendFormat("<input type=\"hidden\" name=\"currency\" value=\"050\"/>");
            sb.AppendFormat("<input type=\"hidden\" name=\"description\" value=\"6512634761253476\"/>");

            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "text/html";
            var buffer = Encoding.UTF8.GetBytes(sb.ToString());

            context.Response.ContentLength = buffer.Length;

            using (var stream = context.Response.Body)
            {
                await stream.WriteAsync(buffer, 0, buffer.Length);
                await stream.FlushAsync();
            }
        }
    }
}
