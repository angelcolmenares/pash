namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Management.Automation.Internal;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class CustomInternalSerializer
    {
        private bool _notypeinformation;
        private XmlWriter _writer;
        private bool firstcall;
        private bool firstobjectcall = true;
        private bool isStopping;

        internal CustomInternalSerializer(XmlWriter writer, bool notypeinformation, bool isfirstcallforObject)
        {
            this._writer = writer;
            this._notypeinformation = notypeinformation;
            this.firstcall = isfirstcallforObject;
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

        private static int GetDepthOfSerialization(PSObject source, int depth)
        {
            if (source == null)
            {
                return depth;
            }
            int serializationDepth = source.GetSerializationDepth(null);
            if (serializationDepth <= 0)
            {
                return depth;
            }
            return serializationDepth;
        }

        private void GetKnownContainerTypeInfo(object source, out ContainerType ct, out IDictionary dictionary, out IEnumerable enumerable)
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
                    enumerable = LanguagePrimitives.GetEnumerable(source);
                    if (enumerable != null)
                    {
                        ct = ContainerType.Enumerable;
                    }
                }
            }
        }

        private string GetStringFromPSObject(PSObject source)
        {
            PSPropertyInfo stringSerializationSource = source.GetStringSerializationSource(null);
            string str = null;
            if (stringSerializationSource != null)
            {
                object obj2 = stringSerializationSource.Value;
                if (obj2 != null)
                {
                    try
                    {
                        str = obj2.ToString();
                    }
                    catch (Exception exception)
                    {
                        CommandProcessorBase.CheckForSevereException(exception);
                    }
                }
                return str;
            }
            try
            {
                str = source.ToString();
            }
            catch (Exception exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
            }
            return str;
        }

        private void HandleComplexTypePSObject(PSObject source, string property, int depth)
        {
            this.WriteStartOfPSObject(source, property, true);
            bool flag = false;
            bool flag2 = false;
            if (!source.immediateBaseObjectIsEmpty)
            {
                flag = source.ImmediateBaseObject is Enum;
                flag2 = source.ImmediateBaseObject is PSObject;
            }
            if (flag)
            {
                object immediateBaseObject = source.ImmediateBaseObject;
                foreach (PSPropertyInfo info in source.Properties)
                {
                    this.WriteOneObject(Convert.ChangeType(immediateBaseObject, Enum.GetUnderlyingType(immediateBaseObject.GetType()), CultureInfo.InvariantCulture), info.Name, depth);
                }
            }
            else if (flag2)
            {
                if (this.firstobjectcall)
                {
                    this.firstobjectcall = false;
                    this.WritePSObjectProperties(source, depth);
                }
                else
                {
                    this.WriteOneObject(source.ImmediateBaseObject, null, depth);
                }
            }
            else
            {
                this.WritePSObjectProperties(source, depth);
            }
            this._writer.WriteEndElement();
        }

        private bool HandleKnownContainerTypes(object source, string property, int depth)
        {
            ContainerType none = ContainerType.None;
            PSObject obj2 = source as PSObject;
            IEnumerable enumerable = null;
            IDictionary dictionary = null;
            if ((obj2 != null) && obj2.immediateBaseObjectIsEmpty)
            {
                return false;
            }
            this.GetKnownContainerTypeInfo((obj2 != null) ? obj2.ImmediateBaseObject : source, out none, out dictionary, out enumerable);
            if (none == ContainerType.None)
            {
                return false;
            }
            this.WriteStartOfPSObject((obj2 != null) ? obj2 : PSObject.AsPSObject(source), property, true);
            switch (none)
            {
                case ContainerType.Dictionary:
                    this.WriteDictionary(dictionary, depth);
                    break;

                case ContainerType.Queue:
                case ContainerType.Stack:
                case ContainerType.List:
                case ContainerType.Enumerable:
                    this.WriteEnumerable(enumerable, depth);
                    break;
            }
            if ((depth != 0) && ((none == ContainerType.Enumerable) || ((obj2 != null) && obj2.isDeserialized)))
            {
                this.WritePSObjectProperties(PSObject.AsPSObject(source), depth);
            }
            if (obj2 != null)
            {
                PSMemberInfoCollection<PSMemberInfo> instanceMembers = obj2.InstanceMembers;
                if (instanceMembers != null)
                {
                    this.WriteMemberInfoCollection(instanceMembers, depth, true);
                }
            }
            this._writer.WriteEndElement();
            return true;
        }

        private bool HandlePrimitiveKnownType(object source, string property)
        {
            TypeSerializationInfo typeSerializationInfo = KnownTypes.GetTypeSerializationInfo(source.GetType());
            if (typeSerializationInfo != null)
            {
                this.WriteOnePrimitiveKnownType(this._writer, property, source, typeSerializationInfo);
                return true;
            }
            return false;
        }

        private bool HandlePrimitiveKnownTypePSObject(object source, string property, int depth)
        {
            bool flag = false;
            PSObject obj2 = source as PSObject;
            if ((obj2 != null) && !obj2.immediateBaseObjectIsEmpty)
            {
                object immediateBaseObject = obj2.ImmediateBaseObject;
                TypeSerializationInfo typeSerializationInfo = KnownTypes.GetTypeSerializationInfo(immediateBaseObject.GetType());
                if (typeSerializationInfo != null)
                {
                    this.WriteOnePrimitiveKnownType(this._writer, property, immediateBaseObject, typeSerializationInfo);
                    flag = true;
                }
            }
            return flag;
        }

        private void HandlePSObjectAsString(PSObject source, string property, int depth)
        {
            bool flag = this.PSObjectHasNotes(source);
            string stringFromPSObject = this.GetStringFromPSObject(source);
            if (stringFromPSObject != null)
            {
                TypeSerializationInfo typeSerializationInfo = KnownTypes.GetTypeSerializationInfo(stringFromPSObject.GetType());
                if (flag)
                {
                    this.WritePrimitiveTypePSObjectWithNotes(source, stringFromPSObject, typeSerializationInfo, property, depth);
                }
                else
                {
                    this.WriteOnePrimitiveKnownType(this._writer, property, source.BaseObject, typeSerializationInfo);
                }
            }
            else if (flag)
            {
                this.WritePrimitiveTypePSObjectWithNotes(source, null, null, property, depth);
            }
            else
            {
                this.WriteNull(property);
            }
        }

        private bool PSObjectHasNotes(PSObject source)
        {
            return ((source.InstanceMembers != null) && (source.InstanceMembers.Count > 0));
        }

        private static bool SerializeAsString(PSObject source)
        {
            return (source.GetSerializationMethod(null) == SerializationMethod.String);
        }

        private void SerializeProperties(PSMemberInfoInternalCollection<PSPropertyInfo> propertyCollection, string name, int depth)
        {
            if (propertyCollection.Count != 0)
            {
                foreach (PSMemberInfo info in propertyCollection)
                {
                    PSPropertyInfo info2 = info as PSPropertyInfo;
                    object source = AutomationNull.Value;
                    try
                    {
                        source = info2.Value;
                    }
                    catch (GetValueException)
                    {
                        continue;
                    }
                    this.WriteOneObject(source, info2.Name, depth);
                }
            }
        }

        internal void Stop()
        {
            this.isStopping = true;
        }

        internal static void WriteAttribute(XmlWriter writer, string name, string value)
        {
            writer.WriteAttributeString(name, value);
        }

        private void WriteDictionary(IDictionary dictionary, int depth)
        {
            IDictionaryEnumerator enumerator = null;
            try
            {
                enumerator = dictionary.GetEnumerator();
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
            }
            if (enumerator != null)
            {
                while (enumerator.MoveNext())
                {
                    this.WriteOneObject(enumerator.Key, "Key", depth);
                    this.WriteOneObject(enumerator.Value, "Value", depth);
                }
            }
        }

        private void WriteEnumerable(IEnumerable enumerable, int depth)
        {
            IEnumerator enumerator = null;
            try
            {
                enumerable.GetEnumerator().Reset();
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
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
                        break;
                    }
                    this.WriteOneObject(source, null, depth);
                }
            }
        }

        private void WriteMemberInfoCollection(PSMemberInfoCollection<PSMemberInfo> me, int depth, bool writeEnclosingMemberSetElementTag)
        {
            bool flag = false;
            foreach (PSMemberInfo info in me)
            {
                if (info.ShouldSerialize)
                {
                    PSPropertyInfo info2 = info as PSPropertyInfo;
                    if (info2 != null)
                    {
                        flag = true;
                        WriteStartElement(this._writer, "Property");
                        WriteAttribute(this._writer, "Name", info.Name);
                        if (!this._notypeinformation)
                        {
                            WriteAttribute(this._writer, "Type", info.GetType().ToString());
                        }
                        this._writer.WriteString(info2.Value.ToString());
                    }
                }
            }
            if (flag)
            {
                this._writer.WriteEndElement();
            }
        }

        private void WriteNull(string property)
        {
            if (property != null)
            {
                WriteStartElement(this._writer, "Property");
                WriteAttribute(this._writer, "Name", property);
            }
            else if (this.firstcall)
            {
                WriteStartElement(this._writer, "Object");
                this.firstcall = false;
            }
            else
            {
                WriteStartElement(this._writer, "Property");
            }
            this._writer.WriteEndElement();
        }

        private void WriteObjectString(XmlWriter writer, string property, object source, TypeSerializationInfo entry)
        {
            if (property != null)
            {
                WriteStartElement(writer, "Property");
                WriteAttribute(writer, "Name", property.ToString());
            }
            else if (this.firstcall)
            {
                WriteStartElement(writer, "Object");
                this.firstcall = false;
            }
            else
            {
                WriteStartElement(writer, "Property");
            }
            if (!this._notypeinformation)
            {
                WriteAttribute(writer, "Type", source.GetType().ToString());
            }
            writer.WriteString(source.ToString());
            writer.WriteEndElement();
        }

        internal void WriteOneObject(object source, string property, int depth)
        {
            this.CheckIfStopping();
            if (source == null)
            {
                this.WriteNull(property);
            }
            else if ((!this.HandlePrimitiveKnownType(source, property) && !this.HandlePrimitiveKnownTypePSObject(source, property, depth)) && !this.HandleKnownContainerTypes(source, property, depth))
            {
                PSObject obj2 = PSObject.AsPSObject(source);
                if ((depth == 0) || SerializeAsString(obj2))
                {
                    this.HandlePSObjectAsString(obj2, property, depth);
                }
                else
                {
                    this.HandleComplexTypePSObject(obj2, property, depth);
                }
            }
        }

        private void WriteOnePrimitiveKnownType(XmlWriter writer, string property, object source, TypeSerializationInfo entry)
        {
            this.WriteObjectString(writer, property, source, entry);
        }

        private void WritePrimitiveTypePSObjectWithNotes(PSObject source, object primitive, TypeSerializationInfo pktInfo, string property, int depth)
        {
            this.WriteStartOfPSObject(source, property, source.ToStringFromDeserialization != null);
            if (pktInfo != null)
            {
                this.WriteOnePrimitiveKnownType(this._writer, null, primitive, pktInfo);
            }
            PSMemberInfoCollection<PSMemberInfo> instanceMembers = source.InstanceMembers;
            if (instanceMembers != null)
            {
                this.WriteMemberInfoCollection(instanceMembers, depth, true);
            }
            this._writer.WriteEndElement();
        }

        private void WritePropertyWithNullValue(XmlWriter writer, PSPropertyInfo source, int depth)
        {
            WriteStartElement(writer, "Property");
            WriteAttribute(writer, "Name", source.Name.ToString());
            if (!this._notypeinformation)
            {
                WriteAttribute(writer, "Type", source.TypeNameOfValue.ToString());
            }
            writer.WriteEndElement();
        }

        private void WritePSObjectProperties(PSObject source, int depth)
        {
            depth = GetDepthOfSerialization(source, depth);
            depth--;
            if (source.GetSerializationMethod(null) == SerializationMethod.SpecificProperties)
            {
                PSMemberInfoInternalCollection<PSPropertyInfo> propertyCollection = new PSMemberInfoInternalCollection<PSPropertyInfo>();
                foreach (string str in source.GetSpecificPropertiesToSerialize(null))
                {
                    PSPropertyInfo member = source.Properties[str];
                    if (member != null)
                    {
                        propertyCollection.Add(member);
                    }
                }
                this.SerializeProperties(propertyCollection, "Property", depth);
            }
            else
            {
                foreach (PSPropertyInfo info2 in source.Properties)
                {
                    object obj2 = AutomationNull.Value;
                    try
                    {
                        obj2 = info2.Value;
                    }
                    catch (GetValueException)
                    {
                        this.WritePropertyWithNullValue(this._writer, info2, depth);
                        continue;
                    }
                    if (obj2 == null)
                    {
                        this.WritePropertyWithNullValue(this._writer, info2, depth);
                    }
                    else
                    {
                        this.WriteOneObject(obj2, info2.Name, depth);
                    }
                }
            }
        }

        internal static void WriteStartElement(XmlWriter writer, string elementTag)
        {
            writer.WriteStartElement(elementTag);
        }

        private void WriteStartOfPSObject(PSObject mshObject, string property, bool writeTNH)
        {
            if (property != null)
            {
                WriteStartElement(this._writer, "Property");
                WriteAttribute(this._writer, "Name", property.ToString());
            }
            else if (this.firstcall)
            {
                WriteStartElement(this._writer, "Object");
                this.firstcall = false;
            }
            else
            {
                WriteStartElement(this._writer, "Property");
            }
            object baseObject = mshObject.BaseObject;
            if (!this._notypeinformation)
            {
                WriteAttribute(this._writer, "Type", baseObject.GetType().ToString());
            }
        }
    }
}

