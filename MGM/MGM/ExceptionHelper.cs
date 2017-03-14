using System;

namespace MGM
{
    public static class ExceptionHelper
    {
        public static string GetDescription(this Exception exception)
        {
            if (exception == null) return "exception = null";
            return
                $@"Time: {DateTime.Now}
Exception: {exception.GetType()}
Message: {exception.Message}
StackTrace: {exception.StackTrace}
InnerException:
{GetDescription(exception.InnerException)}";
        }
    }
}