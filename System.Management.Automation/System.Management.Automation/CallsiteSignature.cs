namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class CallsiteSignature : IEquatable<CallsiteSignature>
    {
        private Type[] argumentTypes;
        private Type[] effectiveArgumentTypes;
        private CallsiteCacheEntryFlags flags;
        private PSMethodInvocationConstraints invocationConstraints;
        private Type targetType;

        internal CallsiteSignature(Type targetType, PSMethodInvocationConstraints invocationConstraints, object[] arguments, CallsiteCacheEntryFlags flags)
        {
            this.targetType = targetType;
            this.invocationConstraints = invocationConstraints;
            this.flags = flags;
            this.argumentTypes = new Type[arguments.Length];
            this.effectiveArgumentTypes = new Type[arguments.Length];
            for (int i = 0; i < arguments.Length; i++)
            {
                this.argumentTypes[i] = (arguments[i] == null) ? typeof(LanguagePrimitives.Null) : arguments[i].GetType();
                this.effectiveArgumentTypes[i] = Adapter.EffectiveArgumentType(arguments[i]);
            }
        }

        public bool Equals(CallsiteSignature other)
        {
            if ((this.targetType != other.targetType) || (this.flags != other.flags))
            {
                return false;
            }
            if (this.invocationConstraints == null)
            {
                if (other.invocationConstraints != null)
                {
                    return false;
                }
            }
            else if (!this.invocationConstraints.Equals(other.invocationConstraints))
            {
                return false;
            }
            if (!EqualsForCollection<Type>(this.argumentTypes, other.argumentTypes))
            {
                return false;
            }
            if (!EqualsForCollection<Type>(this.effectiveArgumentTypes, other.effectiveArgumentTypes))
            {
                return false;
            }
            return true;
        }

        public override bool Equals(object other)
        {
            CallsiteSignature signature = other as CallsiteSignature;
            return ((signature != null) && this.Equals(signature));
        }

        internal static bool EqualsForCollection<T>(ICollection<T> xs, ICollection<T> ys)
        {
            if (xs == null)
            {
                return (ys == null);
            }
            if (ys == null)
            {
                return false;
            }
            if (xs.Count != ys.Count)
            {
                return false;
            }
            return xs.SequenceEqual<T>(ys);
        }

        public override int GetHashCode()
        {
            int num = 0x11;
            num = (num * 0x17) + this.targetType.GetHashCode();
            num = (num * 0x17) + this.flags.GetHashCode();
            num = (num * 0x17) + ((this.invocationConstraints == null) ? 0 : this.invocationConstraints.GetHashCode());
            num = (num * 0x17) + this.argumentTypes.SequenceGetHashCode<Type>();
            return ((num * 0x17) + this.effectiveArgumentTypes.SequenceGetHashCode<Type>());
        }
    }
}

