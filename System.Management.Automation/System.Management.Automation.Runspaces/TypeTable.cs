namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Management.Automation.Security;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Xml;

    public sealed class TypeTable
    {
        internal const string AllowedNodesSeparator = "AllowedNodesSeparator";
        private static Node nameNode = new Node("Name", true, NodeCardinality.One, new Node[0]);
        private static Node[] codeReferenceNodes = new Node[] { new Node("TypeName", true, NodeCardinality.One, new Node[0]), new Node("MethodName", true, NodeCardinality.One, new Node[0]) };
        private static Node codeMethodNode = new Node("CodeMethod", false, NodeCardinality.ZeroToMany, new Node[] { nameNode.Clone(), new Node("CodeReference", false, NodeCardinality.One, Node.CloneNodeArray(codeReferenceNodes)) });
        private static Node codePropertyNode = new Node("CodeProperty", false, NodeCardinality.ZeroToMany, new Node[] { nameNode.Clone(), new Node("GetCodeReference", false, NodeCardinality.ZeroOrOne, Node.CloneNodeArray(codeReferenceNodes)), new Node("SetCodeReference", false, NodeCardinality.ZeroOrOne, Node.CloneNodeArray(codeReferenceNodes)) }, false);
        internal const string CodePropertyShouldHaveGetterOrSetter = "CodePropertyShouldHaveGetterOrSetter";
        private readonly Dictionary<string, PSMemberInfoInternalCollection<PSMemberInfo>> consolidatedMembers;
        private readonly Dictionary<string, Collection<string>> consolidatedSpecificProperties;
        internal const string CouldNotLoadAssembly = "CouldNotLoadAssembly";
        internal const string DefaultDisplayProperty = "DefaultDisplayProperty";
        internal const string DefaultDisplayPropertySet = "DefaultDisplayPropertySet";
        internal const bool defaultInheritPropertySerializationSet = true;
        internal const string DefaultKeyPropertySet = "DefaultKeyPropertySet";
        internal const SerializationMethod defaultSerializationMethod = SerializationMethod.AllPublicProperties;
        internal const string DuplicateMember = "DuplicateMember";
        internal const string ErrorConvertingNote = "ErrorConvertingNote";
        internal const string Exception = "Exception";
        internal const string ExpectedNodeNameInstead = "ExpectedNodeNameInstead";
        internal const string ExpectedNodeTypeInstead = "ExpectedNodeTypeInstead";
        internal const string FileLineError = "FileLineError";
        private static Node fileNode = new Node("File", true, NodeCardinality.ZeroToMany, new Node[0]);
        private static Node filesNode = new Node("Files", false, NodeCardinality.One, new Node[] { fileNode.Clone() });
        internal const string InheritPropertySerializationSet = "InheritPropertySerializationSet";
        internal const string InvalidAdaptedType = "InvalidAdaptedType";
        internal const string IsHiddenAttribute = "IsHidden";
        internal const string IsHiddenNotSupported = "IsHiddenNotSupported";
        internal const string IsHiddenValueShouldBeTrueOrFalse = "IsHiddenValueShouldBeTrueOrFalse";
        internal readonly bool isShared;
        internal const string MemberMustBePresent = "MemberMustBePresent";
        private readonly Dictionary<string, PSMemberInfoInternalCollection<PSMemberInfo>> members;
        private static Node memberSetNode = new Node("MemberSet", false, NodeCardinality.ZeroToMany, new Node[0], false);
        internal const string MemberShouldBeNote = "MemberShouldBeNote";
        internal const string MemberShouldHaveType = "MemberShouldHaveType";
        internal const string MemberShouldNotBePresent = "MemberShouldNotBePresent";
        internal const string NodeNotFoundAtLeastOnce = "NodeNotFoundAtLeastOnce";
        internal const string NodeNotFoundOnce = "NodeNotFoundOnce";
        internal const string NodeShouldHaveInnerText = "NodeShouldHaveInnerText";
        internal const string NodeShouldNotHaveInnerText = "NodeShouldNotHaveInnerText";
        internal const string NotAStandardMember = "NotAStandardMember";
        private static Node noteNode = new Node("NoteProperty", false, NodeCardinality.ZeroToMany, new Node[] { nameNode.Clone(), new Node("Value", true, NodeCardinality.One, new Node[0]), new Node("TypeName", true, NodeCardinality.ZeroOrOne, new Node[0]) }, false);
        internal const string NotMoreThanOnceOne = "NotMoreThanOnceOne";
        internal const string NotMoreThanOnceZeroOrOne = "NotMoreThanOnceZeroOrOne";
        internal const string PropertySerializationSet = "PropertySerializationSet";
        private static Node propertySetNode = new Node("PropertySet", false, NodeCardinality.ZeroToMany, new Node[] { nameNode.Clone(), new Node("ReferencedProperties", false, NodeCardinality.One, new Node[] { new Node("Name", true, NodeCardinality.OneToMany, new Node[0]) }) }, false);
        internal const string PSStandardMembers = "PSStandardMembers";
        internal const string ReservedNameMember = "ReservedNameMember";
        private static Node scriptMethodNode = new Node("ScriptMethod", false, NodeCardinality.ZeroToMany, new Node[] { nameNode.Clone(), new Node("Script", true, NodeCardinality.One, new Node[0]) });
        private static Node scriptPropertyNode = new Node("ScriptProperty", false, NodeCardinality.ZeroToMany, new Node[] { nameNode.Clone(), new Node("GetScriptBlock", true, NodeCardinality.ZeroOrOne, new Node[0]), new Node("SetScriptBlock", true, NodeCardinality.ZeroOrOne, new Node[0]) }, false);
        internal const string ScriptPropertyShouldHaveGetterOrSetter = "ScriptPropertyShouldHaveGetterOrSetter";
        internal const string SerializationDepth = "SerializationDepth";
        internal const string SerializationMethodNode = "SerializationMethod";
        internal const string SerializationSettingsIgnored = "SerializationSettingsIgnored";
        internal const string StringSerializationSource = "StringSerializationSource";
        private bool suppressValidation;
        internal const string TargetTypeForDeserialization = "TargetTypeForDeserialization";
        internal const string Type = "Type";
        internal const string TypeAdapterAlreadyPresent = "TypeAdapterAlreadyPresent";
        private static Node typeAdapterNode = new Node("TypeAdapter", false, NodeCardinality.ZeroOrOne, new Node[] { new Node("TypeName", true, NodeCardinality.One, new Node[0]) });
        private readonly Dictionary<string, PSObject.AdapterSet> typeAdapters;
        internal const string TypeConverterAlreadyPresent = "TypeConverterAlreadyPresent";
        private static Node typeConverterNode = new Node("TypeConverter", false, NodeCardinality.ZeroOrOne, new Node[] { new Node("TypeName", true, NodeCardinality.One, new Node[0]) });
        private readonly Dictionary<string, object> typeConverters;
        private List<string> typeFileList;
        internal const string TypeIsNotTypeAdapter = "TypeIsNotTypeAdapter";
        internal const string TypeIsNotTypeConverter = "TypeIsNotTypeConverter";
        internal const string TypeNodeShouldHaveMembersOrTypeConverters = "TypeNodeShouldHaveMembersOrTypeConverters";
        internal const string Types = "Types";
        internal InitialSessionStateEntryCollection<SessionStateTypeEntry> typesInfo;
        private static Node aliasNode = new Node("AliasProperty", false, NodeCardinality.ZeroToMany, new Node[] { nameNode.Clone(), new Node("ReferencedMemberName", true, NodeCardinality.One, new Node[0]), new Node("TypeName", true, NodeCardinality.ZeroOrOne, new Node[0]) }, false);
        private static Node[] membersNodeArray = new Node[] { noteNode.Clone(), aliasNode.Clone(), scriptPropertyNode.Clone(), codePropertyNode.Clone(), scriptMethodNode.Clone(), codeMethodNode.Clone(), propertySetNode.Clone(), memberSetNode.Clone() };
        private static Node typeNode = new Node("Type", false, NodeCardinality.ZeroToMany, new Node[] { nameNode.Clone(), new Node("Members", false, NodeCardinality.ZeroOrOne, Node.CloneNodeArray(membersNodeArray)), typeConverterNode.Clone(), typeAdapterNode.Clone() });
        private static Node typesNode = new Node("Types", false, NodeCardinality.One, new Node[] { typeNode.Clone() });
        internal const string TypesXml = "TypesXmlStrings";
        internal const string UnableToInstantiateTypeAdapter = "UnableToInstantiateTypeAdapter";
        internal const string UnableToInstantiateTypeConverter = "UnableToInstantiateTypeConverter";
        internal const string UnexpectedNodeType = "UnexpectedNodeType";
        internal const string UnknownNode = "UnknownNode";
        internal const string ValueShouldBeTrueOrFalse = "ValueShouldBeTrueOrFalse";

        internal TypeTable() : this(false)
        {
        }

        internal TypeTable(bool isShared)
        {
            this.consolidatedMembers = new Dictionary<string, PSMemberInfoInternalCollection<PSMemberInfo>>(0x100, StringComparer.OrdinalIgnoreCase);
            this.consolidatedSpecificProperties = new Dictionary<string, Collection<string>>(StringComparer.OrdinalIgnoreCase);
            this.members = new Dictionary<string, PSMemberInfoInternalCollection<PSMemberInfo>>(StringComparer.OrdinalIgnoreCase);
            this.typeConverters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            this.typeAdapters = new Dictionary<string, PSObject.AdapterSet>(StringComparer.OrdinalIgnoreCase);
            this.typesInfo = new InitialSessionStateEntryCollection<SessionStateTypeEntry>();
            this.isShared = isShared;
            this.typeFileList = new List<string>();
        }

        public TypeTable(IEnumerable<string> typeFiles) : this(typeFiles, null, null)
        {
        }

        internal TypeTable(IEnumerable<string> typeFiles, AuthorizationManager authorizationManager, PSHost host)
        {
            this.consolidatedMembers = new Dictionary<string, PSMemberInfoInternalCollection<PSMemberInfo>>(0x100, StringComparer.OrdinalIgnoreCase);
            this.consolidatedSpecificProperties = new Dictionary<string, Collection<string>>(StringComparer.OrdinalIgnoreCase);
            this.members = new Dictionary<string, PSMemberInfoInternalCollection<PSMemberInfo>>(StringComparer.OrdinalIgnoreCase);
            this.typeConverters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            this.typeAdapters = new Dictionary<string, PSObject.AdapterSet>(StringComparer.OrdinalIgnoreCase);
            this.typesInfo = new InitialSessionStateEntryCollection<SessionStateTypeEntry>();
            if (typeFiles == null)
            {
                throw PSTraceSource.NewArgumentNullException("typeFiles");
            }
            this.isShared = true;
            this.typeFileList = new List<string>();
            Collection<string> errors = new Collection<string>();
            foreach (string str in typeFiles)
            {
                bool flag;
                if (string.IsNullOrEmpty(str) || !Path.IsPathRooted(str))
                {
                    throw PSTraceSource.NewArgumentException("typeFile", "TypesXmlStrings", "TypeFileNotRooted", new object[] { str });
                }
                this.Initialize(string.Empty, str, errors, authorizationManager, host, out flag);
                this.typeFileList.Add(str);
            }
            this.InvalidateDynamicSites(false);
            if (errors.Count > 0)
            {
                throw new TypeTableLoadException(errors);
            }
        }

        internal void Add(string typeFile, bool shouldPrepend)
        {
            if (string.IsNullOrEmpty(typeFile) || !Path.IsPathRooted(typeFile))
            {
                throw PSTraceSource.NewArgumentException("typeFile", "TypesXmlStrings", "TypeFileNotRooted", new object[] { typeFile });
            }
            lock (this.typeFileList)
            {
                if (shouldPrepend)
                {
                    this.typeFileList.Insert(0, typeFile);
                    InitialSessionStateEntryCollection<SessionStateTypeEntry> typesInfo = this.typesInfo;
                    InitialSessionStateEntryCollection<SessionStateTypeEntry> entrys2 = new InitialSessionStateEntryCollection<SessionStateTypeEntry>();
                    entrys2.Add(new SessionStateTypeEntry(typeFile));
                    entrys2.Add(typesInfo);
                    this.typesInfo = entrys2;
                }
                else
                {
                    this.typeFileList.Add(typeFile);
                    this.typesInfo.Add(new SessionStateTypeEntry(typeFile));
                }
            }
        }

        private static void AddError(Collection<string> errors, string typeName, string resourceString, params object[] formatArguments)
        {
            string str = StringUtil.Format(resourceString, formatArguments);
            string item = StringUtil.Format(TypesXmlStrings.TypeDataTypeError, typeName, str);
            errors.Add(item);
        }

        private static void AddMember(Collection<string> errors, string typeName, PSMemberInfo member, PSMemberInfoInternalCollection<PSMemberInfo> membersCollection, bool isOverride)
        {
            if (PSMemberInfoCollection<PSMemberInfo>.IsReservedName(member.name))
            {
                AddError(errors, typeName, TypesXmlStrings.ReservedNameMember, new object[] { member.name });
            }
            else if ((membersCollection[member.name] != null) && !isOverride)
            {
                AddError(errors, typeName, TypesXmlStrings.DuplicateMember, new object[] { member.name });
            }
            else
            {
                member.isInstance = false;
                if (membersCollection[member.name] == null)
                {
                    membersCollection.Add(member);
                }
                else
                {
                    membersCollection.Replace(member);
                }
            }
        }

        private static void AddMember(System.Management.Automation.Runspaces.LoadContext context, string typeName, int memberLineNumber, PSMemberInfo member, PSMemberInfoInternalCollection<PSMemberInfo> membersCollection, Collection<int> nodeLineNumbers)
        {
            if (PSMemberInfoCollection<PSMemberInfo>.IsReservedName(member.name))
            {
                context.AddError(typeName, memberLineNumber, TypesXmlStrings.ReservedNameMember, new object[] { member.name });
            }
            else if (membersCollection[member.name] != null)
            {
                context.AddError(typeName, memberLineNumber, TypesXmlStrings.DuplicateMember, new object[] { member.name });
            }
            else
            {
                member.isInstance = false;
                membersCollection.Add(member);
                nodeLineNumbers.Add(memberLineNumber);
            }
        }

        public void AddType(TypeData typeData)
        {
            if (typeData == null)
            {
                throw PSTraceSource.NewArgumentNullException("typeData");
            }
            Collection<string> errors = new Collection<string>();
            lock (this.members)
            {
                this.consolidatedMembers.Clear();
                this.consolidatedSpecificProperties.Clear();
                this.Update(errors, typeData, false);
            }
            FormatAndTypeDataHelper.ThrowExceptionOnError("ErrorsUpdatingTypes", errors, RunspaceConfigurationCategory.Types);
        }

        private static bool CheckStandardMembers(System.Management.Automation.Runspaces.LoadContext context, Collection<string> errors, string typeName, int standardMembersLine, Collection<int> lineNumbers, PSMemberInfoInternalCollection<PSMemberInfo> members, bool useContext)
        {
            PSNoteProperty property;
            PSMemberInfo info2;
            PSMemberInfo info3;
            PSNoteProperty property5;
            PSNoteProperty property6;
            string[] strArray = new string[] { "DefaultDisplayProperty", "DefaultDisplayPropertySet", "DefaultKeyPropertySet", "SerializationMethod", "SerializationDepth", "StringSerializationSource", "PropertySerializationSet", "InheritPropertySerializationSet", "TargetTypeForDeserialization" };
            ArrayList list = new ArrayList();
            for (int i = 0; i < members.Count; i++)
            {
                bool flag = false;
                string name = members[i].Name;
                foreach (string str2 in strArray)
                {
                    if (string.Compare(name, str2, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    list.Add(name);
                    if (useContext)
                    {
                        context.AddError(typeName, lineNumbers[i], TypesXmlStrings.NotAStandardMember, new object[] { name });
                    }
                    else
                    {
                        AddError(errors, typeName, TypesXmlStrings.NotAStandardMember, new object[] { name });
                    }
                }
            }
            foreach (string str3 in list)
            {
                members.Remove(str3);
            }
            bool flag2 = GetCheckNote(context, errors, typeName, lineNumbers, members, "SerializationMethod", typeof(SerializationMethod), out property, useContext);
            if (!flag2)
            {
                goto Label_03AD;
            }
            SerializationMethod allPublicProperties = SerializationMethod.AllPublicProperties;
            if (property != null)
            {
                allPublicProperties = (SerializationMethod) property.Value;
            }
            if (allPublicProperties == SerializationMethod.String)
            {
                flag2 = EnsureNotPresent(context, errors, typeName, lineNumbers, members, "InheritPropertySerializationSet", useContext);
                if (flag2)
                {
                    flag2 = EnsureNotPresent(context, errors, typeName, lineNumbers, members, "PropertySerializationSet", useContext);
                    if (flag2)
                    {
                        flag2 = EnsureNotPresent(context, errors, typeName, lineNumbers, members, "SerializationDepth", useContext);
                        if (flag2)
                        {
                            goto Label_0389;
                        }
                    }
                }
                goto Label_03AD;
            }
            if (allPublicProperties == SerializationMethod.SpecificProperties)
            {
                PSNoteProperty property2;
                flag2 = GetCheckNote(context, errors, typeName, lineNumbers, members, "InheritPropertySerializationSet", typeof(bool), out property2, useContext);
                if (flag2)
                {
                    PSMemberInfo info;
                    flag2 = GetCheckMemberType(context, errors, typeName, lineNumbers, members, "PropertySerializationSet", typeof(PSPropertySet), out info, useContext);
                    if (flag2)
                    {
                        if (((property2 != null) && property2.Value.Equals(false)) && (info == null))
                        {
                            if (useContext)
                            {
                                context.AddError(typeName, standardMembersLine, TypesXmlStrings.MemberMustBePresent, new object[] { "PropertySerializationSet", "SerializationMethod", SerializationMethod.SpecificProperties.ToString(), "InheritPropertySerializationSet", "false" });
                            }
                            else
                            {
                                AddError(errors, typeName, TypesXmlStrings.MemberMustBePresent, new object[] { "PropertySerializationSet", "SerializationMethod", SerializationMethod.SpecificProperties.ToString(), "InheritPropertySerializationSet", "false" });
                            }
                            flag2 = false;
                        }
                        else
                        {
                            PSNoteProperty property3;
                            flag2 = GetCheckNote(context, errors, typeName, lineNumbers, members, "SerializationDepth", typeof(int), out property3, useContext);
                            if (flag2)
                            {
                                goto Label_0389;
                            }
                        }
                    }
                }
                goto Label_03AD;
            }
            if (allPublicProperties == SerializationMethod.AllPublicProperties)
            {
                PSNoteProperty property4;
                flag2 = EnsureNotPresent(context, errors, typeName, lineNumbers, members, "InheritPropertySerializationSet", useContext);
                if (!flag2)
                {
                    goto Label_03AD;
                }
                flag2 = EnsureNotPresent(context, errors, typeName, lineNumbers, members, "PropertySerializationSet", useContext);
                if (!flag2)
                {
                    goto Label_03AD;
                }
                flag2 = GetCheckNote(context, errors, typeName, lineNumbers, members, "SerializationDepth", typeof(int), out property4, useContext);
                if (!flag2)
                {
                    goto Label_03AD;
                }
            }
        Label_0389:
            flag2 = GetCheckMemberType(context, errors, typeName, lineNumbers, members, "StringSerializationSource", typeof(PSPropertyInfo), out info2, useContext);
        Label_03AD:
            if (!flag2)
            {
                if (useContext)
                {
                    context.AddError(typeName, standardMembersLine, TypesXmlStrings.SerializationSettingsIgnored, new object[0]);
                }
                else
                {
                    AddError(errors, typeName, TypesXmlStrings.SerializationSettingsIgnored, new object[0]);
                }
                members.Remove("InheritPropertySerializationSet");
                members.Remove("SerializationMethod");
                members.Remove("StringSerializationSource");
                members.Remove("PropertySerializationSet");
                members.Remove("SerializationDepth");
            }
            if (!GetCheckMemberType(context, errors, typeName, lineNumbers, members, "DefaultDisplayPropertySet", typeof(PSPropertySet), out info3, useContext))
            {
                members.Remove("DefaultDisplayPropertySet");
            }
            if (!GetCheckMemberType(context, errors, typeName, lineNumbers, members, "DefaultKeyPropertySet", typeof(PSPropertySet), out info3, useContext))
            {
                members.Remove("DefaultKeyPropertySet");
            }
            if (!GetCheckNote(context, errors, typeName, lineNumbers, members, "DefaultDisplayProperty", typeof(string), out property5, useContext))
            {
                members.Remove("DefaultDisplayProperty");
            }
            if (!GetCheckNote(context, errors, typeName, lineNumbers, members, "TargetTypeForDeserialization", typeof(System.Type), out property6, useContext))
            {
                members.Remove("TargetTypeForDeserialization");
                return flag2;
            }
            if (property6 != null)
            {
                members.Remove("TargetTypeForDeserialization");
                members.Add(property6, true);
            }
            return flag2;
        }

        internal void Clear()
        {
            lock (this.members)
            {
                this.members.Clear();
                this.typeConverters.Clear();
                this.typeAdapters.Clear();
                this.consolidatedMembers.Clear();
                this.consolidatedSpecificProperties.Clear();
                this.typesInfo.Clear();
            }
        }

        public TypeTable Clone(bool unshared)
        {
            TypeTable table = unshared ? new TypeTable() : new TypeTable(this.isShared);
            table.Update(string.Empty, this, false);
            table.typesInfo.Add(this.typesInfo);
            return table;
        }

        private static bool CreateInstance(Collection<string> errors, string typeName, System.Type type, string errorFormatString, out object instance)
        {
            instance = null;
            System.Exception exception = null;
            try
            {
                instance = Activator.CreateInstance(type);
            }
            catch (TargetInvocationException exception2)
            {
                exception = exception2.InnerException ?? exception2;
            }
            catch (System.Exception exception3)
            {
                CommandProcessorBase.CheckForSevereException(exception3);
                exception = exception3;
            }
            if (exception != null)
            {
                AddError(errors, typeName, errorFormatString, new object[] { type.FullName, exception.Message });
                return false;
            }
            return true;
        }

        private static bool CreateInstance(System.Management.Automation.Runspaces.LoadContext context, string typeName, Node typeNameNode, string errorFormatString, out object instance)
        {
            instance = null;
            System.Type type = GetTypeFromString(context, typeName, typeNameNode);
            if (type == null)
            {
                return false;
            }
            System.Exception exception = null;
            try
            {
                instance = Activator.CreateInstance(type);
            }
            catch (TargetInvocationException exception2)
            {
                exception = exception2.InnerException ?? exception2;
            }
            catch (System.Exception exception3)
            {
                CommandProcessorBase.CheckForSevereException(exception3);
                exception = exception3;
            }
            if (exception != null)
            {
                context.AddError(typeName, typeNameNode.lineNumber, errorFormatString, new object[] { typeNameNode.innerText, exception.Message });
                return false;
            }
            return true;
        }

        private static bool EnsureNotPresent(System.Management.Automation.Runspaces.LoadContext context, Collection<string> errors, string typeName, Collection<int> lineNumbers, PSMemberInfoInternalCollection<PSMemberInfo> members, string memberName, bool useContext)
        {
            for (int i = 0; i < members.Count; i++)
            {
                PSMemberInfo info = members[i];
                if (string.Compare(info.Name, memberName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (useContext)
                    {
                        context.AddError(typeName, lineNumbers[i], TypesXmlStrings.MemberShouldNotBePresent, new object[] { info.Name });
                    }
                    else
                    {
                        AddError(errors, typeName, TypesXmlStrings.MemberShouldNotBePresent, new object[] { info.Name });
                    }
                    return false;
                }
            }
            return true;
        }

        internal void ForEachTypeConverter(Action<string> action)
        {
            lock (this.members)
            {
                foreach (string str in this.typeConverters.Keys)
                {
                    action(str);
                }
            }
        }

        internal Dictionary<string, TypeData> GetAllTypeData()
        {
            lock (this.members)
            {
                Dictionary<string, TypeData> dictionary = new Dictionary<string, TypeData>();
                foreach (string str in this.members.Keys)
                {
                    if (!dictionary.ContainsKey(str))
                    {
                        TypeData typeData = new TypeData(str);
                        bool flag = false;
                        flag |= this.RetrieveMembersToTypeData(typeData);
                        flag |= this.RetrieveConverterToTypeData(typeData);
                        if (flag | this.RetrieveAdapterToTypeData(typeData))
                        {
                            dictionary.Add(str, typeData);
                        }
                    }
                }
                foreach (string str2 in this.typeConverters.Keys)
                {
                    if (!dictionary.ContainsKey(str2))
                    {
                        TypeData data2 = new TypeData(str2);
                        bool flag2 = false;
                        flag2 |= this.RetrieveConverterToTypeData(data2);
                        if (flag2 | this.RetrieveAdapterToTypeData(data2))
                        {
                            dictionary.Add(str2, data2);
                        }
                    }
                }
                foreach (string str3 in this.typeAdapters.Keys)
                {
                    if (!dictionary.ContainsKey(str3))
                    {
                        TypeData data3 = new TypeData(str3);
                        if (this.RetrieveAdapterToTypeData(data3))
                        {
                            dictionary.Add(str3, data3);
                        }
                    }
                }
                return dictionary;
            }
        }

        private static bool GetCheckMemberType(System.Management.Automation.Runspaces.LoadContext context, Collection<string> errors, string typeName, Collection<int> lineNumbers, PSMemberInfoInternalCollection<PSMemberInfo> members, string noteName, System.Type memberType, out PSMemberInfo member, bool useContext)
        {
            member = null;
            int errorLineNumber = 0;
            for (int i = 0; i < members.Count; i++)
            {
                PSMemberInfo info = members[i];
                if (string.Compare(info.Name, noteName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    member = info;
                    if (useContext)
                    {
                        errorLineNumber = lineNumbers[i];
                    }
                }
            }
            if (member == null)
            {
                return true;
            }
            if (memberType.IsAssignableFrom(member.GetType()))
            {
                return true;
            }
            if (useContext)
            {
                context.AddError(typeName, errorLineNumber, TypesXmlStrings.MemberShouldHaveType, new object[] { member.Name, memberType.Name });
            }
            else
            {
                AddError(errors, typeName, TypesXmlStrings.MemberShouldHaveType, new object[] { member.Name, memberType.Name });
            }
            member = null;
            return false;
        }

        private static bool GetCheckNote(System.Management.Automation.Runspaces.LoadContext context, Collection<string> errors, string typeName, Collection<int> lineNumbers, PSMemberInfoInternalCollection<PSMemberInfo> members, string noteName, System.Type noteType, out PSNoteProperty note, bool useContext)
        {
            note = null;
            PSMemberInfo info = null;
            int errorLineNumber = 0;
            for (int i = 0; i < members.Count; i++)
            {
                PSMemberInfo info2 = members[i];
                if (string.Compare(info2.Name, noteName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    info = info2;
                    if (useContext)
                    {
                        errorLineNumber = lineNumbers[i];
                    }
                }
            }
            if (info != null)
            {
                note = info as PSNoteProperty;
                if (note == null)
                {
                    if (useContext)
                    {
                        context.AddError(typeName, errorLineNumber, TypesXmlStrings.MemberShouldBeNote, new object[] { info.Name });
                    }
                    else
                    {
                        AddError(errors, typeName, TypesXmlStrings.MemberShouldBeNote, new object[] { info.Name });
                    }
                    return false;
                }
                object valueToConvert = note.Value;
                if (System.Type.GetTypeCode(noteType).Equals(TypeCode.Boolean))
                {
                    string strA = valueToConvert as string;
                    if (strA != null)
                    {
                        if (strA.Length == 0)
                        {
                            note.noteValue = true;
                        }
                        else
                        {
                            note.noteValue = string.Compare(strA, "false", StringComparison.OrdinalIgnoreCase) != 0;
                        }
                        return true;
                    }
                }
                try
                {
                    note.noteValue = LanguagePrimitives.ConvertTo(valueToConvert, noteType, CultureInfo.InvariantCulture);
                }
                catch (PSInvalidCastException exception)
                {
                    if (useContext)
                    {
                        context.AddError(typeName, errorLineNumber, TypesXmlStrings.ErrorConvertingNote, new object[] { note.Name, exception.Message });
                    }
                    else
                    {
                        AddError(errors, typeName, TypesXmlStrings.ErrorConvertingNote, new object[] { note.Name, exception.Message });
                    }
                    return false;
                }
            }
            return true;
        }

        public static List<string> GetDefaultTypeFiles()
        {
            string str = string.Empty;
            string str2 = string.Empty;
            string defaultPowerShellShellID = Utils.DefaultPowerShellShellID;
            string applicationBase = null;
            applicationBase = Utils.GetApplicationBase(defaultPowerShellShellID);
            if (!string.IsNullOrEmpty(applicationBase))
            {
                str = Path.Combine(applicationBase, "types.ps1xml");
                str2 = Path.Combine(applicationBase, "typesv3.ps1xml");
            }
            return new List<string> { str, str2 };
        }

        private PSMemberInfoInternalCollection<PSMemberInfo> GetMembers(ConsolidatedString types)
        {
            lock (this.members)
            {
                PSMemberInfoInternalCollection<PSMemberInfo> internals;
                if ((types == null) || string.IsNullOrEmpty(types.Key))
                {
                    return new PSMemberInfoInternalCollection<PSMemberInfo>();
                }
                if (!this.consolidatedMembers.TryGetValue(types.Key, out internals))
                {
                    internals = new PSMemberInfoInternalCollection<PSMemberInfo>();
                    for (int i = types.Count - 1; i >= 0; i--)
                    {
                        PSMemberInfoInternalCollection<PSMemberInfo> internals2;
                        if (this.members.TryGetValue(types[i], out internals2))
                        {
                            foreach (PSMemberInfo info in internals2)
                            {
                                PSMemberInfo info2 = internals[info.Name];
                                if (info2 == null)
                                {
                                    internals.Add(info.Copy());
                                }
                                else
                                {
                                    PSMemberSet set = info2 as PSMemberSet;
                                    PSMemberSet set2 = info as PSMemberSet;
                                    if (((set == null) || (set2 == null)) || !set2.InheritMembers)
                                    {
                                        internals.Remove(info.Name);
                                        internals.Add(info.Copy());
                                    }
                                    else
                                    {
                                        foreach (PSMemberInfo info3 in set2.Members)
                                        {
                                            if (set.Members[info3.Name] == null)
                                            {
                                                ((PSMemberInfoIntegratingCollection<PSMemberInfo>) set.Members).AddToTypesXmlCache(info3, false);
                                            }
                                            else
                                            {
                                                set.InternalMembers.Replace(info3);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    this.consolidatedMembers[types.Key] = internals;
                }
                return internals;
            }
        }

        internal PSMemberInfoInternalCollection<T> GetMembers<T>(ConsolidatedString types) where T: PSMemberInfo
        {
            return PSObject.TransformMemberInfoCollection<PSMemberInfo, T>(this.GetMembers(types));
        }

        private string GetModuleContents(string moduleName, string fileToLoad, Collection<string> errors, AuthorizationManager authorizationManager, PSHost host, out bool isFullyTrusted, out bool failToLoadFile)
        {
            ExternalScriptInfo info;
            string scriptContents = null;
            isFullyTrusted = false;
            try
            {
                info = new ExternalScriptInfo(fileToLoad, fileToLoad);
                scriptContents = info.ScriptContents;
                if (((PSLanguageMode) info.DefiningLanguageMode) == PSLanguageMode.FullLanguage)
                {
                    isFullyTrusted = true;
                }
            }
            catch (SecurityException exception)
            {
                string str2 = StringUtil.Format(TypesXmlStrings.Exception, exception.Message);
                string item = StringUtil.Format(TypesXmlStrings.FileError, new object[] { moduleName, fileToLoad, str2 });
                errors.Add(item);
                failToLoadFile = true;
                return null;
            }
            if (authorizationManager != null)
            {
                try
                {
                    authorizationManager.ShouldRunInternal(info, CommandOrigin.Internal, host);
                }
                catch (PSSecurityException exception2)
                {
                    string str4 = StringUtil.Format(TypesXmlStrings.ValidationException, new object[] { moduleName, fileToLoad, exception2.Message });
                    errors.Add(str4);
                    failToLoadFile = true;
                    return null;
                }
            }
            failToLoadFile = false;
            return scriptContents;
        }

        private static T GetParameterType<T>(object sourceValue)
        {
            return (T) LanguagePrimitives.ConvertTo(sourceValue, typeof(T), CultureInfo.InvariantCulture);
        }

        internal Collection<string> GetSpecificProperties(ConsolidatedString types)
        {
            lock (this.members)
            {
                Collection<string> collection;
                if ((types == null) || string.IsNullOrEmpty(types.Key))
                {
                    return new Collection<string>();
                }
                if (!this.consolidatedSpecificProperties.TryGetValue(types.Key, out collection))
                {
                    CacheTable table = new CacheTable();
                    foreach (string str in types)
                    {
                        PSMemberInfoInternalCollection<PSMemberInfo> internals;
                        if (this.members.TryGetValue(str, out internals))
                        {
                            PSMemberSet settings = internals["PSStandardMembers"] as PSMemberSet;
                            if (settings != null)
                            {
                                PSPropertySet set2 = settings.Members["PropertySerializationSet"] as PSPropertySet;
                                if (set2 != null)
                                {
                                    foreach (string str2 in set2.ReferencedPropertyNames)
                                    {
                                        if (table[str2] == null)
                                        {
                                            table.Add(str2, str2);
                                        }
                                    }
                                    if (!((bool) PSObject.GetNoteSettingValue(settings, "InheritPropertySerializationSet", true, typeof(bool), false, null)))
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    collection = new Collection<string>();
                    foreach (string str3 in table.memberCollection)
                    {
                        collection.Add(str3);
                    }
                    this.consolidatedSpecificProperties[types.Key] = collection;
                }
                return collection;
            }
        }

        internal PSObject.AdapterSet GetTypeAdapter(System.Type type)
        {
            if (type == null)
            {
                return null;
            }
            lock (this.members)
            {
                PSObject.AdapterSet set;
                this.typeAdapters.TryGetValue(type.FullName, out set);
                return set;
            }
        }

        private static bool GetTypeAndMethodName(System.Management.Automation.Runspaces.LoadContext context, string typeName, Node codeReferenceNode, out System.Type type, out string methodName)
        {
            type = null;
            methodName = null;
            Node typeNode = codeReferenceNode.possibleChildren[0].actualNodes[0];
            if (typeNode.nodeError)
            {
                return false;
            }
            Node node2 = codeReferenceNode.possibleChildren[1].actualNodes[0];
            if (node2.nodeError)
            {
                return false;
            }
            methodName = node2.innerText;
            type = GetTypeFromString(context, typeName, typeNode);
            if (type == null)
            {
                return false;
            }
            return true;
        }

        internal object GetTypeConverter(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }
            lock (this.members)
            {
                object obj2;
                this.typeConverters.TryGetValue(typeName, out obj2);
                return obj2;
            }
        }

        private static System.Type GetTypeFromString(System.Management.Automation.Runspaces.LoadContext context, string typeName, Node typeNode)
        {
            System.Type type = null;
            try
            {
                type = (System.Type) LanguagePrimitives.ConvertTo(typeNode.innerText, typeof(System.Type), CultureInfo.InvariantCulture);
            }
            catch (PSInvalidCastException exception)
            {
                context.AddError(typeName, typeNode.lineNumber, TypesXmlStrings.Exception, new object[] { exception.Message });
            }
            return type;
        }

        internal void Initialize(string snapinName, string fileToLoad, Collection<string> errors, AuthorizationManager authorizationManager, PSHost host, out bool failToLoadFile)
        {
            bool flag;
            string fileContents = this.GetModuleContents(snapinName, fileToLoad, errors, authorizationManager, host, out flag, out failToLoadFile);
            if (fileContents != null)
            {
                this.UpdateWithModuleContents(fileContents, snapinName, fileToLoad, flag, errors);
            }
        }

        private void InvalidateDynamicSites(bool clearing)
        {
            LanguagePrimitives.ResetCaches(this);
            Adapter.ResetCaches();
            foreach (KeyValuePair<string, PSMemberInfoInternalCollection<PSMemberInfo>> pair in this.members)
            {
                foreach (PSMemberInfo info in pair.Value)
                {
                    PSGetMemberBinder.SetHasTypeTableMember(info.Name, !clearing);
                }
            }
        }

        public static TypeTable LoadDefaultTypeFiles()
        {
            return new TypeTable(GetDefaultTypeFiles());
        }

        private void LoadMembersToTypeData(PSMemberInfo member, TypeData typeData)
        {
            if (member is PSNoteProperty)
            {
                PSNoteProperty property = member as PSNoteProperty;
                typeData.Members.Add(property.Name, new NotePropertyData(property.Name, property.Value));
            }
            else if (member is PSAliasProperty)
            {
                PSAliasProperty property2 = member as PSAliasProperty;
                typeData.Members.Add(property2.Name, new AliasPropertyData(property2.Name, property2.ReferencedMemberName));
            }
            else if (member is PSScriptProperty)
            {
                PSScriptProperty property3 = member as PSScriptProperty;
                ScriptBlock getScriptBlock = property3.IsGettable ? property3.GetterScript : null;
                ScriptBlock setScriptBlock = property3.IsSettable ? property3.SetterScript : null;
                typeData.Members.Add(property3.Name, new ScriptPropertyData(property3.Name, getScriptBlock, setScriptBlock));
            }
            else if (member is PSCodeProperty)
            {
                PSCodeProperty property4 = member as PSCodeProperty;
                MethodInfo getMethod = property4.IsGettable ? property4.GetterCodeReference : null;
                MethodInfo setMethod = property4.IsSettable ? property4.SetterCodeReference : null;
                typeData.Members.Add(property4.Name, new CodePropertyData(property4.Name, getMethod, setMethod));
            }
            else if (member is PSScriptMethod)
            {
                PSScriptMethod method = member as PSScriptMethod;
                typeData.Members.Add(method.Name, new ScriptMethodData(method.Name, method.Script));
            }
            else if (member is PSCodeMethod)
            {
                PSCodeMethod method2 = member as PSCodeMethod;
                typeData.Members.Add(method2.Name, new CodeMethodData(method2.Name, method2.CodeReference));
            }
            else if (member is PSMemberSet)
            {
                PSMemberSet memberSet = member as PSMemberSet;
                if (memberSet.Name.Equals("PSStandardMembers", StringComparison.OrdinalIgnoreCase))
                {
                    this.LoadStandardMembersToTypeData(memberSet, typeData);
                }
            }
        }

        private void LoadStandardMembersToTypeData(PSMemberSet memberSet, TypeData typeData)
        {
            foreach (PSMemberInfo info in memberSet.InternalMembers)
            {
                PSMemberInfo info2 = info.Copy();
                if (info2.Name.Equals("SerializationMethod", StringComparison.OrdinalIgnoreCase))
                {
                    PSNoteProperty property = info2 as PSNoteProperty;
                    typeData.SerializationMethod = GetParameterType<string>(property.Value);
                }
                else if (info2.Name.Equals("TargetTypeForDeserialization", StringComparison.OrdinalIgnoreCase))
                {
                    PSNoteProperty property2 = info2 as PSNoteProperty;
                    typeData.TargetTypeForDeserialization = GetParameterType<System.Type>(property2.Value);
                }
                else if (info2.Name.Equals("SerializationDepth", StringComparison.OrdinalIgnoreCase))
                {
                    PSNoteProperty property3 = info2 as PSNoteProperty;
                    typeData.SerializationDepth = GetParameterType<int>(property3.Value);
                }
                else if (info2.Name.Equals("DefaultDisplayProperty", StringComparison.OrdinalIgnoreCase))
                {
                    PSNoteProperty property4 = info2 as PSNoteProperty;
                    typeData.DefaultDisplayProperty = GetParameterType<string>(property4.Value);
                }
                else if (info2.Name.Equals("InheritPropertySerializationSet", StringComparison.OrdinalIgnoreCase))
                {
                    PSNoteProperty property5 = info2 as PSNoteProperty;
                    typeData.InheritPropertySerializationSet = GetParameterType<bool>(property5.Value);
                }
                else if (info2.Name.Equals("StringSerializationSource", StringComparison.OrdinalIgnoreCase))
                {
                    PSAliasProperty property6 = info2 as PSAliasProperty;
                    typeData.StringSerializationSource = property6.ReferencedMemberName;
                }
                else if (info2.Name.Equals("DefaultDisplayPropertySet", StringComparison.OrdinalIgnoreCase))
                {
                    PSPropertySet set = info2 as PSPropertySet;
                    PropertySetData data = new PropertySetData(set.ReferencedPropertyNames);
                    typeData.DefaultDisplayPropertySet = data;
                }
                else if (info2.Name.Equals("DefaultKeyPropertySet", StringComparison.OrdinalIgnoreCase))
                {
                    PSPropertySet set2 = info2 as PSPropertySet;
                    PropertySetData data2 = new PropertySetData(set2.ReferencedPropertyNames);
                    typeData.DefaultKeyPropertySet = data2;
                }
                else if (info2.Name.Equals("PropertySerializationSet", StringComparison.OrdinalIgnoreCase))
                {
                    PSPropertySet set3 = info2 as PSPropertySet;
                    PropertySetData data3 = new PropertySetData(set3.ReferencedPropertyNames);
                    typeData.PropertySerializationSet = data3;
                }
            }
        }

        private static string PossibleNodes(Node[] nodes)
        {
            StringBuilder builder = new StringBuilder();
            string allowedNodesSeparator = TypesXmlStrings.AllowedNodesSeparator;
            if (nodes.Length != 0)
            {
                foreach (Node node in nodes)
                {
                    builder.Append(node.name);
                    builder.Append(allowedNodesSeparator);
                }
                builder.Remove(builder.Length - allowedNodesSeparator.Length, allowedNodesSeparator.Length);
            }
            return builder.ToString();
        }

        private static void ProcessAlias(System.Management.Automation.Runspaces.LoadContext context, string typeName, Node node, PSMemberInfoInternalCollection<PSMemberInfo> membersCollection, Collection<int> nodeLineNumbers)
        {
            if (!node.nodeError)
            {
                Node node2 = node.possibleChildren[0].actualNodes[0];
                if (!node2.nodeError)
                {
                    Node node3 = node.possibleChildren[1].actualNodes[0];
                    if (!node3.nodeError)
                    {
                        System.Type conversionType = null;
                        Collection<Node> actualNodes = node.possibleChildren[2].actualNodes;
                        if (actualNodes.Count == 1)
                        {
                            if (actualNodes[0].nodeError)
                            {
                                return;
                            }
                            conversionType = GetTypeFromString(context, typeName, actualNodes[0]);
                            if (conversionType == null)
                            {
                                return;
                            }
                        }
                        PSAliasProperty member = new PSAliasProperty(node2.innerText, node3.innerText, conversionType) {
                            isHidden = node.isHidden.HasValue ? node.isHidden.Value : false
                        };
                        AddMember(context, typeName, node.lineNumber, member, membersCollection, nodeLineNumbers);
                    }
                }
            }
        }

        private static void ProcessAliasData(Collection<string> errors, string typeName, AliasPropertyData aliasData, PSMemberInfoInternalCollection<PSMemberInfo> membersCollection, bool isOverride)
        {
            if (string.IsNullOrEmpty(aliasData.ReferencedMemberName))
            {
                AddError(errors, typeName, TypesXmlStrings.TypeDataShouldHaveValue, new object[] { "AliasPropertyData", "ReferencedMemberName" });
            }
            else
            {
                PSAliasProperty member = new PSAliasProperty(aliasData.Name, aliasData.ReferencedMemberName, aliasData.MemberType) {
                    isHidden = aliasData.IsHidden
                };
                AddMember(errors, typeName, member, membersCollection, isOverride);
            }
        }

        private static void ProcessCodeMethod(System.Management.Automation.Runspaces.LoadContext context, string typeName, Node node, PSMemberInfoInternalCollection<PSMemberInfo> membersCollection, Collection<int> nodeLineNumbers)
        {
            if (!node.nodeError)
            {
                Node node2 = node.possibleChildren[0].actualNodes[0];
                if (!node2.nodeError)
                {
                    System.Type type;
                    string str;
                    PSCodeMethod member = new PSCodeMethod(node2.innerText);
                    Node codeReferenceNode = node.possibleChildren[1].actualNodes[0];
                    if (!codeReferenceNode.nodeError && GetTypeAndMethodName(context, typeName, codeReferenceNode, out type, out str))
                    {
                        try
                        {
                            member.SetCodeReference(type, str);
                        }
                        catch (ExtendedTypeSystemException exception)
                        {
                            context.AddError(typeName, codeReferenceNode.lineNumber, TypesXmlStrings.Exception, new object[] { exception.Message });
                            return;
                        }
                        AddMember(context, typeName, node.lineNumber, member, membersCollection, nodeLineNumbers);
                    }
                }
            }
        }

        private static void ProcessCodeMethodData(Collection<string> errors, string typeName, CodeMethodData codeMethodData, PSMemberInfoInternalCollection<PSMemberInfo> membersCollection, bool isOverride)
        {
            if (codeMethodData.CodeReference == null)
            {
                AddError(errors, typeName, TypesXmlStrings.TypeDataShouldHaveValue, new object[] { "CodeMethodData", "CodeReference" });
            }
            else
            {
                PSCodeMethod method;
                try
                {
                    method = new PSCodeMethod(codeMethodData.Name, codeMethodData.CodeReference);
                }
                catch (ExtendedTypeSystemException exception)
                {
                    AddError(errors, typeName, TypesXmlStrings.Exception, new object[] { exception.Message });
                    return;
                }
                AddMember(errors, typeName, method, membersCollection, isOverride);
            }
        }

        private static void ProcessCodeProperty(System.Management.Automation.Runspaces.LoadContext context, string typeName, Node node, PSMemberInfoInternalCollection<PSMemberInfo> membersCollection, Collection<int> nodeLineNumbers)
        {
            if (!node.nodeError)
            {
                Node node2 = node.possibleChildren[0].actualNodes[0];
                if (!node2.nodeError)
                {
                    PSCodeProperty member = new PSCodeProperty(node2.innerText);
                    Collection<Node> actualNodes = node.possibleChildren[1].actualNodes;
                    Collection<Node> collection2 = node.possibleChildren[2].actualNodes;
                    if ((actualNodes.Count == 0) && (collection2.Count == 0))
                    {
                        context.AddError(typeName, node.lineNumber, TypesXmlStrings.CodePropertyShouldHaveGetterOrSetter, new object[0]);
                    }
                    else
                    {
                        if (actualNodes.Count == 1)
                        {
                            System.Type type;
                            string str;
                            Node node3 = actualNodes[0];
                            if (!GetTypeAndMethodName(context, typeName, actualNodes[0], out type, out str))
                            {
                                return;
                            }
                            try
                            {
                                member.SetGetterFromTypeTable(type, str);
                            }
                            catch (ExtendedTypeSystemException exception)
                            {
                                context.AddError(typeName, node3.lineNumber, TypesXmlStrings.Exception, new object[] { exception.Message });
                                return;
                            }
                        }
                        if (collection2.Count == 1)
                        {
                            System.Type type2;
                            string str2;
                            Node codeReferenceNode = collection2[0];
                            if (!GetTypeAndMethodName(context, typeName, codeReferenceNode, out type2, out str2))
                            {
                                return;
                            }
                            try
                            {
                                member.SetSetterFromTypeTable(type2, str2);
                            }
                            catch (ExtendedTypeSystemException exception2)
                            {
                                context.AddError(typeName, codeReferenceNode.lineNumber, TypesXmlStrings.Exception, new object[] { exception2.Message });
                                return;
                            }
                        }
                        member.isHidden = node.isHidden.HasValue ? node.isHidden.Value : false;
                        AddMember(context, typeName, node.lineNumber, member, membersCollection, nodeLineNumbers);
                    }
                }
            }
        }

        private static void ProcessCodePropertyData(Collection<string> errors, string typeName, CodePropertyData codePropertyData, PSMemberInfoInternalCollection<PSMemberInfo> membersCollection, bool isOverride)
        {
            if ((codePropertyData.GetCodeReference == null) && (codePropertyData.SetCodeReference == null))
            {
                AddError(errors, typeName, TypesXmlStrings.CodePropertyShouldHaveGetterOrSetter, new object[0]);
            }
            else
            {
                PSCodeProperty property;
                try
                {
                    property = new PSCodeProperty(codePropertyData.Name, codePropertyData.GetCodeReference, codePropertyData.SetCodeReference);
                }
                catch (ExtendedTypeSystemException exception)
                {
                    AddError(errors, typeName, TypesXmlStrings.Exception, new object[] { exception.Message });
                    return;
                }
                property.isHidden = codePropertyData.IsHidden;
                AddMember(errors, typeName, property, membersCollection, isOverride);
            }
        }

        private static void ProcessEndElement(System.Management.Automation.Runspaces.LoadContext context, Node node)
        {
            foreach (Node node2 in node.possibleChildren)
            {
                if (node2.nodeCount == 0)
                {
                    if (node2.cardinality == NodeCardinality.One)
                    {
                        context.AddError(context.lineNumber, TypesXmlStrings.NodeNotFoundOnce, new object[] { node2.name, node.name });
                        node.nodeError = true;
                    }
                    else if (node2.cardinality == NodeCardinality.OneToMany)
                    {
                        context.AddError(context.lineNumber, TypesXmlStrings.NodeNotFoundAtLeastOnce, new object[] { node2.name, node.name });
                        node.nodeError = true;
                    }
                }
            }
            if ((node.innerText == null) && node.hasInnerText)
            {
                context.AddError(context.lineNumber, TypesXmlStrings.NodeShouldHaveInnerText, new object[] { node.name });
                node.nodeError = true;
            }
        }

        private void ProcessMembers(System.Management.Automation.Runspaces.LoadContext context, string typeName, Node node, PSMemberInfoInternalCollection<PSMemberInfo> membersCollection, Collection<int> nodeLineNumbers)
        {
            if (!node.nodeError)
            {
                foreach (Node node2 in node.possibleChildren[0].actualNodes)
                {
                    ProcessNote(context, typeName, node2, membersCollection, nodeLineNumbers);
                }
                foreach (Node node3 in node.possibleChildren[1].actualNodes)
                {
                    ProcessAlias(context, typeName, node3, membersCollection, nodeLineNumbers);
                }
                foreach (Node node4 in node.possibleChildren[2].actualNodes)
                {
                    ProcessScriptProperty(context, typeName, node4, membersCollection, nodeLineNumbers, this.suppressValidation);
                }
                foreach (Node node5 in node.possibleChildren[3].actualNodes)
                {
                    ProcessCodeProperty(context, typeName, node5, membersCollection, nodeLineNumbers);
                }
                foreach (Node node6 in node.possibleChildren[4].actualNodes)
                {
                    ProcessScriptMethod(context, typeName, node6, membersCollection, nodeLineNumbers);
                }
                foreach (Node node7 in node.possibleChildren[5].actualNodes)
                {
                    ProcessCodeMethod(context, typeName, node7, membersCollection, nodeLineNumbers);
                }
                foreach (Node node8 in node.possibleChildren[6].actualNodes)
                {
                    ProcessPropertySet(context, typeName, node8, membersCollection, nodeLineNumbers);
                }
                foreach (Node node9 in node.possibleChildren[7].actualNodes)
                {
                    this.ProcessMemberSet(context, typeName, node9, membersCollection, nodeLineNumbers);
                }
            }
        }

        private static void ProcessMembersData(Collection<string> errors, string typeName, IEnumerable<TypeMemberData> membersData, PSMemberInfoInternalCollection<PSMemberInfo> membersCollection, bool isOverride)
        {
            foreach (TypeMemberData data in membersData)
            {
                if (data is NotePropertyData)
                {
                    NotePropertyData nodeData = data as NotePropertyData;
                    ProcessNoteData(errors, typeName, nodeData, membersCollection, isOverride);
                }
                else if (data is AliasPropertyData)
                {
                    AliasPropertyData aliasData = data as AliasPropertyData;
                    ProcessAliasData(errors, typeName, aliasData, membersCollection, isOverride);
                }
                else if (data is ScriptPropertyData)
                {
                    ScriptPropertyData scriptPropertyData = data as ScriptPropertyData;
                    ProcessScriptPropertyData(errors, typeName, scriptPropertyData, membersCollection, isOverride);
                }
                else if (data is CodePropertyData)
                {
                    CodePropertyData codePropertyData = data as CodePropertyData;
                    ProcessCodePropertyData(errors, typeName, codePropertyData, membersCollection, isOverride);
                }
                else if (data is ScriptMethodData)
                {
                    ScriptMethodData scriptMethodData = data as ScriptMethodData;
                    ProcessScriptMethodData(errors, typeName, scriptMethodData, membersCollection, isOverride);
                }
                else if (data is CodeMethodData)
                {
                    CodeMethodData codeMethodData = data as CodeMethodData;
                    ProcessCodeMethodData(errors, typeName, codeMethodData, membersCollection, isOverride);
                }
            }
        }

        private void ProcessMemberSet(System.Management.Automation.Runspaces.LoadContext context, string typeName, Node node, PSMemberInfoInternalCollection<PSMemberInfo> membersCollection, Collection<int> nodeLineNumbers)
        {
            if (!node.nodeError)
            {
                Node node2 = node.possibleChildren[0].actualNodes[0];
                if (!node2.nodeError)
                {
                    bool flag = true;
                    Collection<Node> actualNodes = node.possibleChildren[1].actualNodes;
                    if (actualNodes.Count == 1)
                    {
                        if (actualNodes[0].nodeError)
                        {
                            return;
                        }
                        if (!actualNodes[0].innerText.Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!actualNodes[0].innerText.Equals("false", StringComparison.OrdinalIgnoreCase))
                            {
                                context.AddError(typeName, actualNodes[0].lineNumber, TypesXmlStrings.ValueShouldBeTrueOrFalse, new object[] { actualNodes[0].innerText });
                                return;
                            }
                            flag = false;
                        }
                        else
                        {
                            flag = true;
                        }
                    }
                    PSMemberInfoInternalCollection<PSMemberInfo> internals = new PSMemberInfoInternalCollection<PSMemberInfo>();
                    Collection<Node> collection2 = node.possibleChildren[2].actualNodes;
                    if (collection2.Count == 1)
                    {
                        if (collection2[0].nodeError)
                        {
                            return;
                        }
                        this.ProcessMembers(context, typeName, collection2[0], internals, nodeLineNumbers);
                        if (string.Compare(node2.innerText, "PSStandardMembers", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            CheckStandardMembers(context, null, typeName, node.lineNumber, nodeLineNumbers, internals, true);
                            PSMemberSet set = new PSMemberSet(node2.innerText, internals) {
                                inheritMembers = flag,
                                isHidden = true,
                                shouldSerialize = false
                            };
                            AddMember(context, typeName, node.lineNumber, set, membersCollection, nodeLineNumbers);
                            return;
                        }
                    }
                    PSMemberSet member = new PSMemberSet(node2.innerText, internals) {
                        inheritMembers = flag,
                        isHidden = node.isHidden.HasValue ? node.isHidden.Value : false
                    };
                    AddMember(context, typeName, node.lineNumber, member, membersCollection, nodeLineNumbers);
                }
            }
        }

        private static void ProcessNote(System.Management.Automation.Runspaces.LoadContext context, string typeName, Node node, PSMemberInfoInternalCollection<PSMemberInfo> membersCollection, Collection<int> nodeLineNumbers)
        {
            if (!node.nodeError)
            {
                Node node2 = node.possibleChildren[0].actualNodes[0];
                if (!node2.nodeError)
                {
                    Node node3 = node.possibleChildren[1].actualNodes[0];
                    if (!node3.nodeError)
                    {
                        object innerText = node3.innerText;
                        Collection<Node> actualNodes = node.possibleChildren[2].actualNodes;
                        if (actualNodes.Count == 1)
                        {
                            if (actualNodes[0].nodeError)
                            {
                                return;
                            }
                            System.Type resultType = GetTypeFromString(context, typeName, actualNodes[0]);
                            if (resultType == null)
                            {
                                return;
                            }
                            try
                            {
                                innerText = LanguagePrimitives.ConvertTo(innerText, resultType, CultureInfo.InvariantCulture);
                            }
                            catch (PSInvalidCastException exception)
                            {
                                context.AddError(typeName, actualNodes[0].lineNumber, TypesXmlStrings.Exception, new object[] { exception.Message });
                                return;
                            }
                        }
                        PSNoteProperty member = new PSNoteProperty(node2.innerText, innerText) {
                            isHidden = node.isHidden.HasValue ? node.isHidden.Value : false
                        };
                        AddMember(context, typeName, node.lineNumber, member, membersCollection, nodeLineNumbers);
                    }
                }
            }
        }

        private static void ProcessNoteData(Collection<string> errors, string typeName, NotePropertyData nodeData, PSMemberInfoInternalCollection<PSMemberInfo> membersCollection, bool isOverride)
        {
            PSNoteProperty member = new PSNoteProperty(nodeData.Name, nodeData.Value) {
                isHidden = nodeData.IsHidden
            };
            AddMember(errors, typeName, member, membersCollection, isOverride);
        }

        private static void ProcessPropertySet(System.Management.Automation.Runspaces.LoadContext context, string typeName, Node node, PSMemberInfoInternalCollection<PSMemberInfo> membersCollection, Collection<int> nodeLineNumbers)
        {
            if (!node.nodeError)
            {
                Node node2 = node.possibleChildren[0].actualNodes[0];
                if (!node2.nodeError)
                {
                    Node node3 = node.possibleChildren[1].actualNodes[0];
                    if (!node3.nodeError)
                    {
                        Collection<string> referencedPropertyNames = new Collection<string>();
                        foreach (Node node4 in node3.possibleChildren[0].actualNodes)
                        {
                            if (node4.nodeError)
                            {
                                return;
                            }
                            referencedPropertyNames.Add(node4.innerText);
                        }
                        PSPropertySet member = new PSPropertySet(node2.innerText, referencedPropertyNames) {
                            isHidden = node.isHidden.HasValue ? node.isHidden.Value : false
                        };
                        AddMember(context, typeName, node.lineNumber, member, membersCollection, nodeLineNumbers);
                    }
                }
            }
        }

        private static void ProcessPropertySetData(Collection<string> errors, string typeName, PropertySetData propertySetData, PSMemberInfoInternalCollection<PSMemberInfo> membersCollection, bool isOverride)
        {
            if ((propertySetData.ReferencedProperties == null) || (propertySetData.ReferencedProperties.Count == 0))
            {
                AddError(errors, typeName, TypesXmlStrings.TypeDataShouldHaveValue, new object[] { "PropertySetData", "ReferencedProperties" });
            }
            else
            {
                Collection<string> referencedPropertyNames = new Collection<string>();
                foreach (string str in propertySetData.ReferencedProperties)
                {
                    if (string.IsNullOrEmpty(str))
                    {
                        AddError(errors, typeName, TypesXmlStrings.TypeDataShouldNotBeNullOrEmpty, new object[] { "PropertySetData", "ReferencedProperties" });
                    }
                    else
                    {
                        referencedPropertyNames.Add(str);
                    }
                }
                if (referencedPropertyNames.Count != 0)
                {
                    PSPropertySet member = new PSPropertySet(propertySetData.Name, referencedPropertyNames) {
                        isHidden = false
                    };
                    AddMember(errors, typeName, member, membersCollection, isOverride);
                }
            }
        }

        private static void ProcessScriptMethod(System.Management.Automation.Runspaces.LoadContext context, string typeName, Node node, PSMemberInfoInternalCollection<PSMemberInfo> membersCollection, Collection<int> nodeLineNumbers)
        {
            if (!node.nodeError)
            {
                Node node2 = node.possibleChildren[0].actualNodes[0];
                if (!node2.nodeError)
                {
                    Node node3 = node.possibleChildren[1].actualNodes[0];
                    if (!node3.nodeError)
                    {
                        ScriptBlock script = ScriptBlock.Create(node3.innerText);
                        if ((script != null) && context.IsFullyTrusted)
                        {
                            script.LanguageMode = 0;
                        }
                        PSScriptMethod member = new PSScriptMethod(node2.innerText, script, true);
                        AddMember(context, typeName, node.lineNumber, member, membersCollection, nodeLineNumbers);
                    }
                }
            }
        }

        private static void ProcessScriptMethodData(Collection<string> errors, string typeName, ScriptMethodData scriptMethodData, PSMemberInfoInternalCollection<PSMemberInfo> membersCollection, bool isOverride)
        {
            if (scriptMethodData.Script == null)
            {
                AddError(errors, typeName, TypesXmlStrings.TypeDataShouldHaveValue, new object[] { "ScriptMethodData", "Script" });
            }
            else
            {
                PSScriptMethod member = new PSScriptMethod(scriptMethodData.Name, scriptMethodData.Script, true);
                AddMember(errors, typeName, member, membersCollection, isOverride);
            }
        }

        private static void ProcessScriptProperty(System.Management.Automation.Runspaces.LoadContext context, string typeName, Node node, PSMemberInfoInternalCollection<PSMemberInfo> membersCollection, Collection<int> nodeLineNumbers, bool preValidated)
        {
            if (!node.nodeError)
            {
                Node node2 = node.possibleChildren[0].actualNodes[0];
                if (!node2.nodeError)
                {
                    string getterScript = null;
                    Collection<Node> actualNodes = node.possibleChildren[1].actualNodes;
                    if (actualNodes.Count == 1)
                    {
                        if (actualNodes[0].nodeError)
                        {
                            return;
                        }
                        getterScript = actualNodes[0].innerText;
                    }
                    string setterScript = null;
                    Collection<Node> collection2 = node.possibleChildren[2].actualNodes;
                    if (collection2.Count == 1)
                    {
                        if (collection2[0].nodeError)
                        {
                            return;
                        }
                        setterScript = collection2[0].innerText;
                    }
                    if ((setterScript == null) && (getterScript == null))
                    {
                        context.AddError(typeName, node.lineNumber, TypesXmlStrings.ScriptPropertyShouldHaveGetterOrSetter, new object[0]);
                    }
                    else
                    {
                        PSScriptProperty member = null;
                        if (preValidated)
                        {
                            PSLanguageMode? languageMode = null;
                            if (SystemPolicy.GetSystemLockdownPolicy() == SystemEnforcementMode.Enforce)
                            {
                                if (context.IsFullyTrusted)
                                {
                                    languageMode = new PSLanguageMode?(PSLanguageMode.FullLanguage);
                                }
                            }
                            else
                            {
                                languageMode = new PSLanguageMode?(PSLanguageMode.FullLanguage);
                            }
                            member = new PSScriptProperty(node2.innerText, getterScript, setterScript, languageMode, true);
                        }
                        else
                        {
                            ScriptBlock block = (getterScript == null) ? null : ScriptBlock.Create(getterScript);
                            ScriptBlock block2 = (setterScript == null) ? null : ScriptBlock.Create(setterScript);
                            if ((block != null) && context.IsFullyTrusted)
                            {
                                block.LanguageMode = 0;
                            }
                            if ((block2 != null) && context.IsFullyTrusted)
                            {
                                block2.LanguageMode = 0;
                            }
                            member = new PSScriptProperty(node2.innerText, block, block2, true);
                        }
                        member.isHidden = node.isHidden.HasValue ? node.isHidden.Value : false;
                        AddMember(context, typeName, node.lineNumber, member, membersCollection, nodeLineNumbers);
                    }
                }
            }
        }

        private static void ProcessScriptPropertyData(Collection<string> errors, string typeName, ScriptPropertyData scriptPropertyData, PSMemberInfoInternalCollection<PSMemberInfo> membersCollection, bool isOverride)
        {
            ScriptBlock getScriptBlock = scriptPropertyData.GetScriptBlock;
            ScriptBlock setScriptBlock = scriptPropertyData.SetScriptBlock;
            if ((setScriptBlock == null) && (getScriptBlock == null))
            {
                AddError(errors, typeName, TypesXmlStrings.ScriptPropertyShouldHaveGetterOrSetter, new object[0]);
            }
            else
            {
                PSScriptProperty member = new PSScriptProperty(scriptPropertyData.Name, getScriptBlock, setScriptBlock, true) {
                    isHidden = scriptPropertyData.IsHidden
                };
                AddMember(errors, typeName, member, membersCollection, isOverride);
            }
        }

        private static void ProcessStandardMembers(Collection<string> errors, string typeName, IEnumerable<TypeMemberData> standardMembers, IEnumerable<PropertySetData> propertySets, PSMemberInfoInternalCollection<PSMemberInfo> membersCollection, bool isOverride)
        {
            if (membersCollection["PSStandardMembers"] == null)
            {
                PSMemberInfoInternalCollection<PSMemberInfo> internals = new PSMemberInfoInternalCollection<PSMemberInfo>();
                ProcessMembersData(errors, typeName, standardMembers, internals, false);
                foreach (PropertySetData data in propertySets)
                {
                    ProcessPropertySetData(errors, typeName, data, internals, false);
                }
                CheckStandardMembers(new System.Management.Automation.Runspaces.LoadContext(null, null, null), errors, typeName, 0, null, internals, false);
                PSMemberSet member = new PSMemberSet("PSStandardMembers", internals) {
                    inheritMembers = true,
                    isHidden = true,
                    shouldSerialize = false
                };
                AddMember(errors, typeName, member, membersCollection, false);
            }
            else
            {
                PSMemberSet set2 = membersCollection["PSStandardMembers"] as PSMemberSet;
                PSMemberInfoInternalCollection<PSMemberInfo> internals2 = new PSMemberInfoInternalCollection<PSMemberInfo>();
                PSMemberInfoInternalCollection<PSMemberInfo> members = new PSMemberInfoInternalCollection<PSMemberInfo>();
                foreach (PSMemberInfo info in set2.InternalMembers)
                {
                    internals2.Add(info.Copy());
                    members.Add(info.Copy());
                }
                ProcessMembersData(errors, typeName, standardMembers, internals2, isOverride);
                foreach (PropertySetData data2 in propertySets)
                {
                    ProcessPropertySetData(errors, typeName, data2, internals2, isOverride);
                }
                if (CheckStandardMembers(new System.Management.Automation.Runspaces.LoadContext(null, null, null), errors, typeName, 0, null, internals2, false))
                {
                    PSMemberSet set3 = new PSMemberSet("PSStandardMembers", internals2) {
                        inheritMembers = true,
                        isHidden = true,
                        shouldSerialize = false
                    };
                    AddMember(errors, typeName, set3, membersCollection, true);
                }
                else
                {
                    foreach (PSMemberInfo info2 in internals2)
                    {
                        if (members[info2.name] == null)
                        {
                            members.Add(info2);
                        }
                    }
                    PSMemberSet set4 = new PSMemberSet("PSStandardMembers", members) {
                        inheritMembers = true,
                        isHidden = true,
                        shouldSerialize = false
                    };
                    AddMember(errors, typeName, set4, membersCollection, true);
                }
            }
        }

        private static Node ProcessStartElement(System.Management.Automation.Runspaces.LoadContext context, Node node)
        {
            foreach (Node node2 in node.possibleChildren)
            {
                if (node2.name.Equals(context.reader.LocalName))
                {
                    if (node2.nodeCount == 1)
                    {
                        if (node2.cardinality == NodeCardinality.ZeroOrOne)
                        {
                            context.AddError(context.lineNumber, TypesXmlStrings.NotMoreThanOnceZeroOrOne, new object[] { node2.name, node.name });
                            node.nodeError = true;
                        }
                        if (node2.cardinality == NodeCardinality.One)
                        {
                            context.AddError(context.lineNumber, TypesXmlStrings.NotMoreThanOnceOne, new object[] { node2.name, node.name });
                            node.nodeError = true;
                        }
                    }
                    node2.nodeCount++;
                    Node item = node2.Clone();
                    item.lineNumber = context.lineNumber;
                    node2.actualNodes.Add(item);
                    if (context.reader.HasAttributes)
                    {
                        string attribute = context.reader.GetAttribute("IsHidden");
                        if (!item.isHidden.HasValue && (attribute != null))
                        {
                            context.AddError(context.lineNumber, TypesXmlStrings.IsHiddenNotSupported, new object[] { item.name, "IsHidden" });
                            node.nodeError = true;
                            return item;
                        }
                        if (attribute == null)
                        {
                            return item;
                        }
                        if (attribute.Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            item.isHidden = true;
                            return item;
                        }
                        if (attribute.Equals("false", StringComparison.OrdinalIgnoreCase))
                        {
                            item.isHidden = false;
                            return item;
                        }
                        context.AddError(context.lineNumber, TypesXmlStrings.IsHiddenValueShouldBeTrueOrFalse, new object[] { attribute, "IsHidden" });
                        node.nodeError = true;
                    }
                    return item;
                }
            }
            context.AddError(context.lineNumber, TypesXmlStrings.UnknownNode, new object[] { context.reader.LocalName, PossibleNodes(node.possibleChildren), node.name });
            return null;
        }

        private void ProcessTypeAdapter(System.Management.Automation.Runspaces.LoadContext context, string typeName, Node node)
        {
            if (!node.nodeError)
            {
                object obj2;
                if (this.typeAdapters.ContainsKey(typeName))
                {
                    context.AddError(typeName, node.lineNumber, TypesXmlStrings.TypeAdapterAlreadyPresent, new object[0]);
                }
                Node typeNameNode = node.possibleChildren[0].actualNodes[0];
                if (!typeNameNode.nodeError && CreateInstance(context, typeName, typeNameNode, TypesXmlStrings.UnableToInstantiateTypeAdapter, out obj2))
                {
                    PSPropertyAdapter adapter = obj2 as PSPropertyAdapter;
                    if (adapter == null)
                    {
                        context.AddError(typeName, node.lineNumber, TypesXmlStrings.TypeIsNotTypeAdapter, new object[] { typeNameNode.innerText });
                    }
                    else
                    {
                        System.Type type;
                        if (!LanguagePrimitives.TryConvertTo<System.Type>(typeName, out type))
                        {
                            context.AddError(typeName, node.lineNumber, TypesXmlStrings.InvalidAdaptedType, new object[] { typeName });
                        }
                        else
                        {
                            this.typeAdapters[typeName] = PSObject.CreateThirdPartyAdapterSet(type, adapter);
                        }
                    }
                }
            }
        }

        private void ProcessTypeConverter(System.Management.Automation.Runspaces.LoadContext context, string typeName, Node node)
        {
            if (!node.nodeError)
            {
                object obj2;
                if (this.typeConverters.ContainsKey(typeName))
                {
                    context.AddError(typeName, node.lineNumber, TypesXmlStrings.TypeConverterAlreadyPresent, new object[0]);
                }
                Node typeNameNode = node.possibleChildren[0].actualNodes[0];
                if (!typeNameNode.nodeError && CreateInstance(context, typeName, typeNameNode, TypesXmlStrings.UnableToInstantiateTypeConverter, out obj2))
                {
                    if ((obj2 is TypeConverter) || (obj2 is PSTypeConverter))
                    {
                        this.typeConverters[typeName] = obj2;
                    }
                    else
                    {
                        context.AddError(typeName, node.lineNumber, TypesXmlStrings.TypeIsNotTypeConverter, new object[] { typeNameNode.innerText });
                    }
                }
            }
        }

        private void ProcessTypeDataToAdd(Collection<string> errors, TypeData typeData)
        {
            string typeName = typeData.TypeName;
            Collection<PropertySetData> propertySets = new Collection<PropertySetData>();
            if (typeData.DefaultDisplayPropertySet != null)
            {
                propertySets.Add(typeData.DefaultDisplayPropertySet);
            }
            if (typeData.DefaultKeyPropertySet != null)
            {
                propertySets.Add(typeData.DefaultKeyPropertySet);
            }
            if (typeData.PropertySerializationSet != null)
            {
                propertySets.Add(typeData.PropertySerializationSet);
            }
            if ((((typeData.Members.Count == 0) && (typeData.StandardMembers.Count == 0)) && ((typeData.TypeConverter == null) && (typeData.TypeAdapter == null))) && (propertySets.Count == 0))
            {
                AddError(errors, typeName, TypesXmlStrings.TypeDataShouldNotBeEmpty, new object[0]);
            }
            else
            {
                if (typeData.Members.Count > 0)
                {
                    PSMemberInfoInternalCollection<PSMemberInfo> internals;
                    if (!this.members.TryGetValue(typeName, out internals))
                    {
                        internals = new PSMemberInfoInternalCollection<PSMemberInfo>();
                        this.members[typeName] = internals;
                    }
                    ProcessMembersData(errors, typeName, typeData.Members.Values, internals, typeData.IsOverride);
                }
                if ((typeData.StandardMembers.Count > 0) || (propertySets.Count > 0))
                {
                    PSMemberInfoInternalCollection<PSMemberInfo> internals2;
                    if (!this.members.TryGetValue(typeName, out internals2))
                    {
                        internals2 = new PSMemberInfoInternalCollection<PSMemberInfo>();
                        this.members[typeName] = internals2;
                    }
                    ProcessStandardMembers(errors, typeName, typeData.StandardMembers.Values, propertySets, internals2, typeData.IsOverride);
                }
                if (typeData.TypeConverter != null)
                {
                    object obj2;
                    if (this.typeConverters.ContainsKey(typeName) && !typeData.IsOverride)
                    {
                        AddError(errors, typeName, TypesXmlStrings.TypeConverterAlreadyPresent, new object[0]);
                    }
                    if (CreateInstance(errors, typeName, typeData.TypeConverter, TypesXmlStrings.UnableToInstantiateTypeConverter, out obj2))
                    {
                        if ((obj2 is TypeConverter) || (obj2 is PSTypeConverter))
                        {
                            this.typeConverters[typeName] = obj2;
                        }
                        else
                        {
                            AddError(errors, typeName, TypesXmlStrings.TypeIsNotTypeConverter, new object[] { typeData.TypeConverter.FullName });
                        }
                    }
                }
                if (typeData.TypeAdapter != null)
                {
                    object obj3;
                    if (this.typeAdapters.ContainsKey(typeName) && !typeData.IsOverride)
                    {
                        AddError(errors, typeName, TypesXmlStrings.TypeAdapterAlreadyPresent, new object[0]);
                    }
                    if (CreateInstance(errors, typeName, typeData.TypeAdapter, TypesXmlStrings.UnableToInstantiateTypeAdapter, out obj3))
                    {
                        PSPropertyAdapter adapter = obj3 as PSPropertyAdapter;
                        if (adapter == null)
                        {
                            AddError(errors, typeName, TypesXmlStrings.TypeIsNotTypeAdapter, new object[] { typeData.TypeAdapter.FullName });
                        }
                        else
                        {
                            System.Type type;
                            if (LanguagePrimitives.TryConvertTo<System.Type>(typeName, out type))
                            {
                                this.typeAdapters[typeName] = PSObject.CreateThirdPartyAdapterSet(type, adapter);
                            }
                            else
                            {
                                AddError(errors, typeName, TypesXmlStrings.InvalidAdaptedType, new object[] { typeName });
                            }
                        }
                    }
                }
                this.typesInfo.Add(new SessionStateTypeEntry(typeData, false));
            }
        }

        private void ProcessTypeDataToRemove(Collection<string> errors, TypeData typeData)
        {
            string typeName = typeData.TypeName;
            bool flag = false;
            if (this.members.ContainsKey(typeName))
            {
                flag = true;
                this.members.Remove(typeName);
            }
            if (this.typeConverters.ContainsKey(typeName))
            {
                flag = true;
                this.typeConverters.Remove(typeName);
            }
            if (this.typeAdapters.ContainsKey(typeName))
            {
                flag = true;
                this.typeAdapters.Remove(typeName);
            }
            if (!flag)
            {
                AddError(errors, typeName, TypesXmlStrings.TypeNotFound, new object[] { typeName });
            }
            else
            {
                this.typesInfo.Add(new SessionStateTypeEntry(typeData, true));
            }
        }

        private void ProcessTypeNode(System.Management.Automation.Runspaces.LoadContext context, Node node)
        {
            if (!node.nodeError)
            {
                Node node2 = node.possibleChildren[0].actualNodes[0];
                if (!node2.nodeError)
                {
                    string innerText = node2.innerText;
                    Collection<Node> actualNodes = node.possibleChildren[1].actualNodes;
                    Collection<Node> collection2 = node.possibleChildren[2].actualNodes;
                    Collection<Node> collection3 = node.possibleChildren[3].actualNodes;
                    if (((actualNodes.Count == 0) && (collection2.Count == 0)) && (collection3.Count == 0))
                    {
                        context.AddError(innerText, node.lineNumber, TypesXmlStrings.TypeNodeShouldHaveMembersOrTypeConverters, new object[0]);
                    }
                    else
                    {
                        if (actualNodes.Count == 1)
                        {
                            PSMemberInfoInternalCollection<PSMemberInfo> internals;
                            if (!this.members.TryGetValue(innerText, out internals))
                            {
                                internals = new PSMemberInfoInternalCollection<PSMemberInfo>();
                                this.members[innerText] = internals;
                            }
                            Collection<int> nodeLineNumbers = new Collection<int>();
                            this.ProcessMembers(context, innerText, actualNodes[0], internals, nodeLineNumbers);
                        }
                        if (collection2.Count == 1)
                        {
                            this.ProcessTypeConverter(context, innerText, collection2[0]);
                        }
                        if (collection3.Count == 1)
                        {
                            this.ProcessTypeAdapter(context, innerText, collection3[0]);
                        }
                    }
                }
            }
        }

        private bool ReadDocument(System.Management.Automation.Runspaces.LoadContext context, ref Node rootNode)
        {
            Node node = new Node("Document", false, NodeCardinality.One, new Node[] { rootNode.Clone() });
            rootNode.cardinality = NodeCardinality.One;
            try
            {
                ReadNode(context, node);
            }
            catch (XmlException exception)
            {
                context.AddError(TypesXmlStrings.Exception, new object[] { exception.Message });
                return false;
            }
            ProcessEndElement(context, node);
            if (node.nodeError || rootNode.nodeError)
            {
                return false;
            }
            rootNode = node.possibleChildren[0].actualNodes[0];
            return true;
        }

        internal Collection<string> ReadFiles(string PSSnapinName, string xmlFileListFileName, Collection<string> errors, AuthorizationManager authorizationManager, PSHost host, out bool failToLoadFile)
        {
            ExternalScriptInfo info;
            string scriptContents;
            Collection<string> collection = new Collection<string>();
            System.Management.Automation.Runspaces.LoadContext context = new System.Management.Automation.Runspaces.LoadContext(PSSnapinName, xmlFileListFileName, errors);
            try
            {
                info = new ExternalScriptInfo(xmlFileListFileName, xmlFileListFileName);
                scriptContents = info.ScriptContents;
                if (((PSLanguageMode) info.DefiningLanguageMode) == PSLanguageMode.FullLanguage)
                {
                    context.IsFullyTrusted = true;
                }
            }
            catch (SecurityException exception)
            {
                context.AddError(TypesXmlStrings.Exception, new object[] { exception.Message });
                failToLoadFile = true;
                return collection;
            }
            if (authorizationManager != null)
            {
                try
                {
                    authorizationManager.ShouldRunInternal(info, CommandOrigin.Internal, host);
                }
                catch (PSSecurityException exception2)
                {
                    string item = StringUtil.Format(TypesXmlStrings.ValidationException, new object[] { PSSnapinName, xmlFileListFileName, exception2.Message });
                    errors.Add(item);
                    failToLoadFile = true;
                    return collection;
                }
            }
            Node rootNode = filesNode.Clone();
            using (StringReader reader = new StringReader(scriptContents))
            {
                XmlTextReader reader2 = new XmlTextReader(reader);
                context.reader = reader2;
                reader2.WhitespaceHandling = WhitespaceHandling.Significant;
                if (!this.ReadDocument(context, ref rootNode))
                {
                    goto Label_0164;
                }
                reader2.Close();
            }
            foreach (Node node2 in rootNode.possibleChildren[0].actualNodes)
            {
                if (!node2.nodeError)
                {
                    collection.Add(node2.innerText);
                }
            }
        Label_0164:
            failToLoadFile = false;
            return collection;
        }

        private static void ReadNode(System.Management.Automation.Runspaces.LoadContext context, Node node)
        {
            if (node.name.Equals("MemberSet"))
            {
                node.possibleChildren = new Node[] { nameNode.Clone(), new Node("InheritMembers", true, NodeCardinality.ZeroOrOne, new Node[0]), new Node("Members", false, NodeCardinality.ZeroOrOne, Node.CloneNodeArray(membersNodeArray)) };
            }
            while (context.Read())
            {
                if (context.reader.NodeType != XmlNodeType.Comment)
                {
                    if (context.reader.IsEmptyElement)
                    {
                        Node node2 = ProcessStartElement(context, node);
                        if (node2 != null)
                        {
                            ProcessEndElement(context, node2);
                        }
                    }
                    else
                    {
                        if (context.reader.IsStartElement())
                        {
                            Node node3 = ProcessStartElement(context, node);
                            if (node3 == null)
                            {
                                string localName = context.reader.LocalName;
                                SkipUntillNodeEnd(context, localName);
                            }
                            else
                            {
                                ReadNode(context, node3);
                            }
                            continue;
                        }
                        if (context.reader.NodeType == XmlNodeType.EndElement)
                        {
                            ProcessEndElement(context, node);
                            return;
                        }
                        if (context.reader.NodeType == XmlNodeType.Text)
                        {
                            if (node.hasInnerText)
                            {
                                node.innerText = context.reader.Value.Trim();
                            }
                            else
                            {
                                context.AddError(context.lineNumber, TypesXmlStrings.NodeShouldNotHaveInnerText, new object[] { node.name });
                                node.nodeError = true;
                            }
                            continue;
                        }
                        context.AddError(context.lineNumber, TypesXmlStrings.UnexpectedNodeType, new object[] { context.reader.NodeType.ToString() });
                    }
                }
            }
        }

        internal void Remove(string typeFile)
        {
            lock (this.typeFileList)
            {
                this.typeFileList.Remove(typeFile);
                this.typesInfo.Remove(typeFile, null);
            }
        }

        public void RemoveType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw PSTraceSource.NewArgumentNullException("typeName");
            }
            TypeData typeData = new TypeData(typeName);
            Collection<string> errors = new Collection<string>();
            lock (this.members)
            {
                this.consolidatedMembers.Clear();
                this.consolidatedSpecificProperties.Clear();
                this.Update(errors, typeData, true);
            }
            FormatAndTypeDataHelper.ThrowExceptionOnError("ErrorsUpdatingTypes", errors, RunspaceConfigurationCategory.Types);
        }

        private bool RetrieveAdapterToTypeData(TypeData typeData)
        {
            PSObject.AdapterSet set;
            string typeName = typeData.TypeName;
            if (this.typeAdapters.TryGetValue(typeName, out set))
            {
                ThirdPartyAdapter originalAdapter = set.OriginalAdapter as ThirdPartyAdapter;
                typeData.TypeAdapter = originalAdapter.ExternalAdapterType;
                return true;
            }
            return false;
        }

        private bool RetrieveConverterToTypeData(TypeData typeData)
        {
            object obj2;
            string typeName = typeData.TypeName;
            if (this.typeConverters.TryGetValue(typeName, out obj2))
            {
                typeData.TypeConverter = obj2.GetType();
                return true;
            }
            return false;
        }

        private bool RetrieveMembersToTypeData(TypeData typeData)
        {
            PSMemberInfoInternalCollection<PSMemberInfo> internals;
            string typeName = typeData.TypeName;
            if (!this.members.TryGetValue(typeName, out internals))
            {
                return false;
            }
            foreach (PSMemberInfo info in internals)
            {
                PSMemberInfo member = info.Copy();
                this.LoadMembersToTypeData(member, typeData);
            }
            return true;
        }

        private static void SkipUntillNodeEnd(System.Management.Automation.Runspaces.LoadContext context, string nodeName)
        {
            while (context.Read())
            {
                if (context.reader.IsStartElement() && context.reader.LocalName.Equals(nodeName))
                {
                    SkipUntillNodeEnd(context, nodeName);
                }
                else if ((context.reader.NodeType == XmlNodeType.EndElement) && context.reader.LocalName.Equals(nodeName))
                {
                    return;
                }
            }
        }

        private void Update(System.Management.Automation.Runspaces.LoadContext context)
        {
            Node rootNode = typesNode.Clone();
            if (this.ReadDocument(context, ref rootNode))
            {
                foreach (Node node2 in rootNode.possibleChildren[0].actualNodes)
                {
                    this.ProcessTypeNode(context, node2);
                }
                this.InvalidateDynamicSites(false);
            }
        }

        internal void Update(Collection<TypeData> typeDatas, bool isRemove)
        {
            if (typeDatas == null)
            {
                throw new ArgumentNullException("typeDatas");
            }
            if (this.isShared)
            {
                throw PSTraceSource.NewInvalidOperationException("TypesXmlStrings", "SharedTypeTableCannotBeUpdated", new object[0]);
            }
            Collection<string> errors = new Collection<string>();
            lock (this.members)
            {
                if (isRemove)
                {
                    this.InvalidateDynamicSites(true);
                }
                this.consolidatedMembers.Clear();
                this.consolidatedSpecificProperties.Clear();
                foreach (TypeData data in typeDatas)
                {
                    this.Update(errors, data, isRemove);
                }
                this.InvalidateDynamicSites(false);
            }
            FormatAndTypeDataHelper.ThrowExceptionOnError("ErrorsUpdatingTypes", errors, RunspaceConfigurationCategory.Types);
        }

        private void Update(Collection<string> errors, TypeData typeData, bool isRemove)
        {
            if (!isRemove)
            {
                this.ProcessTypeDataToAdd(errors, typeData);
                this.InvalidateDynamicSites(false);
            }
            else
            {
                this.InvalidateDynamicSites(true);
                this.ProcessTypeDataToRemove(errors, typeData);
                this.InvalidateDynamicSites(false);
            }
        }

        internal void Update(string moduleName, TypeTable typeTable, bool clearTable)
        {
            if (typeTable == null)
            {
                throw PSTraceSource.NewArgumentNullException("typeTable");
            }
            lock (this.members)
            {
                if (clearTable)
                {
                    this.InvalidateDynamicSites(true);
                    this.members.Clear();
                    this.typeConverters.Clear();
                    this.typeAdapters.Clear();
                    this.consolidatedMembers.Clear();
                    this.consolidatedSpecificProperties.Clear();
                    this.typesInfo.Clear();
                }
                foreach (string str in typeTable.members.Keys)
                {
                    PSMemberInfoInternalCollection<PSMemberInfo> internals = typeTable.members[str];
                    PSMemberInfoInternalCollection<PSMemberInfo> internals2 = new PSMemberInfoInternalCollection<PSMemberInfo>();
                    foreach (PSMemberInfo info in internals)
                    {
                        internals2.Add(info.Copy());
                    }
                    this.members.Add(str, internals2);
                }
                foreach (string str2 in typeTable.typeAdapters.Keys)
                {
                    this.typeAdapters.Add(str2, typeTable.typeAdapters[str2]);
                }
                foreach (string str3 in typeTable.typeConverters.Keys)
                {
                    this.typeConverters.Add(str3, typeTable.typeConverters[str3]);
                }
                this.InvalidateDynamicSites(false);
            }
        }

        internal void Update(Collection<PSSnapInTypeAndFormatErrors> psSnapinTypes, AuthorizationManager authorizationManager, PSHost host, bool preValidated)
        {
            if (this.isShared)
            {
                throw PSTraceSource.NewInvalidOperationException("TypesXmlStrings", "SharedTypeTableCannotBeUpdated", new object[0]);
            }
            lock (this.members)
            {
                this.InvalidateDynamicSites(true);
                this.members.Clear();
                this.typeConverters.Clear();
                this.typeAdapters.Clear();
                this.consolidatedMembers.Clear();
                this.consolidatedSpecificProperties.Clear();
                this.typesInfo.Clear();
                bool suppressValidation = this.suppressValidation;
                try
                {
                    this.suppressValidation = preValidated;
                    foreach (PSSnapInTypeAndFormatErrors errors in psSnapinTypes)
                    {
                        if (errors.FullPath != null)
                        {
                            this.Initialize(errors.PSSnapinName, errors.FullPath, errors.Errors, authorizationManager, host, out errors.FailToLoadFile);
                        }
                        else
                        {
                            this.Update(errors.Errors, errors.TypeData, errors.IsRemove);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var str = ex.Message;
                }
                finally
                {
                    this.suppressValidation = suppressValidation;
                }
                this.InvalidateDynamicSites(false);
            }
        }

        internal void Update(TypeData type, Collection<string> errors, bool isRemove, bool clearTable)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (errors == null)
            {
                throw new ArgumentNullException("errors");
            }
            if (this.isShared)
            {
                throw PSTraceSource.NewInvalidOperationException("TypesXmlStrings", "SharedTypeTableCannotBeUpdated", new object[0]);
            }
            lock (this.members)
            {
                if (isRemove || clearTable)
                {
                    this.InvalidateDynamicSites(true);
                }
                if (clearTable)
                {
                    this.members.Clear();
                    this.typeConverters.Clear();
                    this.typeAdapters.Clear();
                    this.typesInfo.Clear();
                }
                this.consolidatedMembers.Clear();
                this.consolidatedSpecificProperties.Clear();
                this.Update(errors, type, isRemove);
                this.InvalidateDynamicSites(false);
            }
        }

        internal void Update(string filePath, Collection<string> errors, bool clearTable, AuthorizationManager authorizationManager, PSHost host, out bool failToLoadFile)
        {
            this.Update(filePath, filePath, errors, clearTable, authorizationManager, host, false, out failToLoadFile);
        }

        internal void Update(string moduleName, string filePath, Collection<string> errors, bool clearTable, AuthorizationManager authorizationManager, PSHost host, bool preValidated, out bool failToLoadFile)
        {
            string str;
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }
            if (errors == null)
            {
                throw new ArgumentNullException("errors");
            }
            if (this.isShared)
            {
                throw PSTraceSource.NewInvalidOperationException("TypesXmlStrings", "SharedTypeTableCannotBeUpdated", new object[0]);
            }
            lock (this.members)
            {
                if (clearTable)
                {
                    this.InvalidateDynamicSites(true);
                    this.members.Clear();
                    this.typeConverters.Clear();
                    this.typeAdapters.Clear();
                    this.typesInfo.Clear();
                }
                this.consolidatedMembers.Clear();
                this.consolidatedSpecificProperties.Clear();
            }
            bool suppressValidation = this.suppressValidation;
            bool isFullyTrusted = false;
            try
            {
                this.suppressValidation = preValidated;
                str = this.GetModuleContents(moduleName, filePath, errors, authorizationManager, host, out isFullyTrusted, out failToLoadFile);
                if (str == null)
                {
                    return;
                }
            }
            finally
            {
                this.suppressValidation = suppressValidation;
            }
            lock (this.members)
            {
                suppressValidation = this.suppressValidation;
                try
                {
                    this.suppressValidation = preValidated;
                    this.UpdateWithModuleContents(str, moduleName, filePath, isFullyTrusted, errors);
                }
                finally
                {
                    this.suppressValidation = suppressValidation;
                }
                this.InvalidateDynamicSites(false);
            }
        }

        private void UpdateWithModuleContents(string fileContents, string moduleName, string fileToLoad, bool isFullyTrusted, Collection<string> errors)
        {
            this.typesInfo.Add(new SessionStateTypeEntry(fileToLoad));
            System.Management.Automation.Runspaces.LoadContext context = new System.Management.Automation.Runspaces.LoadContext(moduleName, fileToLoad, errors) {
                IsFullyTrusted = isFullyTrusted
            };
            using (StringReader reader = new StringReader(fileContents))
            {
                XmlTextReader reader2 = new XmlTextReader(reader);
                context.reader = reader2;
                reader2.WhitespaceHandling = WhitespaceHandling.Significant;
                this.Update(context);
                reader2.Close();
            }
        }
    }
}

