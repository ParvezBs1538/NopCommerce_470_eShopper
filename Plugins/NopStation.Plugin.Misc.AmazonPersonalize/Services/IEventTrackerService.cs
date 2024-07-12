using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.AmazonPersonalize.Domains;
using NopStation.Plugin.Misc.AmazonPersonalize.Models;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Services
{
    public interface IEventTrackerService
    {
        Task AddEventTrackerAsync(EventTrackerModel model);
    }
}
