using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.WebApi.Factories
{
    public interface ICommonApiModelFactory
    {
        Task<IList<KeyValueApi>> GetStringRsourcesAsync(int? languageId = null);
    }
}