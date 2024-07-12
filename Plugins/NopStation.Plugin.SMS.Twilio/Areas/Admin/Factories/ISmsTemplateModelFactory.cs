using System.Threading.Tasks;
using NopStation.Plugin.SMS.Twilio.Areas.Admin.Models;
using NopStation.Plugin.SMS.Twilio.Domains;

namespace NopStation.Plugin.SMS.Twilio.Areas.Admin.Factories
{
    public interface ISmsTemplateModelFactory
    {
        SmsTemplateSearchModel PrepareSmsTemplateSearchModel(SmsTemplateSearchModel searchModel);

        Task<SmsTemplateListModel> PrepareSmsTemplateListModelAsync(SmsTemplateSearchModel searchModel);

        Task<SmsTemplateModel> PrepareSmsTemplateModelAsync(SmsTemplateModel model, 
            SmsTemplate smsTemplate, bool excludeProperties = false);
    }
}