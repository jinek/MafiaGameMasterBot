namespace MGM.Localization
{
    public class OneLanguageString : ILocalizedString
    {
        private readonly string _str;

        public OneLanguageString(string str)
        {
            _str = str;
        }

        public string GetLocalizedString()
        {
            return _str;
        }
    }
}