using System;
using Microsoft.ApplicationInsights;

namespace MGM.TelemetryGlobal
{
    public class TelemetryStatic
    {
        [ThreadStatic]
        public static TelemetryClient TelemetryClient;

        public const string AnswerCallbackQueryKey = "AnswerCallbackQuery";
        public const string EditInlineKey = "EditInline";
        public const string SendMessageKey = "SendMessageKey";
        public const string SendPhotoKey = "SendPhotoKey";
        public const string EditMessageKey = "EditMessageKey";
        public const string ToChatKey = "ToChat";
        public const string ChatKey = "ChatId";
        public const string GameKey = "GameId";
    }
}