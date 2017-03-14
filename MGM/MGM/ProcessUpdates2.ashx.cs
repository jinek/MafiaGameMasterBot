using System;
using System.Diagnostics;
using System.IO;
using System.Web;
using MGM.BotFlow.Extensions;
using MGM.TelemetryGlobal;
using Microsoft.ApplicationInsights;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace MGM
{
    public class ProcessUpdates : IHttpHandler
    {
        private readonly JsonSerializer _jsonSerializer = JsonSerializer.Create();

        public void ProcessRequest(HttpContext context)
        {
            context.Response.StatusCode = 200;

            var stream = context.Request.GetBufferedInputStream();
            string readToEnd;
            using (var reader = new StreamReader(stream))
            {
                readToEnd = reader.ReadToEnd();
                //Trace.WriteLine(readToEnd);
            }

            var update = _jsonSerializer.Deserialize<Update>(new JsonTextReader(new StringReader(readToEnd)));
            try
            {
                TelemetryStatic.TelemetryClient = new TelemetryClient();

                TelemetryStatic.TelemetryClient.Context.Operation.Name = "Update process";
                if (WorkHere.GameEngine.ProcessUpdate(update))
                {
                    try
                    {
                        Trace.WriteLine($"Income message: {update.GetMessage().Text}");
                    }
                    catch (NotSupportedException)
                    {
                        Trace.WriteLine(readToEnd);
                    }
                }
                else
                {
                    Trace.WriteLine(readToEnd);
                }
            }
            catch (Exception exception)
            {
                WorkHere.LogException(new WrapException(readToEnd, exception));
            }
            Timer.CheckTimerRun();
        }

        public bool IsReusable => false;
    }

    public class WrapException : Exception
    {
        public WrapException(string message, Exception innerException) : base($"Update was: {message}", innerException)
        {
        }
    }
}