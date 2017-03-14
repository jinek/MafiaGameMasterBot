using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using MGM.Helpers;
using MGM.TelemetryGlobal;

namespace MGM.BotFlow.Processing
{
    public sealed class ApiChatLimiter
    {
        private readonly LockerCollection<long> _chatLockers = new LockerCollection<long>();
        private readonly ConcurrentDictionary<long,DateTime> _callTime = new ConcurrentDictionary<long, DateTime>();

        public T RespectLimitForChat<T>(long chatId,Func<T> action)//todo: low we need to introduce timeout as well. Now there is no need, but in theory....
        {
            var locker = _chatLockers.GetOrCreateLocker(chatId);
            lock (locker)
            {
                if (!_callTime.ContainsKey(chatId))
                {
                    if(!_callTime.TryAdd(chatId,DateTime.Now.AddSeconds(-5)))throw new InvalidOperationException("При создании ключа уже был такой ключ");
                    //todo: low btw concurrentDictionary allows to write easier code... do it
                    //todo: low I had to provide some value.. (-5)  so may be algo not the best one
                }

                //NO RESPECTS ONLY 1 Call per second LIMIT
                DateTime lastCallTime = _callTime[chatId];

                var timeToWait = TimeSpan.FromSeconds(1)-(DateTime.Now - lastCallTime);

                TelemetryStatic.TelemetryClient.TrackMetric("Wait 1 Sec Limit",timeToWait.TotalSeconds);
                if(timeToWait>TimeSpan.Zero)
                {
                    Thread.Sleep(timeToWait);
                }

                var result = action();
                _callTime[chatId] = DateTime.Now;
                return result;
            }
        }
    }
}