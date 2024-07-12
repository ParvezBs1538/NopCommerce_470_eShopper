using System;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Stores;
using NopStation.Plugin.Misc.WidgetManager.Domain.Conditions;
using NopStation.Plugin.Misc.WidgetManager.Domain.Schedules;
using NopStation.Plugin.Misc.WidgetManager.Domain.Widgets;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Domain
{
    public partial class Survey : BaseEntity, ILocalizedEntity, ISlugSupported, IStoreMappingSupported,
        IAclSupported, ISoftDeletedEntity, IWidgetZoneSupported, IScheduleSupported, IConditionSupported,
        ICustomerConditionSupported
    {
        public string Name { get; set; }

        public string SystemName { get; set; }

        public string Description { get; set; }

        public bool AllowMultipleSubmissions { get; set; }

        public bool IncludeInTopMenu { get; set; }

        public bool EnableEmailing { get; set; }

        public int DisplayOrder { get; set; }

        public int EmailAccountId { get; set; }

        public string ToEmailAddresses { get; set; }

        public string BccEmailAddresses { get; set; }

        public int? DelayBeforeSend { get; set; }

        public int DelayPeriodId { get; set; }

        public bool Published { get; set; }

        public string MetaKeywords { get; set; }

        public string MetaDescription { get; set; }

        public string MetaTitle { get; set; }

        public bool Deleted { get; set; }

        public bool SubjectToAcl { get; set; }

        public bool LimitedToStores { get; set; }

        public DateTime? StartDateUtc { get; set; }

        public DateTime? EndDateUtc { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public bool HasWidgetZoneMappingApplied { get; set; }

        #region Schedules

        public bool HasScheduleApplied { get; set; }

        public string TimeZoneId { get; set; }

        public DateTime? AvaliableDateTimeFromUtc { get; set; }

        public DateTime? AvaliableDateTimeToUtc { get; set; }

        public long? TimeOfDayFromTicksUtc { get; set; }

        public long? TimeOfDayToTicksUtc { get; set; }

        public int ScheduleTypeId { get; set; }

        public bool ConsiderNextDay { get; set; }

        public ScheduleType ScheduleType
        {
            get => (ScheduleType)ScheduleTypeId;
            set => ScheduleTypeId = (int)value;
        }

        #endregion

        #region Conditions

        public bool HasConditionApplied { get; set; }

        public bool EnableCondition { get; set; }

        public bool HasCustomerConditionApplied { get; set; }

        public bool EnableCustomerCondition { get; set; }

        #endregion
    }
}
