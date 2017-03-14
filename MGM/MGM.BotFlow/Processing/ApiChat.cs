using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MGM.Localization;
using MGM.TelemetryGlobal;
using Telegram.Bot;
using Telegram.Bot.Types;
using Chat = MGM.BotFlow.Persistance.Chat;

namespace MGM.BotFlow.Processing
{
    [DataContract]
    public sealed class ApiChat
    {
        [IgnoreDataMember]
        private Api _api;
        [IgnoreDataMember]
        private ApiChatLimiter _limiter;

        [DataMember]
        private readonly Chat _chat;

#if ONECHAT_DEBUG

        public static long RealChat;
#endif
        public ApiChat(Api api, Chat chat, ApiChatLimiter limiter)
        {
            _api = api;
            _limiter = limiter;
            _chat = chat;
        }

        public string Title => _chat.Title;

        public Echo Echo(string text, long replyTo, EchoOptions echoOptions)
        {
            var result = SendMessage(_chat.Id, text, replyTo, ref echoOptions);
            return EchoFromMessage(result, echoOptions);
        }

        public void ReplyQuery(string text, string replyTo)
        {
            Trace.WriteLine(text);
            TelemetryStatic.TelemetryClient.TrackEvent(TelemetryStatic.AnswerCallbackQueryKey,new Dictionary<string, string> {[TelemetryStatic.AnswerCallbackQueryKey]= _chat.Id.ToString()});
            _api.AnswerCallbackQuery(replyTo, text);
        }

        private Message SendMessage(long chatId, string text, long replyTo, ref EchoOptions echoOptions)
        {
            Trace.WriteLine(text);
            if (echoOptions == null)
            {
                echoOptions = EchoOptions.SimpleEcho();
            }

#if ONECHAT_DEBUG
            text = $"message to chat {chatId}: {text}";
            chatId = RealChat;
#endif
            var originalText = text;
                text = LS.Escape(text);//todo: çäåñü ïðîáëåìà, ïîòîìó ÷òî âåðí¸òñÿ unescaped è ïîòîì îïÿòü áóäåò escape ïðè îáíîâëåíèè

            EchoOptions options = echoOptions;
            return _limiter.RespectLimitForChat(chatId, () =>
            {
                TelemetryStatic.TelemetryClient.TrackEvent(TelemetryStatic.SendMessageKey,new Dictionary<string, string> {[TelemetryStatic.ToChatKey]=chatId.ToString()});
                var result = _api.SendTextMessage(chatId, text, false, false, (int)replyTo, options.ReplyMarkup,
                    ParseMode.Markdown).FromResult2(text);
                
                var textProperty = result.GetType().GetProperty("Text");
                textProperty.SetValue(result, originalText);//todo: low dirty thing because escaping occure in wrong place

                return result;
            });
            
        }

        private Echo EchoFromMessage(Message result, EchoOptions echoOptions)
        {
            return new Echo(result.MessageId, result.Text, this, echoOptions);
        }

        public Echo UpdateEchoInlineButtons(int messageId, EchoOptions echoOptions)
        {
            // ReSharper disable once JoinDeclarationAndInitializer
            long chatId;
#if ONECHAT_DEBUG
            chatId = RealChat;
#else
            chatId = _chat.Id;
#endif
            TelemetryStatic.TelemetryClient.TrackEvent(TelemetryStatic.EditInlineKey,new Dictionary<string, string> {[TelemetryStatic.ToChatKey]=chatId.ToString()});
                var result = _api.EditMessageReplyMarkup(chatId, messageId, echoOptions.ReplyMarkup).FromResult2();
                return EchoFromMessage(result, echoOptions);
        }

        public Echo UpdateEchoText(int messageId, string newText, EchoOptions echoOptions = null)
        {
            Trace.WriteLine(newText);
            var chatId = _chat.Id;
#if ONECHAT_DEBUG
            newText = $"echo update to chat {chatId} {newText}";
            chatId = RealChat;
#endif
            newText = LS.Escape(newText);
            return _limiter.RespectLimitForChat(chatId, () =>
            {
                TelemetryStatic.TelemetryClient.TrackEvent(TelemetryStatic.EditMessageKey, new Dictionary<string, string> { [TelemetryStatic.ToChatKey] = chatId.ToString() });
                Message result = _api.EditMessageText(chatId, messageId, newText, ParseMode.Markdown, false, echoOptions?.ReplyMarkup).FromResult2(newText);
                return EchoFromMessage(result, null);
            });
            
        }

        public Echo PrivateEcho(long userId, string text, EchoOptions echoOptions = null)
        {
            var message = SendMessage(userId, text, 0, ref echoOptions);
            
            return EchoFromMessage(message, echoOptions);
        }

        [OnDeserialized]
        private void AfterDeserialized(StreamingContext context)
        {
            _api = ((IApiContainer) context.Context).Api;
            _limiter = ((IApiChatLimiterProvider) context.Context).GetLimiter();
        }

        public void PhotoEcho(string body, string photoId)
        {
            // ReSharper disable once UnusedVariable
            var chatId = _chat.Id;
#if ONECHAT_DEBUG
            body = $"photo to {chatId} {body}";
            chatId = RealChat;
#endif
            // ReSharper disable once UnusedVariable
            _limiter.RespectLimitForChat<object>(chatId, () =>
            {
                TelemetryStatic.TelemetryClient.TrackEvent(TelemetryStatic.EditInlineKey, new Dictionary<string, string> { [TelemetryStatic.ToChatKey] = chatId.ToString() });
                _api.SendPhoto(chatId, photoId, body).FromResult2(body);
                return null;
            });
        }
    }

    internal static class ApiChatHelper
    {
        public static T FromResult2<T>(this Task<T> task,string text=null) where T : new()
        {
            try
            {
                return task.Result;
            }
            catch (AggregateException exception)
            {
                var errorText = $"";

                var innerException = exception.InnerException;
                if (innerException is ApiRequestException)
                {
                    var errorCode = ((ApiRequestException)innerException).ErrorCode;
                    if (errorCode == 403) return new T();
                    if (errorCode == 400)
                    {//Error_Code is 400Bad Request: group chat was migrated to a supergroup chat Error_Code is 400Bad Request: group chat was migrated to a supergroup chat
                        TelemetryStatic.TelemetryClient.TrackException(new Exception("Пришла 400 ошибка, мы на ней не падали, если что.",innerException));
                        return new T();
                    }
                    errorText += $@"
Error_Code is {errorCode}";
                }

                if (text != null)
                {
                    errorText += $@"
Text was {text}";
                }
                throw new ApiChatException(errorText, innerException);
            }
        }
    }
}