using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Seo;
using Nop.Services.Topics;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
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
            List<UrlRecordData> urlRecordData = new List<UrlRecordData>();

            foreach (var item in allTopics)
            {
                if(item.SystemName != "PageNotFound")
                {
                    var urlRecord = new UrlRecordData
                    {
                        Slug = _urlRecordService.GetSeName(item.Id, "Topic"),
                        Title = item.Title
                    };
                    urlRecordData.Add(urlRecord);
                }
            }
            ViewBag.UrlRecords = urlRecordData;
            return View();
        }


        public class UrlRecordData
        {
            public string Slug { get; set; }
            public string Title { get; set; }
        }


        #endregion



    }
}
