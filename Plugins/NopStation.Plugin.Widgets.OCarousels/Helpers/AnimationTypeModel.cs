using System.Collections.Generic;

namespace NopStation.Plugin.Widgets.OCarousels.Helpers
{
    public class AnimationTypeModel
    {
        public AnimationTypeModel()
        {
            Options = new List<Option>();
        }

        public string Group { get; set; }

        public IList<Option> Options { get; set; }


        public class Option
        {
            public string Text { get; set; }
            public string Value { get; set; }
        }
    }
}
