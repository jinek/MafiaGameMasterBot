using System;
using System.Web;

namespace MGM
{
    public class SwitchHook : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            var request = context.Request;
            bool on = string.Equals(request.QueryString[null], "on", StringComparison.OrdinalIgnoreCase);
            var hookUri = $"{new UriBuilder(request.Url) {Path = "ProcessUpdates2.ashx",Query = string.Empty,Scheme = "https",Port = 443}.Uri}";
            /*FileToSend? certificate = null;
            FileStream stream = null;
            if (on)
            {
                stream = System.IO.File.OpenRead(Path.Combine(context.Request.PhysicalApplicationPath, "YOURPUBLIC.pem"));
                certificate = new FileToSend("YOURPUBLIC.pem",stream);
            }*/
            WorkHere.GameEngine.Api.SetWebhook(on ? hookUri : ""/*,on?certificate:null*/).Wait();
            //stream?.Dispose();
            context.Response.Write("Включено: " + (on?hookUri:"off"));
        }

        public bool IsReusable => false;
    }
}