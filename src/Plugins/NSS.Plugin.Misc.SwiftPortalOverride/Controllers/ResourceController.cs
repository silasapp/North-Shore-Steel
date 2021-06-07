using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Seo;
using Nop.Services.Topics;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NSS.Plugin.Misc.SwiftCore.Helpers;
using NSS.Plugin.Misc.SwiftCore.Services;
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
        private readonly IWorkContext _workContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerCompanyService _customerCompanyService;
        #endregion

        #region Ctor
        public ResourceController(
            ITopicService topicService,
            IUrlRecordService urlRecordService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IGenericAttributeService genericAttributeService,
            ICustomerCompanyService customerCompanyService
            )
        {
            _topicService = topicService;
            _urlRecordService = urlRecordService;
            _storeContext = storeContext;
            _workContext = workContext;
            _genericAttributeService = genericAttributeService;
            _customerCompanyService = customerCompanyService;
        }

        #endregion


        #region Methods

        [HttpsRequirement]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult Index()
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.CurrentCustomer.Id);
            int eRPCompanyId = Convert.ToInt32(_genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, compIdCookieKey));

            bool isBuyer = _customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.Buyer);
            bool isOperations = _customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.Operations);

            var allTopics = _topicService.GetAllTopics(_storeContext.CurrentStore.Id);
            List<UrlRecordData> urlRecordData = new List<UrlRecordData>();

            foreach (var item in allTopics)
            {
                if(item.SystemName != "PageNotFound")
                {
                    var urlRecord = new UrlRecordData
                    {
                        Slug = _urlRecordService.GetSeName(item.Id, "Topic"),
                        Title = item.Title,
                        IsBuyer = isBuyer,
                        IsOperations = isOperations
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
            public bool IsBuyer { get; set; }
            public bool IsOperations { get; set; }

        }


        #endregion



    }
}
