namespace System.Data.Services
{
    using System;
    using System.Linq;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    internal sealed class MimeTypeAttribute : Attribute
    {
        private readonly string memberName;
        private readonly string mimeType;

        public MimeTypeAttribute(string memberName, string mimeType)
        {
            this.memberName = memberName;
            this.mimeType = mimeType;
        }

        internal static MimeTypeAttribute GetMimeTypeAttribute(MemberInfo member)
        {
            return member.ReflectedType.GetCustomAttributes(typeof(MimeTypeAttribute), true).Cast<MimeTypeAttribute>().FirstOrDefault<MimeTypeAttribute>(o => (o.MemberName == member.Name));
        }

        public string MemberName
        {
            get
            {
                return this.memberName;
            }
        }

        public string MimeType
        {
            get
            {
                return this.mimeType;
            }
        }
    }
}

