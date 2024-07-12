using System.Collections.Generic;
using System.Linq;

namespace NopStation.Plugin.Widgets.SEOExpert.Extensions
{
    public static class SEOExpertSettingsExtensions
    {
        public static List<int> GetRegenerateConditionIds(this SEOExpertSettings settings)
        {
            try
            {
                return settings.RegenerateConditionIds.Split(',').Select(x => int.Parse(x)).ToList();
            }
            catch
            {
                return new List<int>();
            }
        }
    }
}
