namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;

    internal class InnerFormatShapeCommand : InnerFormatShapeCommandBase
    {
        private TypeInfoDataBase _typeInfoDataBase;
        private int enumerationLimit = 4;
        private MshExpressionFactory expressionFactory;
        private FormatObjectDeserializer formatObjectDeserializer;
        private FormattingCommandLineParameters parameters;
        private FormatShape shape;
        private FormatViewManager viewManager = new FormatViewManager();

        internal InnerFormatShapeCommand(FormatShape shape)
        {
            this.shape = shape;
        }

        internal override void BeginProcessing()
        {
            base.BeginProcessing();
            this.enumerationLimit = FormatEnumerationLimit();
            this.expressionFactory = new MshExpressionFactory();
            this.formatObjectDeserializer = new FormatObjectDeserializer(base.TerminatingErrorContext);
        }

        private GroupTransition ComputeGroupTransition(PSObject so)
        {
            InnerFormatShapeCommandBase.FormattingContext context = (InnerFormatShapeCommandBase.FormattingContext) base.contextManager.Peek();
            if (context.state == InnerFormatShapeCommandBase.FormattingContext.State.document)
            {
                this.viewManager.ViewGenerator.UpdateGroupingKeyValue(so);
                return GroupTransition.enter;
            }
            if (!this.viewManager.ViewGenerator.UpdateGroupingKeyValue(so))
            {
                return GroupTransition.none;
            }
            return GroupTransition.startNew;
        }

        internal ScriptBlock CreateScriptBlock(string scriptText)
        {
            ScriptBlock block = this.OuterCmdlet().InvokeCommand.NewScriptBlock(scriptText);
            block.DebuggerStepThrough = true;
            return block;
        }

        internal override void EndProcessing()
        {
            while (true)
            {
                InnerFormatShapeCommandBase.FormattingContext context = (InnerFormatShapeCommandBase.FormattingContext) base.contextManager.Peek();
                if (context.state == InnerFormatShapeCommandBase.FormattingContext.State.none)
                {
                    return;
                }
                if (context.state == InnerFormatShapeCommandBase.FormattingContext.State.group)
                {
                    this.PopGroup();
                }
                else if (context.state == InnerFormatShapeCommandBase.FormattingContext.State.document)
                {
                    FormatEndData o = new FormatEndData();
                    this.WriteObject(o);
                    base.contextManager.Pop();
                }
            }
        }

        internal static int FormatEnumerationLimit()
        {
            object obj2 = null;
            try
            {
                if (LocalPipeline.GetExecutionContextFromTLS() != null)
                {
                    obj2 = LocalPipeline.GetExecutionContextFromTLS().SessionState.PSVariable.GetValue("global:FormatEnumerationLimit");
                }
            }
            catch (ProviderNotFoundException)
            {
            }
            catch (ProviderInvocationException)
            {
            }
            if (obj2 is int)
            {
                return (int) obj2;
            }
            return 4;
        }

        private EnumerableExpansion GetExpansionState(PSObject so)
        {
            if ((this.parameters != null) && this.parameters.expansion.HasValue)
            {
                return this.parameters.expansion.Value;
            }
            ConsolidatedString internalTypeNames = so.InternalTypeNames;
            return DisplayDataQuery.GetEnumerableExpansionFromType(this.expressionFactory, this._typeInfoDataBase, internalTypeNames);
        }

        private void PopGroup()
        {
            GroupEndData o = this.viewManager.ViewGenerator.GenerateGroupEndData();
            this.WriteObject(o);
            base.contextManager.Pop();
        }

        private void ProcessCoreOutOfBand(PSObject so, int count)
        {
            string msg = StringUtil.Format(FormatAndOut_format_xxx.IEnum_Header, new object[0]);
            this.SendCommentOutOfBand(msg);
            this.ProcessOutOfBand(so);
            switch (count)
            {
                case 0:
                    msg = StringUtil.Format(FormatAndOut_format_xxx.IEnum_NoObjects, new object[0]);
                    break;

                case 1:
                    msg = StringUtil.Format(FormatAndOut_format_xxx.IEnum_OneObject, new object[0]);
                    break;

                default:
                    msg = StringUtil.Format(FormatAndOut_format_xxx.IEnum_ManyObjects, count);
                    break;
            }
            this.SendCommentOutOfBand(msg);
        }

        private void ProcessObject(PSObject so)
        {
            if (this.formatObjectDeserializer.IsFormatInfoData(so))
            {
                this.WriteObject(so);
            }
            else if (!this.ProcessOutOfBandObjectOutsideDocumentSequence(so))
            {
                InnerFormatShapeCommandBase.FormattingContext context = (InnerFormatShapeCommandBase.FormattingContext) base.contextManager.Peek();
                if (context.state == InnerFormatShapeCommandBase.FormattingContext.State.none)
                {
                    this.viewManager.Initialize(base.TerminatingErrorContext, this.expressionFactory, this._typeInfoDataBase, so, this.shape, this.parameters);
                    this.WriteFormatStartData(so);
                    base.contextManager.Push(new InnerFormatShapeCommandBase.FormattingContext(InnerFormatShapeCommandBase.FormattingContext.State.document));
                }
                if (!this.ProcessOutOfBandObjectInsideDocumentSequence(so))
                {
                    switch (this.ComputeGroupTransition(so))
                    {
                        case GroupTransition.enter:
                            this.PushGroup(so);
                            this.WritePayloadObject(so);
                            return;

                        case GroupTransition.exit:
                            this.WritePayloadObject(so);
                            this.PopGroup();
                            return;

                        case GroupTransition.startNew:
                            this.PopGroup();
                            this.PushGroup(so);
                            this.WritePayloadObject(so);
                            return;
                    }
                    this.WritePayloadObject(so);
                }
            }
        }

        private bool ProcessOutOfBand(PSObject so)
        {
            return this.ProcessOutOfBand(so, false);
        }

        private bool ProcessOutOfBand(PSObject so, bool isProcessingError)
        {
            List<ErrorRecord> list;
            FormatEntryData o = OutOfBandFormatViewManager.GenerateOutOfBandData(base.TerminatingErrorContext, this.expressionFactory, this._typeInfoDataBase, so, this.enumerationLimit, true, out list);
            if (!isProcessingError)
            {
                this.WriteErrorRecords(list);
            }
            if (o != null)
            {
                this.WriteObject(o);
                return true;
            }
            return false;
        }

        private bool ProcessOutOfBandObjectInsideDocumentSequence(PSObject so)
        {
            if (!this.ShouldProcessOutOfBand)
            {
                return false;
            }
            ConsolidatedString internalTypeNames = so.InternalTypeNames;
            if (this.viewManager.ViewGenerator.IsObjectApplicable(internalTypeNames))
            {
                return false;
            }
            return this.ProcessOutOfBand(so);
        }

        private bool ProcessOutOfBandObjectOutsideDocumentSequence(PSObject so)
        {
            if (this.ShouldProcessOutOfBand)
            {
                List<ErrorRecord> list;
                FormatEntryData o = null;
                if (so.InternalTypeNames.Count == 0)
                {
                    return false;
                }
                o = OutOfBandFormatViewManager.GenerateOutOfBandData(base.TerminatingErrorContext, this.expressionFactory, this._typeInfoDataBase, so, this.enumerationLimit, false, out list);
                this.WriteErrorRecords(list);
                if (o != null)
                {
                    this.WriteObject(o);
                    return true;
                }
            }
            return false;
        }

        internal override void ProcessRecord()
        {
            this._typeInfoDataBase = this.OuterCmdlet().Context.FormatDBManager.GetTypeInfoDataBase();
            PSObject obj2 = this.ReadObject();
            if ((obj2 != null) && (obj2 != AutomationNull.Value))
            {
                IEnumerable enumerable = PSObjectHelper.GetEnumerable(obj2);
                if (enumerable == null)
                {
                    this.ProcessObject(obj2);
                }
                else
                {
                    switch (this.GetExpansionState(obj2))
                    {
                        case EnumerableExpansion.EnumOnly:
                            foreach (object obj3 in enumerable)
                            {
                                this.ProcessObject(PSObjectHelper.AsPSObject(obj3));
                            }
                            break;

                        case EnumerableExpansion.Both:
                        {
                            int count = 0;
                            IEnumerator enumerator2 = enumerable.GetEnumerator();
                            {
                                while (enumerator2.MoveNext())
                                {
                                    object current = enumerator2.Current;
                                    count++;
                                }
                            }
                            this.ProcessCoreOutOfBand(obj2, count);
                            foreach (object obj4 in enumerable)
                            {
                                this.ProcessObject(PSObjectHelper.AsPSObject(obj4));
                            }
                            break;
                        }
                        default:
                            this.ProcessObject(obj2);
                            break;
                    }
                }
            }
        }

        private void PushGroup(PSObject firstObjectInGroup)
        {
            GroupStartData o = this.viewManager.ViewGenerator.GenerateGroupStartData(firstObjectInGroup, this.enumerationLimit);
            this.WriteObject(o);
            base.contextManager.Push(new InnerFormatShapeCommandBase.FormattingContext(InnerFormatShapeCommandBase.FormattingContext.State.group));
        }

        private void SendCommentOutOfBand(string msg)
        {
            FormatEntryData o = OutOfBandFormatViewManager.GenerateOutOfBandObjectAsToString(PSObjectHelper.AsPSObject(msg));
            if (o != null)
            {
                this.WriteObject(o);
            }
        }

        internal void SetCommandLineParameters(FormattingCommandLineParameters commandLineParameters)
        {
            this.parameters = commandLineParameters;
        }

        private void WriteErrorRecords(List<ErrorRecord> errorRecordList)
        {
            if (errorRecordList != null)
            {
                foreach (ErrorRecord record in errorRecordList)
                {
                    this.ProcessOutOfBand(PSObjectHelper.AsPSObject(record), true);
                }
            }
        }

        private void WriteFormatStartData(PSObject so)
        {
            FormatStartData o = this.viewManager.ViewGenerator.GenerateStartData(so);
            this.WriteObject(o);
        }

        protected void WriteInternalErrorMessage(string message)
        {
            FormatEntryData o = new FormatEntryData {
                outOfBand = true
            };
            ComplexViewEntry entry = new ComplexViewEntry();
            FormatEntry item = new FormatEntry();
            entry.formatValueList.Add(item);
            item.formatValueList.Add(new FormatNewLine());
            FormatTextField field = new FormatTextField {
                text = message
            };
            item.formatValueList.Add(field);
            item.formatValueList.Add(new FormatNewLine());
            o.formatEntryInfo = entry;
            this.WriteObject(o);
        }

        private void WritePayloadObject(PSObject so)
        {
            FormatEntryData o = this.viewManager.ViewGenerator.GeneratePayload(so, this.enumerationLimit);
            o.SetStreamTypeFromPSObject(so);
            this.WriteObject(o);
            List<ErrorRecord> errorRecordList = this.viewManager.ViewGenerator.ErrorManager.DrainFailedResultList();
            this.WriteErrorRecords(errorRecordList);
        }

        private bool ShouldProcessOutOfBand
        {
            get
            {
                if ((this.shape != FormatShape.Undefined) && (this.parameters != null))
                {
                    return !this.parameters.forceFormattingAlsoOnOutOfBand;
                }
                return true;
            }
        }

        private enum GroupTransition
        {
            none,
            enter,
            exit,
            startNew
        }
    }
}

