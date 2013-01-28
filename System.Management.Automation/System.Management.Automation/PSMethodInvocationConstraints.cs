namespace System.Management.Automation
{
    using Microsoft.PowerShell;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;

    internal class PSMethodInvocationConstraints
    {
        private readonly Type[] parameterTypes;

        internal PSMethodInvocationConstraints(Type methodTargetType, IEnumerable<Type> parameterTypes)
        {
            this.MethodTargetType = methodTargetType;
            if (parameterTypes != null)
            {
                this.parameterTypes = parameterTypes.ToArray<Type>();
            }
        }

        public bool Equals(PSMethodInvocationConstraints other)
        {
            if (object.ReferenceEquals(null, other))
            {
                return false;
            }
            if (!object.ReferenceEquals(this, other))
            {
                if (!object.Equals(other.MethodTargetType, this.MethodTargetType))
                {
                    return false;
                }
                if (!CallsiteSignature.EqualsForCollection<Type>(this.parameterTypes, other.parameterTypes))
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj))
            {
                return false;
            }
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != typeof(PSMethodInvocationConstraints))
            {
                return false;
            }
            return this.Equals((PSMethodInvocationConstraints) obj);
        }

        public override int GetHashCode()
        {
            int num = 0x3d;
            num = (num * 0x18d) + ((this.MethodTargetType != null) ? this.MethodTargetType.GetHashCode() : 0);
            return ((num * 0x18d) + this.ParameterTypes.SequenceGetHashCode<Type>());
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            string str = "";
            if (this.MethodTargetType != null)
            {
                builder.Append("this: ");
                builder.Append(ToStringCodeMethods.Type(this.MethodTargetType, true));
                str = " ";
            }
            if (this.parameterTypes != null)
            {
                builder.Append(str);
                builder.Append("args: ");
                str = "";
                foreach (Type type in this.parameterTypes)
                {
                    builder.Append(str);
                    builder.Append(ToStringCodeMethods.Type(type, true));
                    str = ", ";
                }
            }
            if (builder.Length == 0)
            {
                builder.Append("<empty>");
            }
            return builder.ToString();
        }

        public Type MethodTargetType { get; private set; }

        public IEnumerable<Type> ParameterTypes
        {
            get
            {
                return this.parameterTypes;
            }
        }
    }
}

