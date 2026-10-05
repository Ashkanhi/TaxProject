using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using TaxCollectData.Library.Abstraction.Clients;
using TaxCollectData.Library.Abstraction.Cryptography;
using TaxCollectData.Library.Factories;
using TaxCollectData.Library.Properties;
using TaxServiceAndApp.BI;

namespace TaxServiceAndApp
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        public BMain bmain;
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            BMain.ServerResourcesPath = Server.MapPath("~/bin/Resources/");
            Application["bmain"] = bmain = new BMain();

        }
        protected void Session_Start(object sender, EventArgs e)
        {
            NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamFinanceUI).LogInfoMessage(" - Session_Started : " + Session.SessionID);
        }
        protected void Session_End(object sender, EventArgs e)
        {
            NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamFinanceUI).LogInfoMessage(" - Session_Ended : " + Session.SessionID);
        }
    }
}
