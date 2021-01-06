using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Seo;
using Nop.Services.Topics;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System.Collections.Generic;
using System.Linq;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{

    public partial class ResourceController : BasePublicController
    {
        #region Fields
        private readonly ITopicService _topicService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStoreContext _storeContext;
        #endregion

        #region Ctor
        public ResourceController(
            ITopicService topicService,
            IUrlRecordService urlRecordService,
            IStoreContext storeContext
            )
        {
            _topicService = topicService;
            _urlRecordService = urlRecordService;
            _storeContext = storeContext;
        }

        #endregion


        #region Methods

        [HttpsRequirement]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult Index()
        {

            var allTopics = _topicService.GetAllTopics(_storeContext.CurrentStore.Id);
            var slugs = new List<string>();

            foreach (var item in allTopics)
            {
                slugs.Add(_urlRecordService.GetSeName(item.Id, "Topic"));
            }

            return View();
        }

        
        #endregion

       

    }
}
