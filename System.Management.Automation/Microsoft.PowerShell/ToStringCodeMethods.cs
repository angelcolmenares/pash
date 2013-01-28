namespace Microsoft.PowerShell
{
    using System;
    using System.DirectoryServices;
    using System.Management.Automation;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;

    public static class ToStringCodeMethods
    {
        public static string PropertyValueCollection(PSObject instance)
        {
            if (instance == null)
            {
                return string.Empty;
            }
            System.DirectoryServices.PropertyValueCollection baseObject = (System.DirectoryServices.PropertyValueCollection) instance.BaseObject;
            if (baseObject == null)
            {
                return string.Empty;
            }
            if (baseObject.Count != 1)
            {
                return PSObject.ToStringEnumerable(null, baseObject, null, null, null);
            }
            if (baseObject[0] == null)
            {
                return string.Empty;
            }
            return PSObject.AsPSObject(baseObject[0]).ToString();
        }

        public static string Type(PSObject instance)
        {
            if (instance == null)
            {
                return string.Empty;
            }
            return Type((System.Type) instance.BaseObject, false);
        }

        internal static string Type(System.Type type, bool dropNamespaces = false)
        {
            string assemblyQualifiedName;
            Exception exception;
            if (type == null)
            {
                return string.Empty;
            }
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                string str2 = Type(type.GetGenericTypeDefinition(), dropNamespaces);
                int num = str2.LastIndexOf('`');
                int length = str2.Length - (str2.Length - num);
                StringBuilder builder = new StringBuilder(str2, 0, length, 0x200);
                builder.Append('[');
                bool flag = true;
                foreach (System.Type type2 in type.GetGenericArguments())
                {
                    if (!flag)
                    {
                        builder.Append(',');
                    }
                    flag = false;
                    builder.Append(Type(type2, dropNamespaces));
                }
                builder.Append(']');
                assemblyQualifiedName = builder.ToString();
            }
            else if (type.IsArray)
            {
                string str3 = Type(type.GetElementType(), dropNamespaces);
                StringBuilder builder2 = new StringBuilder(str3, str3.Length + 10);
                builder2.Append("[");
                for (int i = 0; i < (type.GetArrayRank() - 1); i++)
                {
                    builder2.Append(",");
                }
                builder2.Append("]");
                assemblyQualifiedName = builder2.ToString();
            }
            else
            {
                assemblyQualifiedName = TypeAccelerators.FindBuiltinAccelerator(type) ?? (dropNamespaces ? type.Name : type.ToString());
            }
            if ((!type.IsGenericParameter && !type.ContainsGenericParameters) && (!dropNamespaces && (LanguagePrimitives.ConvertStringToType(assemblyQualifiedName, out exception) != type)))
            {
                assemblyQualifiedName = type.AssemblyQualifiedName;
            }
            return assemblyQualifiedName;
        }

        public static string XmlNode(PSObject instance)
        {
            if (instance == null)
            {
                return string.Empty;
            }
            System.Xml.XmlNode baseObject = (System.Xml.XmlNode) instance.BaseObject;
            if (baseObject == null)
            {
                return string.Empty;
            }
            return baseObject.LocalName;
        }

        public static string XmlNodeList(PSObject instance)
        {
            if (instance == null)
            {
                return string.Empty;
            }
            System.Xml.XmlNodeList baseObject = (System.Xml.XmlNodeList) instance.BaseObject;
            if (baseObject == null)
            {
                return string.Empty;
            }
            if (baseObject.Count != 1)
            {
                return PSObject.ToStringEnumerable(null, baseObject, null, null, null);
            }
            if (baseObject[0] == null)
            {
                return string.Empty;
            }
            return PSObject.AsPSObject(baseObject[0]).ToString();
        }
    }
}

