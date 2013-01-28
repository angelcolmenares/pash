namespace System.Management.Automation
{
    using Microsoft.Management.Infrastructure;
    using Microsoft.PowerShell;
    using Microsoft.PowerShell.Cim;
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data;
    using System.DirectoryServices;
    using System.Dynamic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Management;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;

    [Serializable, TypeDescriptionProvider(typeof(PSObjectTypeDescriptionProvider))]
    public class PSObject : IFormattable, IComparable, ISerializable, IDynamicMetaObjectProvider
    {
        private static readonly ConcurrentDictionary<Type, AdapterSet> _adapterMapping = new ConcurrentDictionary<Type, AdapterSet>();
        private static readonly List<Func<object, AdapterSet>> _adapterSetMappers = new List<Func<object, AdapterSet>> { new Func<object, AdapterSet>(PSObject.MappedInternalAdapterSet) };
        private PSMemberInfoInternalCollection<PSMemberInfo> _instanceMembers;
        private static readonly ConditionalWeakTable<object, PSMemberInfoInternalCollection<PSMemberInfo>> _instanceMembersResurrectionTable = new ConditionalWeakTable<object, PSMemberInfoInternalCollection<PSMemberInfo>>();
        private PSMemberInfoIntegratingCollection<PSMemberInfo> _members;
        private PSMemberInfoIntegratingCollection<PSMethodInfo> _methods;
        private PSMemberInfoIntegratingCollection<PSPropertyInfo> _properties;
        private ConsolidatedString _typeNames;
        private static readonly ConditionalWeakTable<object, ConsolidatedString> _typeNamesResurrectionTable = new ConditionalWeakTable<object, ConsolidatedString>();
        private WeakReference _typeTable;
        internal PSMemberInfoInternalCollection<PSPropertyInfo> adaptedMembers;
        public const string AdaptedMemberSetName = "psadapted";
        private AdapterSet adapterSet;
        private static readonly DotNetAdapter baseAdapterForAdaptedObjects = new BaseDotNetAdapterForAdaptedObjects();
        public const string BaseObjectMemberSetName = "psbase";
        private static AdapterSet cimInstanceAdapter = new AdapterSet(new ThirdPartyAdapter(typeof(CimInstance), new CimInstanceAdapter()), dotNetInstanceAdapter);
        internal PSMemberInfoInternalCollection<PSPropertyInfo> clrMembers;
        private static readonly AdapterSet dataRowAdapter = new AdapterSet(new DataRowAdapter(), baseAdapterForAdaptedObjects);
        private static readonly AdapterSet dataRowViewAdapter = new AdapterSet(new DataRowViewAdapter(), baseAdapterForAdaptedObjects);
        private static readonly AdapterSet directoryEntryAdapter = new AdapterSet(new DirectoryEntryAdapter(), dotNetInstanceAdapter);
        internal static readonly DotNetAdapter dotNetInstanceAdapter = new DotNetAdapter();
        private static readonly AdapterSet dotNetInstanceAdapterSet = new AdapterSet(dotNetInstanceAdapter, null);
        internal static readonly DotNetAdapter dotNetStaticAdapter = new DotNetAdapter(true);
        public const string ExtendedMemberSetName = "psextended";
        internal bool hasGeneratedReservedMembers;
        private object immediateBaseObject;
        internal bool immediateBaseObjectIsEmpty;
        internal bool isDeserialized;
        private bool isHelpObject;
        private object lockObject;
        private static readonly AdapterSet managementClassAdapter = new AdapterSet(new ManagementClassApdapter(), dotNetInstanceAdapter);
        private static readonly AdapterSet managementObjectAdapter = new AdapterSet(new ManagementObjectAdapter(), dotNetInstanceAdapter);
        private static Collection<CollectionEntry<PSMemberInfo>> memberCollection = GetMemberCollection(PSMemberViewTypes.All);
        internal static PSTraceSource memberResolution = PSTraceSource.GetTracer("MemberResolution", "Traces the resolution from member name to the member. A member can be a property, method, etc.", false);
        private static Collection<CollectionEntry<PSMethodInfo>> methodCollection = GetMethodCollection();
        private static readonly AdapterSet mshMemberSetAdapter = new AdapterSet(new PSMemberSetAdapter(), null);
        private static readonly AdapterSet mshObjectAdapter = new AdapterSet(new PSObjectAdapter(), null);
        internal bool preserveToString;
        internal bool preserveToStringSet;
        private static Collection<CollectionEntry<PSPropertyInfo>> propertyCollection = GetPropertyCollection(PSMemberViewTypes.All);
        internal const string PSObjectMemberSetName = "psobject";
        internal const string PSTypeNames = "pstypenames";
        internal string TokenText;
        private string toStringFromDeserialization;
        private static readonly AdapterSet xmlNodeAdapter = new AdapterSet(new XmlNodeAdapter(), baseAdapterForAdaptedObjects);

        public PSObject()
        {
            this.lockObject = new object();
            this.CommonInitialization(PSCustomObject.SelfInstance);
        }

        public PSObject(object obj)
        {
            this.lockObject = new object();
            if (obj == null)
            {
                throw PSTraceSource.NewArgumentNullException("obj");
            }
            this.CommonInitialization(obj);
        }

        protected PSObject(SerializationInfo info, StreamingContext context)
        {
            this.lockObject = new object();
            if (info == null)
            {
                throw PSTraceSource.NewArgumentNullException("info");
            }
            string source = info.GetValue("CliXml", typeof(string)) as string;
            if (source == null)
            {
                throw PSTraceSource.NewArgumentNullException("info");
            }
            PSObject obj2 = AsPSObject(PSSerializer.Deserialize(source));
            this.CommonInitialization(obj2.ImmediateBaseObject);
            CopyDeserializerFields(obj2, this);
        }

        private static T AdapterGetMemberDelegate<T>(PSObject msjObj, string name) where T: PSMemberInfo
        {
            if (msjObj.isDeserialized)
            {
                if (msjObj.adaptedMembers == null)
                {
                    return default(T);
                }
                T local = msjObj.adaptedMembers[name] as T;
                memberResolution.WriteLine("Serialized adapted member: {0}.", new object[] { (local == null) ? "not found" : local.Name });
                return local;
            }
            T local2 = msjObj.InternalAdapter.BaseGetMember<T>(msjObj.immediateBaseObject, name);
            memberResolution.WriteLine("Adapted member: {0}.", new object[] { (local2 == null) ? "not found" : local2.Name });
            return local2;
        }

        private static PSMemberInfoInternalCollection<T> AdapterGetMembersDelegate<T>(PSObject msjObj) where T: PSMemberInfo
        {
            if (msjObj.isDeserialized)
            {
                if (msjObj.adaptedMembers == null)
                {
                    return new PSMemberInfoInternalCollection<T>();
                }
                memberResolution.WriteLine("Serialized adapted members: {0}.", new object[] { msjObj.adaptedMembers.Count });
                return TransformMemberInfoCollection<PSPropertyInfo, T>(msjObj.adaptedMembers);
            }
            PSMemberInfoInternalCollection<T> internals = msjObj.InternalAdapter.BaseGetMembers<T>(msjObj.immediateBaseObject);
            memberResolution.WriteLine("Adapted members: {0}.", new object[] { internals.VisibleCount });
            return internals;
        }

        internal void AddOrSetProperty(PSNoteProperty property)
        {
            PSMemberInfo info;
            if (PSGetMemberBinder.TryGetInstanceMember(this, property.Name, out info) && (info is PSPropertyInfo))
            {
                info.Value = property.Value;
            }
            else
            {
                this.Properties.Add(property);
            }
        }

        internal void AddOrSetProperty(string memberName, object value)
        {
            PSMemberInfo info;
            if (PSGetMemberBinder.TryGetInstanceMember(this, memberName, out info) && (info is PSPropertyInfo))
            {
                info.Value = value;
            }
            else
            {
                this.Properties.Add(new PSNoteProperty(memberName, value));
            }
        }

        public static PSObject AsPSObject(object obj)
        {
            if (obj == null)
            {
                throw PSTraceSource.NewArgumentNullException("obj");
            }
            PSObject obj2 = obj as PSObject;
            if (obj2 != null)
            {
                return obj2;
            }
            return new PSObject(obj);
        }

        internal static object Base(object obj)
        {
            PSObject obj2 = obj as PSObject;
            if (obj2 == null)
            {
                return obj;
            }
            if (obj2 == AutomationNull.Value)
            {
                return null;
            }
            if (obj2.immediateBaseObjectIsEmpty)
            {
                return obj;
            }
            object immediateBaseObject = null;
            do
            {
                immediateBaseObject = obj2.immediateBaseObject;
                obj2 = immediateBaseObject as PSObject;
            }
            while ((obj2 != null) && !obj2.immediateBaseObjectIsEmpty);
            return immediateBaseObject;
        }

        private void CommonInitialization(object obj)
        {
            if (obj is PSCustomObject)
            {
                this.immediateBaseObjectIsEmpty = true;
            }
            this.immediateBaseObject = obj;
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            this._typeTable = (executionContextFromTLS != null) ? new WeakReference(executionContextFromTLS.TypeTable) : null;
        }

        public int CompareTo(object obj)
        {
            int num;
            if (object.ReferenceEquals(this, obj))
            {
                return 0;
            }
            try
            {
                num = LanguagePrimitives.Compare(this.BaseObject, obj);
            }
            catch (ArgumentException exception)
            {
                throw new ExtendedTypeSystemException("PSObjectCompareTo", exception, ExtendedTypeSystem.NotTheSameTypeOrNotIcomparable, new object[] { this.PrivateToString(), AsPSObject(obj).ToString(), "IComparable" });
            }
            return num;
        }

        internal static PSObject ConstructPSObjectFromSerializationInfo(SerializationInfo info, StreamingContext context)
        {
            return new PSObject(info, context);
        }

        public virtual PSObject Copy()
        {
            PSObject owner = (PSObject) base.MemberwiseClone();
            if (this.BaseObject is PSCustomObject)
            {
                owner.immediateBaseObject = PSCustomObject.SelfInstance;
                owner.immediateBaseObjectIsEmpty = true;
            }
            else
            {
                owner.immediateBaseObject = this.immediateBaseObject;
                owner.immediateBaseObjectIsEmpty = false;
            }
            owner._instanceMembers = null;
            owner._typeNames = null;
            owner._members = new PSMemberInfoIntegratingCollection<PSMemberInfo>(owner, memberCollection);
            owner._properties = new PSMemberInfoIntegratingCollection<PSPropertyInfo>(owner, propertyCollection);
            owner._methods = new PSMemberInfoIntegratingCollection<PSMethodInfo>(owner, methodCollection);
            owner.adapterSet = GetMappedAdapter(owner.immediateBaseObject, owner.GetTypeTable());
            ICloneable immediateBaseObject = owner.immediateBaseObject as ICloneable;
            if (immediateBaseObject != null)
            {
                owner.immediateBaseObject = immediateBaseObject.Clone();
            }
            if (owner.immediateBaseObject is System.ValueType)
            {
                owner.immediateBaseObject = CopyValueType(owner.immediateBaseObject);
            }
            if (!object.ReferenceEquals(GetKeyForResurrectionTables(this), GetKeyForResurrectionTables(owner)))
            {
                foreach (PSMemberInfo info in this.InstanceMembers)
                {
                    if (!info.isHidden)
                    {
                        owner.Members.Add(info);
                    }
                }
                owner.TypeNames.Clear();
                foreach (string str in this.InternalTypeNames)
                {
                    owner.TypeNames.Add(str);
                }
            }
            owner.hasGeneratedReservedMembers = false;
            return owner;
        }

        internal static void CopyDeserializerFields(PSObject source, PSObject target)
        {
            if (!target.isDeserialized)
            {
                target.isDeserialized = source.isDeserialized;
                target.adaptedMembers = source.adaptedMembers;
                target.clrMembers = source.clrMembers;
            }
            if (target.toStringFromDeserialization == null)
            {
                target.toStringFromDeserialization = source.toStringFromDeserialization;
                target.TokenText = source.TokenText;
            }
        }

        internal static object CopyValueType(object obj)
        {
            Array array = Array.CreateInstance(obj.GetType(), 1);
            array.SetValue(obj, 0);
            return array.GetValue(0);
        }

        internal static AdapterSet CreateThirdPartyAdapterSet(Type adaptedType, PSPropertyAdapter adapter)
        {
            return new AdapterSet(new ThirdPartyAdapter(adaptedType, adapter), baseAdapterForAdaptedObjects);
        }

        private static T DotNetGetMemberDelegate<T>(PSObject msjObj, string name) where T: PSMemberInfo
        {
            if (msjObj.InternalAdapterSet.DotNetAdapter != null)
            {
                T local = msjObj.InternalAdapterSet.DotNetAdapter.BaseGetMember<T>(msjObj.immediateBaseObject, name);
                memberResolution.WriteLine("DotNet member: {0}.", new object[] { (local == null) ? "not found" : local.Name });
                return local;
            }
            return default(T);
        }

        private static PSMemberInfoInternalCollection<T> DotNetGetMembersDelegate<T>(PSObject msjObj) where T: PSMemberInfo
        {
            if (msjObj.InternalAdapterSet.DotNetAdapter != null)
            {
                PSMemberInfoInternalCollection<T> internals = msjObj.InternalAdapterSet.DotNetAdapter.BaseGetMembers<T>(msjObj.immediateBaseObject);
                memberResolution.WriteLine("DotNet members: {0}.", new object[] { internals.VisibleCount });
                return internals;
            }
            return new PSMemberInfoInternalCollection<T>();
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }
            if (object.ReferenceEquals(this.BaseObject, PSCustomObject.SelfInstance))
            {
                return false;
            }
            return LanguagePrimitives.Equals(this.BaseObject, obj);
        }

        internal PSMemberInfoInternalCollection<PSPropertyInfo> GetAdaptedProperties()
        {
            return this.GetProperties(this.adaptedMembers, this.InternalAdapter);
        }

        internal PSMemberInfoInternalCollection<PSPropertyInfo> GetBaseProperties()
        {
            return this.GetProperties(this.clrMembers, dotNetInstanceAdapter);
        }

        public override int GetHashCode()
        {
            return this.BaseObject.GetHashCode();
        }

        internal static object GetKeyForResurrectionTables(object obj)
        {
            PSObject obj2 = obj as PSObject;
            if (obj2 == null)
            {
                return obj;
            }
            PSObject immediateBaseObject = obj2;
            while (immediateBaseObject.ImmediateBaseObject is PSObject)
            {
                immediateBaseObject = (PSObject) immediateBaseObject.ImmediateBaseObject;
            }
            if ((immediateBaseObject.ImmediateBaseObject is PSCustomObject) || (immediateBaseObject.ImmediateBaseObject is string))
            {
                return immediateBaseObject;
            }
            return immediateBaseObject.ImmediateBaseObject;
        }

        internal static AdapterSet GetMappedAdapter(object obj, TypeTable typeTable)
        {
            AdapterSet dotNetInstanceAdapterSet;
            Type type = obj.GetType();
            if (typeTable != null)
            {
                AdapterSet typeAdapter = typeTable.GetTypeAdapter(type);
                if (typeAdapter != null)
                {
                    return typeAdapter;
                }
            }
            if (_adapterMapping.TryGetValue(type, out dotNetInstanceAdapterSet))
            {
                return dotNetInstanceAdapterSet;
            }
            lock (_adapterSetMappers)
            {
                foreach (Func<object, AdapterSet> func in _adapterSetMappers)
                {
                    dotNetInstanceAdapterSet = func(obj);
                    if (dotNetInstanceAdapterSet != null)
                    {
                        goto Label_0082;
                    }
                }
            }
        Label_0082:
            if (dotNetInstanceAdapterSet == null)
            {
                if (type.IsCOMObject)
                {
                    if (WinRTHelper.IsWinRTType(type))
                    {
                        dotNetInstanceAdapterSet = PSObject.dotNetInstanceAdapterSet;
                    }
                    else
                    {
                        if (type.FullName.Equals("System.__ComObject"))
                        {
                            ComTypeInfo typeinfo = ComTypeInfo.GetDispatchTypeInfo(obj);
                            if (typeinfo == null)
                            {
                                return PSObject.dotNetInstanceAdapterSet;
                            }
                            return new AdapterSet(new ComAdapter(typeinfo), dotNetInstanceAdapter);
                        }
                        ComTypeInfo dispatchTypeInfo = ComTypeInfo.GetDispatchTypeInfo(obj);
                        dotNetInstanceAdapterSet = (dispatchTypeInfo != null) ? new AdapterSet(new DotNetAdapterWithComTypeName(dispatchTypeInfo), null) : PSObject.dotNetInstanceAdapterSet;
                    }
                }
                else
                {
                    dotNetInstanceAdapterSet = PSObject.dotNetInstanceAdapterSet;
                }
            }
            _adapterMapping.GetOrAdd(type, dotNetInstanceAdapterSet);
            return dotNetInstanceAdapterSet;
        }

        internal static Collection<CollectionEntry<PSMemberInfo>> GetMemberCollection(PSMemberViewTypes viewType)
        {
            return GetMemberCollection(viewType, null);
        }

        internal static Collection<CollectionEntry<PSMemberInfo>> GetMemberCollection(PSMemberViewTypes viewType, TypeTable backupTypeTable)
        {
            CollectionEntry<PSMemberInfo>.GetMembersDelegate getMembers = null;
            CollectionEntry<PSMemberInfo>.GetMemberDelegate getMember = null;
            Collection<CollectionEntry<PSMemberInfo>> collection = new Collection<CollectionEntry<PSMemberInfo>>();
            if ((viewType & PSMemberViewTypes.Extended) == PSMemberViewTypes.Extended)
            {
                if (backupTypeTable == null)
                {
                    collection.Add(new CollectionEntry<PSMemberInfo>(new CollectionEntry<PSMemberInfo>.GetMembersDelegate(PSObject.TypeTableGetMembersDelegate<PSMemberInfo>), new CollectionEntry<PSMemberInfo>.GetMemberDelegate(PSObject.TypeTableGetMemberDelegate<PSMemberInfo>), true, true, "type table members"));
                }
                else
                {
                    if (getMembers == null)
                    {
                        getMembers = msjObj => TypeTableGetMembersDelegate<PSMemberInfo>(msjObj, backupTypeTable);
                    }
                    if (getMember == null)
                    {
                        getMember = (msjObj, name) => TypeTableGetMemberDelegate<PSMemberInfo>(msjObj, backupTypeTable, name);
                    }
                    collection.Add(new CollectionEntry<PSMemberInfo>(getMembers, getMember, true, true, "type table members"));
                }
            }
            if ((viewType & PSMemberViewTypes.Adapted) == PSMemberViewTypes.Adapted)
            {
                collection.Add(new CollectionEntry<PSMemberInfo>(new CollectionEntry<PSMemberInfo>.GetMembersDelegate(PSObject.AdapterGetMembersDelegate<PSMemberInfo>), new CollectionEntry<PSMemberInfo>.GetMemberDelegate(PSObject.AdapterGetMemberDelegate<PSMemberInfo>), false, false, "adapted members"));
            }
            if ((viewType & PSMemberViewTypes.Base) == PSMemberViewTypes.Base)
            {
                collection.Add(new CollectionEntry<PSMemberInfo>(new CollectionEntry<PSMemberInfo>.GetMembersDelegate(PSObject.DotNetGetMembersDelegate<PSMemberInfo>), new CollectionEntry<PSMemberInfo>.GetMemberDelegate(PSObject.DotNetGetMemberDelegate<PSMemberInfo>), false, false, "clr members"));
            }
            return collection;
        }

        private static Collection<CollectionEntry<PSMethodInfo>> GetMethodCollection()
        {
            return new Collection<CollectionEntry<PSMethodInfo>> { new CollectionEntry<PSMethodInfo>(new CollectionEntry<PSMethodInfo>.GetMembersDelegate(PSObject.TypeTableGetMembersDelegate<PSMethodInfo>), new CollectionEntry<PSMethodInfo>.GetMemberDelegate(PSObject.TypeTableGetMemberDelegate<PSMethodInfo>), true, true, "type table members"), new CollectionEntry<PSMethodInfo>(new CollectionEntry<PSMethodInfo>.GetMembersDelegate(PSObject.AdapterGetMembersDelegate<PSMethodInfo>), new CollectionEntry<PSMethodInfo>.GetMemberDelegate(PSObject.AdapterGetMemberDelegate<PSMethodInfo>), false, false, "adapted members"), new CollectionEntry<PSMethodInfo>(new CollectionEntry<PSMethodInfo>.GetMembersDelegate(PSObject.DotNetGetMembersDelegate<PSMethodInfo>), new CollectionEntry<PSMethodInfo>.GetMemberDelegate(PSObject.DotNetGetMemberDelegate<PSMethodInfo>), false, false, "clr members") };
        }

        internal static object GetNoteSettingValue(PSMemberSet settings, string noteName, object defaultValue, Type expectedType, bool shouldReplicateInstance, PSObject ownerObject)
        {
            if (settings != null)
            {
                if (shouldReplicateInstance)
                {
                    settings.ReplicateInstance(ownerObject);
                }
                PSNoteProperty property = settings.Members[noteName] as PSNoteProperty;
                if (property == null)
                {
                    return defaultValue;
                }
                object obj2 = property.Value;
                if ((obj2 != null) && obj2.GetType().Equals(expectedType))
                {
                    return property.Value;
                }
            }
            return defaultValue;
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw PSTraceSource.NewArgumentNullException("info");
            }
            PSObject source = new PSObject(this);
            string str = PSSerializer.Serialize(source);
            info.AddValue("CliXml", str);
        }

        private PSMemberInfoInternalCollection<PSPropertyInfo> GetProperties(PSMemberInfoInternalCollection<PSPropertyInfo> serializedMembers, Adapter particularAdapter)
        {
            if (this.isDeserialized)
            {
                return serializedMembers;
            }
            PSMemberInfoInternalCollection<PSPropertyInfo> internals = new PSMemberInfoInternalCollection<PSPropertyInfo>();
            foreach (PSPropertyInfo info in particularAdapter.BaseGetMembers<PSPropertyInfo>(this.immediateBaseObject))
            {
                internals.Add(info);
            }
            return internals;
        }

        internal static Collection<CollectionEntry<PSPropertyInfo>> GetPropertyCollection(PSMemberViewTypes viewType)
        {
            return GetPropertyCollection(viewType, null);
        }

        internal static Collection<CollectionEntry<PSPropertyInfo>> GetPropertyCollection(PSMemberViewTypes viewType, TypeTable backupTypeTable)
        {
            CollectionEntry<PSPropertyInfo>.GetMembersDelegate getMembers = null;
            CollectionEntry<PSPropertyInfo>.GetMemberDelegate getMember = null;
            Collection<CollectionEntry<PSPropertyInfo>> collection = new Collection<CollectionEntry<PSPropertyInfo>>();
            if ((viewType & PSMemberViewTypes.Extended) == PSMemberViewTypes.Extended)
            {
                if (backupTypeTable == null)
                {
                    collection.Add(new CollectionEntry<PSPropertyInfo>(new CollectionEntry<PSPropertyInfo>.GetMembersDelegate(PSObject.TypeTableGetMembersDelegate<PSPropertyInfo>), new CollectionEntry<PSPropertyInfo>.GetMemberDelegate(PSObject.TypeTableGetMemberDelegate<PSPropertyInfo>), true, true, "type table members"));
                }
                else
                {
                    if (getMembers == null)
                    {
                        getMembers = msjObj => TypeTableGetMembersDelegate<PSPropertyInfo>(msjObj, backupTypeTable);
                    }
                    if (getMember == null)
                    {
                        getMember = (msjObj, name) => TypeTableGetMemberDelegate<PSPropertyInfo>(msjObj, backupTypeTable, name);
                    }
                    collection.Add(new CollectionEntry<PSPropertyInfo>(getMembers, getMember, true, true, "type table members"));
                }
            }
            if ((viewType & PSMemberViewTypes.Adapted) == PSMemberViewTypes.Adapted)
            {
                collection.Add(new CollectionEntry<PSPropertyInfo>(new CollectionEntry<PSPropertyInfo>.GetMembersDelegate(PSObject.AdapterGetMembersDelegate<PSPropertyInfo>), new CollectionEntry<PSPropertyInfo>.GetMemberDelegate(PSObject.AdapterGetMemberDelegate<PSPropertyInfo>), false, false, "adapted members"));
            }
            if ((viewType & PSMemberViewTypes.Base) == PSMemberViewTypes.Base)
            {
                collection.Add(new CollectionEntry<PSPropertyInfo>(new CollectionEntry<PSPropertyInfo>.GetMembersDelegate(PSObject.DotNetGetMembersDelegate<PSPropertyInfo>), new CollectionEntry<PSPropertyInfo>.GetMemberDelegate(PSObject.DotNetGetMemberDelegate<PSPropertyInfo>), false, false, "clr members"));
            }
            return collection;
        }

        internal PSMemberInfo GetPSStandardMember(TypeTable backupTypeTable, string memberName)
        {
            PSMemberInfo info = null;
            TypeTable typeTableToUse = (backupTypeTable != null) ? backupTypeTable : this.GetTypeTable();
            if (typeTableToUse != null)
            {
                PSMemberSet owner = TypeTableGetMemberDelegate<PSMemberSet>(this, typeTableToUse, "PSStandardMembers");
                if (owner != null)
                {
                    owner.ReplicateInstance(this);
                    PSMemberInfoIntegratingCollection<PSMemberInfo> integratings = new PSMemberInfoIntegratingCollection<PSMemberInfo>(owner, GetMemberCollection(PSMemberViewTypes.All, backupTypeTable));
                    info = integratings[memberName];
                }
            }
            if (info == null)
            {
                info = this.InstanceMembers["PSStandardMembers"] as PSMemberSet;
            }
            return info;
        }

        internal int GetReferenceHashCode()
        {
            return base.GetHashCode();
        }

        private static string GetSeparator(ExecutionContext context, string separator)
        {
            if (separator != null)
            {
                return separator;
            }
            if (context != null)
            {
                object variableValue = context.GetVariableValue(SpecialVariables.OFSVarPath);
                if (variableValue != null)
                {
                    return variableValue.ToString();
                }
            }
            return " ";
        }

        internal int GetSerializationDepth(TypeTable backupTypeTable)
        {
            int num = 0;
            TypeTable typeTableToUse = (backupTypeTable != null) ? backupTypeTable : this.GetTypeTable();
            if (typeTableToUse != null)
            {
                num = (int) GetNoteSettingValue(TypeTableGetMemberDelegate<PSMemberSet>(this, typeTableToUse, "PSStandardMembers"), "SerializationDepth", 0, typeof(int), true, this);
            }
            return num;
        }

        internal SerializationMethod GetSerializationMethod(TypeTable backupTypeTable)
        {
            SerializationMethod allPublicProperties = SerializationMethod.AllPublicProperties;
            TypeTable typeTableToUse = (backupTypeTable != null) ? backupTypeTable : this.GetTypeTable();
            if (typeTableToUse != null)
            {
                allPublicProperties = (SerializationMethod) GetNoteSettingValue(TypeTableGetMemberDelegate<PSMemberSet>(this, typeTableToUse, "PSStandardMembers"), "SerializationMethod", SerializationMethod.AllPublicProperties, typeof(SerializationMethod), true, this);
            }
            return allPublicProperties;
        }

        internal Collection<string> GetSpecificPropertiesToSerialize(TypeTable backupTypeTable)
        {
            TypeTable table = (backupTypeTable != null) ? backupTypeTable : this.GetTypeTable();
            if (table != null)
            {
                return table.GetSpecificProperties(this.InternalTypeNames);
            }
            return new Collection<string>(new List<string>());
        }

        internal static PSMemberInfo GetStaticCLRMember(object obj, string methodName)
        {
            obj = Base(obj);
            if (((obj != null) && (obj is Type)) && ((methodName != null) && (methodName.Length != 0)))
            {
                return dotNetStaticAdapter.BaseGetMember<PSMemberInfo>(obj, methodName);
            }
            return null;
        }

        internal PSPropertyInfo GetStringSerializationSource(TypeTable backupTypeTable)
        {
            return (this.GetPSStandardMember(backupTypeTable, "StringSerializationSource") as PSPropertyInfo);
        }

        internal Type GetTargetTypeForDeserialization(TypeTable backupTypeTable)
        {
            PSMemberInfo pSStandardMember = this.GetPSStandardMember(backupTypeTable, "TargetTypeForDeserialization");
            if (pSStandardMember != null)
            {
                return (pSStandardMember.Value as Type);
            }
            return null;
        }

        internal static ConsolidatedString GetTypeNames(object obj)
        {
            ConsolidatedString str;
            PSObject obj2 = obj as PSObject;
            if (obj2 != null)
            {
                return obj2.InternalTypeNames;
            }
            if (HasInstanceTypeName(obj, out str))
            {
                return str;
            }
            return GetMappedAdapter(obj, null).OriginalAdapter.BaseGetTypeNameHierarchy(obj);
        }

        internal TypeTable GetTypeTable()
        {
            TypeTable table;
            if ((this._typeTable != null) && this._typeTable.TryGetTarget<TypeTable>(out table))
            {
                return table;
            }
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            if (executionContextFromTLS != null)
            {
                return executionContextFromTLS.TypeTable;
            }
            return null;
        }

        internal static bool HasInstanceMembers(object obj, out PSMemberInfoInternalCollection<PSMemberInfo> instanceMembers)
        {
            PSObject obj2 = obj as PSObject;
            if (obj2 != null)
            {
                lock (obj2)
                {
                    if (obj2._instanceMembers == null)
                    {
                        _instanceMembersResurrectionTable.TryGetValue(GetKeyForResurrectionTables(obj2), out obj2._instanceMembers);
                    }
                }
                instanceMembers = obj2._instanceMembers;
            }
            else if (obj != null)
            {
                _instanceMembersResurrectionTable.TryGetValue(GetKeyForResurrectionTables(obj), out instanceMembers);
            }
            else
            {
                instanceMembers = null;
            }
            return ((instanceMembers != null) && (instanceMembers.Count > 0));
        }

        internal static bool HasInstanceTypeName(object obj, out ConsolidatedString result)
        {
            return _typeNamesResurrectionTable.TryGetValue(GetKeyForResurrectionTables(obj), out result);
        }

        private static AdapterSet MappedInternalAdapterSet(object obj)
        {
            if (obj is CimInstance)
            {
                return cimInstanceAdapter;
            }
            if (obj is ManagementClass)
            {
                return managementClassAdapter;
            }
            if (obj is ManagementBaseObject)
            {
                return managementObjectAdapter;
            }
            if (obj is DirectoryEntry)
            {
                return directoryEntryAdapter;
            }
            if (obj is DataRowView)
            {
                return dataRowViewAdapter;
            }
            if (obj is DataRow)
            {
                return dataRowAdapter;
            }
            if (obj is System.Xml.XmlNode)
            {
                return xmlNodeAdapter;
            }
            if (obj is PSMemberSet)
            {
                return mshMemberSetAdapter;
            }
            if (obj is PSObject)
            {
                return mshObjectAdapter;
            }
            return null;
        }

        public static implicit operator PSObject(bool valueToConvert)
        {
            return AsPSObject(valueToConvert);
        }

        public static implicit operator PSObject(Hashtable valueToConvert)
        {
            return AsPSObject(valueToConvert);
        }

        public static implicit operator PSObject(double valueToConvert)
        {
            return AsPSObject(valueToConvert);
        }

        public static implicit operator PSObject(int valueToConvert)
        {
            return AsPSObject(valueToConvert);
        }

        public static implicit operator PSObject(string valueToConvert)
        {
            return AsPSObject(valueToConvert);
        }

        private string PrivateToString()
        {
            try
            {
                return this.ToString();
            }
            catch (ExtendedTypeSystemException)
            {
                return this.BaseObject.GetType().FullName;
            }
        }

        internal static void RegisterAdapterMapping(Func<object, AdapterSet> mapper)
        {
            lock (_adapterSetMappers)
            {
                _adapterSetMappers.Add(mapper);
            }
        }

        internal void SetCoreOnDeserialization(object value, bool overrideTypeInfo)
        {
            this.immediateBaseObjectIsEmpty = false;
            this.immediateBaseObject = value;
            this.adapterSet = GetMappedAdapter(this.immediateBaseObject, this.GetTypeTable());
            if (overrideTypeInfo)
            {
                this.InternalTypeNames = this.InternalAdapter.BaseGetTypeNameHierarchy(value);
            }
        }

        internal bool ShouldSerializeAdapter()
        {
            if (this.isDeserialized)
            {
                return (this.adaptedMembers != null);
            }
            return !this.immediateBaseObjectIsEmpty;
        }

        internal bool ShouldSerializeBase()
        {
            if (this.isDeserialized)
            {
                return (this.adaptedMembers != this.clrMembers);
            }
            if (this.immediateBaseObjectIsEmpty)
            {
                return false;
            }
            return !this.InternalAdapter.GetType().Equals(typeof(DotNetAdapter));
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new PSDynamicMetaObject(parameter, this);
        }

        public override string ToString()
        {
            if (this.toStringFromDeserialization != null)
            {
                return this.toStringFromDeserialization;
            }
            return ToString(null, this, null, null, null, true, false);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (this.toStringFromDeserialization != null)
            {
                return this.toStringFromDeserialization;
            }
            return ToString(null, this, null, format, formatProvider, true, false);
        }

        internal static string ToString(ExecutionContext context, object obj, string separator, string format, IFormatProvider formatProvider, bool recurse, bool unravelEnumeratorOnRecurse)
        {
            PSMemberInfoInternalCollection<PSMemberInfo> internals;
            string str2;
            PSObject obj2 = obj as PSObject;
            if (obj2 == null)
            {
                if (obj == null)
                {
                    return string.Empty;
                }
                switch (Type.GetTypeCode(obj.GetType()))
                {
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        return obj.ToString();

                    case TypeCode.Single:
                    {
                        float num3 = (float) obj;
                        return num3.ToString(formatProvider);
                    }
                    case TypeCode.Double:
                    {
                        double num2 = (double) obj;
                        return num2.ToString(formatProvider);
                    }
                    case TypeCode.Decimal:
                    {
                        decimal num = (decimal) obj;
                        return num.ToString(formatProvider);
                    }
                    case TypeCode.DateTime:
                    {
                        DateTime time = (DateTime) obj;
                        return time.ToString(formatProvider);
                    }
                    case TypeCode.String:
                        return (string) obj;
                }
                if (recurse)
                {
                    IEnumerable enumerable = LanguagePrimitives.GetEnumerable(obj);
                    if (enumerable != null)
                    {
                        try
                        {
                            return ToStringEnumerable(context, enumerable, separator, format, formatProvider);
                        }
                        catch (Exception exception)
                        {
                            CommandProcessorBase.CheckForSevereException(exception);
                        }
                    }
                    if (unravelEnumeratorOnRecurse)
                    {
                        IEnumerator enumerator = LanguagePrimitives.GetEnumerator(obj);
                        if (enumerator != null)
                        {
                            try
                            {
                                return ToStringEnumerator(context, enumerator, separator, format, formatProvider);
                            }
                            catch (Exception exception2)
                            {
                                CommandProcessorBase.CheckForSevereException(exception2);
                            }
                        }
                    }
                }
                IFormattable formattable = obj as IFormattable;
                try
                {
                    if (formattable == null)
                    {
                        Type type = obj as Type;
                        if (type != null)
                        {
                            return ToStringCodeMethods.Type(type, false);
                        }
                        return obj.ToString();
                    }
                    return formattable.ToString(format, formatProvider);
                }
                catch (Exception exception3)
                {
                    CommandProcessorBase.CheckForSevereException(exception3);
                    throw new ExtendedTypeSystemException("ToStringObjectBasicException", exception3, ExtendedTypeSystem.ToStringException, new object[] { exception3.Message });
                }
            }
            PSMethodInfo info = null;
            if (HasInstanceMembers(obj2, out internals))
            {
                info = internals["ToString"] as PSMethodInfo;
            }
            if ((info == null) && (obj2.InternalTypeNames.Count != 0))
            {
                TypeTable typeTable = obj2.GetTypeTable();
                if (typeTable != null)
                {
                    info = typeTable.GetMembers<PSMethodInfo>(obj2.InternalTypeNames)["ToString"];
                    if (info != null)
                    {
                        info = (PSMethodInfo) info.Copy();
                        info.instance = obj2;
                    }
                }
            }
            if (info != null)
            {
                try
                {
                    object obj3;
                    if ((formatProvider != null) && (info.OverloadDefinitions.Count > 1))
                    {
                        obj3 = info.Invoke(new object[] { format, formatProvider });
                        return ((obj3 != null) ? obj3.ToString() : string.Empty);
                    }
                    obj3 = info.Invoke(new object[0]);
                    return ((obj3 != null) ? obj3.ToString() : string.Empty);
                }
                catch (MethodException exception4)
                {
                    throw new ExtendedTypeSystemException("MethodExceptionNullFormatProvider", exception4, ExtendedTypeSystem.ToStringException, new object[] { exception4.Message });
                }
            }
            if (recurse)
            {
                if (obj2.immediateBaseObjectIsEmpty)
                {
                    try
                    {
                        return ToStringEmptyBaseObject(context, obj2, separator, format, formatProvider);
                    }
                    catch (Exception exception5)
                    {
                        CommandProcessorBase.CheckForSevereException(exception5);
                    }
                }
                IEnumerable enumerable2 = LanguagePrimitives.GetEnumerable(obj2);
                if (enumerable2 != null)
                {
                    try
                    {
                        return ToStringEnumerable(context, enumerable2, separator, format, formatProvider);
                    }
                    catch (Exception exception6)
                    {
                        CommandProcessorBase.CheckForSevereException(exception6);
                    }
                }
                if (unravelEnumeratorOnRecurse)
                {
                    IEnumerator enumerator2 = LanguagePrimitives.GetEnumerator(obj2);
                    if (enumerator2 != null)
                    {
                        try
                        {
                            return ToStringEnumerator(context, enumerator2, separator, format, formatProvider);
                        }
                        catch (Exception exception7)
                        {
                            CommandProcessorBase.CheckForSevereException(exception7);
                        }
                    }
                }
            }
            if (obj2.TokenText != null)
            {
                return obj2.TokenText;
            }
            object immediateBaseObject = obj2.immediateBaseObject;
            IFormattable formattable2 = immediateBaseObject as IFormattable;
            try
            {
                string str;
				if (immediateBaseObject.GetType () == typeof(System.Diagnostics.Process))
			    {
					str = DotNetAdapter.SafeGetProcessName ((System.Diagnostics.Process)immediateBaseObject);
				}
                else if (formattable2 == null)
                {
                    str = immediateBaseObject.ToString();
                }
                else
                {
                    str = formattable2.ToString(format, formatProvider);
                }
                if (str == null)
                {
                    str = string.Empty;
                }
                str2 = str;
            }
            catch (Exception exception8)
            {
                CommandProcessorBase.CheckForSevereException(exception8);
                throw new ExtendedTypeSystemException("ToStringPSObjectBasicException", exception8, ExtendedTypeSystem.ToStringException, new object[] { exception8.Message });
            }
            return str2;
        }

        private static string ToStringEmptyBaseObject(ExecutionContext context, PSObject mshObj, string separator, string format, IFormatProvider formatProvider)
        {
            StringBuilder builder = new StringBuilder();
            ReadOnlyPSMemberInfoCollection<PSPropertyInfo> infos = mshObj.Properties.Match("*");
            if (infos.Count == 0)
            {
                return string.Empty;
            }
            builder.Append("@{");
            string str = "; ";
            foreach (PSPropertyInfo info in infos)
            {
                builder.Append(info.Name);
                builder.Append("=");
                builder.Append(ToString(context, info.Value, separator, format, formatProvider, false, false));
                builder.Append(str);
            }
            int length = str.Length;
            builder.Remove(builder.Length - length, length);
            builder.Append("}");
            return builder.ToString();
        }

        internal static string ToStringEnumerable(ExecutionContext context, IEnumerable enumerable, string separator, string format, IFormatProvider formatProvider)
        {
            StringBuilder builder = new StringBuilder();
            string str = GetSeparator(context, separator);
            foreach (object obj2 in enumerable)
            {
                if (obj2 != null)
                {
                    PSObject obj3 = AsPSObject(obj2);
                    builder.Append(ToString(context, obj3, separator, format, formatProvider, false, false));
                }
                builder.Append(str);
            }
            if (builder.Length == 0)
            {
                return string.Empty;
            }
            int length = str.Length;
            builder.Remove(builder.Length - length, length);
            return builder.ToString();
        }

        internal static string ToStringEnumerator(ExecutionContext context, IEnumerator enumerator, string separator, string format, IFormatProvider formatProvider)
        {
            StringBuilder builder = new StringBuilder();
            string str = GetSeparator(context, separator);
            while (enumerator.MoveNext())
            {
                object current = enumerator.Current;
                builder.Append(ToString(context, current, separator, format, formatProvider, false, false));
                builder.Append(str);
            }
            if (builder.Length == 0)
            {
                return string.Empty;
            }
            int length = str.Length;
            builder.Remove(builder.Length - length, length);
            return builder.ToString();
        }

        internal static string ToStringParser(ExecutionContext context, object obj)
        {
            string str;
            try
            {
                str = ToString(context, obj, null, null, CultureInfo.InvariantCulture, true, true);
            }
            catch (ExtendedTypeSystemException exception)
            {
                throw new PSInvalidCastException("InvalidCastFromAnyTypeToString", exception.InnerException, ExtendedTypeSystem.InvalidCastCannotRetrieveString, new object[0]);
            }
            return str;
        }

        internal static PSMemberInfoInternalCollection<U> TransformMemberInfoCollection<T, U>(PSMemberInfoCollection<T> source) where T: PSMemberInfo where U: PSMemberInfo
        {
            if (typeof(T).Equals(typeof(U)))
            {
                return (source as PSMemberInfoInternalCollection<U>);
            }
            PSMemberInfoInternalCollection<U> internals = new PSMemberInfoInternalCollection<U>();
            foreach (T local in source)
            {
                U member = local as U;
                if (member != null)
                {
                    internals.Add(member);
                }
            }
            return internals;
        }

        internal static T TypeTableGetMemberDelegate<T>(PSObject msjObj, string name) where T: PSMemberInfo
        {
            TypeTable typeTable = msjObj.GetTypeTable();
            return TypeTableGetMemberDelegate<T>(msjObj, typeTable, name);
        }

        private static T TypeTableGetMemberDelegate<T>(PSObject msjObj, TypeTable typeTableToUse, string name) where T: PSMemberInfo
        {
            if (typeTableToUse != null)
            {
                PSMemberInfo info = typeTableToUse.GetMembers<PSMemberInfo>(msjObj.InternalTypeNames)[name];
                if (info == null)
                {
                    memberResolution.WriteLine("\"{0}\" NOT present in type table.", new object[] { name });
                    return default(T);
                }
                T local = info as T;
                if (local != null)
                {
                    memberResolution.WriteLine("\"{0}\" present in type table.", new object[] { name });
                    return local;
                }
                memberResolution.WriteLine("\"{0}\" from types table ignored because it has type {1} instead of {2}.", new object[] { name, info.GetType(), typeof(T) });
            }
            return default(T);
        }

        internal static PSMemberInfoInternalCollection<T> TypeTableGetMembersDelegate<T>(PSObject msjObj) where T: PSMemberInfo
        {
            TypeTable typeTable = msjObj.GetTypeTable();
            return TypeTableGetMembersDelegate<T>(msjObj, typeTable);
        }

        internal static PSMemberInfoInternalCollection<T> TypeTableGetMembersDelegate<T>(PSObject msjObj, TypeTable typeTableToUse) where T: PSMemberInfo
        {
            if (typeTableToUse == null)
            {
                return new PSMemberInfoInternalCollection<T>();
            }
            PSMemberInfoInternalCollection<T> members = typeTableToUse.GetMembers<T>(msjObj.InternalTypeNames);
            memberResolution.WriteLine("Type table members: {0}.", new object[] { members.Count });
            return members;
        }

        public object BaseObject
        {
            get
            {
                object immediateBaseObject = null;
                PSObject obj3 = this;
                do
                {
                    immediateBaseObject = obj3.immediateBaseObject;
                    obj3 = immediateBaseObject as PSObject;
                }
                while (obj3 != null);
                return immediateBaseObject;
            }
        }

        public object ImmediateBaseObject
        {
            get
            {
                return this.immediateBaseObject;
            }
        }

        internal PSMemberInfoInternalCollection<PSMemberInfo> InstanceMembers
        {
            get
            {
                if (this._instanceMembers == null)
                {
                    lock (this.lockObject)
                    {
                        if (this._instanceMembers == null)
                        {
                            this._instanceMembers = _instanceMembersResurrectionTable.GetValue(GetKeyForResurrectionTables(this), _ => new PSMemberInfoInternalCollection<PSMemberInfo>());
                        }
                    }
                }
                return this._instanceMembers;
            }
            set
            {
                this._instanceMembers = value;
            }
        }

        internal Adapter InternalAdapter
        {
            get
            {
                return this.InternalAdapterSet.OriginalAdapter;
            }
            set
            {
                this.InternalAdapterSet.OriginalAdapter = value;
            }
        }

        private AdapterSet InternalAdapterSet
        {
            get
            {
                if (this.adapterSet == null)
                {
                    lock (this.lockObject)
                    {
                        if (this.adapterSet == null)
                        {
                            this.adapterSet = GetMappedAdapter(this.immediateBaseObject, this.GetTypeTable());
                        }
                    }
                }
                return this.adapterSet;
            }
        }

        internal Adapter InternalBaseDotNetAdapter
        {
            get
            {
                return this.InternalAdapterSet.DotNetAdapter;
            }
        }

        internal ConsolidatedString InternalTypeNames
        {
            get
            {
                if (this._typeNames == null)
                {
                    lock (this.lockObject)
                    {
                        if ((this._typeNames == null) && !_typeNamesResurrectionTable.TryGetValue(GetKeyForResurrectionTables(this), out this._typeNames))
                        {
                            this._typeNames = this.InternalAdapter.BaseGetTypeNameHierarchy(this.immediateBaseObject);
                        }
                    }
                }
                return this._typeNames;
            }
            set
            {
                this._typeNames = value;
            }
        }

        internal bool IsHelpObject
        {
            get
            {
                return this.isHelpObject;
            }
            set
            {
                this.isHelpObject = value;
            }
        }

        public PSMemberInfoCollection<PSMemberInfo> Members
        {
            get
            {
                if (this._members == null)
                {
                    lock (this.lockObject)
                    {
                        if (this._members == null)
                        {
                            this._members = new PSMemberInfoIntegratingCollection<PSMemberInfo>(this, memberCollection);
                        }
                    }
                }
                return this._members;
            }
        }

        public PSMemberInfoCollection<PSMethodInfo> Methods
        {
            get
            {
                if (this._methods == null)
                {
                    lock (this.lockObject)
                    {
                        if (this._methods == null)
                        {
                            this._methods = new PSMemberInfoIntegratingCollection<PSMethodInfo>(this, methodCollection);
                        }
                    }
                }
                return this._methods;
            }
        }

        internal bool PreserveToString
        {
            get
            {
                if (!this.preserveToStringSet)
                {
                    this.preserveToStringSet = true;
                    if (this.InternalTypeNames.Count == 0)
                    {
                        return false;
                    }
                    this.preserveToString = false;
                }
                return this.preserveToString;
            }
        }

        public PSMemberInfoCollection<PSPropertyInfo> Properties
        {
            get
            {
                if (this._properties == null)
                {
                    lock (this.lockObject)
                    {
                        if (this._properties == null)
                        {
                            this._properties = new PSMemberInfoIntegratingCollection<PSPropertyInfo>(this, propertyCollection);
                        }
                    }
                }
                return this._properties;
            }
        }

        internal PSMemberSet PSStandardMembers
        {
            get
            {
                PSMemberSet set = null;
                set = TypeTableGetMemberDelegate<PSMemberSet>(this, "PSStandardMembers");
                if (set != null)
                {
                    set = (PSMemberSet) set.Copy();
                    set.ReplicateInstance(this);
                    return set;
                }
                return (this.InstanceMembers["PSStandardMembers"] as PSMemberSet);
            }
        }

        internal string ToStringFromDeserialization
        {
            get
            {
                return this.toStringFromDeserialization;
            }
            set
            {
                this.toStringFromDeserialization = value;
            }
        }

        public Collection<string> TypeNames
        {
            get
            {
                ConditionalWeakTable<object, ConsolidatedString>.CreateValueCallback createValueCallback = null;
                ConsolidatedString internalTypeNames = this.InternalTypeNames;
                if (internalTypeNames.IsReadOnly)
                {
                    lock (this.lockObject)
                    {
                        if (!internalTypeNames.IsReadOnly)
                        {
                            return internalTypeNames;
                        }
                        if (createValueCallback == null)
                        {
                            createValueCallback = _ => new ConsolidatedString(this._typeNames);
                        }
                        this._typeNames = _typeNamesResurrectionTable.GetValue(GetKeyForResurrectionTables(this), createValueCallback);
                        object baseObject = this.BaseObject;
                        if (baseObject != null)
                        {
                            PSVariableAssignmentBinder.NoteTypeHasInstanceMemberOrTypeName(baseObject.GetType());
                        }
                        return this._typeNames;
                    }
                }
                return internalTypeNames;
            }
        }

        internal class AdapterSet
        {
            private Adapter originalAdapter;
            private System.Management.Automation.DotNetAdapter ultimatedotNetAdapter;

            internal AdapterSet(Adapter adapter, System.Management.Automation.DotNetAdapter dotnetAdapter)
            {
                this.originalAdapter = adapter;
                this.ultimatedotNetAdapter = dotnetAdapter;
            }

            internal System.Management.Automation.DotNetAdapter DotNetAdapter
            {
                get
                {
                    return this.ultimatedotNetAdapter;
                }
            }

            internal Adapter OriginalAdapter
            {
                get
                {
                    return this.originalAdapter;
                }
                set
                {
                    this.originalAdapter = value;
                }
            }
        }

        internal class PSDynamicMetaObject : DynamicMetaObject
        {
            internal PSDynamicMetaObject(Expression expression, PSObject value) : base(expression, BindingRestrictions.Empty, value)
            {
            }

            public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
            {
                if (this.MustDeferIDMOP())
                {
                    return this.DeferForIDMOP(binder, new DynamicMetaObject[] { arg });
                }
                return binder.FallbackBinaryOperation(this.GetUnwrappedObject(), arg);
            }

            public override DynamicMetaObject BindConvert(ConvertBinder binder)
            {
                if (this.MustDeferIDMOP())
                {
                    return this.DeferForIDMOP(binder, new DynamicMetaObject[0]);
                }
                return binder.FallbackConvert(this);
            }

            public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
            {
                if (this.MustDeferIDMOP())
                {
                    return this.DeferForIDMOP(binder, indexes);
                }
                return binder.FallbackDeleteIndex(this.GetUnwrappedObject(), indexes);
            }

            public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
            {
                if (this.MustDeferIDMOP())
                {
                    return this.DeferForIDMOP(binder, new DynamicMetaObject[0]);
                }
                return binder.FallbackDeleteMember(this.GetUnwrappedObject());
            }

            public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
            {
                if (this.MustDeferIDMOP())
                {
                    return this.DeferForIDMOP(binder, indexes);
                }
                return binder.FallbackGetIndex(this.GetUnwrappedObject(), indexes);
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
            {
                if (this.MustDeferIDMOP())
                {
                    return this.DeferForIDMOP(binder, new DynamicMetaObject[0]);
                }
                return ((binder as PSGetMemberBinder) ?? PSGetMemberBinder.Get(binder.Name, false)).FallbackGetMember(this);
            }

            public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
            {
                if (this.MustDeferIDMOP())
                {
                    return this.DeferForIDMOP(binder, args);
                }
                return binder.FallbackInvoke(this.GetUnwrappedObject(), args);
            }

            public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
            {
                if (this.MustDeferIDMOP())
                {
                    return this.DeferForIDMOP(binder, args);
                }
                return ((binder as PSInvokeMemberBinder) ?? PSInvokeMemberBinder.Get(binder.Name, binder.CallInfo, false, false, null)).FallbackInvokeMember(this, args);
            }

            public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
            {
                if (this.MustDeferIDMOP())
                {
                    return this.DeferForIDMOP(binder, indexes.Append<DynamicMetaObject>(value).ToArray<DynamicMetaObject>());
                }
                return binder.FallbackSetIndex(this.GetUnwrappedObject(), indexes, value);
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
            {
                if (this.MustDeferIDMOP())
                {
                    return this.DeferForIDMOP(binder, new DynamicMetaObject[] { value });
                }
                return ((binder as PSSetMemberBinder) ?? PSSetMemberBinder.Get(binder.Name, false)).FallbackSetMember(this, value);
            }

            public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder)
            {
                if (this.MustDeferIDMOP())
                {
                    return this.DeferForIDMOP(binder, new DynamicMetaObject[0]);
                }
                return binder.FallbackUnaryOperation(this.GetUnwrappedObject());
            }

            private DynamicMetaObject DeferForIDMOP(DynamicMetaObjectBinder binder, params DynamicMetaObject[] args)
            {
                Expression[] arguments = new Expression[args.Length + 1];
                BindingRestrictions restrictions = (base.Restrictions == BindingRestrictions.Empty) ? this.PSGetTypeRestriction() : base.Restrictions;
                arguments[0] = Expression.Call(CachedReflectionInfo.PSObject_Base, base.Expression.Cast(typeof(object)));
                for (int i = 0; i < args.Length; i++)
                {
                    arguments[i + 1] = args[i].Expression;
                    restrictions = restrictions.Merge((args[i].Restrictions == BindingRestrictions.Empty) ? args[i].PSGetTypeRestriction() : args[i].Restrictions);
                }
                return new DynamicMetaObject(Expression.Dynamic(binder, binder.ReturnType, arguments), restrictions);
            }

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return (from member in this.Value.Members select member.Name);
            }

            private DynamicMetaObject GetUnwrappedObject()
            {
                return new DynamicMetaObject(Expression.Call(CachedReflectionInfo.PSObject_Base, base.Expression), base.Restrictions, PSObject.Base(this.Value));
            }

            private bool MustDeferIDMOP()
            {
                object obj2 = PSObject.Base(this.Value);
                return ((obj2 is IDynamicMetaObjectProvider) && !(obj2 is PSObject));
            }

            private PSObject Value
            {
                get
                {
                    return (PSObject) base.Value;
                }
            }
        }
    }
}

