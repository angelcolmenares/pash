namespace System.Management.Automation.Remoting
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security;

    internal class RemoteHostEncoder
    {
        private static bool ArrayIsZeroBased(Array array)
        {
            int rank = array.Rank;
            for (int i = 0; i < rank; i++)
            {
                if (array.GetLowerBound(i) != 0)
                {
                    return false;
                }
            }
            return true;
        }

        private static Array DecodeArray(PSObject psObject, Type type)
        {
            Type elementType = type.GetElementType();
            ArrayList list = SafelyGetBaseObject<ArrayList>(SafelyGetPropertyValue<PSObject>(psObject, "mae"));
            int[] lengths = (int[]) SafelyGetBaseObject<ArrayList>(SafelyGetPropertyValue<PSObject>(psObject, "mal")).ToArray(typeof(int));
            Indexer indexer = new Indexer(lengths);
            Array array = Array.CreateInstance(elementType, lengths);
            int num = 0;
            foreach (int[] numArray2 in indexer)
            {
                object obj4 = DecodeObject(list[num++], elementType);
                array.SetValue(obj4, numArray2);
            }
            return array;
        }

        private static object DecodeClassOrStruct(PSObject psObject, Type type)
        {
            object uninitializedObject = FormatterServices.GetUninitializedObject(type);
            foreach (PSPropertyInfo info in psObject.Properties)
            {
                FieldInfo field = type.GetField(info.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (info.Value == null)
                {
                    throw RemoteHostExceptions.NewDecodingFailedException();
                }
                object obj3 = DecodeObject(info.Value, field.FieldType);
                if (obj3 == null)
                {
                    throw RemoteHostExceptions.NewDecodingFailedException();
                }
                field.SetValue(uninitializedObject, obj3);
            }
            return uninitializedObject;
        }

        private static IList DecodeCollection(PSObject psObject, Type collectionType)
        {
            Type type = collectionType.GetGenericArguments()[0];
            ArrayList list = SafelyGetBaseObject<ArrayList>(psObject);
            IList list2 = (IList) Activator.CreateInstance(collectionType);
            foreach (object obj2 in list)
            {
                list2.Add(DecodeObject(obj2, type));
            }
            return list2;
        }

        private static IDictionary DecodeDictionary(PSObject psObject, Type dictionaryType)
        {
            if (IsObjectDictionaryType(dictionaryType))
            {
                return DecodeObjectDictionary(psObject, dictionaryType);
            }
            Type[] genericArguments = dictionaryType.GetGenericArguments();
            Type type = genericArguments[0];
            Type type2 = genericArguments[1];
            Hashtable hashtable = SafelyGetBaseObject<Hashtable>(psObject);
            IDictionary dictionary = (IDictionary) Activator.CreateInstance(dictionaryType);
            foreach (object obj2 in hashtable.Keys)
            {
                dictionary.Add(DecodeObject(obj2, type), DecodeObject(hashtable[obj2], type2));
            }
            return dictionary;
        }

        private static Exception DecodeException(PSObject psObject)
        {
            ErrorRecord record = ErrorRecord.FromPSObjectForRemoting(psObject);
            if (record == null)
            {
                throw RemoteHostExceptions.NewDecodingErrorForErrorRecordException();
            }
            return record.Exception;
        }

        internal static object DecodeObject(object obj, Type type)
        {
            if (obj == null)
            {
                return obj;
            }
            if (type == typeof(PSObject))
            {
                return DecodePSObject(obj);
			}
			if (type == typeof(System.Management.Automation.Runspaces.Runspace))
			{
				return RemoteRunspace.FromPSObjectForRemoting(PSObject.AsPSObject(obj));
			}
            if (type == typeof(ProgressRecord))
            {
                return ProgressRecord.FromPSObjectForRemoting(PSObject.AsPSObject(obj));
            }
            if (IsKnownType(type))
            {
                return obj;
            }
            if (obj is SecureString)
            {
                return obj;
            }
            if (obj is PSCredential)
            {
                return obj;
            }
            if ((obj is PSObject) && (type == typeof(PSCredential)))
            {
                PSObject obj2 = (PSObject) obj;
                try
                {
                    return new PSCredential((string) obj2.Properties["UserName"].Value, (SecureString) obj2.Properties["Password"].Value);
                }
                catch (GetValueException)
                {
                    return null;
                }
            }
            if ((obj is int) && type.IsEnum)
            {
                return Enum.ToObject(type, (int) obj);
            }
            if ((obj is string) && (type == typeof(CultureInfo)))
            {
                return new CultureInfo((string) obj);
            }
            if ((obj is PSObject) && (type == typeof(Exception)))
            {
                return DecodeException((PSObject) obj);
            }
            if ((obj is PSObject) && (type == typeof(object[])))
            {
                return DecodeObjectArray((PSObject) obj);
            }
            if ((obj is PSObject) && type.IsArray)
            {
                return DecodeArray((PSObject) obj, type);
            }
            if ((obj is PSObject) && IsCollection(type))
            {
                return DecodeCollection((PSObject) obj, type);
            }
            if ((obj is PSObject) && IsDictionary(type))
            {
                return DecodeDictionary((PSObject) obj, type);
            }
            if ((obj is PSObject) && IsEncodingAllowedForClassOrStruct(type))
            {
                return DecodeClassOrStruct((PSObject) obj, type);
            }
            if ((obj is PSObject) && IsGenericIEnumerableOfInt(type))
            {
                return DecodeCollection((PSObject) obj, typeof(Collection<int>));
            }
            if ((obj is PSObject) && (type == typeof(RemoteHostCall)))
            {
                return RemoteHostCall.Decode((PSObject) obj);
            }
            if (!(obj is PSObject) || (type != typeof(RemoteHostResponse)))
            {
                throw RemoteHostExceptions.NewRemoteHostDataDecodingNotSupportedException(type);
            }
            return RemoteHostResponse.Decode((PSObject) obj);
        }

        private static object[] DecodeObjectArray(PSObject psObject)
        {
            ArrayList list = SafelyGetBaseObject<ArrayList>(psObject);
            object[] objArray = new object[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                objArray[i] = DecodeObjectWithType(list[i]);
            }
            return objArray;
        }

        private static IDictionary DecodeObjectDictionary(PSObject psObject, Type dictionaryType)
        {
            Type[] genericArguments = dictionaryType.GetGenericArguments();
            Type type = genericArguments[0];
            Type type1 = genericArguments[1];
            Hashtable hashtable = SafelyGetBaseObject<Hashtable>(psObject);
            IDictionary dictionary = (IDictionary) Activator.CreateInstance(dictionaryType);
            foreach (object obj2 in hashtable.Keys)
            {
                dictionary.Add(DecodeObject(obj2, type), DecodeObjectWithType(hashtable[obj2]));
            }
            return dictionary;
        }

        private static object DecodeObjectWithType(object obj)
        {
            if (obj == null)
            {
                return null;
            }
            PSObject psObject = SafelyCastObject<PSObject>(obj);
            Type type = Type.GetType(SafelyGetPropertyValue<string>(psObject, "T"));
            return DecodeObject(SafelyGetPropertyValue<object>(psObject, "V"), type);
        }

        internal static object DecodePropertyValue(PSObject psObject, string propertyName, Type propertyValueType)
        {
            ReadOnlyPSMemberInfoCollection<PSPropertyInfo> infos = psObject.Properties.Match(propertyName);
            if (infos.Count == 0)
            {
                return null;
            }
            return DecodeObject(infos[0].Value, propertyValueType);
        }

        private static PSObject DecodePSObject(object obj)
        {
            if (obj is PSObject)
            {
                return (PSObject) obj;
            }
            return new PSObject(obj);
        }

        internal static void EncodeAndAddAsProperty(PSObject psObject, string propertyName, object propertyValue)
        {
            if (propertyValue != null)
            {
                psObject.Properties.Add(new PSNoteProperty(propertyName, EncodeObject(propertyValue)));
            }
        }

        private static PSObject EncodeArray(Array array)
        {
            array.GetType().GetElementType();
            int rank = array.Rank;
            int[] lengths = new int[rank];
            for (int i = 0; i < rank; i++)
            {
                lengths[i] = array.GetUpperBound(i) + 1;
            }
            Indexer indexer = new Indexer(lengths);
            ArrayList list = new ArrayList();
            foreach (int[] numArray2 in indexer)
            {
                object obj2 = array.GetValue(numArray2);
                list.Add(EncodeObject(obj2));
            }
            PSObject obj3 = RemotingEncoder.CreateEmptyPSObject();
            obj3.Properties.Add(new PSNoteProperty("mae", list));
            obj3.Properties.Add(new PSNoteProperty("mal", lengths));
            return obj3;
        }

        private static PSObject EncodeClassOrStruct(object obj)
        {
            PSObject obj2 = RemotingEncoder.CreateEmptyPSObject();
            foreach (FieldInfo info in obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                object obj3 = info.GetValue(obj);
                if (obj3 != null)
                {
                    object obj4 = EncodeObject(obj3);
                    obj2.Properties.Add(new PSNoteProperty(info.Name, obj4));
                }
            }
            return obj2;
        }

        private static PSObject EncodeCollection(IList collection)
        {
            ArrayList list = new ArrayList();
            foreach (object obj2 in collection)
            {
                list.Add(EncodeObject(obj2));
            }
            return new PSObject(list);
        }

        private static PSObject EncodeDictionary(IDictionary dictionary)
        {
            if (IsObjectDictionaryType(dictionary.GetType()))
            {
                return EncodeObjectDictionary(dictionary);
            }
            Hashtable hashtable = new Hashtable();
            foreach (object obj2 in dictionary.Keys)
            {
                hashtable.Add(EncodeObject(obj2), EncodeObject(dictionary[obj2]));
            }
            return new PSObject(hashtable);
        }

        private static PSObject EncodeException(Exception exception)
        {
            ErrorRecord record = null;
            IContainsErrorRecord record2 = exception as IContainsErrorRecord;
            if (record2 == null)
            {
                record = new ErrorRecord(exception, "RemoteHostExecutionException", ErrorCategory.NotSpecified, null);
            }
            else
            {
                record = new ErrorRecord(record2.ErrorRecord, exception);
            }
            PSObject dest = RemotingEncoder.CreateEmptyPSObject();
            record.ToPSObjectForRemoting(dest);
            return dest;
        }

        internal static object EncodeObject (object obj)
		{
			if (obj == null) {
				return null;
			}
			Type type = obj.GetType ();
			if (obj is PSObject) {
				return EncodePSObject ((PSObject)obj);
			}
			if (obj is RemoteRunspace) {
				return EncodePSObject (((RemoteRunspace)obj).ToPSObjectForRemoting ());
			}
            if (obj is ProgressRecord)
            {
                return ((ProgressRecord) obj).ToPSObjectForRemoting();
            }
            if (IsKnownType(type))
            {
                return obj;
            }
            if (type.IsEnum)
            {
                return (int) obj;
            }
            if (obj is CultureInfo)
            {
                return obj.ToString();
            }
            if (obj is Exception)
            {
                return EncodeException((Exception) obj);
            }
            if (type == typeof(object[]))
            {
                return EncodeObjectArray((object[]) obj);
            }
            if (type.IsArray)
            {
                return EncodeArray((Array) obj);
            }
            if (!(obj is IList) || !IsCollection(type))
            {
                if ((obj is IDictionary) && IsDictionary(type))
                {
                    return EncodeDictionary((IDictionary) obj);
                }
                if (type.IsSubclassOf(typeof(FieldDescription)) || (type == typeof(FieldDescription)))
                {
                    return EncodeClassOrStruct(UpcastFieldDescriptionSubclassAndDropAttributes((FieldDescription) obj));
                }
                if (IsEncodingAllowedForClassOrStruct(type))
                {
                    return EncodeClassOrStruct(obj);
                }
                if (obj is RemoteHostCall)
                {
                    return ((RemoteHostCall) obj).Encode();
                }
                if (obj is RemoteHostResponse)
                {
                    return ((RemoteHostResponse) obj).Encode();
                }
                if (obj is SecureString)
                {
                    return obj;
                }
                if (obj is PSCredential)
                {
                    return obj;
                }
                if (!IsGenericIEnumerableOfInt(type))
                {
                    throw RemoteHostExceptions.NewRemoteHostDataEncodingNotSupportedException(type);
                }
            }
            return EncodeCollection((IList) obj);
        }

        private static PSObject EncodeObjectArray(object[] objects)
        {
            ArrayList list = new ArrayList();
            foreach (object obj2 in objects)
            {
                list.Add(EncodeObjectWithType(obj2));
            }
            return new PSObject(list);
        }

        private static PSObject EncodeObjectDictionary(IDictionary dictionary)
        {
            Hashtable hashtable = new Hashtable();
            foreach (object obj2 in dictionary.Keys)
            {
                hashtable.Add(EncodeObject(obj2), EncodeObjectWithType(dictionary[obj2]));
            }
            return new PSObject(hashtable);
        }

        private static PSObject EncodeObjectWithType(object obj)
        {
            if (obj == null)
            {
                return null;
            }
            PSObject obj2 = RemotingEncoder.CreateEmptyPSObject();
            obj2.Properties.Add(new PSNoteProperty("T", obj.GetType().ToString()));
            obj2.Properties.Add(new PSNoteProperty("V", EncodeObject(obj)));
            return obj2;
        }

        private static PSObject EncodePSObject(PSObject psObject)
        {
            return psObject;
        }

        private static bool IsCollection(Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Collection<>)));
        }

        private static bool IsDictionary(Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Dictionary<,>)));
        }

        private static bool IsEncodingAllowedForClassOrStruct(Type type)
        {
            if ((((!(type == typeof(KeyInfo)) && !(type == typeof(Coordinates))) && (!(type == typeof(Size)) && !(type == typeof(KeyInfo)))) && ((!(type == typeof(BufferCell)) && !(type == typeof(Rectangle))) && (!(type == typeof(ProgressRecord)) && !(type == typeof(FieldDescription))))) && ((!(type == typeof(ChoiceDescription)) && !(type == typeof(HostInfo))) && !(type == typeof(HostDefaultData))))
            {
                return (type == typeof(RemoteSessionCapability));
            }
            return true;
        }

        private static bool IsGenericIEnumerableOfInt(Type type)
        {
            return type.Equals(typeof(IEnumerable<int>));
        }

        private static bool IsKnownType(Type type)
        {
            return (KnownTypes.GetTypeSerializationInfo(type) != null);
        }

        private static bool IsObjectDictionaryType(Type dictionaryType)
        {
            if (!IsDictionary(dictionaryType))
            {
                return false;
            }
            Type[] genericArguments = dictionaryType.GetGenericArguments();
            if (genericArguments.Length != 2)
            {
                return false;
            }
            Type type = genericArguments[1];
            return (type == typeof(object));
        }

        private static T SafelyCastObject<T>(object obj)
        {
            if (!(obj is T))
            {
                throw RemoteHostExceptions.NewDecodingFailedException();
            }
            return (T) obj;
        }

        private static T SafelyGetBaseObject<T>(PSObject psObject)
        {
            if (((psObject == null) || (psObject.BaseObject == null)) || !(psObject.BaseObject is T))
            {
                throw RemoteHostExceptions.NewDecodingFailedException();
            }
            return (T) psObject.BaseObject;
        }

        private static T SafelyGetPropertyValue<T>(PSObject psObject, string key)
        {
            PSPropertyInfo info = psObject.Properties[key];
            if (((info == null) || (info.Value == null)) || !(info.Value is T))
            {
                throw RemoteHostExceptions.NewDecodingFailedException();
            }
            return (T) info.Value;
        }

        private static FieldDescription UpcastFieldDescriptionSubclassAndDropAttributes(FieldDescription fieldDescription1)
        {
            FieldDescription description = new FieldDescription(fieldDescription1.Name) {
                Label = fieldDescription1.Label,
                HelpMessage = fieldDescription1.HelpMessage,
                IsMandatory = fieldDescription1.IsMandatory,
                DefaultValue = fieldDescription1.DefaultValue
            };
            description.SetParameterTypeName(fieldDescription1.ParameterTypeName);
            description.SetParameterTypeFullName(fieldDescription1.ParameterTypeFullName);
            description.SetParameterAssemblyFullName(fieldDescription1.ParameterAssemblyFullName);
            return description;
        }
    }
}

