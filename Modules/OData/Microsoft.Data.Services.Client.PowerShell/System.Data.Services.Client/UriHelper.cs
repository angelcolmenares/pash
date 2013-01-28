namespace System.Data.Services.Client
{
    using System;
    using System.Data.Services.Client.Metadata;

    internal static class UriHelper
    {
        internal const string ADD = "add";
        internal const char AMPERSAND = '&';
        internal const string AND = "and";
        internal const char ASTERISK = '*';
        internal const char ATSIGN = '@';
        internal const string CAST = "cast";
        internal const char COLON = ':';
        internal const char COMMA = ',';
        internal const string COUNT = "count";
        internal const string COUNTALL = "allpages";
        internal const string DIV = "div";
        internal const char DOLLARSIGN = '$';
        internal const string EQ = "eq";
        internal const char EQUALSSIGN = '=';
        internal const char FORWARDSLASH = '/';
        internal const string GE = "ge";
        internal const string GT = "gt";
        internal const string ISOF = "isof";
        internal const string LE = "le";
        internal const char LEFTPAREN = '(';
        internal const string LT = "lt";
        internal const string MOD = "mod";
        internal const string MUL = "mul";
        internal const string NE = "ne";
        internal const string NEGATE = "-";
        internal const string NOT = "not";
        internal const string NULL = "null";
        internal const string OPTIONCOUNT = "inlinecount";
        internal const string OPTIONDESC = "desc";
        internal const string OPTIONEXPAND = "expand";
        internal const string OPTIONFILTER = "filter";
        internal const string OPTIONORDERBY = "orderby";
        internal const string OPTIONSELECT = "select";
        internal const string OPTIONSKIP = "skip";
        internal const string OPTIONTOP = "top";
        internal const string OR = "or";
        internal const char QUESTIONMARK = '?';
        internal const char QUOTE = '\'';
        internal const char RIGHTPAREN = ')';
        internal const char SPACE = ' ';
        internal const string SUB = "sub";

        internal static string GetEntityTypeNameForUriAndValidateMaxProtocolVersion(Type type, DataServiceContext context, ref Version uriVersion)
        {
            if (context.MaxProtocolVersionAsVersion < Util.DataServiceVersion3)
            {
                throw new NotSupportedException(Strings.ALinq_TypeAsNotSupportedForMaxDataServiceVersionLessThan3);
            }
            if (!ClientTypeUtil.TypeOrElementTypeIsEntity(type))
            {
                throw new NotSupportedException(Strings.ALinq_TypeAsArgumentNotEntityType(type.FullName));
            }
            WebUtil.RaiseVersion(ref uriVersion, Util.DataServiceVersion3);
            return (context.ResolveNameFromType(type) ?? type.FullName);
        }

        internal static string GetTypeNameForUri(Type type, DataServiceContext context)
        {
            PrimitiveType type2;
            type = Nullable.GetUnderlyingType(type) ?? type;
            if (PrimitiveType.TryGetPrimitiveType(type, out type2))
            {
                if (!type2.HasReverseMapping)
                {
                    throw new NotSupportedException(Strings.ALinq_CantCastToUnsupportedPrimitive(type.Name));
                }
                return type2.EdmTypeName;
            }
            return (context.ResolveNameFromType(type) ?? type.FullName);
        }
    }
}

