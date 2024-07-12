using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Data.Mapping;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Factories;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;
using NopStation.Plugin.Misc.WidgetManager.Domain.Conditions;
using NopStation.Plugin.Misc.WidgetManager.Domain.Schedules;
using NopStation.Plugin.Misc.WidgetManager.Domain.Widgets;
using NopStation.Plugin.Misc.WidgetManager.Services;

namespace NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Controllers;

public class BaseWidgetAdminController : NopStationAdminController
{
    #region Widget zone mappings

    protected virtual async Task<WidgetZoneListModel> WidgetZoneListAsync<TEntity>(WidgetZoneSearchModel searchModel, TEntity entity)
        where TEntity : BaseEntity, IWidgetZoneSupported
    {
        var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));
        if (entityName != searchModel.EntityName || entity.Id != searchModel.EntityId)
            throw new ArgumentException("Widget zone mapping is not matching with entity");

        var widgetZoneModelFactory = EngineContext.Current.Resolve<IWidgetZoneModelFactory>();
        var model = await widgetZoneModelFactory.PrepareWidgetZoneMappingListModelAsync(searchModel);

        return model;
    }

    protected virtual async Task WidgetZoneCreateAsync<TEntity>(WidgetZoneModel model, TEntity entity)
        where TEntity : BaseEntity, IWidgetZoneSupported
    {
        var widgetZoneService = EngineContext.Current.Resolve<IWidgetZoneService>();

        var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));
        if (entityName != model.EntityName || entity.Id != model.EntityId)
            throw new ArgumentException("Widget zone mapping is not matching with entity");

        if (await widgetZoneService.GetWidgetZoneAppliedToEntityAsync(entity, model.WidgetZone) is not null)
            return;

        var widgetZoneMapping = model.ToEntity<WidgetZoneMapping>();
        await widgetZoneService.InsertWidgetZoneMappingAsync(widgetZoneMapping);

        await widgetZoneService.UpdateHasWidgetZonesPropertyAsync(entity);
    }

    protected virtual async Task WidgetZoneEditAsync<TEntity>(WidgetZoneModel model, TEntity entity)
        where TEntity : BaseEntity, IWidgetZoneSupported
    {
        var widgetZoneService = EngineContext.Current.Resolve<IWidgetZoneService>();
        var widgetZoneMapping = await widgetZoneService.GetWidgetZoneMappingByIdAsync(model.Id)
            ?? throw new ArgumentException("No widget zone mapping found with the specified id");

        var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));
        if (entityName != model.EntityName || entity.Id != model.EntityId)
            throw new ArgumentException("Widget zone mapping is not matching with entity");

        widgetZoneMapping.DisplayOrder = model.DisplayOrder;
        await widgetZoneService.UpdateWidgetZoneMappingAsync(widgetZoneMapping);

        await widgetZoneService.UpdateHasWidgetZonesPropertyAsync(entity);
    }

    protected virtual async Task WidgetZoneDeleteAsync<TEntity>(int id, TEntity entity)
        where TEntity : BaseEntity, IWidgetZoneSupported
    {
        var widgetZoneService = EngineContext.Current.Resolve<IWidgetZoneService>();
        var widgetZoneMapping = await widgetZoneService.GetWidgetZoneMappingByIdAsync(id)
            ?? throw new ArgumentException("No widget zone mapping found with the specified id");

        var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));
        if (entityName != widgetZoneMapping.EntityName || entity.Id != widgetZoneMapping.EntityId)
            throw new ArgumentException("Widget zone mapping is not matching with entity");

        await widgetZoneService.DeleteWidgetZoneMappingAsync(widgetZoneMapping);

        await widgetZoneService.UpdateHasWidgetZonesPropertyAsync(entity);
    }

    #endregion

    #region Schedule mappings

    protected virtual async Task UpdateScheduleMappingsAsync<TEntity>(ScheduleModel model, TEntity entity, Func<TEntity, Task> acquire = null)
        where TEntity : BaseEntity, IScheduleSupported
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var scheduleMappingService = EngineContext.Current.Resolve<IScheduleService>();

        if (model.ScheduleTypeId == (int)ScheduleType.Monthly)
            await scheduleMappingService.UpdateMonthlyScheduleMappingsAsync(entity, model.SelectedDaysOfMonth);
        if (model.ScheduleTypeId == (int)ScheduleType.Weekly)
            await scheduleMappingService.UpdateWeeklyScheduleMappingsAsync(entity, model.SelectedDaysOfWeek);

        if (model.TimeOfDayFromUtc.HasValue)
            entity.TimeOfDayFromTicksUtc = model.TimeOfDayFromUtc.Value.TimeOfDay.Ticks;
        else
            entity.TimeOfDayFromTicksUtc = null;

        if (model.TimeOfDayToUtc.HasValue)
            entity.TimeOfDayToTicksUtc = model.TimeOfDayToUtc.Value.TimeOfDay.Ticks;
        else
            entity.TimeOfDayToTicksUtc = null;

        entity.ScheduleTypeId = model.ScheduleTypeId;
        entity.AvaliableDateTimeToUtc = model.AvaliableDateTimeToUtc;
        entity.AvaliableDateTimeFromUtc = model.AvaliableDateTimeFromUtc;

        if (acquire != null)
            await acquire(entity);
    }

    #endregion

    #region Customer condition mappings

    protected virtual async Task<CustomerConditionListModel> CustomerConditionListAsync<TEntity>(CustomerConditionSearchModel searchModel, TEntity entity)
          where TEntity : BaseEntity, ICustomerConditionSupported
    {
        var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));
        if (entityName != searchModel.EntityName || entity.Id != searchModel.EntityId)
            throw new ArgumentException("Customer condition mapping is not matching with entity");

        var conditionModelFactory = EngineContext.Current.Resolve<IConditionModelFactory>();
        var model = await conditionModelFactory.PrepareCustomerConditionMappingListModelAsync(searchModel);

        return model;
    }

    protected virtual async Task CustomerConditionDeleteAsync<TEntity>(int id, TEntity entity)
        where TEntity : BaseEntity, ICustomerConditionSupported
    {
        var conditionService = EngineContext.Current.Resolve<IConditionService>();
        var cusomerConditionMapping = await conditionService.GetCustomerConditionMappingByIdAsync(id)
            ?? throw new ArgumentException("No customer condition mapping found with the specified id");

        var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));
        if (entityName != cusomerConditionMapping.EntityName || entity.Id != cusomerConditionMapping.EntityId)
            throw new ArgumentException("Customer condition mapping is not matching with entity");

        await conditionService.DeleteCustomerConditionMappingAsync(cusomerConditionMapping);

        await conditionService.UpdateHasCustomerConditionsPropertyAsync(entity);
    }

    protected virtual async Task CustomerConditionAddAsync<TEntity>(AddCustomerToConditionModel model, TEntity entity)
        where TEntity : BaseEntity, ICustomerConditionSupported
    {
        var customerService = EngineContext.Current.Resolve<ICustomerService>();

        var selectedCustomers = await customerService.GetCustomersByIdsAsync(model.SelectedCustomerIds.ToArray());
        if (selectedCustomers.Any())
        {
            var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));
            if (entityName != model.EntityName || entity.Id != model.EntityId)
                throw new ArgumentException("Customer condition mapping is not matching with entity");

            var conditionService = EngineContext.Current.Resolve<IConditionService>();
            foreach (var customer in selectedCustomers)
            {
                if (await conditionService.GetCustomerConditionAppliedToEntityAsync(entity.Id, entityName, customer.Id) is null)
                    await conditionService.InsertCustomerConditionMappingAsync(new CustomerConditionMapping
                    {
                        CustomerId = customer.Id,
                        EntityId = entity.Id,
                        EntityName = entityName
                    });
            }
            await conditionService.UpdateHasCustomerConditionsPropertyAsync(entity);
        }
    }

    #endregion

    #region Product condition mappings

    protected virtual async Task<ProductConditionListModel> ProductConditionListAsync<TEntity>(ProductConditionSearchModel searchModel, TEntity entity)
          where TEntity : BaseEntity, IProductConditionSupported
    {
        var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));
        if (entityName != searchModel.EntityName || entity.Id != searchModel.EntityId)
            throw new ArgumentException("Product condition mapping is not matching with entity");

        var conditionModelFactory = EngineContext.Current.Resolve<IConditionModelFactory>();
        var model = await conditionModelFactory.PrepareProductConditionMappingListModelAsync(searchModel);

        return model;
    }

    protected virtual async Task ProductConditionDeleteAsync<TEntity>(int id, TEntity entity)
        where TEntity : BaseEntity, IProductConditionSupported
    {
        var conditionService = EngineContext.Current.Resolve<IConditionService>();
        var cusomerConditionMapping = await conditionService.GetProductConditionMappingByIdAsync(id)
            ?? throw new ArgumentException("No product condition mapping found with the specified id");

        var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));
        if (entityName != cusomerConditionMapping.EntityName || entity.Id != cusomerConditionMapping.EntityId)
            throw new ArgumentException("Product condition mapping is not matching with entity");

        await conditionService.DeleteProductConditionMappingAsync(cusomerConditionMapping);

        await conditionService.UpdateHasProductConditionsPropertyAsync(entity);
    }

    protected virtual async Task ProductConditionAddAsync<TEntity>(AddProductToConditionModel model, TEntity entity)
        where TEntity : BaseEntity, IProductConditionSupported
    {
        var productService = EngineContext.Current.Resolve<IProductService>();

        var selectedProducts = await productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
        if (selectedProducts.Any())
        {
            var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));
            if (entityName != model.EntityName || entity.Id != model.EntityId)
                throw new ArgumentException("Product condition mapping is not matching with entity");

            var conditionService = EngineContext.Current.Resolve<IConditionService>();
            foreach (var product in selectedProducts)
            {
                if (await conditionService.GetProductConditionAppliedToEntityAsync(entity.Id, entityName, product.Id) is null)
                    await conditionService.InsertProductConditionMappingAsync(new ProductConditionMapping
                    {
                        ProductId = product.Id,
                        EntityId = entity.Id,
                        EntityName = entityName
                    });
            }
            await conditionService.UpdateHasProductConditionsPropertyAsync(entity);
        }
    }

    #endregion
}
