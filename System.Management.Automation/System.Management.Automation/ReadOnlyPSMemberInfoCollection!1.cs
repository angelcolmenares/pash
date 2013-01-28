namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    public class ReadOnlyPSMemberInfoCollection<T> : IEnumerable<T>, IEnumerable where T: PSMemberInfo
    {
        private PSMemberInfoInternalCollection<T> members;

        internal ReadOnlyPSMemberInfoCollection(PSMemberInfoInternalCollection<T> members)
        {
            if (members == null)
            {
                throw PSTraceSource.NewArgumentNullException("members");
            }
            this.members = members;
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return this.members.GetEnumerator();
        }

        public ReadOnlyPSMemberInfoCollection<T> Match(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            return this.members.Match(name);
        }

        public ReadOnlyPSMemberInfoCollection<T> Match(string name, PSMemberTypes memberTypes)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            return this.members.Match(name, memberTypes);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this.members.Count;
            }
        }

        public T this[string name]
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw PSTraceSource.NewArgumentException("name");
                }
                return this.members[name];
            }
        }

        public T this[int index]
        {
            get
            {
                return this.members[index];
            }
        }
    }
}

