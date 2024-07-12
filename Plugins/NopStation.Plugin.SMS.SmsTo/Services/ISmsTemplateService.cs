﻿using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.SMS.SmsTo.Domains;

namespace NopStation.Plugin.SMS.SmsTo.Services
{
    public interface ISmsTemplateService
    {
        Task DeleteSmsTemplateAsync(SmsTemplate smsTemplate);

        Task InsertSmsTemplateAsync(SmsTemplate smsTemplate);

        Task UpdateSmsTemplateAsync(SmsTemplate smsTemplate);

        Task<SmsTemplate> GetSmsTemplateByIdAsync(int smsTemplateId);

        Task<IList<SmsTemplate>> GetAllSmsTemplatesAsync(int storeId);

        Task<IList<SmsTemplate>> GetSmsTemplatesByNameAsync(string messageTemplateName, int? storeId = null);
    }
}