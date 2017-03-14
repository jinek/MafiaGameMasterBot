using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using MGM.BotFlow.Processing;
using MGM.Game.Engine;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace MGM
{//https://azure.microsoft.com/en-us/documentation/articles/app-insights-api-custom-events-metrics/
    public static class WorkHere
    {
        private static readonly string CurrentWebApplicationPhysycalPath = HttpRuntime.AppDomainAppPath;

        static WorkHere()
        {
            var physicalApplicationPath = CurrentWebApplicationPhysycalPath;
            Trace.WriteLine("got path: "+ physicalApplicationPath);
            if (physicalApplicationPath == null) throw new NullReferenceException("PhysicalApplicationPath is null!");
            var versionNumberFileName = Path.Combine(physicalApplicationPath, "versionNumber.txt");
            var version = File.Exists(versionNumberFileName) ? File.ReadAllText(versionNumberFileName) : "0";
            Trace.WriteLine("version="+version);


            TelemetryConfiguration.Active.TelemetryInitializers.Add(new VersionTelemetryInitializer(version));

            string history=null;
            var versionHistoryFilePath = Path.Combine(physicalApplicationPath, @"VersionHistory.txt");

            try
            {
                history = File.ReadAllText(versionHistoryFilePath);
                Trace.WriteLine("history:"+history);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(versionHistoryFilePath,exception);
            }
            Trace.WriteLine("creation gameEngine");
            GameEngine = new GameEngine(WebConfigurationManager.ConnectionStrings["DbContext"].ConnectionString, version,
                history);
            Trace.WriteLine("Game engine created");
            GameEngine.OnError += GameEngineOnOnError;

            Trace.WriteLine("Versions "+ GameEngine.VersionNumber+"   "+version);
            if (GameEngine.VersionNumber != version)
            {
                File.WriteAllText(versionNumberFileName, GameEngine.VersionNumber);
            }
            GameEngine.Feedback += GameEngineOnFeedback;
        }

        private static void GameEngineOnOnError(Exception exception)
        {
            LogException(exception);//todo: to introduce more properties I need to incapsulate TelemetryClient functionality that I need
        }

        private static void GameEngineOnFeedback(object sender, FeedbackEventArgs feedbackEventArgs)
        {
            TelemetryClient.TrackEvent("Feedback",new Dictionary<string, string> {["Message"]= feedbackEventArgs.FeedbackText },new Dictionary<string, double> {["FeedbackHere"]=1});
        }

        internal static GameEngine GameEngine { get; }

        public static readonly TelemetryClient TelemetryClient = new TelemetryClient();

        public static void LogException(Exception error, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            TelemetryClient.TrackException(error,properties,metrics);//todo: low later there is no need in this function, every place need to call TrackException напрямую and with providing additional data
        }
    }
}