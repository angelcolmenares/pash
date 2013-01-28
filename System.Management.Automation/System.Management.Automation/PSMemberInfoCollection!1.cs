namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    public abstract class PSMemberInfoCollection<T> : IEnumerable<T>, IEnumerable where T: PSMemberInfo
    {
        protected PSMemberInfoCollection()
        {
        }

        public abstract void Add(T member);
        public abstract void Add(T member, bool preValidated);
        public abstract IEnumerator<T> GetEnumerator();
        internal static bool IsReservedName(string name)
        {
            if ((!string.Equals(name, "psbase", StringComparison.OrdinalIgnoreCase) && !string.Equals(name, "psadapted", StringComparison.OrdinalIgnoreCase)) && (!string.Equals(name, "psextended", StringComparison.OrdinalIgnoreCase) && !string.Equals(name, "psobject", StringComparison.OrdinalIgnoreCase)))
            {
                return string.Equals(name, "pstypenames", StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }

        public abstract ReadOnlyPSMemberInfoCollection<T> Match(string name);
        public abstract ReadOnlyPSMemberInfoCollection<T> Match(string name, PSMemberTypes memberTypes);
        internal abstract ReadOnlyPSMemberInfoCollection<T> Match(string name, PSMemberTypes memberTypes, MshMemberMatchOptions matchOptions);
        public abstract void Remove(string name);
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public abstract T this[string name] { get; }
    }
}

