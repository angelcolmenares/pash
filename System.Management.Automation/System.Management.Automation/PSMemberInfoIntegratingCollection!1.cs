namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class PSMemberInfoIntegratingCollection<T> : PSMemberInfoCollection<T>, IEnumerable<T>, IEnumerable where T: PSMemberInfo
    {
        private Collection<CollectionEntry<T>> collections;
        private PSMemberSet memberSetOwner;
        private PSObject mshOwner;

        internal PSMemberInfoIntegratingCollection(object owner, Collection<CollectionEntry<T>> collections)
        {
            if (owner == null)
            {
                throw PSTraceSource.NewArgumentNullException("owner");
            }
            this.mshOwner = owner as PSObject;
            this.memberSetOwner = owner as PSMemberSet;
            if ((this.mshOwner == null) && (this.memberSetOwner == null))
            {
                throw PSTraceSource.NewArgumentException("owner");
            }
            if (collections == null)
            {
                throw PSTraceSource.NewArgumentNullException("collections");
            }
            this.collections = collections;
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
            if (!preValidated)
            {
                if ((member.MemberType == PSMemberTypes.Property) || (member.MemberType == PSMemberTypes.Method))
                {
                    throw new ExtendedTypeSystemException("CannotAddMethodOrProperty", null, ExtendedTypeSystem.CannotAddPropertyOrMethod, new object[0]);
                }
                if ((this.memberSetOwner != null) && this.memberSetOwner.IsReservedMember)
                {
                    throw new ExtendedTypeSystemException("CannotAddToReservedNameMemberset", null, ExtendedTypeSystem.CannotChangeReservedMember, new object[] { this.memberSetOwner.Name });
                }
            }
            this.AddToReservedMemberSet(member, preValidated);
        }

        internal void AddToReservedMemberSet(T member, bool preValidated)
        {
            if ((!preValidated && (this.memberSetOwner != null)) && !this.memberSetOwner.IsInstance)
            {
                throw new ExtendedTypeSystemException("RemoveMemberFromStaticMemberSet", null, ExtendedTypeSystem.ChangeStaticMember, new object[] { member.Name });
            }
            this.AddToTypesXmlCache(member, preValidated);
        }

        internal void AddToTypesXmlCache(T member, bool preValidated)
        {
            if (member == null)
            {
                throw PSTraceSource.NewArgumentNullException("member");
            }
            if (!preValidated && PSMemberInfoCollection<T>.IsReservedName(member.Name))
            {
                throw new ExtendedTypeSystemException("PSObjectMembersMembersAddReservedName", null, ExtendedTypeSystem.ReservedMemberName, new object[] { member.Name });
            }
            PSMemberInfo info = member.Copy();
            if (this.mshOwner != null)
            {
                if (!preValidated)
                {
                    TypeTable typeTable = this.mshOwner.GetTypeTable();
                    if ((typeTable != null) && (typeTable.GetMembers<T>(this.mshOwner.InternalTypeNames)[member.Name] != null))
                    {
                        throw new ExtendedTypeSystemException("AlreadyPresentInTypesXml", null, ExtendedTypeSystem.MemberAlreadyPresentFromTypesXml, new object[] { member.Name });
                    }
                }
                info.ReplicateInstance(this.mshOwner);
                this.mshOwner.InstanceMembers.Add(info, preValidated);
                PSGetMemberBinder.SetHasInstanceMember(info.Name);
                PSVariableAssignmentBinder.NoteTypeHasInstanceMemberOrTypeName(PSObject.Base(this.mshOwner).GetType());
            }
            else
            {
                this.memberSetOwner.InternalMembers.Add(info, preValidated);
            }
        }

        private void EnsureReservedMemberIsLoaded(string name)
        {
            string str;
            if ((((name.Length >= 6) && ((name[0] == 'p') || (name[0] == 'P'))) && ((name[1] == 's') || (name[1] == 'S'))) && ((str = name.ToLowerInvariant()) != null))
            {
                if (!(str == "psbase"))
                {
                    if (!(str == "psadapted"))
                    {
                        if (!(str == "psextended"))
                        {
                            if (!(str == "psobject"))
                            {
                                if (str == "pstypenames")
                                {
                                    ReservedNameMembers.GeneratePSTypeNames(this.mshOwner);
                                }
                                return;
                            }
                            ReservedNameMembers.GeneratePSObjectMemberSet(this.mshOwner);
                            return;
                        }
                        ReservedNameMembers.GeneratePSExtendedMemberSet(this.mshOwner);
                        return;
                    }
                }
                else
                {
                    ReservedNameMembers.GeneratePSBaseMemberSet(this.mshOwner);
                    return;
                }
                ReservedNameMembers.GeneratePSAdaptedMemberSet(this.mshOwner);
            }
        }

        private void GenerateAllReservedMembers()
        {
            if (!this.mshOwner.hasGeneratedReservedMembers)
            {
                this.mshOwner.hasGeneratedReservedMembers = true;
                ReservedNameMembers.GeneratePSExtendedMemberSet(this.mshOwner);
                ReservedNameMembers.GeneratePSBaseMemberSet(this.mshOwner);
                ReservedNameMembers.GeneratePSObjectMemberSet(this.mshOwner);
                ReservedNameMembers.GeneratePSAdaptedMemberSet(this.mshOwner);
                ReservedNameMembers.GeneratePSTypeNames(this.mshOwner);
            }
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return new Enumerator<T>((PSMemberInfoIntegratingCollection<T>) this);
        }

        private PSMemberInfoInternalCollection<T> GetIntegratedMembers(MshMemberMatchOptions matchOptions)
        {
            using (PSObject.memberResolution.TraceScope("Generating the total list of members", new object[0]))
            {
                object mshOwner;
                PSMemberInfoInternalCollection<T> internals = new PSMemberInfoInternalCollection<T>();
                if (this.mshOwner != null)
                {
                    mshOwner = this.mshOwner;
                    foreach (PSMemberInfo info in this.mshOwner.InstanceMembers)
                    {
                        if (info.MatchesOptions(matchOptions))
                        {
                            T member = info as T;
                            if (member != null)
                            {
                                internals.Add(member);
                            }
                        }
                    }
                }
                else
                {
                    mshOwner = this.memberSetOwner.instance;
                    foreach (PSMemberInfo info2 in this.memberSetOwner.InternalMembers)
                    {
                        if (info2.MatchesOptions(matchOptions))
                        {
                            T local2 = info2 as T;
                            if (local2 != null)
                            {
                                info2.ReplicateInstance(mshOwner);
                                internals.Add(local2);
                            }
                        }
                    }
                }
                if (mshOwner != null)
                {
                    mshOwner = PSObject.AsPSObject(mshOwner);
                    foreach (CollectionEntry<T> entry in this.collections)
                    {
                        foreach (T local3 in entry.GetMembers((PSObject) mshOwner))
                        {
                            PSMemberInfo info3 = internals[local3.Name];
                            if (info3 != null)
                            {
                                PSObject.memberResolution.WriteLine("Member \"{0}\" of type \"{1}\" has been ignored because a member with the same name and type \"{2}\" is already present.", new object[] { local3.Name, local3.MemberType, info3.MemberType });
                            }
                            else if (!local3.MatchesOptions(matchOptions))
                            {
                                PSObject.memberResolution.WriteLine("Skipping hidden member \"{0}\".", new object[] { local3.Name });
                            }
                            else
                            {
                                T local4;
                                if (entry.ShouldCloneWhenReturning)
                                {
                                    local4 = (T) local3.Copy();
                                }
                                else
                                {
                                    local4 = local3;
                                }
                                if (entry.ShouldReplicateWhenReturning)
                                {
                                    local4.ReplicateInstance(mshOwner);
                                }
                                internals.Add(local4);
                            }
                        }
                    }
                }
                return internals;
            }
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
            using (PSObject.memberResolution.TraceScope("Matching \"{0}\"", new object[] { name }))
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw PSTraceSource.NewArgumentException("name");
                }
                if (this.mshOwner != null)
                {
                    this.GenerateAllReservedMembers();
                }
                WildcardPattern namePattern = MemberMatch.GetNamePattern(name);
                ReadOnlyPSMemberInfoCollection<T> infos = new ReadOnlyPSMemberInfoCollection<T>(MemberMatch.Match<T>(this.GetIntegratedMembers(matchOptions), name, namePattern, memberTypes));
                PSObject.memberResolution.WriteLine("{0} total matches.", new object[] { infos.Count });
                return infos;
            }
        }

        public override void Remove(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            if (this.mshOwner != null)
            {
                this.mshOwner.InstanceMembers.Remove(name);
            }
            else
            {
                if (!this.memberSetOwner.IsInstance)
                {
                    throw new ExtendedTypeSystemException("AddMemberToStaticMemberSet", null, ExtendedTypeSystem.ChangeStaticMember, new object[] { name });
                }
                if (PSMemberInfoCollection<T>.IsReservedName(this.memberSetOwner.Name))
                {
                    throw new ExtendedTypeSystemException("CannotRemoveFromReservedNameMemberset", null, ExtendedTypeSystem.CannotChangeReservedMember, new object[] { this.memberSetOwner.Name });
                }
                this.memberSetOwner.InternalMembers.Remove(name);
            }
        }

        internal Collection<CollectionEntry<T>> Collections
        {
            get
            {
                return this.collections;
            }
        }

        public override T this[string name]
        {
            get
            {
                using (PSObject.memberResolution.TraceScope("Lookup", new object[0]))
                {
                    PSMemberInfo info;
                    object mshOwner;
                    if (string.IsNullOrEmpty(name))
                    {
                        throw PSTraceSource.NewArgumentException("name");
                    }
                    if (this.mshOwner != null)
                    {
                        PSMemberInfoInternalCollection<PSMemberInfo> internals;
                        this.EnsureReservedMemberIsLoaded(name);
                        mshOwner = this.mshOwner;
                        if (PSObject.HasInstanceMembers(this.mshOwner, out internals))
                        {
                            info = internals[name];
                            T local = info as T;
                            if (local != null)
                            {
                                PSObject.memberResolution.WriteLine("Found PSObject instance member: {0}.", new object[] { name });
                                return local;
                            }
                        }
                    }
                    else
                    {
                        info = this.memberSetOwner.InternalMembers[name];
                        mshOwner = this.memberSetOwner.instance;
                        T local2 = info as T;
                        if (local2 != null)
                        {
                            PSObject.memberResolution.WriteLine("Found PSMemberSet member: {0}.", new object[] { name });
                            info.ReplicateInstance(mshOwner);
                            return local2;
                        }
                    }
                    if (mshOwner != null)
                    {
                        mshOwner = PSObject.AsPSObject(mshOwner);
                        foreach (CollectionEntry<T> entry in this.collections)
                        {
                            T local3 = entry.GetMember((PSObject) mshOwner, name);
                            if (local3 != null)
                            {
                                if (entry.ShouldCloneWhenReturning)
                                {
                                    local3 = (T) local3.Copy();
                                }
                                if (entry.ShouldReplicateWhenReturning)
                                {
                                    local3.ReplicateInstance(mshOwner);
                                }
                                return local3;
                            }
                        }
                    }
                    return default(T);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Enumerator<S> : IEnumerator<S>, IDisposable, IEnumerator where S: PSMemberInfo
        {
            private S current;
            private int currentIndex;
            private PSMemberInfoInternalCollection<S> allMembers;
            internal Enumerator(PSMemberInfoIntegratingCollection<S> integratingCollection)
            {
                using (PSObject.memberResolution.TraceScope("Enumeration Start", new object[0]))
                {
                    this.currentIndex = -1;
                    this.current = default(S);
                    this.allMembers = integratingCollection.GetIntegratedMembers(MshMemberMatchOptions.None);
                    if (integratingCollection.mshOwner != null)
                    {
                        integratingCollection.GenerateAllReservedMembers();
                        PSObject.memberResolution.WriteLine("Enumerating PSObject with type \"{0}\".", new object[] { integratingCollection.mshOwner.ImmediateBaseObject.GetType().FullName });
                        PSObject.memberResolution.WriteLine("PSObject instance members: {0}", new object[] { this.allMembers.VisibleCount });
                    }
                    else
                    {
                        PSObject.memberResolution.WriteLine("Enumerating PSMemberSet \"{0}\".", new object[] { integratingCollection.memberSetOwner.Name });
                        PSObject.memberResolution.WriteLine("MemberSet instance members: {0}", new object[] { this.allMembers.VisibleCount });
                    }
                }
            }

            public bool MoveNext()
            {
                this.currentIndex++;
                S local = default(S);
                while (this.currentIndex < this.allMembers.Count)
                {
                    local = this.allMembers[this.currentIndex];
                    if (!local.IsHidden)
                    {
                        break;
                    }
                    this.currentIndex++;
                }
                if (this.currentIndex < this.allMembers.Count)
                {
                    this.current = local;
                    return true;
                }
                this.current = default(S);
                return false;
            }

            S IEnumerator<S>.Current
            {
                get
                {
                    if (this.currentIndex == -1)
                    {
                        throw PSTraceSource.NewInvalidOperationException();
                    }
                    return this.current;
                }
            }
            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            public S Current
            {
                get { return this.current; }
            }

            void IEnumerator.Reset()
            {
                this.currentIndex = -1;
                this.current = default(S);
            }

            public void Dispose()
            {
            }
        }
    }
}

