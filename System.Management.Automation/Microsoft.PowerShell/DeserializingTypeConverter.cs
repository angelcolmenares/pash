namespace Microsoft.PowerShell
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Net;
    using System.Net.Mail;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;

    public sealed class DeserializingTypeConverter : PSTypeConverter
    {
        private static readonly Dictionary<Type, Converter<PSObject, object>> converter = new Dictionary<Type, Converter<PSObject, object>>();

        static DeserializingTypeConverter()
        {
            converter.Add(typeof(PSPrimitiveDictionary), new Converter<PSObject, object>(DeserializingTypeConverter.RehydratePrimitiveHashtable));
            converter.Add(typeof(SwitchParameter), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateSwitchParameter));
            converter.Add(typeof(PSListModifier), new Converter<PSObject, object>(DeserializingTypeConverter.RehydratePSListModifier));
            converter.Add(typeof(PSCredential), new Converter<PSObject, object>(DeserializingTypeConverter.RehydratePSCredential));
            converter.Add(typeof(PSSenderInfo), new Converter<PSObject, object>(DeserializingTypeConverter.RehydratePSSenderInfo));
            converter.Add(typeof(IPAddress), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateIPAddress));
            converter.Add(typeof(MailAddress), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateMailAddress));
            converter.Add(typeof(CultureInfo), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateCultureInfo));
            converter.Add(typeof(X509Certificate2), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateX509Certificate2));
            converter.Add(typeof(X500DistinguishedName), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateX500DistinguishedName));
            converter.Add(typeof(DirectorySecurity), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateObjectSecurity<DirectorySecurity>));
            converter.Add(typeof(FileSecurity), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateObjectSecurity<FileSecurity>));
            converter.Add(typeof(RegistrySecurity), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateObjectSecurity<RegistrySecurity>));
            converter.Add(typeof(ParameterSetMetadata), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateParameterSetMetadata));
            converter.Add(typeof(ExtendedTypeDefinition), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateExtendedTypeDefinition));
            converter.Add(typeof(FormatViewDefinition), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateFormatViewDefinition));
            converter.Add(typeof(PSControl), new Converter<PSObject, object>(DeserializingTypeConverter.RehydratePSControl));
            converter.Add(typeof(DisplayEntry), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateDisplayEntry));
            converter.Add(typeof(TableControlColumnHeader), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateTableControlColumnHeader));
            converter.Add(typeof(TableControlRow), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateTableControlRow));
            converter.Add(typeof(TableControlColumn), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateTableControlColumn));
            converter.Add(typeof(ListControlEntry), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateListControlEntry));
            converter.Add(typeof(ListControlEntryItem), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateListControlEntryItem));
            converter.Add(typeof(WideControlEntryItem), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateWideControlEntryItem));
            converter.Add(typeof(CompletionResult), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateCompletionResult));
            converter.Add(typeof(CommandCompletion), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateCommandCompletion));
            converter.Add(typeof(JobStateInfo), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateJobStateInfo));
            converter.Add(typeof(JobStateEventArgs), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateJobStateEventArgs));
            converter.Add(typeof(PSSessionOption), new Converter<PSObject, object>(DeserializingTypeConverter.RehydratePSSessionOption));
        }

        public override bool CanConvertFrom(PSObject sourceValue, Type destinationType)
        {
            foreach (Type type in converter.Keys)
            {
                if (Deserializer.IsDeserializedInstanceOfType(sourceValue, type))
                {
                    return true;
                }
            }
            return false;
        }

        public override bool CanConvertFrom(object sourceValue, Type destinationType)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvertTo(PSObject sourceValue, Type destinationType)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvertTo(object sourceValue, Type destinationType)
        {
            return false;
        }

        private static object ConvertFrom(PSObject o, Converter<PSObject, object> converter)
        {
            PSObject input = o;
            object obj3 = converter(input);
            bool flag = false;
            PSObject obj4 = PSObject.AsPSObject(obj3);
            foreach (PSMemberInfo info in input.InstanceMembers)
            {
                if ((info.MemberType == (info.MemberType & (PSMemberTypes.MemberSet | PSMemberTypes.PropertySet | PSMemberTypes.Properties))) && (obj4.Members[info.Name] == null))
                {
                    obj4.InstanceMembers.Add(info);
                    flag = true;
                }
            }
            if (flag)
            {
                return obj4;
            }
            return obj3;
        }

        public override object ConvertFrom(PSObject sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase)
        {
            if (destinationType == null)
            {
                throw PSTraceSource.NewArgumentNullException("destinationType");
            }
            if (sourceValue == null)
            {
                throw new PSInvalidCastException("InvalidCastWhenRehydratingFromNull", PSTraceSource.NewArgumentNullException("sourceValue"), ExtendedTypeSystem.InvalidCastFromNull, new object[] { destinationType.ToString() });
            }
            foreach (KeyValuePair<Type, Converter<PSObject, object>> pair in DeserializingTypeConverter.converter)
            {
                Type key = pair.Key;
                Converter<PSObject, object> converter = pair.Value;
                if (Deserializer.IsDeserializedInstanceOfType(sourceValue, key))
                {
                    return ConvertFrom(sourceValue, converter);
                }
            }
            throw new PSInvalidCastException("InvalidCastEnumFromTypeNotAString", null, ExtendedTypeSystem.InvalidCastException, new object[] { sourceValue, destinationType });
        }

        public override object ConvertFrom(object sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase)
        {
            throw new NotImplementedException();
        }

        public override object ConvertTo(PSObject sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase)
        {
            throw new NotImplementedException();
        }

        public override object ConvertTo(object sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase)
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        public static Guid GetFormatViewDefinitionInstanceId(PSObject instance)
        {
            if (instance == null)
            {
                throw PSTraceSource.NewArgumentNullException("instance");
            }
            FormatViewDefinition baseObject = instance.BaseObject as FormatViewDefinition;
            if (baseObject == null)
            {
                throw PSTraceSource.NewArgumentNullException("instance");
            }
            return baseObject.InstanceId;
        }

        public static int GetParameterSetMetadataFlags(PSObject instance)
        {
            if (instance == null)
            {
                throw PSTraceSource.NewArgumentNullException("instance");
            }
            ParameterSetMetadata baseObject = instance.BaseObject as ParameterSetMetadata;
            if (baseObject == null)
            {
                throw PSTraceSource.NewArgumentNullException("instance");
            }
            return (int) baseObject.Flags;
        }

        private static T GetPropertyValue<T>(PSObject pso, string propertyName)
        {
            return GetPropertyValue<T>(pso, propertyName, RehydrationFlags.NullValueBad);
        }

        internal static T GetPropertyValue<T>(PSObject pso, string propertyName, RehydrationFlags flags)
        {
            PSPropertyInfo info = pso.Properties[propertyName];
            if ((info == null) && (RehydrationFlags.MissingPropertyOk == (flags & RehydrationFlags.MissingPropertyOk)))
            {
                return default(T);
            }
            object valueToConvert = info.Value;
            if ((valueToConvert == null) && (RehydrationFlags.NullValueOk == (flags & RehydrationFlags.NullValueOk)))
            {
                return default(T);
            }
            return (T) LanguagePrimitives.ConvertTo(valueToConvert, typeof(T), CultureInfo.InvariantCulture);
        }

        private static CommandCompletion RehydrateCommandCompletion(PSObject pso)
        {
            Collection<CompletionResult> matches = new Collection<CompletionResult>();
            foreach (object obj2 in GetPropertyValue<ArrayList>(pso, "CompletionMatches"))
            {
                matches.Add((CompletionResult) obj2);
            }
            int propertyValue = GetPropertyValue<int>(pso, "CurrentMatchIndex");
            int replacementIndex = GetPropertyValue<int>(pso, "ReplacementIndex");
            return new CommandCompletion(matches, propertyValue, replacementIndex, GetPropertyValue<int>(pso, "ReplacementLength"));
        }

        private static CompletionResult RehydrateCompletionResult(PSObject pso)
        {
            string propertyValue = GetPropertyValue<string>(pso, "CompletionText");
            string listItemText = GetPropertyValue<string>(pso, "ListItemText");
            string toolTip = GetPropertyValue<string>(pso, "ToolTip");
            return new CompletionResult(propertyValue, listItemText, GetPropertyValue<CompletionResultType>(pso, "ResultType"), toolTip);
        }

        private static CultureInfo RehydrateCultureInfo(PSObject pso)
        {
            return new CultureInfo(pso.ToString());
        }

        private static DisplayEntry RehydrateDisplayEntry(PSObject deserializedDisplayEntry)
        {
            return new DisplayEntry { Value = GetPropertyValue<string>(deserializedDisplayEntry, "Value"), ValueType = GetPropertyValue<DisplayEntryValueType>(deserializedDisplayEntry, "ValueType") };
        }

        private static ExtendedTypeDefinition RehydrateExtendedTypeDefinition(PSObject deserializedTypeDefinition)
        {
            string propertyValue = GetPropertyValue<string>(deserializedTypeDefinition, "TypeName");
            return new ExtendedTypeDefinition(propertyValue, RehydrateList<List<FormatViewDefinition>, FormatViewDefinition>(deserializedTypeDefinition, "FormatViewDefinition", RehydrationFlags.NullValueBad));
        }

        private static FormatViewDefinition RehydrateFormatViewDefinition(PSObject deserializedViewDefinition)
        {
            string propertyValue = GetPropertyValue<string>(deserializedViewDefinition, "Name");
            Guid instanceid = GetPropertyValue<Guid>(deserializedViewDefinition, "InstanceId");
            return new FormatViewDefinition(propertyValue, GetPropertyValue<PSControl>(deserializedViewDefinition, "Control"), instanceid);
        }

        private static IPAddress RehydrateIPAddress(PSObject pso)
        {
            return IPAddress.Parse(pso.ToString());
        }

        internal static JobStateEventArgs RehydrateJobStateEventArgs(PSObject pso)
        {
            JobStateInfo jobStateInfo = RehydrateJobStateInfo(PSObject.AsPSObject(pso.Properties["JobStateInfo"].Value));
            JobStateInfo previousJobStateInfo = null;
            PSPropertyInfo info3 = pso.Properties["PreviousJobStateInfo"];
            if ((info3 != null) && (info3.Value != null))
            {
                previousJobStateInfo = RehydrateJobStateInfo(PSObject.AsPSObject(info3.Value));
            }
            return new JobStateEventArgs(jobStateInfo, previousJobStateInfo);
        }

        private static JobStateInfo RehydrateJobStateInfo(PSObject pso)
        {
            JobState propertyValue = GetPropertyValue<JobState>(pso, "State");
            Exception reason = null;
            object o = null;
            PSPropertyInfo info = pso.Properties["Reason"];
            string str = string.Empty;
            if (info != null)
            {
                o = info.Value;
            }
            if (o != null)
            {
                if (Deserializer.IsDeserializedInstanceOfType(o, typeof(Exception)))
                {
                    str = PSObject.AsPSObject(o).Properties["Message"].Value as string;
                }
                else if (o is Exception)
                {
                    reason = (Exception) o;
                }
                else
                {
                    str = o.ToString();
                }
                if (!string.IsNullOrEmpty(str))
                {
                    try
                    {
                        reason = (Exception) LanguagePrimitives.ConvertTo(str, typeof(Exception), CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                        reason = null;
                    }
                }
            }
            return new JobStateInfo(propertyValue, reason);
        }

        private static ListType RehydrateList<ListType, ItemType>(PSObject pso, string propertyName, RehydrationFlags flags) where ListType: IList, new()
        {
            ArrayList list = GetPropertyValue<ArrayList>(pso, propertyName, flags);
            if (list == null)
            {
                if (RehydrationFlags.NullValueMeansEmptyList != (flags & RehydrationFlags.NullValueMeansEmptyList))
                {
                    return default(ListType);
                }
                if (default(ListType) != null)
                {
                    return default(ListType);
                }
                return Activator.CreateInstance<ListType>();
            }
            ListType local = (default(ListType) == null) ? Activator.CreateInstance<ListType>() : default(ListType);
            foreach (object obj2 in list)
            {
                ItemType local2 = (ItemType) LanguagePrimitives.ConvertTo(obj2, typeof(ItemType), CultureInfo.InvariantCulture);
                local.Add(local2);
            }
            return local;
        }

        private static ListControlEntry RehydrateListControlEntry(PSObject deserializedEntry)
        {
            return new ListControlEntry { Items = RehydrateList<List<ListControlEntryItem>, ListControlEntryItem>(deserializedEntry, "Items", RehydrationFlags.NullValueBad), SelectedBy = RehydrateList<List<string>, string>(deserializedEntry, "SelectedBy", RehydrationFlags.NullValueOk) };
        }

        private static ListControlEntryItem RehydrateListControlEntryItem(PSObject deserializedEntryItem)
        {
            return new ListControlEntryItem { DisplayEntry = GetPropertyValue<DisplayEntry>(deserializedEntryItem, "DisplayEntry"), Label = GetPropertyValue<string>(deserializedEntryItem, "Label", RehydrationFlags.NullValueOk) };
        }

        private static MailAddress RehydrateMailAddress(PSObject pso)
        {
            return new MailAddress(pso.ToString());
        }

        private static T RehydrateObjectSecurity<T>(PSObject pso) where T: ObjectSecurity, new()
        {
            string propertyValue = GetPropertyValue<string>(pso, "SDDL");
            T local = Activator.CreateInstance<T>();
            local.SetSecurityDescriptorSddlForm(propertyValue);
            return local;
        }

        private static ParameterSetMetadata RehydrateParameterSetMetadata(PSObject pso)
        {
            int propertyValue = GetPropertyValue<int>(pso, "Position");
            int num2 = GetPropertyValue<int>(pso, "Flags");
            return new ParameterSetMetadata(propertyValue, (ParameterSetMetadata.ParameterFlags) num2, GetPropertyValue<string>(pso, "HelpMessage"));
        }

        private static object RehydratePrimitiveHashtable(PSObject pso)
        {
            return new PSPrimitiveDictionary((Hashtable) LanguagePrimitives.ConvertTo(pso, typeof(Hashtable), CultureInfo.InvariantCulture));
        }

        private static PSControl RehydratePSControl(PSObject deserializedControl)
        {
            if (Deserializer.IsDeserializedInstanceOfType(deserializedControl, typeof(TableControl)))
            {
                return new TableControl { Headers = RehydrateList<List<TableControlColumnHeader>, TableControlColumnHeader>(deserializedControl, "Headers", RehydrationFlags.NullValueBad), Rows = RehydrateList<List<TableControlRow>, TableControlRow>(deserializedControl, "Rows", RehydrationFlags.NullValueBad) };
            }
            if (Deserializer.IsDeserializedInstanceOfType(deserializedControl, typeof(ListControl)))
            {
                return new ListControl { Entries = RehydrateList<List<ListControlEntry>, ListControlEntry>(deserializedControl, "Entries", RehydrationFlags.NullValueBad) };
            }
            if (!Deserializer.IsDeserializedInstanceOfType(deserializedControl, typeof(WideControl)))
            {
                throw PSTraceSource.NewArgumentException("pso");
            }
            return new WideControl { Alignment = GetPropertyValue<Alignment>(deserializedControl, "Alignment"), Columns = GetPropertyValue<int>(deserializedControl, "Columns"), Entries = RehydrateList<List<WideControlEntryItem>, WideControlEntryItem>(deserializedControl, "Entries", RehydrationFlags.NullValueBad) };
        }

        private static PSCredential RehydratePSCredential(PSObject pso)
        {
            string propertyValue = GetPropertyValue<string>(pso, "UserName");
            return new PSCredential(propertyValue, GetPropertyValue<SecureString>(pso, "Password"));
        }

        private static PSListModifier RehydratePSListModifier(PSObject pso)
        {
            Hashtable hash = new Hashtable();
            PSPropertyInfo info = pso.Properties["Add"];
            if ((info != null) && (info.Value != null))
            {
                hash.Add("Add", info.Value);
            }
            PSPropertyInfo info2 = pso.Properties["Remove"];
            if ((info2 != null) && (info2.Value != null))
            {
                hash.Add("Remove", info2.Value);
            }
            PSPropertyInfo info3 = pso.Properties["Replace"];
            if ((info3 != null) && (info3.Value != null))
            {
                hash.Add("Replace", info3.Value);
            }
            return new PSListModifier(hash);
        }

        internal static PSSenderInfo RehydratePSSenderInfo(PSObject pso)
        {
            PSObject propertyValue = GetPropertyValue<PSObject>(GetPropertyValue<PSObject>(pso, "UserInfo"), "Identity");
            PSObject obj4 = GetPropertyValue<PSObject>(propertyValue, "CertificateDetails");
            PSCertificateDetails cert = (obj4 == null) ? null : new PSCertificateDetails(GetPropertyValue<string>(obj4, "Subject"), GetPropertyValue<string>(obj4, "IssuerName"), GetPropertyValue<string>(obj4, "IssuerThumbprint"));
            PSIdentity identity = new PSIdentity(GetPropertyValue<string>(propertyValue, "AuthenticationType"), GetPropertyValue<bool>(propertyValue, "IsAuthenticated"), GetPropertyValue<string>(propertyValue, "Name"), cert);
            return new PSSenderInfo(new PSPrincipal(identity, WindowsIdentity.GetCurrent()), GetPropertyValue<string>(pso, "ConnectionString")) { ClientTimeZone = TimeZone.CurrentTimeZone, ApplicationArguments = GetPropertyValue<PSPrimitiveDictionary>(pso, "ApplicationArguments") };
        }

        internal static PSSessionOption RehydratePSSessionOption(PSObject pso)
        {
            return new PSSessionOption { 
                ApplicationArguments = GetPropertyValue<PSPrimitiveDictionary>(pso, "ApplicationArguments"), CancelTimeout = GetPropertyValue<TimeSpan>(pso, "CancelTimeout"), Culture = GetPropertyValue<CultureInfo>(pso, "Culture"), IdleTimeout = GetPropertyValue<TimeSpan>(pso, "IdleTimeout"), MaximumConnectionRedirectionCount = GetPropertyValue<int>(pso, "MaximumConnectionRedirectionCount"), MaximumReceivedDataSizePerCommand = GetPropertyValue<int?>(pso, "MaximumReceivedDataSizePerCommand"), MaximumReceivedObjectSize = GetPropertyValue<int?>(pso, "MaximumReceivedObjectSize"), NoCompression = GetPropertyValue<bool>(pso, "NoCompression"), NoEncryption = GetPropertyValue<bool>(pso, "NoEncryption"), NoMachineProfile = GetPropertyValue<bool>(pso, "NoMachineProfile"), OpenTimeout = GetPropertyValue<TimeSpan>(pso, "OpenTimeout"), OperationTimeout = GetPropertyValue<TimeSpan>(pso, "OperationTimeout"), OutputBufferingMode = GetPropertyValue<OutputBufferingMode>(pso, "OutputBufferingMode"), ProxyAccessType = GetPropertyValue<ProxyAccessType>(pso, "ProxyAccessType"), ProxyAuthentication = GetPropertyValue<AuthenticationMechanism>(pso, "ProxyAuthentication"), ProxyCredential = GetPropertyValue<PSCredential>(pso, "ProxyCredential"), 
                SkipCACheck = GetPropertyValue<bool>(pso, "SkipCACheck"), SkipCNCheck = GetPropertyValue<bool>(pso, "SkipCNCheck"), SkipRevocationCheck = GetPropertyValue<bool>(pso, "SkipRevocationCheck"), UICulture = GetPropertyValue<CultureInfo>(pso, "UICulture"), UseUTF16 = GetPropertyValue<bool>(pso, "UseUTF16"), IncludePortInSPN = GetPropertyValue<bool>(pso, "IncludePortInSPN")
             };
        }

        private static object RehydrateSwitchParameter(PSObject pso)
        {
            return GetPropertyValue<SwitchParameter>(pso, "IsPresent");
        }

        private static TableControlColumn RehydrateTableControlColumn(PSObject deserializedColumn)
        {
            return new TableControlColumn { Alignment = GetPropertyValue<Alignment>(deserializedColumn, "Alignment"), DisplayEntry = GetPropertyValue<DisplayEntry>(deserializedColumn, "DisplayEntry") };
        }

        private static TableControlColumnHeader RehydrateTableControlColumnHeader(PSObject deserializedHeader)
        {
            return new TableControlColumnHeader { Alignment = GetPropertyValue<Alignment>(deserializedHeader, "Alignment"), Label = GetPropertyValue<string>(deserializedHeader, "Label", RehydrationFlags.NullValueOk), Width = GetPropertyValue<int>(deserializedHeader, "Width") };
        }

        private static TableControlRow RehydrateTableControlRow(PSObject deserializedRow)
        {
            return new TableControlRow { Columns = RehydrateList<List<TableControlColumn>, TableControlColumn>(deserializedRow, "Columns", RehydrationFlags.NullValueBad) };
        }

        private static WideControlEntryItem RehydrateWideControlEntryItem(PSObject deserializedEntryItem)
        {
            return new WideControlEntryItem { DisplayEntry = GetPropertyValue<DisplayEntry>(deserializedEntryItem, "DisplayEntry"), SelectedBy = RehydrateList<List<string>, string>(deserializedEntryItem, "SelectedBy", RehydrationFlags.NullValueOk) };
        }

        private static X500DistinguishedName RehydrateX500DistinguishedName(PSObject pso)
        {
            return new X500DistinguishedName(GetPropertyValue<byte[]>(pso, "RawData"));
        }

        private static X509Certificate2 RehydrateX509Certificate2(PSObject pso)
        {
            return new X509Certificate2(GetPropertyValue<byte[]>(pso, "RawData"));
        }

        [Flags]
        internal enum RehydrationFlags
        {
            MissingPropertyBad = 0,
            MissingPropertyOk = 4,
            NullValueBad = 0,
            NullValueMeansEmptyList = 3,
            NullValueOk = 1
        }
    }
}

