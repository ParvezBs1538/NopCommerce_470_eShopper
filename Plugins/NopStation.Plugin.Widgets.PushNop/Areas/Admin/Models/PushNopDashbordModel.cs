using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.PushNop.Areas.Admin.Models
{
    public partial record PushNopDashbordModel : BaseNopModel
    {
        public int NumberOfSubscribers { get; set; }

        public int NumberOfNotifications { get; set; }

        public int NumberOfCampaignsSent { get; set; }

        public int NumberOfCampaignsScheduled { get; set; }

        public int NumberOfNewSubscribersByDay { get; set; }
        public int NumberOfNewSubscribersByWeek { get; set; }
        public int NumberOfNewSubscribersByMonth { get; set; }
        public int NumberOfNewSubscribersByYear { get; set; }
    }
}
