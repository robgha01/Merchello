﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.UI.JavaScript;
using Merchello.Web.Editors;

namespace Merchello.Web.Trees
{
    public class ServerVariablesParsingEvents : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            base.ApplicationStarted(umbracoApplication, applicationContext);

            ServerVariablesParser.Parsing += ServerVariablesParserParsing;
        }
        
        /// <summary>
        /// Adds a product with all of its variants to the index if the product has an identity
        /// </summary>
        static void ServerVariablesParserParsing(object sender, Dictionary<string, object> items)
        {
            if (items.ContainsKey("umbracoUrls"))
            {
                Dictionary<string, object> umbracoUrls = (Dictionary<string, object>)items["umbracoUrls"];

                UrlHelper Url = new UrlHelper(new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData()));

                umbracoUrls.Add("merchelloProductApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<ProductApiController>(
                                                       controller => controller.GetAllProducts()));
            }
        }
    }
}
