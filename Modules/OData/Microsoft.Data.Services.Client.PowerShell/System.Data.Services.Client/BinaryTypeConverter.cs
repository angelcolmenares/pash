namespace System.Data.Services.Client
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal sealed class BinaryTypeConverter : PrimitiveTypeConverter
    {
        private MethodInfo convertToByteArrayMethodInfo;

        internal override object Parse(string text)
        {
            return Activator.CreateInstance(BinaryType, new object[] { Convert.FromBase64String(text) });
        }

        internal byte[] ToArray(object instance)
        {
            if (this.convertToByteArrayMethodInfo == null)
            {
                this.convertToByteArrayMethodInfo = instance.GetType().GetMethod("ToArray", BindingFlags.Public | BindingFlags.Instance);
            }
            return (byte[]) this.convertToByteArrayMethodInfo.Invoke(instance, null);
        }

        internal override string ToString(object instance)
        {
            return instance.ToString();
        }

        internal static Type BinaryType
        {
			get;set;
        }
    }
}

