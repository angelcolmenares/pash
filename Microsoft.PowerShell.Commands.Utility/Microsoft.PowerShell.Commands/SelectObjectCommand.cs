namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Runtime.CompilerServices;
    using System.Threading;

    [Cmdlet("Select", "Object", DefaultParameterSetName="DefaultParameter", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113387", RemotingCapability=RemotingCapability.None)]
    public sealed class SelectObjectCommand : PSCmdlet
    {
        private PSObject _inputObject = AutomationNull.Value;
        private string[] excludeArray;
        private MshExpressionFilter exclusionFilter;
        private string expand;
        private List<MshParameter> expandMshParameterList;
        private object[] expr;
        private int first;
        private bool firstOrLastSpecified;
        private int[] index;
        private int indexCount;
        private int indexOfCurrentObject;
        private bool indexSpecified;
        private int last;
        private List<MshParameter> propertyMshParameterList;
        private string ResourcesBaseName = "SelectObjectStrings";
        private SelectObjectQueue selectObjectQueue;
        private int skip;
        private bool unique;
        private List<UniquePSObjectHelper> uniques;

        protected override void BeginProcessing()
        {
            this.ProcessExpressionParameter();
            if (this.unique)
            {
                this.uniques = new List<UniquePSObjectHelper>();
            }
            this.selectObjectQueue = new SelectObjectQueue(this.first, this.last, this.skip, this.firstOrLastSpecified);
        }

        protected override void EndProcessing()
        {
            if (this.first == 0)
            {
                while (this.selectObjectQueue.Count > 0)
                {
                    if (this.selectObjectQueue.Count <= this.skip)
                    {
                        break;
                    }
                    this.ProcessObjectAndHandleErrors(this.selectObjectQueue.Dequeue());
                }
            }
            else
            {
                while (this.selectObjectQueue.Count > 0)
                {
                    this.ProcessObjectAndHandleErrors(this.selectObjectQueue.Dequeue());
                }
            }
            if (this.uniques != null)
            {
                foreach (UniquePSObjectHelper helper in this.uniques)
                {
                    if ((helper.WrittenObject != null) && (helper.WrittenObject != AutomationNull.Value))
                    {
                        base.WriteObject(helper.WrittenObject);
                    }
                }
            }
        }

        private void FilteredWriteObject(PSObject obj, List<PSNoteProperty> addedNoteProperties)
        {
            if (!this.unique)
            {
                if (obj != AutomationNull.Value)
                {
                    this.SetPSCustomObject(obj);
                    base.WriteObject(obj);
                }
            }
            else if (this.unique)
            {
                bool flag = true;
                foreach (UniquePSObjectHelper helper in this.uniques)
                {
                    ObjectCommandComparer comparer = new ObjectCommandComparer(true, Thread.CurrentThread.CurrentCulture, true);
                    if ((comparer.Compare(obj.BaseObject, helper.WrittenObject.BaseObject) == 0) && (helper.NotePropertyCount == addedNoteProperties.Count))
                    {
                        bool flag2 = true;
                        foreach (PSNoteProperty property in addedNoteProperties)
                        {
                            PSMemberInfo info = helper.WrittenObject.Properties[property.Name];
                            if ((info == null) || (comparer.Compare(info.Value, property.Value) != 0))
                            {
                                flag2 = false;
                                break;
                            }
                        }
                        if (flag2)
                        {
                            flag = false;
                            break;
                        }
                    }
                }
                if (flag)
                {
                    this.SetPSCustomObject(obj);
                    this.uniques.Add(new UniquePSObjectHelper(obj, addedNoteProperties.Count));
                }
            }
        }

        private void ProcessExpandParameter(MshParameter p, PSObject inputObject, List<PSNoteProperty> matchedProperties)
        {
            List<MshExpressionResult> values = (p.GetEntry("expression") as MshExpression).GetValues(inputObject);
            if (values.Count == 0)
            {
                ErrorRecord errorRecord = new ErrorRecord(PSTraceSource.NewArgumentException("ExpandProperty", this.ResourcesBaseName, "PropertyNotFound", new object[] { this.expand }), "ExpandPropertyNotFound", ErrorCategory.InvalidArgument, inputObject);
                throw new SelectObjectException(errorRecord);
            }
            if (values.Count > 1)
            {
                ErrorRecord record2 = new ErrorRecord(PSTraceSource.NewArgumentException("ExpandProperty", this.ResourcesBaseName, "MutlipleExpandProperties", new object[] { this.expand }), "MutlipleExpandProperties", ErrorCategory.InvalidArgument, inputObject);
                throw new SelectObjectException(record2);
            }
            MshExpressionResult result = values[0];
            if (result.Exception == null)
            {
                IEnumerable enumerable = LanguagePrimitives.GetEnumerable(result.Result);
                if (enumerable == null)
                {
                    PSObject obj2 = PSObject.AsPSObject(result.Result);
                    this.FilteredWriteObject(obj2, matchedProperties);
                }
                else
                {
                    foreach (object obj3 in enumerable)
                    {
                        if (obj3 != null)
                        {
                            PSObject obj4 = PSObject.AsPSObject(obj3);
                            foreach (PSNoteProperty property in matchedProperties)
                            {
                                try
                                {
                                    if (obj4.Properties[property.Name] != null)
                                    {
                                        this.WriteAlreadyExistingPropertyError(property.Name, inputObject, "AlreadyExistingUserSpecifiedPropertyExpand");
                                    }
                                    else
                                    {
                                        obj4.Properties.Add(property);
                                    }
                                }
                                catch (ExtendedTypeSystemException)
                                {
                                    this.WriteAlreadyExistingPropertyError(property.Name, inputObject, "AlreadyExistingUserSpecifiedPropertyExpand");
                                }
                            }
                            this.FilteredWriteObject(obj4, matchedProperties);
                        }
                    }
                }
            }
            else
            {
                ErrorRecord record3 = new ErrorRecord(result.Exception, "PropertyEvaluationExpand", ErrorCategory.InvalidResult, inputObject);
                throw new SelectObjectException(record3);
            }
        }

        private void ProcessExpressionParameter()
        {
            TerminatingErrorContext invocationContext = new TerminatingErrorContext(this);
            ParameterProcessor processor = new ParameterProcessor(new SelectObjectExpressionParameterDefinition());
            if ((this.expr != null) && (this.expr.Length != 0))
            {
                this.propertyMshParameterList = processor.ProcessParameters(this.expr, invocationContext);
            }
            else
            {
                this.propertyMshParameterList = new List<MshParameter>();
            }
            if (!string.IsNullOrEmpty(this.expand))
            {
                this.expandMshParameterList = processor.ProcessParameters(new string[] { this.expand }, invocationContext);
            }
            if (this.excludeArray != null)
            {
                this.exclusionFilter = new MshExpressionFilter(this.excludeArray);
            }
        }

        private void ProcessObject(PSObject inputObject)
        {
            if (((this.expr == null) || (this.expr.Length == 0)) && string.IsNullOrEmpty(this.expand))
            {
                this.FilteredWriteObject(inputObject, new List<PSNoteProperty>());
            }
            else
            {
                List<PSNoteProperty> result = new List<PSNoteProperty>();
                foreach (MshParameter parameter in this.propertyMshParameterList)
                {
                    this.ProcessParameter(parameter, inputObject, result);
                }
                if (string.IsNullOrEmpty(this.expand))
                {
                    PSObject obj2 = new PSObject();
                    if (result.Count != 0)
                    {
                        HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        foreach (PSNoteProperty property in result)
                        {
                            try
                            {
                                if (!set.Contains(property.Name))
                                {
                                    set.Add(property.Name);
                                    obj2.Properties.Add(property);
                                }
                                else
                                {
                                    this.WriteAlreadyExistingPropertyError(property.Name, inputObject, "AlreadyExistingUserSpecifiedPropertyNoExpand");
                                }
                            }
                            catch (ExtendedTypeSystemException)
                            {
                                this.WriteAlreadyExistingPropertyError(property.Name, inputObject, "AlreadyExistingUserSpecifiedPropertyNoExpand");
                            }
                        }
                    }
                    this.FilteredWriteObject(obj2, result);
                }
                else
                {
                    this.ProcessExpandParameter(this.expandMshParameterList[0], inputObject, result);
                }
            }
        }

        private void ProcessObjectAndHandleErrors(PSObject pso)
        {
            try
            {
                this.ProcessObject(pso);
            }
            catch (SelectObjectException exception)
            {
                base.WriteError(exception.ErrorRecord);
            }
        }

        private void ProcessParameter(MshParameter p, PSObject inputObject, List<PSNoteProperty> result)
        {
            string entry = p.GetEntry("name") as string;
            MshExpression re = p.GetEntry("expression") as MshExpression;
            List<MshExpressionResult> list = new List<MshExpressionResult>();
            foreach (MshExpression expression2 in re.ResolveNames(inputObject))
            {
                if ((this.exclusionFilter == null) || !this.exclusionFilter.IsMatch(expression2))
                {
                    List<MshExpressionResult> values = expression2.GetValues(inputObject);
                    if (values != null)
                    {
                        foreach (MshExpressionResult result2 in values)
                        {
                            list.Add(result2);
                        }
                    }
                }
            }
            if (list.Count == 0)
            {
                list.Add(new MshExpressionResult(null, re, null));
            }
            else if (!string.IsNullOrEmpty(entry) && (list.Count > 1))
            {
                ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(SelectObjectStrings.RenamingMultipleResults), "RenamingMultipleResults", ErrorCategory.InvalidOperation, inputObject);
                base.WriteError(errorRecord);
                return;
            }
            foreach (MshExpressionResult result3 in list)
            {
                if ((this.exclusionFilter == null) || !this.exclusionFilter.IsMatch(result3.ResolvedExpression))
                {
                    PSNoteProperty property;
                    if (string.IsNullOrEmpty(entry))
                    {
                        string str3 = result3.ResolvedExpression.ToString();
                        if (string.IsNullOrEmpty(str3))
                        {
                            PSArgumentException exception = PSTraceSource.NewArgumentException("Property", this.ResourcesBaseName, "EmptyScriptBlockAndNoName", new object[0]);
                            base.ThrowTerminatingError(new ErrorRecord(exception, "EmptyScriptBlockAndNoName", ErrorCategory.InvalidArgument, null));
                        }
                        property = new PSNoteProperty(str3, result3.Result);
                    }
                    else
                    {
                        property = new PSNoteProperty(entry, result3.Result);
                    }
                    result.Add(property);
                }
            }
        }

        protected override void ProcessRecord()
        {
            if ((this._inputObject != AutomationNull.Value) && (this._inputObject != null))
            {
                if (!this.indexSpecified)
                {
                    this.selectObjectQueue.Enqueue(this._inputObject);
                    PSObject pso = this.selectObjectQueue.StreamingDequeue();
                    if (pso != null)
                    {
                        this.ProcessObjectAndHandleErrors(pso);
                    }
                    if (this.selectObjectQueue.AllRequestedObjectsProcessed && (this.Wait == 0))
                    {
                        this.EndProcessing();
                        throw new StopUpstreamCommandsException(this);
                    }
                }
                else
                {
                    if (this.indexOfCurrentObject < this.index.Length)
                    {
                        int num = this.index[this.indexOfCurrentObject];
                        if (this.indexCount == num)
                        {
                            this.ProcessObjectAndHandleErrors(this._inputObject);
                            while ((this.indexOfCurrentObject < this.index.Length) && (this.index[this.indexOfCurrentObject] == num))
                            {
                                this.indexOfCurrentObject++;
                            }
                        }
                    }
                    if ((this.Wait == 0) && (this.indexOfCurrentObject >= this.index.Length))
                    {
                        this.EndProcessing();
                        throw new StopUpstreamCommandsException(this);
                    }
                    this.indexCount++;
                }
            }
        }

        private void SetPSCustomObject(PSObject psObj)
        {
            if (psObj.ImmediateBaseObject is PSCustomObject)
            {
                psObj.TypeNames.Insert(0, "Selected." + this._inputObject.BaseObject.GetType().ToString());
            }
        }

        private void WriteAlreadyExistingPropertyError(string name, object inputObject, string errorId)
        {
            ErrorRecord errorRecord = new ErrorRecord(PSTraceSource.NewArgumentException("Property", this.ResourcesBaseName, "AlreadyExistingProperty", new object[] { name }), errorId, ErrorCategory.InvalidOperation, inputObject);
            base.WriteError(errorRecord);
        }

        [Parameter(ParameterSetName="DefaultParameter")]
        public string[] ExcludeProperty
        {
            get
            {
                return this.excludeArray;
            }
            set
            {
                this.excludeArray = value;
            }
        }

        [Parameter(ParameterSetName="DefaultParameter")]
        public string ExpandProperty
        {
            get
            {
                return this.expand;
            }
            set
            {
                this.expand = value;
            }
        }

        [Parameter(ParameterSetName="DefaultParameter"), ValidateRange(0, 0x7fffffff)]
        public int First
        {
            get
            {
                return this.first;
            }
            set
            {
                this.first = value;
                this.firstOrLastSpecified = true;
            }
        }

        [ValidateRange(0, 0x7fffffff), Parameter(ParameterSetName="IndexParameter")]
        public int[] Index
        {
            get
            {
                return this.index;
            }
            set
            {
                this.index = value;
                this.indexSpecified = true;
                Array.Sort<int>(this.index);
            }
        }

        [Parameter(ValueFromPipeline=true)]
        public PSObject InputObject
        {
            get
            {
                return this._inputObject;
            }
            set
            {
                this._inputObject = value;
            }
        }

        [ValidateRange(0, 0x7fffffff), Parameter(ParameterSetName="DefaultParameter")]
        public int Last
        {
            get
            {
                return this.last;
            }
            set
            {
                this.last = value;
                this.firstOrLastSpecified = true;
            }
        }

        [Parameter(Position=0, ParameterSetName="DefaultParameter")]
        public object[] Property
        {
            get
            {
                return this.expr;
            }
            set
            {
                this.expr = value;
            }
        }

        [Parameter(ParameterSetName="DefaultParameter"), ValidateRange(0, 0x7fffffff)]
        public int Skip
        {
            get
            {
                return this.skip;
            }
            set
            {
                this.skip = value;
            }
        }

        [Parameter]
        public SwitchParameter Unique
        {
            get
            {
                return this.unique;
            }
            set
            {
                this.unique = (bool) value;
            }
        }

        [Parameter]
        public SwitchParameter Wait { get; set; }

        private class SelectObjectQueue : Queue<PSObject>
        {
            private int first;
            private bool firstOrLastSpecified;
            private int last;
            private int skip;
            private int streamedObjectCount;

            internal SelectObjectQueue(int first, int last, int skip, bool firstOrLastSpecified)
            {
                this.first = first;
                this.last = last;
                this.skip = skip;
                this.firstOrLastSpecified = firstOrLastSpecified;
            }

            public void Enqueue(PSObject obj)
            {
                if (((this.last > 0) && (base.Count >= (this.last + this.skip))) && (this.first == 0))
                {
                    base.Dequeue();
                }
                else if (((this.last > 0) && (base.Count >= this.last)) && (this.first != 0))
                {
                    base.Dequeue();
                }
                base.Enqueue(obj);
            }

            public PSObject StreamingDequeue()
            {
                if (this.skip == 0)
                {
                    if ((this.streamedObjectCount < this.first) || !this.firstOrLastSpecified)
                    {
                        this.streamedObjectCount++;
                        return base.Dequeue();
                    }
                    if (this.last == 0)
                    {
                        base.Dequeue();
                    }
                    return null;
                }
                if (this.last == 0)
                {
                    base.Dequeue();
                    this.skip--;
                }
                else if (this.first != 0)
                {
                    if (this.skip == 0)
                    {
                        return base.Dequeue();
                    }
                    this.skip--;
                    base.Dequeue();
                    return null;
                }
                return null;
            }

            public bool AllRequestedObjectsProcessed
            {
                get
                {
                    return (((this.firstOrLastSpecified && (this.last == 0)) && (this.first != 0)) && (this.streamedObjectCount >= this.first));
                }
            }
        }

        private class UniquePSObjectHelper
        {
            private int notePropertyCount;
            internal readonly PSObject WrittenObject;

            internal UniquePSObjectHelper(PSObject o, int notePropertyCount)
            {
                this.WrittenObject = o;
                this.notePropertyCount = notePropertyCount;
            }

            internal int NotePropertyCount
            {
                get
                {
                    return this.notePropertyCount;
                }
            }
        }
    }
}

