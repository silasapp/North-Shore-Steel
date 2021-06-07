﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Infrastructure;
using Nop.Services.Vendors;

namespace NSS.Plugin.Misc.SwiftApi.Attributes
{
    public class ValidateVendor : BaseValidationAttribute
    {
        private Dictionary<string, string> _errors;

        private IVendorService _vendorService;

        private IVendorService VendorService
        {
            get
            {
                if (_vendorService == null)
                {
                    _vendorService = EngineContext.Current.Resolve<IVendorService>();
                }

                return _vendorService;
            }
        }

        public ValidateVendor()
        {
            _errors = new Dictionary<string, string>();
        }

        public override async Task ValidateAsync(object instance)
        {
            var vendorId = 0;

            if (instance != null && int.TryParse(instance.ToString(), out vendorId))
            {
                if (vendorId > 0)
                {
                    var vendor = await VendorService.GetVendorByIdAsync(vendorId);

                    if (vendor == null)
                    {
                        _errors.Add("Invalid vendor id", "Non existing vendor");
                    }
                }
            }
        }

        public override Dictionary<string, string> GetErrors()
        {
            return _errors;
        }
    }
}