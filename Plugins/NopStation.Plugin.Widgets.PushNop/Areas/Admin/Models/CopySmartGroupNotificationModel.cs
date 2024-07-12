using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.PushNop.Areas.Admin.Models
{
    public record CopySmartGroupNotificationModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.PushNop.GroupNotifications.Copy.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PushNop.GroupNotifications.Copy.SendingWillStartOn")]
        [UIHint("DateTime")]
        public DateTime SendingWillStartOnUtc { get; set; }
    }
}
