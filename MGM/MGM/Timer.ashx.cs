using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MGM
{
    public class Timer : IHttpHandler
    {
        static Timer()
        {
            RunTimer(HttpContext.Current.Request.Url);
        }
        
        public void ProcessRequest(HttpContext context)
        {
            //todo: low обновить телеграм
        }

        public static void CheckTimerRun()
        {
            //ничего не делаем, всё в конструкторе
        }

        private static void RunTimer(Uri originalUri)
        {
            //todo: low посмотреть че будет если бота заблочить или удалить чат или выкинуть его из чата
            Trace.WriteLine("Timer invoked");

            ThreadPool.QueueUserWorkItem(state =>
            {
                while (true) //todo: low узнать как узнать что скоро приложение выключится
                {
                    Thread.Sleep(10000);

                    try
                    {
                        if (WorkHere.GameEngine.Timer())
                        {
//если есть ещё работы, следим что бы сервер не лёг
                            Trace.WriteLine("Есть игры на будущее, таймер запускает сам себя");
                            try
                            {
                                using (var webClient = new WebClient())
                                {
                                    webClient.DownloadString(
                                        new UriBuilder(originalUri) {Path = "Timer.ashx", Query = string.Empty}.Uri);
                                }
                            }
                            catch (WebException exception)
                            {
                                throw new WebException($"Original Uri was {originalUri}",exception);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                      WorkHere.LogException(exception);
                    }
                    //todo: low это как бы может выключиться посередине какой-нить хрени, например пользователи сохранятся, а game нет
                }
                // ReSharper disable once FunctionNeverReturns function must work till shutdown by timer
            });
        }

        public bool IsReusable => false;
    }
}