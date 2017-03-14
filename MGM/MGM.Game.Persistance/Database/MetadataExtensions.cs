using Telerik.OpenAccess;
using Telerik.OpenAccess.Metadata.Fluent;
using Telerik.OpenAccess.Metadata.Fluent.Advanced;

namespace MGM.Game.Persistance.Database
{
    internal static class MetadataExtensions
    {
        public static StringPropertyConfiguration AsInfiniteString(
            this StringPropertyConfiguration propertyConfiguration)
        {
            return propertyConfiguration.HasColumnType(TextColumnType)
                .WithOpenAccessType(OpenAccessType.StringInfiniteLength);
        }

        public static StringPropertyConfiguration AsInfiniteStringNText(
            this StringPropertyConfiguration propertyConfiguration)
        {
            return propertyConfiguration.HasColumnType(NTextColumnType)
                .WithOpenAccessType(OpenAccessType.StringInfiniteLength);
        }

        public static StringPropertyConfiguration AsNVarChar(this StringPropertyConfiguration propertyConfiguration,
            int n = 50)
        {
            return propertyConfiguration.HasColumnType(NVarCharColumnType)
                .WithOpenAccessType(OpenAccessType.StringVariableLength)
                .HasLength(n);
        }

        internal const string NTextColumnType = "ntext";
        internal const string TextColumnType = "text";
        internal const string NVarCharColumnType = "nvarchar";
        internal const string BitColumnType = "bit";
    }
}