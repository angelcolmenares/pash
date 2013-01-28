namespace System.Data.Services.Client.Materialization
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client;

    internal sealed class ODataEntriesEntityMaterializer : ODataEntityMaterializer
    {
        private IEnumerator<ODataEntry> feedEntries;
        private bool isFinished;

        public ODataEntriesEntityMaterializer(IEnumerable<ODataEntry> entries, ResponseInfo responseInfo, QueryComponents queryComponents, Type expectedType, ProjectionPlan materializeEntryPlan) : base(responseInfo, queryComponents, expectedType, materializeEntryPlan)
        {
            this.feedEntries = entries.GetEnumerator();
        }

        protected override void OnDispose()
        {
            if (this.feedEntries != null)
            {
                this.feedEntries.Dispose();
                this.feedEntries = null;
            }
        }

        protected override bool ReadNextFeedOrEntry()
        {
            if (!this.isFinished && !this.feedEntries.MoveNext())
            {
                this.isFinished = true;
            }
            return !this.isFinished;
        }

        internal override long CountValue
        {
            get
            {
                throw new InvalidOperationException(System.Data.Services.Client.Strings.MaterializeFromAtom_CountNotPresent);
            }
        }

        internal override ODataEntry CurrentEntry
        {
            get
            {
                base.VerifyNotDisposed();
                return this.feedEntries.Current;
            }
        }

        internal override ODataFeed CurrentFeed
        {
            get
            {
                return null;
            }
        }

        internal override bool IsCountable
        {
            get
            {
                return false;
            }
        }

        protected override bool IsDisposed
        {
            get
            {
                return (this.feedEntries == null);
            }
        }

        internal override bool IsEndOfStream
        {
            get
            {
                return this.isFinished;
            }
        }
    }
}

