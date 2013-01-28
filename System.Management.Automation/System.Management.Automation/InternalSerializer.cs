namespace System.Management.Automation
{
    using Microsoft.Management.Infrastructure;
    using Microsoft.Management.Infrastructure.Serialization;
    using Microsoft.PowerShell;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Tracing;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Threading;
    using System.Xml;

    internal class InternalSerializer
    {
        private readonly SerializationContext _context;
        private System.Management.Automation.Runspaces.TypeTable _typeTable;
        private readonly XmlWriter _writer;
        private Collection<CollectionEntry<PSPropertyInfo>> allPropertiesCollection;
        private bool? canUseDefaultRunspaceInThreadSafeManner;
        private static Lazy<CimSerializer> cimSerializer = new Lazy<CimSerializer>(new Func<CimSerializer>(CimSerializer.Create));
        internal const string DefaultVersion = "1.1.0.1";
        private int depthBelowTopLevel;
        private Collection<CollectionEntry<PSMemberInfo>> extendedMembersCollection;
        private static readonly char[] hexlookup = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        private bool isStopping;
        private const int MaxDepthBelowTopLevel = 50;
        private readonly ReferenceIdHandlerForSerializer<object> objectRefIdHandler;
        private readonly ReferenceIdHandlerForSerializer<ConsolidatedString> typeRefIdHandler;

        internal InternalSerializer(XmlWriter writer, SerializationContext context)
        {
            this._writer = writer;
            this._context = context;
            IDictionary<object, ulong> dictionary = null;
            if ((this._context.options & SerializationOptions.NoObjectRefIds) == SerializationOptions.None)
            {
                dictionary = new WeakReferenceDictionary<ulong>();
            }
            this.objectRefIdHandler = new ReferenceIdHandlerForSerializer<object>(dictionary);
            this.typeRefIdHandler = new ReferenceIdHandlerForSerializer<ConsolidatedString>(new Dictionary<ConsolidatedString, ulong>(new ConsolidatedStringEqualityComparer()));
        }

        private void CheckIfStopping()
        {
            if (this.isStopping)
            {
                throw PSTraceSource.NewInvalidOperationException("Serialization", "Stopping", new object[0]);
            }
        }

        private static bool DerivesFromGenericType(Type derived, Type baseType)
        {
            while (derived != null)
            {
                if (derived.IsGenericType)
                {
                    derived = derived.GetGenericTypeDefinition();
                }
                if (derived == baseType)
                {
                    return true;
                }
                derived = derived.BaseType;
            }
            return false;
        }

        private static string EncodeString(string s)
        {
            int length = s.Length;
            for (int i = 0; i < length; i++)
            {
                char ch = s[i];
                if ((((ch <= '\x001f') || ((ch >= '\x007f') && (ch <= '\x009f'))) || ((ch >= 0xd800) && (ch <= 0xdfff))) || (((ch == '_') && ((i + 1) < length)) && ((s[i + 1] == 'x') || (s[i + 1] == 'X'))))
                {
                    return EncodeString(s, i);
                }
            }
            return s;
        }

        private static string EncodeString(string s, int indexOfFirstEncodableCharacter)
        {
            int length = s.Length;
            char[] chrArray = new char[indexOfFirstEncodableCharacter + (length - indexOfFirstEncodableCharacter) * 7];
            s.CopyTo(0, chrArray, 0, indexOfFirstEncodableCharacter);
            int num = indexOfFirstEncodableCharacter;
            for (int i = indexOfFirstEncodableCharacter; i < length; i++)
            {
                char chr = s[i];
                if (chr <= '\u001F' || chr >= '\u007F' && chr <= '\u009F' || chr >= '\uD800' && chr <= '\uDFFF' || chr == '\u005F' && i + 1 < length && (s[i + 1] == 'x' || s[i + 1] == 'X'))
                {
                    if (chr != '\u005F')
                    {
                        chrArray[num] = '\u005F';
                        chrArray[num + 1] = 'x';
                        chrArray[num + 2 + 3] = InternalSerializer.hexlookup[chr & '\u000F'];
                        chr = (char)((ushort)(chr >> '\u0004'));
                        chrArray[num + 2 + 2] = InternalSerializer.hexlookup[chr & '\u000F'];
                        chr = (char)((ushort)(chr >> '\u0004'));
                        chrArray[num + 2 + 1] = InternalSerializer.hexlookup[chr & '\u000F'];
                        chr = (char)((ushort)(chr >> '\u0004'));
                        chrArray[num + 2] = InternalSerializer.hexlookup[chr & '\u000F'];
                        chrArray[num + 6] = '\u005F';
                        num = num + 7;
                    }
                    else
                    {
                        chrArray[num] = '\u005F';
                        chrArray[num + 1] = 'x';
                        chrArray[num + 2] = '0';
                        chrArray[num + 3] = '0';
                        chrArray[num + 4] = '5';
                        chrArray[num + 5] = 'F';
                        chrArray[num + 6] = '\u005F';
                        num = num + 7;
                    }
                }
                else
                {
                    int num1 = num;
                    num = num1 + 1;
                    chrArray[num1] = chr;
                }
            }
            return new string(chrArray, 0, num);
        }

        internal void End()
        {
            if (SerializationOptions.NoRootElement != (this._context.options & SerializationOptions.NoRootElement))
            {
                this._writer.WriteEndElement();
            }
            this._writer.Flush();
        }

        private int GetDepthOfSerialization(object source, int depth)
        {
            PSObject obj2 = PSObject.AsPSObject(source);
            if (obj2 != null)
            {
                if (obj2.BaseObject is CimInstance)
                {
                    return 1;
                }
                if (obj2.BaseObject is PSCredential)
                {
                    return 1;
                }
                if (obj2.BaseObject is PSSenderInfo)
                {
                    return 4;
                }
                if (obj2.BaseObject is SwitchParameter)
                {
                    return 1;
                }
                if ((this._context.options & SerializationOptions.UseDepthFromTypes) != SerializationOptions.None)
                {
                    int serializationDepth = obj2.GetSerializationDepth(this._typeTable);
                    if ((serializationDepth > 0) && (serializationDepth != depth))
                    {
                        PSEtwLog.LogAnalyticVerbose(PSEventId.Serializer_DepthOverride, PSOpcode.SerializationSettings, PSTask.Serialization, PSKeyword.Serializer | PSKeyword.UseAlwaysAnalytic, new object[] { obj2.InternalTypeNames.Key, depth, serializationDepth, this.depthBelowTopLevel });
                        return serializationDepth;
                    }
                }
                if ((((this._context.options & SerializationOptions.PreserveSerializationSettingOfOriginal) != SerializationOptions.None) && obj2.isDeserialized) && (depth <= 0))
                {
                    return 1;
                }
            }
            return depth;
        }

        private static void GetKnownContainerTypeInfo(object source, out ContainerType ct, out IDictionary dictionary, out IEnumerable enumerable)
        {
            ct = ContainerType.None;
            dictionary = null;
            enumerable = null;
            dictionary = source as IDictionary;
            if (dictionary != null)
            {
                ct = ContainerType.Dictionary;
            }
            else
            {
                if (source is Stack)
                {
                    ct = ContainerType.Stack;
                    enumerable = LanguagePrimitives.GetEnumerable(source);
                }
                else if (source is Queue)
                {
                    ct = ContainerType.Queue;
                    enumerable = LanguagePrimitives.GetEnumerable(source);
                }
                else if (source is IList)
                {
                    ct = ContainerType.List;
                    enumerable = LanguagePrimitives.GetEnumerable(source);
                }
                else
                {
                    Type derived = source.GetType();
                    if (derived.IsGenericType)
                    {
                        if (DerivesFromGenericType(derived, typeof(Stack<>)))
                        {
                            ct = ContainerType.Stack;
                            enumerable = LanguagePrimitives.GetEnumerable(source);
                        }
                        else if (DerivesFromGenericType(derived, typeof(Queue<>)))
                        {
                            ct = ContainerType.Queue;
                            enumerable = LanguagePrimitives.GetEnumerable(source);
                        }
                        else if (DerivesFromGenericType(derived, typeof(List<>)))
                        {
                            ct = ContainerType.List;
                            enumerable = LanguagePrimitives.GetEnumerable(source);
                        }
                    }
                }
                if (ct == ContainerType.None)
                {
                    try
                    {
                        enumerable = LanguagePrimitives.GetEnumerable(source);
                        if (enumerable != null)
                        {
                            ct = ContainerType.Enumerable;
                        }
                    }
                    catch (Exception exception)
                    {
                        CommandProcessorBase.CheckForSevereException(exception);
                        PSEtwLog.LogAnalyticWarning(PSEventId.Serializer_EnumerationFailed, PSOpcode.Exception, PSTask.Serialization, PSKeyword.Serializer, new object[] { source.GetType().AssemblyQualifiedName, exception.ToString() });
                    }
                }
                if (ct == ContainerType.None)
                {
                    enumerable = source as IEnumerable;
                    if (enumerable != null)
                    {
                        ct = ContainerType.Enumerable;
                    }
                }
            }
        }

        private object GetPropertyValueInThreadSafeManner(PSPropertyInfo property, out bool success)
        {
            if (!property.IsGettable)
            {
                success = false;
                return null;
            }
            PSAliasProperty property2 = property as PSAliasProperty;
            if (property2 != null)
            {
                property = property2.ReferencedMember as PSPropertyInfo;
            }
            PSScriptProperty property3 = property as PSScriptProperty;
            if ((property3 != null) && !this.CanUseDefaultRunspaceInThreadSafeManner)
            {
                PSEtwLog.LogAnalyticWarning(PSEventId.Serializer_ScriptPropertyWithoutRunspace, PSOpcode.Exception, PSTask.Serialization, PSKeyword.Serializer | PSKeyword.UseAlwaysAnalytic, new object[] { property.Name, (property.instance == null) ? string.Empty : PSObject.GetTypeNames(property.instance).Key, property3.GetterScript.ToString() });
                success = false;
                return null;
            }
            try
            {
                object obj2 = property.Value;
                success = true;
                return obj2;
            }
            catch (ExtendedTypeSystemException exception)
            {
                PSEtwLog.LogAnalyticWarning(PSEventId.Serializer_PropertyGetterFailed, PSOpcode.Exception, PSTask.Serialization, PSKeyword.Serializer | PSKeyword.UseAlwaysAnalytic, new object[] { property.Name, (property.instance == null) ? string.Empty : PSObject.GetTypeNames(property.instance).Key, exception.ToString(), (exception.InnerException == null) ? string.Empty : exception.InnerException.ToString() });
                success = false;
                return null;
            }
        }

        private string GetSerializationString(PSObject source)
        {
            PSPropertyInfo property = null;
            try
            {
                property = source.GetStringSerializationSource(this._typeTable);
            }
            catch (ExtendedTypeSystemException exception)
            {
                PSEtwLog.LogAnalyticWarning(PSEventId.Serializer_ToStringFailed, PSOpcode.Exception, PSTask.Serialization, PSKeyword.Serializer | PSKeyword.UseAlwaysAnalytic, new object[] { source.GetType().AssemblyQualifiedName, (exception.InnerException != null) ? exception.InnerException.ToString() : exception.ToString() });
            }
            string toString = null;
            if (property != null)
            {
                bool flag;
                object propertyValueInThreadSafeManner = this.GetPropertyValueInThreadSafeManner(property, out flag);
                if (flag && (propertyValueInThreadSafeManner != null))
                {
                    toString = GetToString(propertyValueInThreadSafeManner);
                }
                return toString;
            }
            return GetToString(source);
        }

        private PSMemberInfoInternalCollection<PSPropertyInfo> GetSpecificPropertiesToSerialize(PSObject source)
        {
            if (source == null)
            {
                return null;
            }
            if (source.GetSerializationMethod(this._typeTable) != SerializationMethod.SpecificProperties)
            {
                return null;
            }
            PSEtwLog.LogAnalyticVerbose(PSEventId.Serializer_ModeOverride, PSOpcode.SerializationSettings, PSTask.Serialization, PSKeyword.Serializer | PSKeyword.UseAlwaysAnalytic, new object[] { source.InternalTypeNames.Key, 2 });
            PSMemberInfoInternalCollection<PSPropertyInfo> internals = new PSMemberInfoInternalCollection<PSPropertyInfo>();
            PSMemberInfoIntegratingCollection<PSPropertyInfo> integratings = new PSMemberInfoIntegratingCollection<PSPropertyInfo>(source, this.AllPropertiesCollection);
            foreach (string str in source.GetSpecificPropertiesToSerialize(this._typeTable))
            {
                PSPropertyInfo member = integratings[str];
                if (member == null)
                {
                    PSEtwLog.LogAnalyticWarning(PSEventId.Serializer_SpecificPropertyMissing, PSOpcode.Exception, PSTask.Serialization, PSKeyword.Serializer | PSKeyword.UseAlwaysAnalytic, new object[] { source.InternalTypeNames.Key, str });
                }
                else
                {
                    internals.Add(member);
                }
            }
            return internals;
        }

        private static string GetToString(object source)
        {
            string str = null;
            try
            {
                str = Convert.ToString(source, CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                PSEtwLog.LogAnalyticWarning(PSEventId.Serializer_ToStringFailed, PSOpcode.Exception, PSTask.Serialization, PSKeyword.Serializer | PSKeyword.UseAlwaysAnalytic, new object[] { source.GetType().AssemblyQualifiedName, exception.ToString() });
            }
            return str;
        }

        private string GetToStringForPrimitiveObject(PSObject pso)
        {
            if (pso != null)
            {
                if (pso.ToStringFromDeserialization != null)
                {
                    return pso.ToStringFromDeserialization;
                }
                string tokenText = pso.TokenText;
                if (tokenText != null)
                {
                    string toString = GetToString(pso.BaseObject);
                    if ((toString == null) || !string.Equals(tokenText, toString, StringComparison.Ordinal))
                    {
                        return tokenText;
                    }
                }
            }
            return null;
        }

        private void HandleComplexTypePSObject(object source, string streamName, string property, int depth)
        {
            PSObject dest = PSObject.AsPSObject(source);
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            bool flag5 = false;
            if (!dest.immediateBaseObjectIsEmpty)
            {
                if (dest.ImmediateBaseObject is CimInstance)
                {
                    flag5 = true;
                }
                else
                {
                    ErrorRecord immediateBaseObject = dest.ImmediateBaseObject as ErrorRecord;
                    if (immediateBaseObject != null)
                    {
                        immediateBaseObject.ToPSObjectForRemoting(dest);
                        flag = true;
                    }
                    else
                    {
                        InformationalRecord record2 = dest.ImmediateBaseObject as InformationalRecord;
                        if (record2 != null)
                        {
                            record2.ToPSObjectForRemoting(dest);
                            flag2 = true;
                        }
                        else
                        {
                            flag3 = dest.ImmediateBaseObject is Enum;
                            flag4 = dest.ImmediateBaseObject is PSObject;
                        }
                    }
                }
            }
            bool flag6 = true;
            if ((dest.ToStringFromDeserialization == null) && dest.immediateBaseObjectIsEmpty)
            {
                flag6 = false;
            }
            string refId = this.objectRefIdHandler.SetRefId(source);
            this.WriteStartOfPSObject(dest, streamName, property, refId, true, flag6 ? GetToString(dest) : null);
            PSMemberInfoInternalCollection<PSPropertyInfo> specificPropertiesToSerialize = this.GetSpecificPropertiesToSerialize(dest);
            if (flag3)
            {
                object obj3 = dest.ImmediateBaseObject;
                this.WriteOneObject(Convert.ChangeType(obj3, Enum.GetUnderlyingType(obj3.GetType()), CultureInfo.InvariantCulture), null, null, depth);
            }
            else if (flag4)
            {
                this.WriteOneObject(dest.ImmediateBaseObject, null, null, depth);
            }
            else if (!flag && !flag2)
            {
                this.WritePSObjectProperties(dest, depth, specificPropertiesToSerialize);
            }
            if (flag5)
            {
                CimInstance cimInstance = dest.ImmediateBaseObject as CimInstance;
                this.PrepareCimInstanceForSerialization(dest, cimInstance);
            }
            this.SerializeExtendedProperties(dest, depth, specificPropertiesToSerialize);
            this._writer.WriteEndElement();
        }

        private bool HandleKnownContainerTypes(object source, string streamName, string property, int depth)
        {
            ContainerType none = ContainerType.None;
            PSObject obj2 = source as PSObject;
            IEnumerable enumerable = null;
            IDictionary dictionary = null;
            if ((obj2 != null) && obj2.immediateBaseObjectIsEmpty)
            {
                return false;
            }
            GetKnownContainerTypeInfo((obj2 != null) ? obj2.ImmediateBaseObject : source, out none, out dictionary, out enumerable);
            if (none == ContainerType.None)
            {
                return false;
            }
            string refId = this.objectRefIdHandler.SetRefId(source);
            this.WriteStartOfPSObject(obj2 ?? PSObject.AsPSObject(source), streamName, property, refId, true, null);
            switch (none)
            {
                case ContainerType.Dictionary:
                    this.WriteDictionary(dictionary, "DCT", depth);
                    break;

                case ContainerType.Queue:
                    this.WriteEnumerable(enumerable, "QUE", depth);
                    break;

                case ContainerType.Stack:
                    this.WriteEnumerable(enumerable, "STK", depth);
                    break;

                case ContainerType.List:
                    this.WriteEnumerable(enumerable, "LST", depth);
                    break;

                case ContainerType.Enumerable:
                    this.WriteEnumerable(enumerable, "IE", depth);
                    break;
            }
            if (depth != 0)
            {
                if ((none == ContainerType.Enumerable) || ((obj2 != null) && obj2.isDeserialized))
                {
                    PSObject obj3 = PSObject.AsPSObject(source);
                    PSMemberInfoInternalCollection<PSPropertyInfo> specificPropertiesToSerialize = this.GetSpecificPropertiesToSerialize(obj3);
                    this.WritePSObjectProperties(obj3, depth, specificPropertiesToSerialize);
                    this.SerializeExtendedProperties(obj3, depth, specificPropertiesToSerialize);
                }
                else if (obj2 != null)
                {
                    this.SerializeInstanceProperties(obj2, depth);
                }
            }
            this._writer.WriteEndElement();
            return true;
        }

        private bool HandleMaxDepth(object source, string streamName, string property)
        {
            if (this.depthBelowTopLevel == MaxDepthBelowTopLevel)
            {
                PSEtwLog.LogAnalyticError(PSEventId.Serializer_MaxDepthWhenSerializing, PSOpcode.Exception, PSTask.Serialization, PSKeyword.Serializer, new object[] { source.GetType().AssemblyQualifiedName, property, this.depthBelowTopLevel });
                string deserializationTooDeep = Serialization.DeserializationTooDeep;
                this.HandlePrimitiveKnownType(deserializationTooDeep, streamName, property);
                return true;
            }
            return false;
        }

        private bool HandlePrimitiveKnownType(object source, string streamName, string property)
        {
            TypeSerializationInfo typeSerializationInfo = KnownTypes.GetTypeSerializationInfo(source.GetType());
            if (typeSerializationInfo != null)
            {
                WriteOnePrimitiveKnownType(this, streamName, property, source, typeSerializationInfo);
                return true;
            }
            return false;
        }

        private bool HandlePrimitiveKnownTypeByConvertingToPSObject(object source, string streamName, string property, int depth)
        {
            if (KnownTypes.GetTypeSerializationInfo(source.GetType()) != null)
            {
                PSObject obj2 = PSObject.AsPSObject(source);
                return this.HandlePrimitiveKnownTypePSObject(obj2, streamName, property, depth);
            }
            return false;
        }

        private bool HandlePrimitiveKnownTypePSObject(object source, string streamName, string property, int depth)
        {
            bool flag = false;
            PSObject obj2 = source as PSObject;
            if ((obj2 != null) && !obj2.immediateBaseObjectIsEmpty)
            {
                object immediateBaseObject = obj2.ImmediateBaseObject;
                TypeSerializationInfo typeSerializationInfo = KnownTypes.GetTypeSerializationInfo(immediateBaseObject.GetType());
                if (typeSerializationInfo != null)
                {
                    this.WritePrimitiveTypePSObject(obj2, immediateBaseObject, typeSerializationInfo, streamName, property, depth);
                    flag = true;
                }
            }
            return flag;
        }

        private void HandlePSObjectAsString(PSObject source, string streamName, string property, int depth)
        {
            string serializationString = this.GetSerializationString(source);
            TypeSerializationInfo pktInfo = null;
            if (serializationString != null)
            {
                pktInfo = KnownTypes.GetTypeSerializationInfo(serializationString.GetType());
            }
            this.WritePrimitiveTypePSObject(source, serializationString, pktInfo, streamName, property, depth);
        }

        private bool HandleSecureString(object source, string streamName, string property)
        {
            SecureString immediateBaseObject = null;
            PSObject obj2;
            immediateBaseObject = source as SecureString;
            if (immediateBaseObject != null)
            {
                obj2 = PSObject.AsPSObject(immediateBaseObject);
            }
            else
            {
                obj2 = source as PSObject;
            }
            if (((obj2 != null) && !obj2.immediateBaseObjectIsEmpty) && (obj2.ImmediateBaseObject.GetType() == typeof(SecureString)))
            {
                immediateBaseObject = obj2.ImmediateBaseObject as SecureString;
                try
                {
                    string str2;
                    if (this._context.cryptoHelper != null)
                    {
                        str2 = this._context.cryptoHelper.EncryptSecureString(immediateBaseObject);
                    }
                    else
                    {
                        str2 = SecureStringHelper.Protect(immediateBaseObject);
                    }
                    if (property != null)
                    {
                        this.WriteStartElement("SS");
                        this.WriteNameAttribute(property);
                    }
                    else
                    {
                        this.WriteStartElement("SS");
                    }
                    if (streamName != null)
                    {
                        this.WriteAttribute("S", streamName);
                    }
                    this._writer.WriteString(str2);
                    this._writer.WriteEndElement();
                    return true;
                }
                catch (PSCryptoException)
                {
                }
            }
            return false;
        }

        internal static bool IsPrimitiveKnownType(Type input)
        {
            return (KnownTypes.GetTypeSerializationInfo(input) != null);
        }

        private void PrepareCimInstanceForSerialization(PSObject psObject, CimInstance cimInstance)
        {
            Queue<CimClassSerializationId> queue = new Queue<CimClassSerializationId>();
            ArrayList list = new ArrayList();
            for (CimClass class2 = cimInstance.CimClass; class2 != null; class2 = class2.CimSuperClass)
            {
                PSObject obj2 = new PSObject();
                obj2.TypeNames.Clear();
                list.Add(obj2);
                obj2.Properties.Add(new PSNoteProperty("ClassName", class2.CimSystemProperties.ClassName));
                obj2.Properties.Add(new PSNoteProperty("Namespace", class2.CimSystemProperties.Namespace));
                obj2.Properties.Add(new PSNoteProperty("ServerName", class2.CimSystemProperties.ServerName));
                obj2.Properties.Add(new PSNoteProperty("Hash", class2.GetHashCode()));
                CimClassSerializationId key = new CimClassSerializationId(class2.CimSystemProperties.ClassName, class2.CimSystemProperties.Namespace, class2.CimSystemProperties.ServerName, class2.GetHashCode());
                if (this._context.cimClassSerializationIdCache.DoesDeserializerAlreadyHaveCimClass(key))
                {
                    break;
                }
                queue.Enqueue(key);
                byte[] bytes = cimSerializer.Value.Serialize(class2, ClassSerializationOptions.None);
                string str = Encoding.Unicode.GetString(bytes);
                obj2.Properties.Add(new PSNoteProperty("MiXml", str));
            }
            list.Reverse();
            foreach (CimClassSerializationId id2 in queue)
            {
                this._context.cimClassSerializationIdCache.AddClassToCache(id2);
            }
            PSPropertyInfo info = psObject.Properties["__ClassMetadata"];
            if (info != null)
            {
                info.Value = list;
            }
            else
            {
                PSNoteProperty member = new PSNoteProperty("__ClassMetadata", list) {
                    isHidden = true
                };
                psObject.Properties.Add(member);
            }
            List<string> values = (from p in cimInstance.CimInstanceProperties
                where p.IsValueModified
                select p.Name).ToList<string>();
            if (values.Count != 0)
            {
                PSObject obj3 = new PSObject();
                PSPropertyInfo info2 = psObject.Properties["__InstanceMetadata"];
                if (info2 != null)
                {
                    info2.Value = obj3;
                }
                else
                {
                    PSNoteProperty property2 = new PSNoteProperty("__InstanceMetadata", obj3) {
                        isHidden = true
                    };
                    psObject.Properties.Add(property2);
                }
                obj3.InternalTypeNames = ConsolidatedString.Empty;
                obj3.Properties.Add(new PSNoteProperty("Modified", string.Join(" ", values)));
            }
        }

        private static bool PSObjectHasModifiedTypesCollection(PSObject pso)
        {
            ConsolidatedString internalTypeNames = pso.InternalTypeNames;
            Collection<string> collection = pso.InternalAdapter.BaseGetTypeNameHierarchy(pso.ImmediateBaseObject);
            if (internalTypeNames.Count != collection.Count)
            {
                return true;
            }
            IEnumerator<string> enumerator = internalTypeNames.GetEnumerator();
            IEnumerator<string> enumerator2 = collection.GetEnumerator();
            while (enumerator.MoveNext() && enumerator2.MoveNext())
            {
                if (!enumerator.Current.Equals(enumerator2.Current, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool PSObjectHasNotes(PSObject source)
        {
            return ((source.InstanceMembers != null) && (source.InstanceMembers.Count > 0));
        }

        private bool SerializeAsString(PSObject source)
        {
            if (source.GetSerializationMethod(this._typeTable) == SerializationMethod.String)
            {
                PSEtwLog.LogAnalyticVerbose(PSEventId.Serializer_ModeOverride, PSOpcode.SerializationSettings, PSTask.Serialization, PSKeyword.Serializer | PSKeyword.UseAlwaysAnalytic, new object[] { source.InternalTypeNames.Key, 1 });
                return true;
            }
            return false;
        }

        private void SerializeExtendedProperties(PSObject source, int depth, IEnumerable<PSPropertyInfo> specificPropertiesToSerialize)
        {
            IEnumerable<PSMemberInfo> me = null;
            if (specificPropertiesToSerialize == null)
            {
                me = new PSMemberInfoIntegratingCollection<PSMemberInfo>(source, this.ExtendedMembersCollection).Match("*", PSMemberTypes.MemberSet | PSMemberTypes.PropertySet | PSMemberTypes.Properties, MshMemberMatchOptions.OnlySerializable | MshMemberMatchOptions.IncludeHidden);
            }
            else
            {
                List<PSMemberInfo> list = new List<PSMemberInfo>(source.InstanceMembers);
                me = list;
                foreach (PSMemberInfo info in specificPropertiesToSerialize)
                {
                    if (!info.IsInstance && !(info is PSProperty))
                    {
                        list.Add(info);
                    }
                }
            }
            if (me != null)
            {
                this.WriteMemberInfoCollection(me, depth, true);
            }
        }

        private void SerializeInstanceProperties(PSObject source, int depth)
        {
            PSMemberInfoCollection<PSMemberInfo> instanceMembers = source.InstanceMembers;
            if (instanceMembers != null)
            {
                this.WriteMemberInfoCollection(instanceMembers, depth, true);
            }
        }

        private void SerializeProperties(IEnumerable<PSPropertyInfo> propertyCollection, string name, int depth)
        {
            bool flag = false;
            foreach (PSMemberInfo info in propertyCollection)
            {
                PSProperty property = info as PSProperty;
                if (property != null)
                {
                    bool flag2;
                    if (!flag)
                    {
                        this.WriteStartElement(name);
                        flag = true;
                    }
                    object propertyValueInThreadSafeManner = this.GetPropertyValueInThreadSafeManner(property, out flag2);
                    if (flag2)
                    {
                        this.WriteOneObject(propertyValueInThreadSafeManner, null, property.Name, depth);
                    }
                }
            }
            if (flag)
            {
                this._writer.WriteEndElement();
            }
        }

        internal void Start()
        {
            if (SerializationOptions.NoRootElement != (this._context.options & SerializationOptions.NoRootElement))
            {
                this.WriteStartElement("Objs");
                this.WriteAttribute("Version", "1.1.0.1");
            }
        }

        internal void Stop()
        {
            this.isStopping = true;
        }

        private void WriteAttribute(string name, string value)
        {
            this._writer.WriteAttributeString(name, value);
        }

        internal static void WriteBoolean(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
        {
            WriteRawString(serializer, streamName, property, XmlConvert.ToString((bool) source), entry);
        }

        internal static void WriteByteArray(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
        {
            byte[] buffer = (byte[]) source;
            if (property != null)
            {
                serializer.WriteStartElement(entry.PropertyTag);
                serializer.WriteNameAttribute(property);
            }
            else
            {
                serializer.WriteStartElement(entry.ItemTag);
            }
            if (streamName != null)
            {
                serializer.WriteAttribute("S", streamName);
            }
            serializer._writer.WriteBase64(buffer, 0, buffer.Length);
            serializer._writer.WriteEndElement();
        }

        internal static void WriteChar(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
        {
            WriteRawString(serializer, streamName, property, XmlConvert.ToString((ushort) ((char) source)), entry);
        }

        internal static void WriteDateTime(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
        {
            WriteRawString(serializer, streamName, property, XmlConvert.ToString((DateTime) source, XmlDateTimeSerializationMode.RoundtripKind), entry);
        }

        private void WriteDictionary(IDictionary dictionary, string tag, int depth)
        {
            object key;
            this.WriteStartElement(tag);
            IDictionaryEnumerator enumerator = null;
            try
            {
                enumerator = dictionary.GetEnumerator();
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                PSEtwLog.LogAnalyticWarning(PSEventId.Serializer_EnumerationFailed, PSOpcode.Exception, PSTask.Serialization, PSKeyword.Serializer | PSKeyword.UseAlwaysAnalytic, new object[] { dictionary.GetType().AssemblyQualifiedName, exception.ToString() });
            }
            if (enumerator == null)
            {
                goto Label_00FA;
            }
        Label_005A:
            key = null;
            object source = null;
            try
            {
                if (!enumerator.MoveNext())
                {
                    goto Label_00FA;
                }
                key = enumerator.Key;
                source = enumerator.Value;
            }
            catch (Exception exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
                PSEtwLog.LogAnalyticWarning(PSEventId.Serializer_EnumerationFailed, PSOpcode.Exception, PSTask.Serialization, PSKeyword.Serializer | PSKeyword.UseAlwaysAnalytic, new object[] { dictionary.GetType().AssemblyQualifiedName, exception2.ToString() });
                goto Label_00FA;
            }
            if (key != null)
            {
                this.WriteStartElement("En");
                this.WriteOneObject(key, null, "Key", depth);
                this.WriteOneObject(source, null, "Value", depth);
                this._writer.WriteEndElement();
                goto Label_005A;
            }
        Label_00FA:
            this._writer.WriteEndElement();
        }

        internal static void WriteDouble(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
        {
            WriteRawString(serializer, streamName, property, XmlConvert.ToString((double) source), entry);
        }

        private void WriteEncodedElementString(string name, string value)
        {
            this.CheckIfStopping();
            value = EncodeString(value);
            if (SerializationOptions.NoNamespace == (this._context.options & SerializationOptions.NoNamespace))
            {
                this._writer.WriteElementString(name, value);
            }
            else
            {
                this._writer.WriteElementString(name, "http://schemas.microsoft.com/powershell/2004/04", value);
            }
        }

        internal static void WriteEncodedString(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
        {
            if (property != null)
            {
                serializer.WriteStartElement(entry.PropertyTag);
                serializer.WriteNameAttribute(property);
            }
            else
            {
                serializer.WriteStartElement(entry.ItemTag);
            }
            if (streamName != null)
            {
                serializer.WriteAttribute("S", streamName);
            }
            string s = (string) source;
            string text = EncodeString(s);
            serializer._writer.WriteString(text);
            serializer._writer.WriteEndElement();
        }

        private void WriteEnumerable(IEnumerable enumerable, string tag, int depth)
        {
            this.WriteStartElement(tag);
            IEnumerator enumerator = null;
            try
            {
				enumerator = enumerable.GetEnumerator();
				enumerator.Reset();
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                PSEtwLog.LogAnalyticWarning(PSEventId.Serializer_EnumerationFailed, PSOpcode.Exception, PSTask.Serialization, PSKeyword.Serializer | PSKeyword.UseAlwaysAnalytic, new object[] { enumerable.GetType().AssemblyQualifiedName, exception.ToString() });
                enumerator = null;
            }
            if (enumerator != null)
            {
                while (true)
                {
                    object source = null;
                    try
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        source = enumerator.Current;
                    }
                    catch (Exception exception2)
                    {
                        CommandProcessorBase.CheckForSevereException(exception2);
                        PSEtwLog.LogAnalyticWarning(PSEventId.Serializer_EnumerationFailed, PSOpcode.Exception, PSTask.Serialization, PSKeyword.Serializer | PSKeyword.UseAlwaysAnalytic, new object[] { enumerable.GetType().AssemblyQualifiedName, exception2.ToString() });
                        break;
                    }
                    this.WriteOneObject(source, null, null, depth);
                }
            }
            this._writer.WriteEndElement();
        }

        private void WriteMemberInfoCollection(IEnumerable<PSMemberInfo> me, int depth, bool writeEnclosingMemberSetElementTag)
        {
            bool flag = false;
            foreach (PSMemberInfo info in me)
            {
                if (info.ShouldSerialize)
                {
                    int num = info.IsInstance ? depth : (depth - 1);
                    if (info.MemberType == (info.MemberType & PSMemberTypes.Properties))
                    {
                        bool flag2;
                        object propertyValueInThreadSafeManner = this.GetPropertyValueInThreadSafeManner((PSPropertyInfo) info, out flag2);
                        if (flag2)
                        {
                            if (writeEnclosingMemberSetElementTag && !flag)
                            {
                                flag = true;
                                this.WriteStartElement("MS");
                            }
                            this.WriteOneObject(propertyValueInThreadSafeManner, null, info.Name, num);
                        }
                    }
                    else if (info.MemberType == PSMemberTypes.MemberSet)
                    {
                        if (writeEnclosingMemberSetElementTag && !flag)
                        {
                            flag = true;
                            this.WriteStartElement("MS");
                        }
                        this.WriteMemberSet((PSMemberSet) info, num);
                    }
                }
            }
            if (flag)
            {
                this._writer.WriteEndElement();
            }
        }

        private void WriteMemberSet(PSMemberSet set, int depth)
        {
            if (set.ShouldSerialize)
            {
                this.WriteStartElement("MS");
                this.WriteNameAttribute(set.Name);
                this.WriteMemberInfoCollection(set.Members, depth, false);
                this._writer.WriteEndElement();
            }
        }

        private void WriteNameAttribute(string value)
        {
            this.WriteAttribute("N", EncodeString(value));
        }

        private void WriteNull(string streamName, string property)
        {
            this.WriteStartElement("Nil");
            if (streamName != null)
            {
                this.WriteAttribute("S", streamName);
            }
            if (property != null)
            {
                this.WriteNameAttribute(property);
            }
            this._writer.WriteEndElement();
        }

        private void WriteOneObject(object source, string streamName, string property, int depth)
        {
            this.CheckIfStopping();
            if (source == null)
            {
                this.WriteNull(streamName, property);
            }
            else
            {
                try
                {
                    this.depthBelowTopLevel++;
                    if (!this.HandleMaxDepth(source, streamName, property))
                    {
                        depth = this.GetDepthOfSerialization(source, depth);
                        if (!this.HandlePrimitiveKnownTypeByConvertingToPSObject(source, streamName, property, depth))
                        {
                            string refId = this.objectRefIdHandler.GetRefId(source);
                            if (refId != null)
                            {
                                this.WritePSObjectReference(streamName, property, refId);
                            }
                            else if (!this.HandlePrimitiveKnownTypePSObject(source, streamName, property, depth) && !this.HandleKnownContainerTypes(source, streamName, property, depth))
                            {
                                PSObject obj2 = PSObject.AsPSObject(source);
                                if ((depth == 0) || this.SerializeAsString(obj2))
                                {
                                    this.HandlePSObjectAsString(obj2, streamName, property, depth);
                                }
                                else
                                {
                                    this.HandleComplexTypePSObject(source, streamName, property, depth);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    this.depthBelowTopLevel--;
                }
            }
        }

        private static void WriteOnePrimitiveKnownType(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
        {
            if (entry.Serializer == null)
            {
                string raw = Convert.ToString(source, CultureInfo.InvariantCulture);
                WriteRawString(serializer, streamName, property, raw, entry);
            }
            else
            {
                entry.Serializer(serializer, streamName, property, source, entry);
            }
        }

        internal void WriteOneTopLevelObject(object source, string streamName)
        {
            this.WriteOneObject(source, streamName, null, this._context.depth);
        }

        private void WritePrimitiveTypePSObject(PSObject source, object primitive, TypeSerializationInfo pktInfo, string streamName, string property, int depth)
        {
            string toStringForPrimitiveObject = this.GetToStringForPrimitiveObject(source);
            bool hasModifiedTypesCollection = PSObjectHasModifiedTypesCollection(source);
            bool flag2 = PSObjectHasNotes(source);
            bool flag3 = toStringForPrimitiveObject != null;
            if ((flag2 || hasModifiedTypesCollection) || flag3)
            {
                this.WritePrimitiveTypePSObjectWithNotes(source, primitive, hasModifiedTypesCollection, toStringForPrimitiveObject, pktInfo, streamName, property, depth);
            }
            else if (primitive != null)
            {
                WriteOnePrimitiveKnownType(this, streamName, property, primitive, pktInfo);
            }
            else
            {
                this.WriteNull(streamName, property);
            }
        }

        private void WritePrimitiveTypePSObjectWithNotes(PSObject source, object primitive, bool hasModifiedTypesCollection, string toStringValue, TypeSerializationInfo pktInfo, string streamName, string property, int depth)
        {
            string refId = this.objectRefIdHandler.SetRefId(source);
            this.WriteStartOfPSObject(source, streamName, property, refId, hasModifiedTypesCollection, toStringValue);
            if (pktInfo != null)
            {
                WriteOnePrimitiveKnownType(this, streamName, null, primitive, pktInfo);
            }
            this.SerializeInstanceProperties(source, depth);
            this._writer.WriteEndElement();
        }

        internal static void WriteProgressRecord(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
        {
            ProgressRecord record = (ProgressRecord) source;
            serializer.WriteStartElement(entry.PropertyTag);
            if (property != null)
            {
                serializer.WriteNameAttribute(property);
            }
            if (streamName != null)
            {
                serializer.WriteAttribute("S", streamName);
            }
            serializer.WriteEncodedElementString("AV", record.Activity);
            serializer.WriteEncodedElementString("AI", record.ActivityId.ToString(CultureInfo.InvariantCulture));
            serializer.WriteOneObject(record.CurrentOperation, null, null, 1);
            serializer.WriteEncodedElementString("PI", record.ParentActivityId.ToString(CultureInfo.InvariantCulture));
            serializer.WriteEncodedElementString("PC", record.PercentComplete.ToString(CultureInfo.InvariantCulture));
            serializer.WriteEncodedElementString("T", record.RecordType.ToString());
            serializer.WriteEncodedElementString("SR", record.SecondsRemaining.ToString(CultureInfo.InvariantCulture));
            serializer.WriteEncodedElementString("SD", record.StatusDescription);
            serializer._writer.WriteEndElement();
        }

        private void WritePSObjectProperties(PSObject source, int depth, IEnumerable<PSPropertyInfo> specificPropertiesToSerialize)
        {
            depth--;
            if (specificPropertiesToSerialize != null)
            {
                this.SerializeProperties(specificPropertiesToSerialize, "Props", depth);
            }
            else if (source.ShouldSerializeAdapter())
            {
                IEnumerable<PSPropertyInfo> propertyCollection = null;
                propertyCollection = source.GetAdaptedProperties();
                if (propertyCollection != null)
                {
                    this.SerializeProperties(propertyCollection, "Props", depth);
                }
            }
        }

        private void WritePSObjectReference(string streamName, string property, string refId)
        {
            this.WriteStartElement("Ref");
            if (streamName != null)
            {
                this.WriteAttribute("S", streamName);
            }
            if (property != null)
            {
                this.WriteNameAttribute(property);
            }
            this.WriteAttribute("RefId", refId);
            this._writer.WriteEndElement();
        }

        private static void WriteRawString(InternalSerializer serializer, string streamName, string property, string raw, TypeSerializationInfo entry)
        {
            if (property != null)
            {
                serializer.WriteStartElement(entry.PropertyTag);
                serializer.WriteNameAttribute(property);
            }
            else
            {
                serializer.WriteStartElement(entry.ItemTag);
            }
            if (streamName != null)
            {
                serializer.WriteAttribute("S", streamName);
            }
            serializer._writer.WriteRaw(raw);
            serializer._writer.WriteEndElement();
        }

        internal static void WriteScriptBlock(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
        {
            WriteEncodedString(serializer, streamName, property, Convert.ToString(source, CultureInfo.InvariantCulture), entry);
        }

        internal static void WriteSecureString(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
        {
            serializer.HandleSecureString(source, streamName, property);
        }

        internal static void WriteSingle(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
        {
            WriteRawString(serializer, streamName, property, XmlConvert.ToString((float) source), entry);
        }

        private void WriteStartElement(string elementTag)
        {
            if (SerializationOptions.NoNamespace == (this._context.options & SerializationOptions.NoNamespace))
            {
                this._writer.WriteStartElement(elementTag);
            }
            else
            {
                this._writer.WriteStartElement(elementTag, "http://schemas.microsoft.com/powershell/2004/04");
            }
        }

        private void WriteStartOfPSObject(PSObject mshObject, string streamName, string property, string refId, bool writeTypeNames, string toStringValue)
        {
            this.WriteStartElement("Obj");
            if (streamName != null)
            {
                this.WriteAttribute("S", streamName);
            }
            if (property != null)
            {
                this.WriteNameAttribute(property);
            }
            if (refId != null)
            {
                this.WriteAttribute("RefId", refId);
            }
            if (writeTypeNames)
            {
                ConsolidatedString internalTypeNames = mshObject.InternalTypeNames;
                if (internalTypeNames.Count > 0)
                {
                    string str2 = this.typeRefIdHandler.GetRefId(internalTypeNames);
                    if (str2 == null)
                    {
                        this.WriteStartElement("TN");
                        string str3 = this.typeRefIdHandler.SetRefId(internalTypeNames);
                        this.WriteAttribute("RefId", str3);
                        foreach (string str4 in internalTypeNames)
                        {
                            this.WriteEncodedElementString("T", str4);
                        }
                        this._writer.WriteEndElement();
                    }
                    else
                    {
                        this.WriteStartElement("TNRef");
                        this.WriteAttribute("RefId", str2);
                        this._writer.WriteEndElement();
                    }
                }
            }
            if (toStringValue != null)
            {
                this.WriteEncodedElementString("ToString", toStringValue);
            }
        }

        internal static void WriteTimeSpan(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
        {
            WriteRawString(serializer, streamName, property, XmlConvert.ToString((TimeSpan) source), entry);
        }

        internal static void WriteUri(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
        {
            WriteEncodedString(serializer, streamName, property, Convert.ToString(source, CultureInfo.InvariantCulture), entry);
        }

        internal static void WriteVersion(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
        {
            WriteRawString(serializer, streamName, property, Convert.ToString(source, CultureInfo.InvariantCulture), entry);
        }

        internal static void WriteXmlDocument(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
        {
            string outerXml = ((XmlDocument) source).OuterXml;
            WriteEncodedString(serializer, streamName, property, outerXml, entry);
        }

        private Collection<CollectionEntry<PSPropertyInfo>> AllPropertiesCollection
        {
            get
            {
                if (this.allPropertiesCollection == null)
                {
                    this.allPropertiesCollection = PSObject.GetPropertyCollection(PSMemberViewTypes.All, this._typeTable);
                }
                return this.allPropertiesCollection;
            }
        }

        private bool CanUseDefaultRunspaceInThreadSafeManner
        {
            get
            {
                if (!this.canUseDefaultRunspaceInThreadSafeManner.HasValue)
                {
                    this.canUseDefaultRunspaceInThreadSafeManner = false;
                    RunspaceBase defaultRunspace = Runspace.DefaultRunspace as RunspaceBase;
                    if (defaultRunspace != null)
                    {
                        LocalPipeline currentlyRunningPipeline = defaultRunspace.GetCurrentlyRunningPipeline() as LocalPipeline;
                        if ((currentlyRunningPipeline != null) && (currentlyRunningPipeline.NestedPipelineExecutionThread != null))
                        {
                            this.canUseDefaultRunspaceInThreadSafeManner = new bool?(currentlyRunningPipeline.NestedPipelineExecutionThread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId);
                        }
                    }
                }
                return this.canUseDefaultRunspaceInThreadSafeManner.Value;
            }
        }

        private Collection<CollectionEntry<PSMemberInfo>> ExtendedMembersCollection
        {
            get
            {
                if (this.extendedMembersCollection == null)
                {
                    this.extendedMembersCollection = PSObject.GetMemberCollection(PSMemberViewTypes.Extended, this._typeTable);
                }
                return this.extendedMembersCollection;
            }
        }

        internal System.Management.Automation.Runspaces.TypeTable TypeTable
        {
            get
            {
                return this._typeTable;
            }
            set
            {
                this._typeTable = value;
            }
        }

        private class ConsolidatedStringEqualityComparer : IEqualityComparer<ConsolidatedString>
        {
            bool IEqualityComparer<ConsolidatedString>.Equals(ConsolidatedString x, ConsolidatedString y)
            {
                return x.Key.Equals(y.Key, StringComparison.Ordinal);
            }

            int IEqualityComparer<ConsolidatedString>.GetHashCode(ConsolidatedString obj)
            {
                return obj.Key.GetHashCode();
            }
        }
    }
}

