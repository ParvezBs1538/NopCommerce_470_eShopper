using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Topics;
using Nop.Web.Factories;
using Nop.Web.Models.Topics;
using NopStation.Plugin.Misc.Core.Models.Api;
using NopStation.Plugin.Misc.WebApi.Models.Topics;

namespace NopStation.Plugin.Misc.WebApi.Controllers
{
    [Route("api/topic")]
    public class TopicsApiController : BaseApiController
    {
        #region Fields

        private readonly IAclService _aclService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;
        private readonly ITopicModelFactory _topicModelFactory;
        private readonly ITopicService _topicService;

        #endregion

        #region Ctor

        public TopicsApiController(IAclService aclService,
            ILocalizationService localizationService,
            IStoreMappingService storeMappingService,
            IPermissionService permissionService,
            ITopicModelFactory topicModelFactory,
            ITopicService topicService)
        {
            _aclService = aclService;
            _localizationService = localizationService;
            _storeMappingService = storeMappingService;
            _permissionService = permissionService;
            _topicModelFactory = topicModelFactory;
            _topicService = topicService;
        }

        #endregion

        #region Methods

        [HttpGet("details/{systemName}")]
        public virtual async Task<IActionResult> Details(string systemName)
        {
            var model = await _topicModelFactory.PrepareTopicModelBySystemNameAsync(systemName);
            if (model == null)
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Topic.FailedToLoad"));

            return OkWrap(model);
        }

        [HttpGet("detailsbyid/{id}")]
        public virtual async Task<IActionResult> DetailsById(int id)
        {
            var topic = await _topicService.GetTopicByIdAsync(id);

            if (topic == null)
                return NotFound();

            var notAvailable = !topic.Published ||
                //ACL (access control list)
                !await _aclService.AuthorizeAsync(topic) ||
                //store mapping
                !await _storeMappingService.AuthorizeAsync(topic);

            //allow administrators to preview any topic
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel)
                && await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTopics);

            if (notAvailable && !hasAdminAccess)
                return NotFound();

            var model = await _topicModelFactory.PrepareTopicModelAsync(topic);
            if (model == null)
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Topic.FailedToLoad"));

            return OkWrap(model);
        }

        [HttpPost("authenticate")]
        public virtual async Task<IActionResult> Authenticate([FromBody] BaseQueryModel<AuthenticateModel> queryModel)
        {
            var topic = await _topicService.GetTopicBySystemNameAsync(queryModel.Data.SystemName);
            if (topic != null &&
                topic.Published &&
                //password protected?
                topic.IsPasswordProtected &&
                //store mapping
                await _storeMappingService.AuthorizeAsync(topic) &&
                //ACL (access control list)
                await _aclService.AuthorizeAsync(topic))
            {
                var response = new TopicModel();
                if (topic.Password != null && topic.Password.Equals(queryModel.Data.Password))
                {
                    response = await _topicModelFactory.PrepareTopicModelBySystemNameAsync(queryModel.Data.SystemName);
                    return OkWrap(response);
                }
                else
                {
                    return BadRequestWrap(response, errors: new List<string> { await _localizationService.GetResourceAsync("Topic.WrongPassword") });
                }
            }

            return Unauthorized();
        }

        #endregion
    }
}
