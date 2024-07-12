using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Events;
using Nop.Data.Mapping;
using Nop.Services.Events;
using NopStation.Plugin.Misc.WidgetManager.Domain.Conditions;
using NopStation.Plugin.Misc.WidgetManager.Domain.Schedules;
using NopStation.Plugin.Misc.WidgetManager.Domain.Widgets;
using NopStation.Plugin.Widgets.Popups.Domains;

namespace NopStation.Plugin.Widgets.Popups.Services;

public class EventConsumers : IConsumer<EntityInsertedEvent<MonthlyScheduleMapping>>,
    IConsumer<EntityUpdatedEvent<MonthlyScheduleMapping>>,
    IConsumer<EntityDeletedEvent<MonthlyScheduleMapping>>,

    IConsumer<EntityInsertedEvent<WeeklyScheduleMapping>>,
    IConsumer<EntityUpdatedEvent<WeeklyScheduleMapping>>,
    IConsumer<EntityDeletedEvent<WeeklyScheduleMapping>>,

    IConsumer<EntityInsertedEvent<WidgetZoneMapping>>,
    IConsumer<EntityUpdatedEvent<WidgetZoneMapping>>,
    IConsumer<EntityDeletedEvent<WidgetZoneMapping>>,

    IConsumer<EntityInsertedEvent<ProductConditionMapping>>,
    IConsumer<EntityUpdatedEvent<ProductConditionMapping>>,
    IConsumer<EntityDeletedEvent<ProductConditionMapping>>,

    IConsumer<EntityInsertedEvent<CustomerConditionMapping>>,
    IConsumer<EntityUpdatedEvent<CustomerConditionMapping>>,
    IConsumer<EntityDeletedEvent<CustomerConditionMapping>>,

    IConsumer<EntityInsertedEvent<ConditionGroupMapping>>,
    IConsumer<EntityUpdatedEvent<ConditionGroupMapping>>,
    IConsumer<EntityDeletedEvent<ConditionGroupMapping>>
{
    private readonly IStaticCacheManager _staticCacheManager;

    public EventConsumers(IStaticCacheManager staticCacheManager)
    {
        _staticCacheManager = staticCacheManager;
    }

    protected async Task RemoveCacheAsync(string entityName, int entityId)
    {
        if (entityName != NameCompatibilityManager.GetTableName(typeof(Popup)))
            return;

        await _staticCacheManager.RemoveByPrefixAsync(PopupCacheDefaults.PopupModelPrefix, entityId);
    }

    public async Task HandleEventAsync(EntityInsertedEvent<MonthlyScheduleMapping> eventMessage)
    {
        await RemoveCacheAsync(eventMessage.Entity.EntityName, eventMessage.Entity.EntityId);
    }
    public async Task HandleEventAsync(EntityUpdatedEvent<MonthlyScheduleMapping> eventMessage)
    {
        await RemoveCacheAsync(eventMessage.Entity.EntityName, eventMessage.Entity.EntityId);
    }
    public async Task HandleEventAsync(EntityDeletedEvent<MonthlyScheduleMapping> eventMessage)
    {
        await RemoveCacheAsync(eventMessage.Entity.EntityName, eventMessage.Entity.EntityId);
    }

    public async Task HandleEventAsync(EntityInsertedEvent<WeeklyScheduleMapping> eventMessage)
    {
        await RemoveCacheAsync(eventMessage.Entity.EntityName, eventMessage.Entity.EntityId);
    }
    public async Task HandleEventAsync(EntityUpdatedEvent<WeeklyScheduleMapping> eventMessage)
    {
        await RemoveCacheAsync(eventMessage.Entity.EntityName, eventMessage.Entity.EntityId);
    }
    public async Task HandleEventAsync(EntityDeletedEvent<WeeklyScheduleMapping> eventMessage)
    {
        await RemoveCacheAsync(eventMessage.Entity.EntityName, eventMessage.Entity.EntityId);
    }

    public async Task HandleEventAsync(EntityInsertedEvent<WidgetZoneMapping> eventMessage)
    {
        await RemoveCacheAsync(eventMessage.Entity.EntityName, eventMessage.Entity.EntityId);
    }
    public async Task HandleEventAsync(EntityUpdatedEvent<WidgetZoneMapping> eventMessage)
    {
        await RemoveCacheAsync(eventMessage.Entity.EntityName, eventMessage.Entity.EntityId);
    }
    public async Task HandleEventAsync(EntityDeletedEvent<WidgetZoneMapping> eventMessage)
    {
        await RemoveCacheAsync(eventMessage.Entity.EntityName, eventMessage.Entity.EntityId);
    }

    public async Task HandleEventAsync(EntityInsertedEvent<ProductConditionMapping> eventMessage)
    {
        await RemoveCacheAsync(eventMessage.Entity.EntityName, eventMessage.Entity.EntityId);
    }
    public async Task HandleEventAsync(EntityUpdatedEvent<ProductConditionMapping> eventMessage)
    {
        await RemoveCacheAsync(eventMessage.Entity.EntityName, eventMessage.Entity.EntityId);
    }
    public async Task HandleEventAsync(EntityDeletedEvent<ProductConditionMapping> eventMessage)
    {
        await RemoveCacheAsync(eventMessage.Entity.EntityName, eventMessage.Entity.EntityId);
    }

    public async Task HandleEventAsync(EntityInsertedEvent<CustomerConditionMapping> eventMessage)
    {
        await RemoveCacheAsync(eventMessage.Entity.EntityName, eventMessage.Entity.EntityId);
    }
    public async Task HandleEventAsync(EntityUpdatedEvent<CustomerConditionMapping> eventMessage)
    {
        await RemoveCacheAsync(eventMessage.Entity.EntityName, eventMessage.Entity.EntityId);
    }
    public async Task HandleEventAsync(EntityDeletedEvent<CustomerConditionMapping> eventMessage)
    {
        await RemoveCacheAsync(eventMessage.Entity.EntityName, eventMessage.Entity.EntityId);
    }

    public async Task HandleEventAsync(EntityInsertedEvent<ConditionGroupMapping> eventMessage)
    {
        await RemoveCacheAsync(eventMessage.Entity.EntityName, eventMessage.Entity.EntityId);
    }
    public async Task HandleEventAsync(EntityUpdatedEvent<ConditionGroupMapping> eventMessage)
    {
        await RemoveCacheAsync(eventMessage.Entity.EntityName, eventMessage.Entity.EntityId);
    }
    public async Task HandleEventAsync(EntityDeletedEvent<ConditionGroupMapping> eventMessage)
    {
        await RemoveCacheAsync(eventMessage.Entity.EntityName, eventMessage.Entity.EntityId);
    }
}
