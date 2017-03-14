using System;
using System.Configuration;
using System.Web;
using MGM.BotFlow.Processing;
using Microsoft.ApplicationInsights.Extensibility;

namespace MGM
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            TelemetryConfiguration.Active.InstrumentationKey =
    ConfigurationManager.AppSettings["APPINSIGHTS_INSTRUMENTATIONKEY"];
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var error = Server.GetLastError();
            Server.ClearError();
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.StatusCode = 200;

            WorkHere.LogException(error);
        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {
        }
    }
}