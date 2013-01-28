namespace System.Management.Automation
{
    using System;
    using System.Runtime.CompilerServices;

    internal class CollectionEntry<T> where T: PSMemberInfo
    {
        private string collectionNameForTracing;
        private GetMemberDelegate getMember;
        private GetMembersDelegate getMembers;
        private bool shouldCloneWhenReturning;
        private bool shouldReplicateWhenReturning;

        internal CollectionEntry(GetMembersDelegate getMembers, GetMemberDelegate getMember, bool shouldReplicateWhenReturning, bool shouldCloneWhenReturning, string collectionNameForTracing)
        {
            this.getMembers = getMembers;
            this.getMember = getMember;
            this.shouldReplicateWhenReturning = shouldReplicateWhenReturning;
            this.shouldCloneWhenReturning = shouldCloneWhenReturning;
            this.collectionNameForTracing = collectionNameForTracing;
        }

        internal string CollectionNameForTracing
        {
            get
            {
                return this.collectionNameForTracing;
            }
        }

        internal GetMemberDelegate GetMember
        {
            get
            {
                return this.getMember;
            }
        }

        internal GetMembersDelegate GetMembers
        {
            get
            {
                return this.getMembers;
            }
        }

        internal bool ShouldCloneWhenReturning
        {
            get
            {
                return this.shouldCloneWhenReturning;
            }
        }

        internal bool ShouldReplicateWhenReturning
        {
            get
            {
                return this.shouldReplicateWhenReturning;
            }
        }

        internal delegate T GetMemberDelegate(PSObject obj, string name);

        internal delegate PSMemberInfoInternalCollection<T> GetMembersDelegate(PSObject obj);
    }
}

