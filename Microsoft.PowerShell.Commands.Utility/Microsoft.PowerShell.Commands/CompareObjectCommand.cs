namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;

    [Cmdlet("Compare", "Object", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113286", RemotingCapability=RemotingCapability.None)]
    public sealed class CompareObjectCommand : ObjectCmdletBase
    {
        private PSObject[] _differenceObject;
        private bool _excludeDifferent;
        private bool _includeEqual;
        private bool _passThru;
        private object[] _property;
        private PSObject[] _referenceObject;
        private int _syncWindow = 0x7fffffff;
        private OrderByPropertyComparer comparer;
        private List<OrderByPropertyEntry> differenceEntryBacklog = new List<OrderByPropertyEntry>();
        private const string InputObjectPropertyName = "InputObject";
        private OrderByProperty orderByProperty;
        private List<OrderByPropertyEntry> referenceEntries;
        private List<OrderByPropertyEntry> referenceEntryBacklog = new List<OrderByPropertyEntry>();
        private int referenceObjectIndex;
        private const string SideIndicatorDifference = "=>";
        private const string SideIndicatorMatch = "==";
        private const string SideIndicatorPropertyName = "SideIndicator";
        private const string SideIndicatorReference = "<=";

        private void Emit(OrderByPropertyEntry entry, string sideIndicator)
        {
            PSObject obj2;
            if (this.PassThru != 0)
            {
                obj2 = PSObject.AsPSObject(entry.inputObject);
            }
            else
            {
                obj2 = new PSObject();
                if ((this.Property == null) || (this.Property.Length == 0))
                {
                    PSNoteProperty property = new PSNoteProperty("InputObject", entry.inputObject);
                    obj2.Properties.Add(property);
                }
                else
                {
                    List<MshParameter> mshParameterList = this.orderByProperty.MshParameterList;
                    for (int i = 0; i < this.Property.Length; i++)
                    {
                        MshParameter parameter = mshParameterList[i];
                        object obj3 = parameter.hash["expression"];
                        PSNoteProperty property2 = new PSNoteProperty(obj3.ToString(), entry.orderValues[i].PropertyValue);
                        try
                        {
                            obj2.Properties.Add(property2);
                        }
                        catch (ExtendedTypeSystemException)
                        {
                        }
                    }
                }
            }
            obj2.Properties.Remove("SideIndicator");
            PSNoteProperty member = new PSNoteProperty("SideIndicator", sideIndicator);
            obj2.Properties.Add(member);
            base.WriteObject(obj2);
        }

        private void EmitDifferenceOnly(OrderByPropertyEntry entry)
        {
            if (this.ExcludeDifferent == 0)
            {
                this.Emit(entry, "=>");
            }
        }

        private void EmitMatch(OrderByPropertyEntry entry)
        {
            if (this.IncludeEqual != 0)
            {
                this.Emit(entry, "==");
            }
        }

        private void EmitReferenceOnly(OrderByPropertyEntry entry)
        {
            if (this.ExcludeDifferent == 0)
            {
                this.Emit(entry, "<=");
            }
        }

        protected override void EndProcessing()
        {
            if (this.referenceEntries != null)
            {
                while (this.referenceObjectIndex < this.referenceEntries.Count)
                {
                    this.Process(null);
                }
            }
            foreach (OrderByPropertyEntry entry in this.differenceEntryBacklog)
            {
                this.EmitDifferenceOnly(entry);
            }
            this.differenceEntryBacklog.Clear();
            foreach (OrderByPropertyEntry entry2 in this.referenceEntryBacklog)
            {
                this.EmitReferenceOnly(entry2);
            }
            this.referenceEntryBacklog.Clear();
        }

        private void HandleDifferenceObjectOnly()
        {
            if ((this.DifferenceObject != null) && (this.DifferenceObject.Length != 0))
            {
                List<PSObject> inputObjects = new List<PSObject>(this.DifferenceObject);
                this.orderByProperty = new OrderByProperty(this, inputObjects, this.Property, true, base._cultureInfo, (bool) base.CaseSensitive);
                foreach (OrderByPropertyEntry entry in OrderByProperty.CreateOrderMatrix(this, inputObjects, this.orderByProperty.MshParameterList))
                {
                    this.EmitDifferenceOnly(entry);
                }
            }
        }

        private void HandleReferenceObjectOnly()
        {
            if ((this.ReferenceObject != null) && (this.ReferenceObject.Length != 0))
            {
                this.InitComparer();
                this.Process(null);
            }
        }

        private void InitComparer()
        {
            if (this.comparer == null)
            {
                List<PSObject> inputObjects = new List<PSObject>(this.ReferenceObject);
                this.orderByProperty = new OrderByProperty(this, inputObjects, this.Property, true, base._cultureInfo, (bool) base.CaseSensitive);
                if (((this.orderByProperty.Comparer != null) && (this.orderByProperty.OrderMatrix != null)) && (this.orderByProperty.OrderMatrix.Count != 0))
                {
                    this.comparer = this.orderByProperty.Comparer;
                    this.referenceEntries = this.orderByProperty.OrderMatrix;
                }
            }
        }

        private OrderByPropertyEntry MatchAndRemove(OrderByPropertyEntry match, List<OrderByPropertyEntry> list)
        {
            if ((match != null) && (list != null))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    OrderByPropertyEntry secondEntry = list[i];
                    if (this.comparer.Compare(match, secondEntry) == 0)
                    {
                        list.RemoveAt(i);
                        return secondEntry;
                    }
                }
            }
            return null;
        }

        private void Process(OrderByPropertyEntry differenceEntry)
        {
            OrderByPropertyEntry firstEntry = null;
            if (this.referenceObjectIndex < this.referenceEntries.Count)
            {
                firstEntry = this.referenceEntries[this.referenceObjectIndex++];
            }
            if (((firstEntry != null) && (differenceEntry != null)) && (this.comparer.Compare(firstEntry, differenceEntry) == 0))
            {
                this.EmitMatch(firstEntry);
            }
            else
            {
                OrderByPropertyEntry entry = this.MatchAndRemove(differenceEntry, this.referenceEntryBacklog);
                if (entry != null)
                {
                    this.EmitMatch(entry);
                    differenceEntry = null;
                }
                if (this.MatchAndRemove(firstEntry, this.differenceEntryBacklog) != null)
                {
                    this.EmitMatch(firstEntry);
                    firstEntry = null;
                }
                if (differenceEntry != null)
                {
                    if (0 < this.SyncWindow)
                    {
                        while (this.differenceEntryBacklog.Count >= this.SyncWindow)
                        {
                            this.EmitDifferenceOnly(this.differenceEntryBacklog[0]);
                            this.differenceEntryBacklog.RemoveAt(0);
                        }
                        this.differenceEntryBacklog.Add(differenceEntry);
                    }
                    else
                    {
                        this.EmitDifferenceOnly(differenceEntry);
                    }
                }
                if (firstEntry != null)
                {
                    if (0 < this.SyncWindow)
                    {
                        while (this.referenceEntryBacklog.Count >= this.SyncWindow)
                        {
                            this.EmitReferenceOnly(this.referenceEntryBacklog[0]);
                            this.referenceEntryBacklog.RemoveAt(0);
                        }
                        this.referenceEntryBacklog.Add(firstEntry);
                    }
                    else
                    {
                        this.EmitReferenceOnly(firstEntry);
                    }
                }
            }
        }

        protected override void ProcessRecord()
        {
            if ((this.ReferenceObject == null) || (this.ReferenceObject.Length == 0))
            {
                this.HandleDifferenceObjectOnly();
            }
            else if ((this.DifferenceObject == null) || (this.DifferenceObject.Length == 0))
            {
                this.HandleReferenceObjectOnly();
            }
            else
            {
                if ((this.comparer == null) && (0 < this.DifferenceObject.Length))
                {
                    this.InitComparer();
                }
                List<PSObject> inputObjects = new List<PSObject>(this.DifferenceObject);
                foreach (OrderByPropertyEntry entry in OrderByProperty.CreateOrderMatrix(this, inputObjects, this.orderByProperty.MshParameterList))
                {
                    this.Process(entry);
                }
            }
        }

        [Parameter(Position=1, Mandatory=true, ValueFromPipeline=true), AllowEmptyCollection]
        public PSObject[] DifferenceObject
        {
            get
            {
                return this._differenceObject;
            }
            set
            {
                this._differenceObject = value;
            }
        }

        [Parameter]
        public SwitchParameter ExcludeDifferent
        {
            get
            {
                return this._excludeDifferent;
            }
            set
            {
                this._excludeDifferent = (bool) value;
            }
        }

        [Parameter]
        public SwitchParameter IncludeEqual
        {
            get
            {
                return this._includeEqual;
            }
            set
            {
                this._includeEqual = (bool) value;
            }
        }

        [Parameter]
        public SwitchParameter PassThru
        {
            get
            {
                return this._passThru;
            }
            set
            {
                this._passThru = (bool) value;
            }
        }

        [Parameter]
        public object[] Property
        {
            get
            {
                return this._property;
            }
            set
            {
                this._property = value;
            }
        }

        [AllowEmptyCollection, Parameter(Position=0, Mandatory=true)]
        public PSObject[] ReferenceObject
        {
            get
            {
                return this._referenceObject;
            }
            set
            {
                this._referenceObject = value;
            }
        }

        [ValidateRange(0, 0x7fffffff), Parameter]
        public int SyncWindow
        {
            get
            {
                return this._syncWindow;
            }
            set
            {
                this._syncWindow = value;
            }
        }
    }
}

