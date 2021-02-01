using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace NSS.Plugin.Misc.SwiftApi.JSON.ActionResults
{
    public class ErrorActionResult : ActionResult
    {
        private readonly string _jsonString;
        private readonly HttpStatusCode _statusCode;

        public ErrorActionResult(string jsonString, HttpStatusCode statusCode)
        {
            _jsonString = jsonString;
            _statusCode = statusCode;
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var response = context.HttpContext.Response;

            response.StatusCode = (int)_statusCode;
            response.ContentType = "application/json";

            await response.WriteAsync(_jsonString);
        }
    }
}
