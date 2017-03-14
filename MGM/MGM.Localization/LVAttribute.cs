using System;

namespace MGM.Localization
{
    [AttributeUsage(AttributeTargets.Field,AllowMultiple = true)]
    internal class LVAttribute : Attribute
    {
        public LVAttribute(string str)
        {
            Str = str;
        }

        public string Str { get; }
    }
}