using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Data.Mapping;
using NopStation.Plugin.Misc.WidgetManager.Domain.Schedules;

namespace NopStation.Plugin.Misc.WidgetManager.Services;

public class ScheduleService : IScheduleService
{
    #region Fields

    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IRepository<MonthlyScheduleMapping> _monthlyScheduleMappingRepository;
    private readonly IRepository<WeeklyScheduleMapping> _weeklyScheduleMappingRepository;

    #endregion

    #region Ctor

    public ScheduleService(IStaticCacheManager staticCacheManager,
        IRepository<MonthlyScheduleMapping> monthlyScheduleMappingRepository,
        IRepository<WeeklyScheduleMapping> weeklyScheduleMappingRepository)
    {
        _staticCacheManager = staticCacheManager;
        _monthlyScheduleMappingRepository = monthlyScheduleMappingRepository;
        _weeklyScheduleMappingRepository = weeklyScheduleMappingRepository;
    }

    #endregion

    #region Methods

    #region Monthly schedule mappings

    public async Task InsertMonthlyScheduleMappingAsync<TEntity>(TEntity entity, int day)
         where TEntity : BaseEntity, IScheduleSupported
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));

        await _monthlyScheduleMappingRepository.InsertAsync(new MonthlyScheduleMapping
        {
            DayId = day,
            EntityId = entity.Id,
            EntityName = entityName
        });
    }

    public async Task InsertMonthlyScheduleMappingAsync(MonthlyScheduleMapping monthlyScheduleMapping)
    {
        await _monthlyScheduleMappingRepository.InsertAsync(monthlyScheduleMapping);
    }

    public async Task DeleteMonthlyScheduleMappingAsync(MonthlyScheduleMapping monthlyScheduleMapping)
    {
        await _monthlyScheduleMappingRepository.DeleteAsync(monthlyScheduleMapping);
    }

    public virtual async Task<IList<int>> GetMonthlyScheduleDaysAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity, IScheduleSupported
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entityId = entity.Id;
        var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));

        var key = _staticCacheManager.PrepareKeyForDefaultCache(WidgetManagerDefaults.ScheduleMonthlyDaysCacheKey, entityId, entityName);

        var query = from m in _monthlyScheduleMappingRepository.Table
                    where m.EntityId == entityId &&
                    m.EntityName == entityName
                    select m.DayId;

        var days = await _staticCacheManager.GetAsync(key, async () => await query.ToListAsync());

        return days;
    }

    public virtual async Task<IList<MonthlyScheduleMapping>> GetMonthlyScheduleMappingsAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity, IScheduleSupported
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entityId = entity.Id;
        var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));

        var key = _staticCacheManager.PrepareKeyForDefaultCache(WidgetManagerDefaults.ScheduleMonthlyMappingsCacheKey, entityId, entityName);

        var query = from m in _monthlyScheduleMappingRepository.Table
                    where m.EntityId == entityId &&
                    m.EntityName == entityName
                    select m;

        var mappings = await _staticCacheManager.GetAsync(key, async () => await query.ToListAsync());

        return mappings;
    }

    public virtual async Task UpdateMonthlyScheduleMappingsAsync<TEntity>(TEntity entity, IList<int> dayIds)
        where TEntity : BaseEntity, IScheduleSupported
    {
        var existingMappings = await GetMonthlyScheduleMappingsAsync(entity);
        var allDays = Enum.GetValues(typeof(DayOfMonth)).Cast<int>();
        foreach (var day in allDays)
        {
            if (dayIds.Contains(day))
            {
                //new store
                if (!existingMappings.Any(m => m.DayId == day))
                    await InsertMonthlyScheduleMappingAsync(entity, day);
            }
            else
            {
                //remove store
                var mappingToDelete = existingMappings.FirstOrDefault(sm => sm.DayId == day);
                if (mappingToDelete != null)
                    await DeleteMonthlyScheduleMappingAsync(mappingToDelete);
            }
        }
    }

    #endregion

    #region Weekly schedule mappings

    public async Task InsertWeeklyScheduleMappingAsync<TEntity>(TEntity entity, int day)
         where TEntity : BaseEntity, IScheduleSupported
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));

        await _weeklyScheduleMappingRepository.InsertAsync(new WeeklyScheduleMapping
        {
            DayId = day,
            EntityId = entity.Id,
            EntityName = entityName
        });
    }

    public async Task InsertWeeklyScheduleMappingAsync(WeeklyScheduleMapping weeklyScheduleMapping)
    {
        await _weeklyScheduleMappingRepository.InsertAsync(weeklyScheduleMapping);
    }

    public async Task DeleteWeeklyScheduleMappingAsync(WeeklyScheduleMapping weeklyScheduleMapping)
    {
        await _weeklyScheduleMappingRepository.DeleteAsync(weeklyScheduleMapping);
    }

    public virtual async Task<IList<int>> GetWeeklyScheduleDaysAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity, IScheduleSupported
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entityId = entity.Id;
        var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));

        var key = _staticCacheManager.PrepareKeyForDefaultCache(WidgetManagerDefaults.ScheduleWeeklyDaysCacheKey, entityId, entityName);

        var query = from m in _weeklyScheduleMappingRepository.Table
                    where m.EntityId == entityId &&
                    m.EntityName == entityName
                    select m.DayId;

        var days = await _staticCacheManager.GetAsync(key, async () => await query.ToListAsync());

        return days;
    }

    public virtual async Task<IList<WeeklyScheduleMapping>> GetWeeklyScheduleMappingsAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity, IScheduleSupported
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entityId = entity.Id;
        var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));

        var key = _staticCacheManager.PrepareKeyForDefaultCache(WidgetManagerDefaults.ScheduleWeeklyMappingsCacheKey, entityId, entityName);

        var query = from m in _weeklyScheduleMappingRepository.Table
                    where m.EntityId == entityId &&
                    m.EntityName == entityName
                    select m;

        var mappings = await _staticCacheManager.GetAsync(key, async () => await query.ToListAsync());

        return mappings;
    }

    public virtual async Task UpdateWeeklyScheduleMappingsAsync<TEntity>(TEntity entity, IList<int> dayIds)
        where TEntity : BaseEntity, IScheduleSupported
    {
        var existingMappings = await GetWeeklyScheduleMappingsAsync(entity);
        var allDays = Enum.GetValues(typeof(DayOfWeek)).Cast<int>();
        foreach (var day in allDays)
        {
            if (dayIds.Contains(day))
            {
                //new store
                if (!existingMappings.Any(m => m.DayId == day))
                    await InsertWeeklyScheduleMappingAsync(entity, day);
            }
            else
            {
                //remove store
                var mappingToDelete = existingMappings.FirstOrDefault(sm => sm.DayId == day);
                if (mappingToDelete != null)
                    await DeleteWeeklyScheduleMappingAsync(mappingToDelete);
            }
        }
    }

    #endregion

    #region Common

    public virtual async Task<IQueryable<TEntity>> ApplyScheduleMappingAsync<TEntity>(IQueryable<TEntity> query)
        where TEntity : BaseEntity, IScheduleSupported
    {
        return await ApplyScheduleMappingAsync(query, DateTime.UtcNow);
    }

    public virtual Task<IQueryable<TEntity>> ApplyScheduleMappingAsync<TEntity>(IQueryable<TEntity> query, DateTime dateTime)
        where TEntity : BaseEntity, IScheduleSupported
    {
        if (query is null)
            throw new ArgumentNullException(nameof(query));

        var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));

        var ticks = dateTime.TimeOfDay.Ticks;

        query = from entity in query
                where (entity.ScheduleTypeId == (int)ScheduleType.Daily ||
                (entity.ScheduleTypeId == (int)ScheduleType.Weekly &&
                    _weeklyScheduleMappingRepository.Table.Where(ws => ws.EntityId == entity.Id &&
                    ws.EntityName == entityName && ws.DayId == (int)dateTime.DayOfWeek).Any()) ||
                (entity.ScheduleTypeId == (int)ScheduleType.Monthly &&
                    _monthlyScheduleMappingRepository.Table.Where(ms => ms.EntityId == entity.Id &&
                    ms.EntityName == entityName && ms.DayId == dateTime.Day).Any())) &&
                (!entity.AvaliableDateTimeFromUtc.HasValue || entity.AvaliableDateTimeFromUtc <= dateTime) &&
                (!entity.AvaliableDateTimeToUtc.HasValue || entity.AvaliableDateTimeToUtc >= dateTime) &&
                (!entity.TimeOfDayFromTicksUtc.HasValue || entity.TimeOfDayFromTicksUtc <= ticks) &&
                (!entity.TimeOfDayToTicksUtc.HasValue || entity.TimeOfDayToTicksUtc >= ticks)
                select entity;

        return Task.FromResult(query);
    }

    public virtual bool IsValidByDateTime<TEntity>(TEntity entity) where TEntity : BaseEntity, IScheduleSupported
    {
        return IsValidByDateTime(entity, DateTime.UtcNow);
    }

    public virtual bool IsValidByDateTime<TEntity>(TEntity entity, DateTime dateTime) where TEntity : BaseEntity, IScheduleSupported
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (!entity.AvaliableDateTimeToUtc.HasValue && !entity.AvaliableDateTimeFromUtc.HasValue &&
            !entity.TimeOfDayToTicksUtc.HasValue && !entity.TimeOfDayFromTicksUtc.HasValue)
            return true;

        if (entity.AvaliableDateTimeToUtc.HasValue && entity.AvaliableDateTimeToUtc.Value < dateTime)
            return false;

        if (entity.AvaliableDateTimeFromUtc.HasValue && entity.AvaliableDateTimeFromUtc.Value > dateTime)
            return false;

        var ticks = dateTime.TimeOfDay.Ticks;

        var fromTicks = entity.TimeOfDayFromTicksUtc ?? 0;
        var toTicks = entity.TimeOfDayToTicksUtc ?? DateTime.UtcNow.Date.AddTicks(-1).Ticks;

        if (fromTicks > ticks)
            return false;
        if (toTicks < ticks)
            return false;

        return true;
    }

    #endregion

    #endregion
}
