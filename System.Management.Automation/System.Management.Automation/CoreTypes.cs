namespace System.Management.Automation
{
    using Microsoft.Management.Infrastructure;
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.DirectoryServices;
    using System.Globalization;
    using System.Management;
    using System.Management.Automation.Language;
    using System.Net;
    using System.Net.Mail;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Text.RegularExpressions;
    using System.Xml;

    internal static class CoreTypes
    {
        internal static Lazy<Dictionary<Type, string[]>> Items = new Lazy<Dictionary<Type, string[]>>(delegate {
            Dictionary<Type, string[]> dictionary = new Dictionary<Type, string[]>();
            dictionary.Add(typeof(AliasAttribute), new string[] { "Alias" });
            dictionary.Add(typeof(AllowEmptyCollectionAttribute), new string[] { "AllowEmptyCollection" });
            dictionary.Add(typeof(AllowEmptyStringAttribute), new string[] { "AllowEmptyString" });
            dictionary.Add(typeof(AllowNullAttribute), new string[] { "AllowNull" });
            dictionary.Add(typeof(Array), new string[] { "array" });
            dictionary.Add(typeof(bool), new string[] { "bool" });
            dictionary.Add(typeof(byte), new string[] { "byte" });
            dictionary.Add(typeof(char), new string[] { "char" });
            dictionary.Add(typeof(CmdletBindingAttribute), new string[] { "CmdletBinding" });
            dictionary.Add(typeof(DateTime), new string[] { "datetime" });
            dictionary.Add(typeof(decimal), new string[] { "decimal" });
            dictionary.Add(typeof(DirectoryEntry), new string[] { "adsi" });
            dictionary.Add(typeof(DirectorySearcher), new string[] { "adsisearcher" });
            dictionary.Add(typeof(double), new string[] { "double" });
            dictionary.Add(typeof(float), new string[] { "float", "single" });
            dictionary.Add(typeof(Guid), new string[] { "guid" });
            dictionary.Add(typeof(Hashtable), new string[] { "hashtable" });
            dictionary.Add(typeof(int), new string[] { "int", "int32" });
            dictionary.Add(typeof(short), new string[] { "int16" });
            dictionary.Add(typeof(long), new string[] { "long", "int64" });
            dictionary.Add(typeof(ManagementClass), new string[] { "wmiclass" });
            dictionary.Add(typeof(ManagementObject), new string[] { "wmi" });
            dictionary.Add(typeof(ManagementObjectSearcher), new string[] { "wmisearcher" });
            dictionary.Add(typeof(CimInstance), new string[] { "ciminstance" });
            dictionary.Add(typeof(ModuleSpecification), null);
            dictionary.Add(typeof(NullString), new string[] { "NullString" });
            dictionary.Add(typeof(OutputTypeAttribute), new string[] { "OutputType" });
            dictionary.Add(typeof(object[]), null);
            dictionary.Add(typeof(ParameterAttribute), new string[] { "Parameter" });
            dictionary.Add(typeof(PSCredential), new string[] { "pscredential" });
            dictionary.Add(typeof(PSDefaultValueAttribute), new string[] { "PSDefaultValue" });
            dictionary.Add(typeof(PSListModifier), new string[] { "pslistmodifier" });
            dictionary.Add(typeof(PSObject), new string[] { "psobject", "pscustomobject" });
            dictionary.Add(typeof(PSPrimitiveDictionary), new string[] { "psprimitivedictionary" });
            dictionary.Add(typeof(PSReference), new string[] { "ref" });
            dictionary.Add(typeof(PSTypeNameAttribute), new string[] { "PSTypeNameAttribute" });
            dictionary.Add(typeof(Regex), new string[] { "regex" });
            dictionary.Add(typeof(sbyte), new string[] { "sbyte" });
            dictionary.Add(typeof(string), new string[] { "string" });
            dictionary.Add(typeof(SupportsWildcardsAttribute), new string[] { "SupportsWildcards" });
            dictionary.Add(typeof(SwitchParameter), new string[] { "switch" });
            dictionary.Add(typeof(CultureInfo), new string[] { "cultureinfo" });
            dictionary.Add(typeof(IPAddress), new string[] { "ipaddress" });
            dictionary.Add(typeof(MailAddress), new string[] { "mailaddress" });
            dictionary.Add(typeof(BigInteger), new string[] { "bigint" });
            dictionary.Add(typeof(SecureString), new string[] { "securestring" });
            dictionary.Add(typeof(TimeSpan), new string[] { "timespan" });
            dictionary.Add(typeof(ushort), new string[] { "uint16" });
            dictionary.Add(typeof(uint), new string[] { "uint32" });
            dictionary.Add(typeof(ulong), new string[] { "uint64" });
            dictionary.Add(typeof(Uri), new string[] { "uri" });
            dictionary.Add(typeof(ValidateCountAttribute), new string[] { "ValidateCount" });
            dictionary.Add(typeof(ValidateLengthAttribute), new string[] { "ValidateLength" });
            dictionary.Add(typeof(ValidateNotNullAttribute), new string[] { "ValidateNotNull" });
            dictionary.Add(typeof(ValidateNotNullOrEmptyAttribute), new string[] { "ValidateNotNullOrEmpty" });
            dictionary.Add(typeof(ValidatePatternAttribute), new string[] { "ValidatePattern" });
            dictionary.Add(typeof(ValidateRangeAttribute), new string[] { "ValidateRange" });
            dictionary.Add(typeof(ValidateScriptAttribute), new string[] { "ValidateScript" });
            dictionary.Add(typeof(ValidateSetAttribute), new string[] { "ValidateSet" });
            dictionary.Add(typeof(Version), new string[] { "version" });
            dictionary.Add(typeof(void), new string[] { "void" });
            dictionary.Add(typeof(XmlDocument), new string[] { "xml" });
            return dictionary;
        });


        internal static bool Contains(Type inputType)
        {
            if (((!Items.Value.ContainsKey(inputType) && !inputType.IsEnum) && (!inputType.IsGenericType || (inputType.GetGenericTypeDefinition() != typeof(Nullable<>)))) && (!inputType.IsGenericType || (inputType.GetGenericTypeDefinition() != typeof(FlagsExpression<>))))
            {
                return (inputType.IsArray && Contains(inputType.GetElementType()));
            }
            return true;
        }
    }
}

