using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.PushNop.Areas.Admin.Models;
using NopStation.Plugin.Widgets.PushNop.Domains;
using NopStation.Plugin.Widgets.PushNop.Services;

namespace NopStation.Plugin.Widgets.PushNop.Areas.Admin.Factories
{
    public class SmartGroupModelFactory : ISmartGroupModelFactory
    {
        #region Field

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ISmartGroupService _smartGroupService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public SmartGroupModelFactory(IDateTimeHelper dateTimeHelper,
            ISmartGroupService smartGroupService,
            ICustomerService customerService,
            ILocalizationService localizationService)
        {
            _dateTimeHelper = dateTimeHelper;
            _smartGroupService = smartGroupService;
            _customerService = customerService;
            _localizationService = localizationService;
        }

        #endregion

        #region Methods

        public virtual Task<SmartGroupSearchModel> PrepareSmartGroupSearchModelAsync(SmartGroupSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        public virtual async Task<SmartGroupListModel> PrepareSmartGroupListModelAsync(SmartGroupSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get All Smart Group 
            var smartGroups = await _smartGroupService.GetAllSmartGroupsAsync(searchModel.Page - 1, searchModel.PageSize);

            //prepare list model
            var model = await new SmartGroupListModel().PrepareToGridAsync(searchModel, smartGroups, () =>
            {
                return smartGroups.SelectAwait(async smartGroup =>
                {
                    return await PrepareSmartGroupModelAsync(null, smartGroup, false);
                });
            });

            return model;
        }

        public virtual async Task<SmartGroupModel> PrepareSmartGroupModelAsync(SmartGroupModel model,
            SmartGroup smartGroup, bool excludeProperties = false)
        {
            if (smartGroup != null)
            {
                model = model ?? smartGroup.ToModel<SmartGroupModel>();
                model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(smartGroup.CreatedOnUtc, DateTimeKind.Utc);

                if (!excludeProperties)
                {
                    model.SmartGroupConditionSearchModel.SmartGroupConditionId = smartGroup.Id;
                    model.SmartGroupConditionSearchModel.SetGridPageSize();
                }
            }

            if (!excludeProperties)
            {

            }

            return model;
        }

        public virtual async Task<SmartGroupConditionListModel> PrepareSmartGroupConditionListModelAsync(
            SmartGroupConditionSearchModel searchModel, SmartGroup smartGroup)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (smartGroup == null)
                throw new ArgumentNullException(nameof(smartGroup));

            //get smartGroupCondition
            var smartGroupConditions = (await _smartGroupService.GetSmartGroupConditionsBySmartGroupIdAsync(smartGroup.Id)).ToPagedList(searchModel);

            //prepare grid model
            var model = await new SmartGroupConditionListModel().PrepareToGridAsync(searchModel, smartGroupConditions, () =>
            {
                return smartGroupConditions.SelectAwait(async smartGroupCondition =>
                {
                    var sgcm = await PrepareSmartGroupConditionModelAsync(smartGroup, null, smartGroupCondition, true);
                    switch (sgcm.ConditionColumnTypeId)
                    {
                        case (int)ConditionColumnType.SubscribedOnUtc:
                        case (int)ConditionColumnType.CustomerLastActivityDateUtc:
                        case (int)ConditionColumnType.CustomerRegisteredOnUtc:
                            sgcm.ValueString = sgcm.ValueDateTime.HasValue ? sgcm.ValueDateTime.Value.ToString() : "";
                            break;

                        case (int)ConditionColumnType.OrderedBeforeDateUtc:
                        case (int)ConditionColumnType.OrderedAfterDateUtc:
                            sgcm.ConditionTypeStr = "--";
                            break;

                        case (int)ConditionColumnType.NeverOrdered:
                            sgcm.ValueString = "--";
                            sgcm.ConditionTypeStr = "--";
                            break;

                        case (int)ConditionColumnType.TotalNumberOfProductsOrdered:
                        case (int)ConditionColumnType.TotalSpentAmountOnOrder:
                        case (int)ConditionColumnType.PurchasedFromCategoryId:
                        case (int)ConditionColumnType.PurchasedFromVendorId:
                        case (int)ConditionColumnType.PurchasedFromManufacturerId:
                        case (int)ConditionColumnType.PurchasedWithDiscountId:
                            sgcm.ValueString = sgcm.ValueInt.ToString();
                            break;

                        default:
                            break;
                    }
                    return sgcm;
                });
            });

            return model;
        }

        public async Task<SmartGroupConditionModel> PrepareSmartGroupConditionModelAsync(SmartGroup smartGroup, SmartGroupConditionModel model,
            SmartGroupCondition smartGroupCondition, bool excludeProperties = false)
        {
            if (smartGroup == null)
                throw new ArgumentNullException(nameof(smartGroup));

            if (smartGroupCondition != null)
            {
                if (model == null)
                {
                    model = smartGroupCondition.ToModel<SmartGroupConditionModel>();
                    model.ConditionColumnTypeStr = await _localizationService.GetLocalizedEnumAsync(smartGroupCondition.ConditionColumnType);
                    model.ConditionTypeStr = await _localizationService.GetLocalizedEnumAsync(smartGroupCondition.ConditionType);
                    model.LogicTypeStr = await _localizationService.GetLocalizedEnumAsync(smartGroupCondition.LogicType);
                    model.SmartGroupName = smartGroup.Name;
                }
            }

            if (!excludeProperties)
            {
                model.AvailableConditionColumnTypes = (await ConditionColumnType.CustomerEmail.ToSelectListAsync()).ToList();
                model.AvailableConditionTypes = (await ConditionType.Contains.ToSelectListAsync()).ToList();
                model.AvailableLogicTypes = (await LogicType.And.ToSelectListAsync()).ToList();
            }
            model.SmartGroupId = smartGroup.Id;

            return model;
        }

        #endregion
    }
}
