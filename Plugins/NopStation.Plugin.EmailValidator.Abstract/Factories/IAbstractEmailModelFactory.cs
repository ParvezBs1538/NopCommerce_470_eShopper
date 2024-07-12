

using System.Threading.Tasks;
using NopStation.Plugin.EmailValidator.Abstract.Models;

namespace NopStation.Plugin.EmailValidator.Abstract.Factories
{
    public interface IAbstractEmailModelFactory
    {
        Task<AbstractEmailSearchModel> PrepareAbstractEmailSearchModelAsync(AbstractEmailSearchModel searchModel);

        Task<AbstractEmailListModel> PrepareAbstractEmailListModelAsync(AbstractEmailSearchModel searchModel);
    }
}