namespace System.Data.Services.Client
{
    using System;

    internal abstract class Descriptor
    {
        private uint changeOrder = uint.MaxValue;
        private bool saveContentGenerated;
        private Exception saveError;
        private EntityStates saveResultProcessed;
        private EntityStates state;

        internal Descriptor(EntityStates state)
        {
            this.state = state;
        }

        internal abstract void ClearChanges();

        internal uint ChangeOrder
        {
            get
            {
                return this.changeOrder;
            }
            set
            {
                this.changeOrder = value;
            }
        }

        internal bool ContentGeneratedForSave
        {
            get
            {
                return this.saveContentGenerated;
            }
            set
            {
                this.saveContentGenerated = value;
            }
        }

        internal abstract System.Data.Services.Client.DescriptorKind DescriptorKind { get; }

        internal virtual bool IsModified
        {
            get
            {
                return (EntityStates.Unchanged != this.state);
            }
        }

        internal Exception SaveError
        {
            get
            {
                return this.saveError;
            }
            set
            {
                this.saveError = value;
            }
        }

        internal EntityStates SaveResultWasProcessed
        {
            get
            {
                return this.saveResultProcessed;
            }
            set
            {
                this.saveResultProcessed = value;
            }
        }

        public EntityStates State
        {
            get
            {
                return this.state;
            }
            internal set
            {
                this.state = value;
            }
        }
    }
}

