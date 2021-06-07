using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Topics;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Filters;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{
    public partial class TopicOverrideController : TopicController
    {
        #region Fields

        private readonly IAclService _aclService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ITopicModelFactory _topicModelFactory;
        private readonly ITopicService _topicService;

        #endregion

        #region Ctor

        public TopicOverrideController(
            IAclService aclService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IStoreMappingService storeMappingService,
            ITopicModelFactory topicModelFactory,
            ITopicService topicService) : base(aclService, localizationService, permissionService, storeMappingService, topicModelFactory, topicService)
        {
            _aclService = aclService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _storeMappingService = storeMappingService;
            _topicModelFactory = topicModelFactory;
            _topicService = topicService;
        }

        #endregion

        #region Methods


        
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public override IActionResult TopicDetailsPopup(string systemName)
        {
            var model = _topicModelFactory.PrepareTopicModelBySystemName(systemName);
            if (model == null)
                return InvokeHttp404();

            ViewBag.IsPopup = true;

            //template
            var templateViewPath = _topicModelFactory.PrepareTemplateViewPath(model.TopicTemplateId);
            return PartialView(templateViewPath, model);
        }

       
        #endregion
    }
}