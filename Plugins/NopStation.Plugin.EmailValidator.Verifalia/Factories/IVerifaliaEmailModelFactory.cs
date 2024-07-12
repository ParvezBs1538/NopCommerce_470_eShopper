

using System.Threading.Tasks;
using NopStation.Plugin.EmailValidator.Verifalia.Models;

namespace NopStation.Plugin.EmailValidator.Verifalia.Factories
{
    public interface IVerifaliaEmailModelFactory
    {
        Task<VerifaliaEmailSearchModel> PrepareVerifaliaEmailSearchModelAsync(VerifaliaEmailSearchModel searchModel);

        Task<VerifaliaEmailListModel> PrepareVerifaliaEmailListModelAsync(VerifaliaEmailSearchModel searchModel);
    }
}