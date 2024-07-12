using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;
using NopStation.Plugin.Misc.WidgetManager.Domain.Schedules;

namespace NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Factories;

public interface IScheduleModelFactory
{
    Task PrepareScheduleMappingModelAsync<TModel, TEntity>(TModel model, TEntity entity)
        where TEntity : BaseEntity, IScheduleSupported
        where TModel : IScheduleSupportedModel;
}