using System.Threading.Tasks;
using NopStation.Plugin.SMS.Afilnet.Areas.Admin.Models;
using NopStation.Plugin.SMS.Afilnet.Domains;

namespace NopStation.Plugin.SMS.Afilnet.Areas.Admin.Factories
{
    public interface ISmsTemplateModelFactory
    {
        SmsTemplateSearchModel PrepareSmsTemplateSearchModel(SmsTemplateSearchModel searchModel);

        Task<SmsTemplateListModel> PrepareSmsTemplateListModelAsync(SmsTemplateSearchModel searchModel);

        Task<SmsTemplateModel> PrepareSmsTemplateModelAsync(SmsTemplateModel model, 
            SmsTemplate smsTemplate, bool excludeProperties = false);
    }
}