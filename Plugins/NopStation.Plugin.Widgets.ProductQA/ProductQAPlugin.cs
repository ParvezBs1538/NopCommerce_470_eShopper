using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Components;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer
{
    public class ProductQAPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor
        public ProductQAPlugin(ILocalizationService localizationService,
            INopStationCoreService nopStationCoreService,
            ISettingService settingService,
            IPermissionService permissionService,
            IWebHelper webHelper)
        {
            _localizationService = localizationService;
            _nopStationCoreService = nopStationCoreService;
            _settingService = settingService;
            _permissionService = permissionService;
            _webHelper = webHelper;
        }
        #endregion

        #region Methods

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.ProductDetailsBottom });
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/ProductQAConfiguration/Configure";
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(ProductQAViewComponent);
        }

        public bool HideInWidgetList => false;

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.ProductQuestionAnswer.Menu.ProductQnA")
            };

            if (await _permissionService.AuthorizeAsync(ProductQAPermissionProvider.ManageConfiguration))
            {
                var settings = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.ProductQuestionAnswer.Menu.Configuration"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/ProductQAConfiguration/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "ProductQA.Configuration"
                };
                menu.ChildNodes.Add(settings);
            }

            if (await _permissionService.AuthorizeAsync(ProductQAPermissionProvider.ManageProductQA))
            {
                var list = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.ProductQuestionAnswer.Menu.QuestionsList"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/ProductQA/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "ProductQAList"
                };
                menu.ChildNodes.Add(list);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/product-qna-documentation?utm_source=admin-panel?utm_source=admin-panel&utm_medium=products&utm_campaign=product-qna",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menu.ChildNodes.Add(documentation);
            }
            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
        }


        #region Install & UnInstall

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new ProductQAPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new ProductQAPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>();

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.Menu.ProductQnA", "Product Q&A"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.Menu.Configuration", "Configuration"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.Menu.QuestionsList", "Product Q&A list"));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.Configuration.Updated", "Product q&a configuration updated successfully."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Updated", "Product q&a updated successfully."));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.All", "All"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.Approved", "Approved"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.Pending", "Pending"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.Answered", "Answered"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.NoAnswerYet", "Not Answered"));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.GuestCustomer", "Guest"));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.Configuration.Fields.Enable", "Enable plugin"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.Configuration.Fields.Enable.Hint", "Enable this plugin."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.Configuration.Fields.QuestionAnonymous", "Allow customer to question as anonymous user"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.Configuration.Fields.QuestionAnonymous.Hint", "If this option checked then customer can asked their question as anonymous user."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.Configuration.Fields..Store", "Limited store"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.Configuration.Fields..Store.Hint", "Select the limited store."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.Configuration.Fields..WhoAskTheQuestion", "Who can ask the question"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.Configuration.Fields..WhoAskTheQuestion.Hint", "Select the customer role who can ask the questions."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.Configuration.Fields..WhoAnswerTheQuestion", "Who can answer the question"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.Configuration.Fields..WhoAnswerTheQuestion.Hint", "Select the customer role who can answer the questions."));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.ProductId", "Store"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.ProductId.Hint", "Select store."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.ProductInformation", "Product"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.ProductInformation.Hint", "This is the product for which question is asked."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.AnsweredBy", "Answered by"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.AnsweredBy.Hint", "Who answered the question?"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.CustomerInformation", "Questioned by"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.CustomerInformation.Hint", "Who asked the question?"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.QuestionText", "Question"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.QuestionText.Hint", "Question from customer."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.AnswerText", "Answer"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.AnswerText.Hint", "Write answer of this question."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.IsApproved", "Approve"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.IsApproved.Hint", "Are you approve this question?"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.CreatedOnUtc", "Asked date"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.CreatedOnUtc.Hint", "When Asked this question?"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.UpdatedOnUtc", "Answered date"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.UpdatedOnUtc.Hint", "When Answered the question?"));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.Fields.SearchStoreId", "Store"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.Fields.SearchStoreId.Hint", "Search by store."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.Fields.SearchProductId", "Product"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.Fields.SearchProductId.Hint", "Search by product."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.Fields.SearchApproveOptionId", "Approval status"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.Fields.SearchApproveOptionId.Hint", "Search by approval status."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.Fields.SearchAnswerOptionId", "Answered"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.Fields.SearchAnswerOptionId.Hint", "Search by answer status."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.Fields.CreatedFrom", "Created from"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.Fields.CreatedFrom.Hint", "The start date for the search."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.Fields.CreatedTo", "Created to"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.Fields.CreatedTo.Hint", "The end date for the search."));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.Configuration.WhoAskTheQuestion.Required", "This field is required."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.Configuration.WhoAnswerTheQuestion.Required", "This field is required."));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.Configuration", "Configuration"));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnAList", "Product QnA list"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Edit", "Edit"));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.EditDetails", "Edit question details"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.ProductQuestionAnswer.List.BackToList", "Back to question list"));

            list.Add(new KeyValuePair<string, string>("NopStation.ProductQuestionAnswer.ProductQnA.Saved", "Successfully submitted the question."));
            list.Add(new KeyValuePair<string, string>("NopStation.ProductQuestionAnswer.ProductQnA.GuestCustomer", "Guest"));
            list.Add(new KeyValuePair<string, string>("NopStation.ProductQuestionAnswer.ProductQnA.AnonymousCustomer", "Anonymous"));
            list.Add(new KeyValuePair<string, string>("NopStation.ProductQuestionAnswer.ProductQnA.QuestionBy", "by"));
            list.Add(new KeyValuePair<string, string>("NopStation.ProductQuestionAnswer.ProductQnA.NoAnswer", "No answer yet"));
            list.Add(new KeyValuePair<string, string>("NopStation.ProductQuestionAnswer.ProductQnA.AnswerBy", "by"));
            list.Add(new KeyValuePair<string, string>("NopStation.ProductQuestionAnswer.ProductQnA.Warning.NoQuestionsFound", "Questions/Answers are not found..."));
            list.Add(new KeyValuePair<string, string>("NopStation.ProductQuestionAnswer.List", "Question and answer about this product"));
            list.Add(new KeyValuePair<string, string>("NopStation.ProductQuestionAnswer.Fields.HaveQuestion", "Don't see the answer you're looking for?"));
            list.Add(new KeyValuePair<string, string>("NopStation.ProductQuestionAnswer.ProductQnA.Fields.AskQuestions.Placeholder", "Ask your questions here..."));
            list.Add(new KeyValuePair<string, string>("NopStation.ProductQuestionAnswer.ProductQnA.Fields.Warning.QuestionBodyNotEmpty", "Question body is not empty..."));
            list.Add(new KeyValuePair<string, string>("NopStation.ProductQuestionAnswer.ProductQnA.Fields.Success.Question", "Successfully saved the question."));
            list.Add(new KeyValuePair<string, string>("NopStation.ProductQuestionAnswer.ProductQnA.Fields.QuestionAsAnAnonymous", "Question as an anonymous...."));

            return list;
        }

        #endregion

        #endregion
    }
}
