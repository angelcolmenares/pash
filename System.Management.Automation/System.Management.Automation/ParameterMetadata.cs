namespace System.Management.Automation
{
    using Microsoft.PowerShell;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    public sealed class ParameterMetadata
    {
        private Collection<string> aliases;
        private const string AliasesFormat = "{0}[Alias({1})]";
        private const string AllowEmptyCollectionFormat = "{0}[AllowEmptyCollection()]";
        private const string AllowEmptyStringFormat = "{0}[AllowEmptyString()]";
        private const string AllowNullFormat = "{0}[AllowNull()]";
        private Collection<Attribute> attributes;
        private bool isDynamic;
        private string name;
        private const string ParameterNameFormat = "{0}${{{1}}}";
        private const string ParameterSetNameFormat = "ParameterSetName='{0}'";
        private Dictionary<string, ParameterSetMetadata> parameterSets;
        private Type parameterType;
        private const string ParameterTypeFormat = "{0}[{1}]";
        private const string PSTypeNameFormat = "{0}[PSTypeName('{1}')]";
        private const string ValidateCountFormat = "{0}[ValidateCount({1}, {2})]";
        private const string ValidateLengthFormat = "{0}[ValidateLength({1}, {2})]";
        private const string ValidateNotNullFormat = "{0}[ValidateNotNull()]";
        private const string ValidateNotNullOrEmptyFormat = "{0}[ValidateNotNullOrEmpty()]";
        private const string ValidatePatternFormat = "{0}[ValidatePattern('{1}')]";
        private const string ValidateRangeFloatFormat = "{0}[ValidateRange({1:R}, {2:R})]";
        private const string ValidateRangeFormat = "{0}[ValidateRange({1}, {2})]";
        private const string ValidateScriptFormat = "{0}[ValidateScript({{ {1} }})]";
        private const string ValidateSetFormat = "{0}[ValidateSet({1})]";

        internal ParameterMetadata(CompiledCommandParameter cmdParameterMD)
        {
            this.Initialize(cmdParameterMD);
        }

        public ParameterMetadata(ParameterMetadata other)
        {
            if (other == null)
            {
                throw PSTraceSource.NewArgumentNullException("other");
            }
            this.isDynamic = other.isDynamic;
            this.name = other.name;
            this.parameterType = other.parameterType;
            this.aliases = new Collection<string>(new List<string>(other.aliases.Count));
            foreach (string str in other.aliases)
            {
                this.aliases.Add(str);
            }
            if (other.attributes == null)
            {
                this.attributes = null;
            }
            else
            {
                this.attributes = new Collection<Attribute>(new List<Attribute>(other.attributes.Count));
                foreach (Attribute attribute in other.attributes)
                {
                    this.attributes.Add(attribute);
                }
            }
            this.parameterSets = null;
            if (other.parameterSets == null)
            {
                this.parameterSets = null;
            }
            else
            {
                this.parameterSets = new Dictionary<string, ParameterSetMetadata>(other.parameterSets.Count);
                foreach (KeyValuePair<string, ParameterSetMetadata> pair in other.parameterSets)
                {
                    this.parameterSets.Add(pair.Key, new ParameterSetMetadata(pair.Value));
                }
            }
        }

        public ParameterMetadata(string name) : this(name, null)
        {
        }

        public ParameterMetadata(string name, Type parameterType)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            this.name = name;
            this.parameterType = parameterType;
            this.attributes = new Collection<Attribute>();
            this.aliases = new Collection<string>();
            this.parameterSets = new Dictionary<string, ParameterSetMetadata>();
        }

        internal ParameterMetadata(Collection<string> aliases, bool isDynamic, string name, Dictionary<string, ParameterSetMetadata> parameterSets, Type parameterType)
        {
            this.aliases = aliases;
            this.isDynamic = isDynamic;
            this.name = name;
            this.parameterSets = parameterSets;
            this.parameterType = parameterType;
            this.attributes = new Collection<Attribute>();
        }

        internal static Dictionary<string, ParameterMetadata> GetParameterMetadata(MergedCommandParameterMetadata cmdParameterMetadata)
        {
            Dictionary<string, ParameterMetadata> dictionary = new Dictionary<string, ParameterMetadata>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, MergedCompiledCommandParameter> pair in cmdParameterMetadata.BindableParameters)
            {
                string key = pair.Key;
                ParameterMetadata metadata = new ParameterMetadata(pair.Value.Parameter);
                dictionary.Add(key, metadata);
            }
            return dictionary;
        }

        public static Dictionary<string, ParameterMetadata> GetParameterMetadata(Type type)
        {
            if (null == type)
            {
                throw PSTraceSource.NewArgumentNullException("type");
            }
            CommandMetadata metadata = new CommandMetadata(type);
            Dictionary<string, ParameterMetadata> parameters = metadata.Parameters;
            metadata = null;
            return parameters;
        }

        private string GetProxyAttributeData(Attribute attrib, string prefix)
        {
            ValidateLengthAttribute attribute = attrib as ValidateLengthAttribute;
            if (attribute != null)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}[ValidateLength({1}, {2})]", new object[] { prefix, attribute.MinLength, attribute.MaxLength });
            }
            ValidateRangeAttribute attribute2 = attrib as ValidateRangeAttribute;
            if (attribute2 != null)
            {
                string str2;
                Type type = attribute2.MinRange.GetType();
                if ((type == typeof(float)) || (type == typeof(double)))
                {
                    str2 = "{0}[ValidateRange({1:R}, {2:R})]";
                }
                else
                {
                    str2 = "{0}[ValidateRange({1}, {2})]";
                }
                return string.Format(CultureInfo.InvariantCulture, str2, new object[] { prefix, attribute2.MinRange, attribute2.MaxRange });
            }
            if (attrib is AllowNullAttribute)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}[AllowNull()]", new object[] { prefix });
            }
            if (attrib is AllowEmptyStringAttribute)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}[AllowEmptyString()]", new object[] { prefix });
            }
            if (attrib is AllowEmptyCollectionAttribute)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}[AllowEmptyCollection()]", new object[] { prefix });
            }
            ValidatePatternAttribute attribute6 = attrib as ValidatePatternAttribute;
            if (attribute6 != null)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}[ValidatePattern('{1}')]", new object[] { prefix, CommandMetadata.EscapeSingleQuotedString(attribute6.RegexPattern) });
            }
            ValidateCountAttribute attribute7 = attrib as ValidateCountAttribute;
            if (attribute7 != null)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}[ValidateCount({1}, {2})]", new object[] { prefix, attribute7.MinLength, attribute7.MaxLength });
            }
            if (attrib is ValidateNotNullAttribute)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}[ValidateNotNull()]", new object[] { prefix });
            }
            if (attrib is ValidateNotNullOrEmptyAttribute)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}[ValidateNotNullOrEmpty()]", new object[] { prefix });
            }
            ValidateSetAttribute attribute10 = attrib as ValidateSetAttribute;
            if (attribute10 != null)
            {
                StringBuilder builder = new StringBuilder();
                string str3 = "";
                foreach (string str4 in attribute10.ValidValues)
                {
                    builder.AppendFormat(CultureInfo.InvariantCulture, "{0}'{1}'", new object[] { str3, CommandMetadata.EscapeSingleQuotedString(str4) });
                    str3 = ",";
                }
                return string.Format(CultureInfo.InvariantCulture, "{0}[ValidateSet({1})]", new object[] { prefix, builder.ToString() });
            }
            ValidateScriptAttribute attribute11 = attrib as ValidateScriptAttribute;
            if (attribute11 != null)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}[ValidateScript({{ {1} }})]", new object[] { prefix, attribute11.ScriptBlock.ToString() });
            }
            PSTypeNameAttribute attribute12 = attrib as PSTypeNameAttribute;
            if (attribute12 != null)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}[PSTypeName('{1}')]", new object[] { prefix, CommandMetadata.EscapeSingleQuotedString(attribute12.PSTypeName) });
            }
            return null;
        }

        internal string GetProxyParameterData(string prefix, string paramNameOverride, bool isProxyForCmdlet)
        {
            StringBuilder builder = new StringBuilder();
            if ((this.parameterSets != null) && isProxyForCmdlet)
            {
                foreach (string str in this.parameterSets.Keys)
                {
                    string proxyParameterData = this.parameterSets[str].GetProxyParameterData();
                    if (!string.IsNullOrEmpty(proxyParameterData) || !str.Equals("__AllParameterSets"))
                    {
                        string str3 = "";
                        builder.Append(prefix);
                        builder.Append("[Parameter(");
                        if (!str.Equals("__AllParameterSets"))
                        {
                            builder.AppendFormat(CultureInfo.InvariantCulture, "ParameterSetName='{0}'", new object[] { CommandMetadata.EscapeSingleQuotedString(str) });
                            str3 = ", ";
                        }
                        if (!string.IsNullOrEmpty(proxyParameterData))
                        {
                            builder.Append(str3);
                            builder.Append(proxyParameterData);
                        }
                        builder.Append(")]");
                    }
                }
            }
            if ((this.aliases != null) && (this.aliases.Count > 0))
            {
                StringBuilder builder2 = new StringBuilder();
                string str4 = "";
                foreach (string str5 in this.aliases)
                {
                    builder2.AppendFormat(CultureInfo.InvariantCulture, "{0}'{1}'", new object[] { str4, CommandMetadata.EscapeSingleQuotedString(str5) });
                    str4 = ",";
                }
                builder.AppendFormat(CultureInfo.InvariantCulture, "{0}[Alias({1})]", new object[] { prefix, builder2.ToString() });
            }
            if ((this.attributes != null) && (this.attributes.Count > 0))
            {
                foreach (Attribute attribute in this.attributes)
                {
                    string proxyAttributeData = this.GetProxyAttributeData(attribute, prefix);
                    if (!string.IsNullOrEmpty(proxyAttributeData))
                    {
                        builder.Append(proxyAttributeData);
                    }
                }
            }
            if (this.SwitchParameter)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "{0}[{1}]", new object[] { prefix, "switch" });
            }
            else if (this.parameterType != null)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "{0}[{1}]", new object[] { prefix, ToStringCodeMethods.Type(this.parameterType, false) });
            }
            builder.AppendFormat(CultureInfo.InvariantCulture, "{0}${{{1}}}", new object[] { prefix, CommandMetadata.EscapeVariableName(string.IsNullOrEmpty(paramNameOverride) ? this.name : paramNameOverride) });
            return builder.ToString();
        }

        internal void Initialize(CompiledCommandParameter compiledParameterMD)
        {
            this.name = compiledParameterMD.Name;
            this.parameterType = compiledParameterMD.Type;
            this.isDynamic = compiledParameterMD.IsDynamic;
            this.parameterSets = new Dictionary<string, ParameterSetMetadata>(StringComparer.OrdinalIgnoreCase);
            foreach (string str in compiledParameterMD.ParameterSetData.Keys)
            {
                ParameterSetSpecificMetadata psMD = compiledParameterMD.ParameterSetData[str];
                this.parameterSets.Add(str, new ParameterSetMetadata(psMD));
            }
            this.aliases = new Collection<string>();
            foreach (string str2 in compiledParameterMD.Aliases)
            {
                this.aliases.Add(str2);
            }
            this.attributes = new Collection<Attribute>();
            foreach (Attribute attribute in compiledParameterMD.CompiledAttributes)
            {
                this.attributes.Add(attribute);
            }
        }

        internal bool IsMatchingType(PSTypeName psTypeName)
        {
            Type fromType = psTypeName.Type;
            if (fromType != null)
            {
                bool flag = LanguagePrimitives.FigureConversion(typeof(object), this.ParameterType).Rank >= ConversionRank.AssignableS2A;
                if (fromType.Equals(typeof(object)))
                {
                    return flag;
                }
                if (flag)
                {
                    return ((psTypeName.Type != null) && psTypeName.Type.Equals(typeof(object)));
                }
                LanguagePrimitives.ConversionData data = LanguagePrimitives.FigureConversion(fromType, this.ParameterType);
                return ((data != null) && (data.Rank >= ConversionRank.NumericImplicitS2A));
            }
            WildcardPattern pattern = new WildcardPattern("*" + (psTypeName.Name ?? ""), WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
            if (pattern.IsMatch(this.ParameterType.FullName))
            {
                return true;
            }
            if (this.ParameterType.IsArray && pattern.IsMatch(this.ParameterType.GetElementType().FullName))
            {
                return true;
            }
            if (this.Attributes != null)
            {
                PSTypeNameAttribute attribute = this.Attributes.OfType<PSTypeNameAttribute>().FirstOrDefault<PSTypeNameAttribute>();
                if ((attribute != null) && pattern.IsMatch(attribute.PSTypeName))
                {
                    return true;
                }
            }
            return false;
        }

        public Collection<string> Aliases
        {
            get
            {
                return this.aliases;
            }
        }

        public Collection<Attribute> Attributes
        {
            get
            {
                return this.attributes;
            }
        }

        public bool IsDynamic
        {
            get
            {
                return this.isDynamic;
            }
            set
            {
                this.isDynamic = value;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw PSTraceSource.NewArgumentNullException("Name");
                }
                this.name = value;
            }
        }

        public Dictionary<string, ParameterSetMetadata> ParameterSets
        {
            get
            {
                return this.parameterSets;
            }
        }

        public Type ParameterType
        {
            get
            {
                return this.parameterType;
            }
            set
            {
                this.parameterType = value;
            }
        }

        public bool SwitchParameter
        {
            get
            {
                return ((this.parameterType != null) && this.parameterType.Equals(typeof(System.Management.Automation.SwitchParameter)));
            }
        }
    }
}

