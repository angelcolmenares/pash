namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Reflection;

    internal class PSMemberInfoInternalCollection<T> : PSMemberInfoCollection<T>, IEnumerable<T>, IEnumerable where T: PSMemberInfo
    {
        private int countHidden;
        private readonly OrderedDictionary members;

        internal PSMemberInfoInternalCollection()
        {
            this.members = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
        }

        public override void Add(T member)
        {
            this.Add(member, false);
        }

        public override void Add(T member, bool preValidated)
        {
            if (member == null)
            {
                throw PSTraceSource.NewArgumentNullException("member");
            }
            lock (this.members)
            {
                T oldMember = this.members[member.Name] as T;
                if (oldMember != null)
                {
                    this.Replace(oldMember, member);
                }
                else
                {
                    this.members[member.Name] = member;
                    if (member.IsHidden)
                    {
                        this.countHidden++;
                    }
                }
            }
        }

        public override IEnumerator<T> GetEnumerator()
        {
            lock (this.members)
            {
                return this.members.Values.OfType<T>().ToList<T>().GetEnumerator();
            }
        }

        private PSMemberInfoInternalCollection<T> GetInternalMembers(MshMemberMatchOptions matchOptions)
        {
            PSMemberInfoInternalCollection<T> internals = new PSMemberInfoInternalCollection<T>();
            lock (this.members)
            {
                foreach (T local in this.members.Values.OfType<T>())
                {
                    if (local.MatchesOptions(matchOptions))
                    {
                        internals.Add(local);
                    }
                }
            }
            return internals;
        }

        public override ReadOnlyPSMemberInfoCollection<T> Match(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            return this.Match(name, PSMemberTypes.All, MshMemberMatchOptions.None);
        }

        public override ReadOnlyPSMemberInfoCollection<T> Match(string name, PSMemberTypes memberTypes)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            return this.Match(name, memberTypes, MshMemberMatchOptions.None);
        }

        internal override ReadOnlyPSMemberInfoCollection<T> Match(string name, PSMemberTypes memberTypes, MshMemberMatchOptions matchOptions)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            return new ReadOnlyPSMemberInfoCollection<T>(MemberMatch.Match<T>(this.GetInternalMembers(matchOptions), name, MemberMatch.GetNamePattern(name), memberTypes));
        }

        public override void Remove(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            if (PSMemberInfoCollection<T>.IsReservedName(name))
            {
                throw new ExtendedTypeSystemException("PSMemberInfoInternalCollectionRemoveReservedName", null, ExtendedTypeSystem.ReservedMemberName, new object[] { name });
            }
            lock (this.members)
            {
                PSMemberInfo info = this.members[name] as PSMemberInfo;
                if (info != null)
                {
                    if (info.IsHidden)
                    {
                        this.countHidden--;
                    }
                    this.members.Remove(name);
                }
            }
        }

        internal void Replace(T newMember)
        {
            lock (this.members)
            {
                T oldMember = this.members[newMember.Name] as T;
                this.Replace(oldMember, newMember);
            }
        }

        private void Replace(T oldMember, T newMember)
        {
            this.members[newMember.Name] = newMember;
            if (oldMember.IsHidden)
            {
                this.countHidden--;
            }
            if (newMember.IsHidden)
            {
                this.countHidden++;
            }
        }

        internal int Count
        {
            get
            {
                lock (this.members)
                {
                    return this.members.Count;
                }
            }
        }

        public override T this[string name]
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw PSTraceSource.NewArgumentException("name");
                }
                lock (this.members)
                {
                    return (this.members[name] as T);
                }
            }
        }

        internal T this[int index]
        {
            get
            {
                lock (this.members)
                {
                    return (this.members[index] as T);
                }
            }
        }

        internal int VisibleCount
        {
            get
            {
                lock (this.members)
                {
                    return (this.members.Count - this.countHidden);
                }
            }
        }
    }
}

