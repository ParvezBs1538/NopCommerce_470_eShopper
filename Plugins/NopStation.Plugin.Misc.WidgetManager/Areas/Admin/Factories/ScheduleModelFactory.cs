using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;
using NopStation.Plugin.Misc.WidgetManager.Domain.Schedules;
using NopStation.Plugin.Misc.WidgetManager.Services;

namespace NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Factories;

public class ScheduleModelFactory : IScheduleModelFactory
{
    #region Fields

    private readonly IScheduleService _scheduleMappingService;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public ScheduleModelFactory(IScheduleService scheduleMappingService,
        ILocalizationService localizationService)
    {
        _scheduleMappingService = scheduleMappingService;
        _localizationService = localizationService;
    }

    #endregion

    #region Methods

    public async Task PrepareScheduleMappingModelAsync<TModel, TEntity>(TModel model, TEntity entity)
        where TEntity : BaseEntity, IScheduleSupported
        where TModel : IScheduleSupportedModel
    {
        if (model is null)
            throw new ArgumentNullException(nameof(model));

        var scheduleModel = new ScheduleModel();

        if (entity != null)
        {
            scheduleModel.ScheduleTypeId = entity.ScheduleTypeId;
            scheduleModel.ScheduleTypeString = await _localizationService.GetLocalizedEnumAsync(entity.ScheduleType);
            scheduleModel.AvaliableDateTimeFromUtc = entity.AvaliableDateTimeFromUtc;
            scheduleModel.AvaliableDateTimeToUtc = entity.AvaliableDateTimeToUtc;

            if (entity.TimeOfDayFromTicksUtc.HasValue)
                scheduleModel.TimeOfDayFromUtc = DateTime.Today.AddTicks(entity.TimeOfDayFromTicksUtc.Value);
            if (entity.TimeOfDayToTicksUtc.HasValue)
                scheduleModel.TimeOfDayToUtc = DateTime.Today.AddTicks(entity.TimeOfDayToTicksUtc.Value);

            scheduleModel.SelectedDaysOfMonth = await _scheduleMappingService.GetMonthlyScheduleDaysAsync(entity);
            scheduleModel.SelectedDaysOfWeek = await _scheduleMappingService.GetWeeklyScheduleDaysAsync(entity);
        }

        foreach (var item in Enum.GetValues(typeof(DayOfMonth)).Cast<int>())
        {
            scheduleModel.AvailableDaysOfMonth.Add(new SelectListItem
            {
                Selected = scheduleModel.SelectedDaysOfMonth.Contains(item),
                Text = await _localizationService.GetLocalizedEnumAsync((DayOfMonth)item),
                Value = item.ToString()
            });
        }

        foreach (var item in Enum.GetValues(typeof(DayOfWeek)).Cast<int>())
        {
            scheduleModel.AvailableDaysOfWeek.Add(new SelectListItem
            {
                Selected = scheduleModel.SelectedDaysOfWeek.Contains(item),
                Text = await _localizationService.GetLocalizedEnumAsync((DayOfWeek)item),
                Value = item.ToString()
            });
        }

        scheduleModel.AvailableScheduleTypes = (await ScheduleType.Daily.ToSelectListAsync()).ToList();

        model.Schedule = scheduleModel;
    }

    #endregion
}
