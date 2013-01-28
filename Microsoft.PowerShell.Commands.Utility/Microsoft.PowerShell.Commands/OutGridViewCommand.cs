namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;

    [Cmdlet("Out", "GridView", DefaultParameterSetName="PassThru", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113364")]
    public class OutGridViewCommand : PSCmdlet, IDisposable
    {
        private const string DataNotQualifiedForGridView = "DataNotQualifiedForGridView";
        private MshExpressionFactory expressionFactory;
        private GridHeader gridHeader;
        private PSObject inputObject = AutomationNull.Value;
        private OutputModeOption outputMode;
        private const string RemotingNotSupported = "RemotingNotSupported";
        internal string title;
        private TypeInfoDataBase typeInfoDataBase;
        private SwitchParameter wait;
        private OutWindowProxy windowProxy;

        protected override void BeginProcessing()
        {
            this.expressionFactory = new MshExpressionFactory();
            if (this.title != null)
            {
                this.windowProxy = new OutWindowProxy(this.title, this.outputMode, this);
            }
            else
            {
                this.windowProxy = new OutWindowProxy(base.MyInvocation.Line, this.outputMode, this);
            }
            this.typeInfoDataBase = base.Context.FormatDBManager.GetTypeInfoDataBase();
        }

        internal string ConvertToString(PSObject liveObject)
        {
            StringFormatError formatErrorObject = new StringFormatError();
            string str = PSObjectHelper.SmartToString(liveObject, this.expressionFactory, InnerFormatShapeCommand.FormatEnumerationLimit(), formatErrorObject);
            if (formatErrorObject.exception != null)
            {
                base.WriteError(new ErrorRecord(formatErrorObject.exception, "ErrorFormattingType", ErrorCategory.InvalidResult, liveObject));
            }
            return str;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isDisposing)
        {
            if (isDisposing && (this.windowProxy != null))
            {
                this.windowProxy.Dispose();
                this.windowProxy = null;
            }
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
            if (this.windowProxy != null)
            {
                if ((this.Wait != 0) || (this.OutputMode != OutputModeOption.None))
                {
                    this.windowProxy.BlockUntillClosed();
                }
                List<PSObject> selectedItems = this.windowProxy.GetSelectedItems();
                if ((this.OutputMode != OutputModeOption.None) && (selectedItems != null))
                {
                    foreach (PSObject obj2 in selectedItems)
                    {
                        if (obj2 != null)
                        {
                            PSPropertyInfo info = obj2.Properties["OutGridViewOriginalObject"];
                            if (info == null)
                            {
                                break;
                            }
                            base.WriteObject(info.Value, false);
                        }
                    }
                }
            }
        }

        ~OutGridViewCommand()
        {
            this.Dispose(false);
        }

        private void ProcessObject(PSObject input)
        {
            if (this.windowProxy.IsWindowClosed())
            {
                LocalPipeline currentlyRunningPipeline = (LocalPipeline) base.Context.CurrentRunspace.GetCurrentlyRunningPipeline();
                if ((currentlyRunningPipeline != null) && !currentlyRunningPipeline.IsStopping)
                {
                    currentlyRunningPipeline.StopAsync();
                }
            }
            else
            {
                object baseObject = input.BaseObject;
                if (((baseObject is ScriptBlock) || (baseObject is SwitchParameter)) || (((baseObject is PSReference) || (baseObject is FormatInfoData)) || (baseObject is PSObject)))
                {
                    ErrorRecord errorRecord = new ErrorRecord(new FormatException(StringUtil.Format(FormatAndOut_out_gridview.DataNotQualifiedForGridView, new object[0])), "DataNotQualifiedForGridView", ErrorCategory.InvalidType, null);
                    base.ThrowTerminatingError(errorRecord);
                }
                if (this.gridHeader == null)
                {
                    this.windowProxy.ShowWindow();
                    this.gridHeader = GridHeader.ConstructGridHeader(input, this);
                }
                else
                {
                    this.gridHeader.ProcessInputObject(input);
                }
                Exception lastException = this.windowProxy.GetLastException();
                if (lastException != null)
                {
                    ErrorRecord record2 = new ErrorRecord(lastException, "ManagementListInvocationException", ErrorCategory.OperationStopped, null);
                    base.ThrowTerminatingError(record2);
                }
            }
        }

        protected override void ProcessRecord()
        {
            if ((this.inputObject != null) && (this.inputObject != AutomationNull.Value))
            {
                IDictionary baseObject = this.inputObject.BaseObject as IDictionary;
                if (baseObject != null)
                {
                    foreach (DictionaryEntry entry in baseObject)
                    {
                        this.ProcessObject(PSObjectHelper.AsPSObject(entry));
                    }
                }
                else
                {
                    this.ProcessObject(this.inputObject);
                }
            }
        }

        protected override void StopProcessing()
        {
            if ((this.Wait != 0) || (this.OutputMode != OutputModeOption.None))
            {
                this.windowProxy.CloseWindow();
            }
        }

        [Parameter(ValueFromPipeline=true)]
        public PSObject InputObject
        {
            get
            {
                return this.inputObject;
            }
            set
            {
                this.inputObject = value;
            }
        }

        [Parameter(ParameterSetName="OutputMode")]
        public OutputModeOption OutputMode
        {
            get
            {
                return this.outputMode;
            }
            set
            {
                this.outputMode = value;
            }
        }

        [Parameter(ParameterSetName="PassThru")]
        public SwitchParameter PassThru
        {
            get
            {
                if (this.outputMode != OutputModeOption.Multiple)
                {
                    return new SwitchParameter(false);
                }
                return new SwitchParameter(true);
            }
            set
            {
                this.OutputMode = value.IsPresent ? OutputModeOption.Multiple : OutputModeOption.None;
            }
        }

        [Parameter, ValidateNotNullOrEmpty]
        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = value;
            }
        }

        [Parameter(ParameterSetName="Wait")]
        public SwitchParameter Wait
        {
            get
            {
                return this.wait;
            }
            set
            {
                this.wait = value;
            }
        }

        internal abstract class GridHeader
        {
            protected OutGridViewCommand parentCmd;

            internal GridHeader(OutGridViewCommand parentCmd)
            {
                this.parentCmd = parentCmd;
            }

            internal static OutGridViewCommand.GridHeader ConstructGridHeader(PSObject input, OutGridViewCommand parentCmd)
            {
                if (!DefaultScalarTypes.IsTypeInList(input.TypeNames) && !OutOfBandFormatViewManager.IsPropertyLessObject(input))
                {
                    return new OutGridViewCommand.NonscalarTypeHeader(parentCmd, input);
                }
                return new OutGridViewCommand.ScalarTypeHeader(parentCmd, input);
            }

            internal abstract void ProcessInputObject(PSObject input);
        }

        internal class HeteroTypeHeader : OutGridViewCommand.GridHeader
        {
            internal HeteroTypeHeader(OutGridViewCommand parentCmd, PSObject input) : base(parentCmd)
            {
                base.parentCmd.windowProxy.AddHeteroViewColumnsAndItem(input);
            }

            internal override void ProcessInputObject(PSObject input)
            {
                base.parentCmd.windowProxy.AddHeteroViewItem(input);
            }
        }

        internal class NonscalarTypeHeader : OutGridViewCommand.GridHeader
        {
            private AppliesTo appliesTo;

            internal NonscalarTypeHeader(OutGridViewCommand parentCmd, PSObject input) : base(parentCmd)
            {
                TableView tableView = new TableView();
                tableView.Initialize(parentCmd.expressionFactory, parentCmd.typeInfoDataBase);
                ViewDefinition definition = DisplayDataQuery.GetViewByShapeAndType(parentCmd.expressionFactory, parentCmd.typeInfoDataBase, FormatShape.Table, input.TypeNames, null);
                if (definition != null)
                {
                    parentCmd.windowProxy.AddColumnsAndItem(input, tableView, (TableControlBody) definition.mainControl);
                    this.appliesTo = definition.appliesTo;
                }
                else
                {
                    parentCmd.windowProxy.AddColumnsAndItem(input, tableView);
                    this.appliesTo = new AppliesTo();
                    int num = 0;
                    foreach (string str in input.TypeNames)
                    {
                        if ((num > 0) && (str.Equals(typeof(object).FullName, StringComparison.OrdinalIgnoreCase) || str.Equals(typeof(MarshalByRefObject).FullName, StringComparison.OrdinalIgnoreCase)))
                        {
                            break;
                        }
                        this.appliesTo.AddAppliesToType(str);
                        num++;
                    }
                }
            }

            internal override void ProcessInputObject(PSObject input)
            {
                foreach (TypeOrGroupReference reference in this.appliesTo.referenceList)
                {
                    if (reference is TypeReference)
                    {
                        string name = reference.name;
                        Deserializer.AddDeserializationPrefix(ref name);
                        for (int i = 0; i < input.TypeNames.Count; i++)
                        {
                            if (reference.name.Equals(input.TypeNames[i], StringComparison.OrdinalIgnoreCase) || name.Equals(input.TypeNames[i], StringComparison.OrdinalIgnoreCase))
                            {
                                base.parentCmd.windowProxy.AddItem(input);
                                return;
                            }
                        }
                    }
                    else
                    {
                        foreach (TypeGroupDefinition definition in base.parentCmd.typeInfoDataBase.typeGroupSection.typeGroupDefinitionList)
                        {
                            if (definition.name.Equals(reference.name, StringComparison.OrdinalIgnoreCase))
                            {
                                foreach (TypeReference reference2 in definition.typeReferenceList)
                                {
                                    string type = reference2.name;
                                    Deserializer.AddDeserializationPrefix(ref type);
                                    if ((input.TypeNames.Count > 0) && (reference2.name.Equals(input.TypeNames[0], StringComparison.OrdinalIgnoreCase) || type.Equals(input.TypeNames[0], StringComparison.OrdinalIgnoreCase)))
                                    {
                                        base.parentCmd.windowProxy.AddItem(input);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                base.parentCmd.gridHeader = new OutGridViewCommand.HeteroTypeHeader(base.parentCmd, input);
            }
        }

        internal class ScalarTypeHeader : OutGridViewCommand.GridHeader
        {
            private Type originalScalarType;

            internal ScalarTypeHeader(OutGridViewCommand parentCmd, PSObject input) : base(parentCmd)
            {
                this.originalScalarType = input.BaseObject.GetType();
                base.parentCmd.windowProxy.AddColumnsAndItem(input);
            }

            internal override void ProcessInputObject(PSObject input)
            {
                if (!this.originalScalarType.Equals(input.BaseObject.GetType()))
                {
                    base.parentCmd.gridHeader = new OutGridViewCommand.HeteroTypeHeader(base.parentCmd, input);
                }
                else
                {
                    base.parentCmd.windowProxy.AddItem(input);
                }
            }
        }
    }
}

