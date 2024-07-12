using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.AjaxFilter.Domains.Enums;

namespace NopStation.Plugin.Misc.AjaxFilter.Models
{
    public class FilterRatingModel
    {
        public FilterRatingModel()
        {
            Ratings = new List<RatingModel>();
        }
        public string ProductRatingIds { get; set; }
        public IList<RatingModel> Ratings  { get; set; }
    }
}
