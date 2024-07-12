using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.WidgetManager.Domain.Schedules;

namespace NopStation.Plugin.Misc.WidgetManager.Services;

public interface IScheduleService
{
    #region Monthly schedule mappings

    Task InsertMonthlyScheduleMappingAsync<TEntity>(TEntity entity, int day)
         where TEntity : BaseEntity, IScheduleSupported;

    Task InsertMonthlyScheduleMappingAsync(MonthlyScheduleMapping monthlyScheduleMapping);

    Task DeleteMonthlyScheduleMappingAsync(MonthlyScheduleMapping monthlyScheduleMapping);

    Task<IList<int>> GetMonthlyScheduleDaysAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity, IScheduleSupported;

    Task<IList<MonthlyScheduleMapping>> GetMonthlyScheduleMappingsAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity, IScheduleSupported;

    Task UpdateMonthlyScheduleMappingsAsync<TEntity>(TEntity entity, IList<int> dayIds)
        where TEntity : BaseEntity, IScheduleSupported;

    #endregion

    #region Weekly schedule mappings

    Task InsertWeeklyScheduleMappingAsync<TEntity>(TEntity entity, int day)
         where TEntity : BaseEntity, IScheduleSupported;

    Task InsertWeeklyScheduleMappingAsync(WeeklyScheduleMapping weeklyScheduleMapping);

    Task DeleteWeeklyScheduleMappingAsync(WeeklyScheduleMapping weeklyScheduleMapping);

    Task<IList<int>> GetWeeklyScheduleDaysAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity, IScheduleSupported;

    Task<IList<WeeklyScheduleMapping>> GetWeeklyScheduleMappingsAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity, IScheduleSupported;

    Task UpdateWeeklyScheduleMappingsAsync<TEntity>(TEntity entity, IList<int> dayIds)
        where TEntity : BaseEntity, IScheduleSupported;

    #endregion

    #region Common

    Task<IQueryable<TEntity>> ApplyScheduleMappingAsync<TEntity>(IQueryable<TEntity> query)
        where TEntity : BaseEntity, IScheduleSupported;

    Task<IQueryable<TEntity>> ApplyScheduleMappingAsync<TEntity>(IQueryable<TEntity> query, DateTime dateTime)
        where TEntity : BaseEntity, IScheduleSupported;

    bool IsValidByDateTime<TEntity>(TEntity entity) where TEntity : BaseEntity, IScheduleSupported;

    bool IsValidByDateTime<TEntity>(TEntity entity, DateTime dateTime) where TEntity : BaseEntity, IScheduleSupported;

    #endregion
}
