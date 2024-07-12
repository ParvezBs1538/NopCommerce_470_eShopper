using NopStation.Plugin.Misc.AjaxFilter.Domains.Enums;

namespace NopStation.Plugin.Misc.AjaxFilter.Models
{
    public class VendorsModel
    {
        public VendorsModel()
        {
            CheckedState = CheckedState.UnChecked;
        }

        public int Id { get; set; }

        public int Count { get; set; }

        public CheckedState CheckedState { get; set; }

        public string Name { get; set; }
    }
}
