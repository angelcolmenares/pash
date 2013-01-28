namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Reflection;

    internal class OutCommandInner : ImplementationCommandBase
    {
        private FormattedObjectsCache cache;
        private CommandWrapper command;
        private FormatMessagesContextManager ctxManager = new FormatMessagesContextManager();
        private FormattingState currentFormattingState;
        private FormatObjectDeserializer formatObjectDeserializer;
        private FormattingHint formattingHint;
        private Microsoft.PowerShell.Commands.Internal.Format.LineOutput lo;
        [TraceSource("format_out_OutCommandInner", "OutCommandInner")]
        internal static PSTraceSource tracer = PSTraceSource.GetTracer("format_out_OutCommandInner", "OutCommandInner");

        private Array ApplyFormatting(object o)
        {
            if (this.command == null)
            {
                this.command = new CommandWrapper();
                this.command.Initialize(this.OuterCmdlet().Context, "format-default", typeof(FormatDefaultCommand));
            }
            return this.command.Process(o);
        }

        internal override void BeginProcessing()
        {
            base.BeginProcessing();
            this.formatObjectDeserializer = new FormatObjectDeserializer(base.TerminatingErrorContext);
            this.ctxManager.contextCreation = new FormatMessagesContextManager.FormatContextCreationCallback(this.CreateOutputContext);
            this.ctxManager.fs = new FormatMessagesContextManager.FormatStartCallback(this.ProcessFormatStart);
            this.ctxManager.fe = new FormatMessagesContextManager.FormatEndCallback(this.ProcessFormatEnd);
            this.ctxManager.gs = new FormatMessagesContextManager.GroupStartCallback(this.ProcessGroupStart);
            this.ctxManager.ge = new FormatMessagesContextManager.GroupEndCallback(this.ProcessGroupEnd);
            this.ctxManager.payload = new FormatMessagesContextManager.PayloadCallback(this.ProcessPayload);
        }

        private FormatMessagesContextManager.OutputContext CreateOutputContext(FormatMessagesContextManager.OutputContext parentContext, FormatInfoData formatInfoData)
        {
            FormatStartData formatData = formatInfoData as FormatStartData;
            if (formatData != null)
            {
                return new FormatOutputContext(parentContext, formatData);
            }
            GroupStartData data2 = formatInfoData as GroupStartData;
            if (data2 == null)
            {
                return null;
            }
            GroupOutputContext context2 = null;
            switch (this.ActiveFormattingShape)
            {
                case FormatShape.Table:
                    context2 = new TableOutputContext(this, parentContext, data2);
                    break;

                case FormatShape.List:
                    context2 = new ListOutputContext(this, parentContext, data2);
                    break;

                case FormatShape.Wide:
                    context2 = new WideOutputContext(this, parentContext, data2);
                    break;

                case FormatShape.Complex:
                    context2 = new ComplexOutputContext(this, parentContext, data2);
                    break;
            }
            context2.Initialize();
            return context2;
        }

        private void DrainCache()
        {
            if (this.cache != null)
            {
                List<PacketInfoData> list = this.cache.Drain();
                if (list != null)
                {
                    foreach (object obj2 in list)
                    {
                        this.ctxManager.Process(obj2);
                    }
                }
            }
        }

        internal override void EndProcessing()
        {
            base.EndProcessing();
            if (this.command != null)
            {
                Array array = this.command.ShutDown();
                if (array != null)
                {
                    foreach (object obj2 in array)
                    {
                        this.ProcessObject(PSObjectHelper.AsPSObject(obj2));
                    }
                }
            }
            if (this.LineOutput.RequiresBuffering)
            {
                Microsoft.PowerShell.Commands.Internal.Format.LineOutput.DoPlayBackCall playback = new Microsoft.PowerShell.Commands.Internal.Format.LineOutput.DoPlayBackCall(this.DrainCache);
                this.LineOutput.ExecuteBufferPlayBack(playback);
            }
            else
            {
                this.DrainCache();
            }
        }

        protected override void InternalDispose()
        {
            base.InternalDispose();
            if (this.command != null)
            {
                this.command.Dispose();
                this.command = null;
            }
        }

        private bool NeedsPreprocessing(object o)
        {
            FormatEntryData data = o as FormatEntryData;
            if (data != null)
            {
                if (!data.outOfBand)
                {
                    this.ValidateCurrentFormattingState(FormattingState.InsideGroup, o);
                }
                return false;
            }
            if (o is FormatStartData)
            {
                if (this.currentFormattingState == FormattingState.InsideGroup)
                {
                    this.EndProcessing();
                    this.BeginProcessing();
                }
                this.ValidateCurrentFormattingState(FormattingState.Reset, o);
                this.currentFormattingState = FormattingState.Formatting;
                return false;
            }
            if (o is FormatEndData)
            {
                this.ValidateCurrentFormattingState(FormattingState.Formatting, o);
                this.currentFormattingState = FormattingState.Reset;
                return false;
            }
            if (o is GroupStartData)
            {
                this.ValidateCurrentFormattingState(FormattingState.Formatting, o);
                this.currentFormattingState = FormattingState.InsideGroup;
                return false;
            }
            if (o is GroupEndData)
            {
                this.ValidateCurrentFormattingState(FormattingState.InsideGroup, o);
                this.currentFormattingState = FormattingState.Formatting;
                return false;
            }
            return true;
        }

        private void ProcessCachedGroup(FormatStartData formatStartData, List<PacketInfoData> objects)
        {
            this.formattingHint = null;
            TableHeaderInfo shapeInfo = formatStartData.shapeInfo as TableHeaderInfo;
            if (shapeInfo != null)
            {
                this.ProcessCachedGroupOnTable(shapeInfo, objects);
            }
            else
            {
                WideViewHeaderInfo wvhi = formatStartData.shapeInfo as WideViewHeaderInfo;
                if (wvhi != null)
                {
                    this.ProcessCachedGroupOnWide(wvhi, objects);
                }
            }
        }

        private void ProcessCachedGroupOnTable(TableHeaderInfo thi, List<PacketInfoData> objects)
        {
            if (thi.tableColumnInfoList.Count != 0)
            {
                int[] numArray = new int[thi.tableColumnInfoList.Count];
                for (int i = 0; i < thi.tableColumnInfoList.Count; i++)
                {
                    string label = thi.tableColumnInfoList[i].label;
                    if (string.IsNullOrEmpty(label))
                    {
                        label = thi.tableColumnInfoList[i].propertyName;
                    }
                    if (string.IsNullOrEmpty(label))
                    {
                        numArray[i] = 0;
                    }
                    else
                    {
                        numArray[i] = this.lo.DisplayCells.Length(label);
                    }
                }
                foreach (PacketInfoData data in objects)
                {
                    FormatEntryData data2 = data as FormatEntryData;
                    if (data2 != null)
                    {
                        TableRowEntry formatEntryInfo = data2.formatEntryInfo as TableRowEntry;
                        int index = 0;
                        foreach (FormatPropertyField field in formatEntryInfo.formatPropertyFieldList)
                        {
                            int num2 = this.lo.DisplayCells.Length(field.propertyValue);
                            if (numArray[index] < num2)
                            {
                                numArray[index] = num2;
                            }
                            index++;
                        }
                    }
                }
                TableFormattingHint hint = new TableFormattingHint {
                    columnWidths = numArray
                };
                this.formattingHint = hint;
            }
        }

        private void ProcessCachedGroupOnWide(WideViewHeaderInfo wvhi, List<PacketInfoData> objects)
        {
            if (wvhi.columns == 0)
            {
                int num = 0;
                foreach (PacketInfoData data in objects)
                {
                    FormatEntryData data2 = data as FormatEntryData;
                    if (data2 != null)
                    {
                        WideViewEntry formatEntryInfo = data2.formatEntryInfo as WideViewEntry;
                        FormatPropertyField formatPropertyField = formatEntryInfo.formatPropertyField;
                        if (!string.IsNullOrEmpty(formatPropertyField.propertyValue))
                        {
                            int num2 = this.lo.DisplayCells.Length(formatPropertyField.propertyValue);
                            if (num2 > num)
                            {
                                num = num2;
                            }
                        }
                    }
                }
                WideFormattingHint hint = new WideFormattingHint {
                    maxWidth = num
                };
                this.formattingHint = hint;
            }
        }

        private void ProcessFormatEnd(FormatEndData fe, FormatMessagesContextManager.OutputContext c)
        {
            this.LineOutput.WriteLine("");
        }

        private void ProcessFormatStart(FormatMessagesContextManager.OutputContext c)
        {
            this.LineOutput.WriteLine("");
        }

        private void ProcessGroupEnd(GroupEndData ge, FormatMessagesContextManager.OutputContext c)
        {
            ((GroupOutputContext) c).GroupEnd();
            this.LineOutput.WriteLine("");
        }

        private void ProcessGroupStart(FormatMessagesContextManager.OutputContext c)
        {
            GroupOutputContext context = (GroupOutputContext) c;
            if (context.Data.groupingEntry != null)
            {
                this.lo.WriteLine("");
                ComplexWriter writer = new ComplexWriter();
                writer.Initialize(this.lo, this.lo.ColumnNumber);
                writer.WriteObject(context.Data.groupingEntry.formatValueList);
                this.LineOutput.WriteLine("");
            }
            context.GroupStart();
        }

        private bool ProcessObject(PSObject so)
        {
            object o = this.formatObjectDeserializer.Deserialize(so);
            if (this.NeedsPreprocessing(o))
            {
                return false;
            }
            if (this.cache == null)
            {
                this.cache = new FormattedObjectsCache(this.LineOutput.RequiresBuffering);
            }
            FormatStartData data = o as FormatStartData;
            if ((data != null) && (data.autosizeInfo != null))
            {
                FormattedObjectsCache.ProcessCachedGroupNotification callBack = new FormattedObjectsCache.ProcessCachedGroupNotification(this.ProcessCachedGroup);
                this.cache.EnableGroupCaching(callBack, data.autosizeInfo.objectCount);
            }
            List<PacketInfoData> list = this.cache.Add((PacketInfoData) o);
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    this.ctxManager.Process(list[i]);
                }
            }
            return true;
        }

        private void ProcessOutOfBandPayload(FormatEntryData fed)
        {
            RawTextFormatEntry formatEntryInfo = fed.formatEntryInfo as RawTextFormatEntry;
            if (formatEntryInfo != null)
            {
                if (fed.isHelpObject)
                {
                    ComplexWriter writer = new ComplexWriter();
                    writer.Initialize(this.lo, this.lo.ColumnNumber);
                    writer.WriteString(formatEntryInfo.text);
                }
                else
                {
                    this.lo.WriteLine(formatEntryInfo.text);
                }
            }
            else
            {
                ComplexViewEntry entry2 = fed.formatEntryInfo as ComplexViewEntry;
                if ((entry2 != null) && (entry2.formatValueList != null))
                {
                    ComplexWriter writer2 = new ComplexWriter();
                    writer2.Initialize(this.lo, this.lo.ColumnNumber);
                    writer2.WriteObject(entry2.formatValueList);
                }
                else
                {
                    ListViewEntry lve = fed.formatEntryInfo as ListViewEntry;
                    if ((lve != null) && (lve.listViewFieldList != null))
                    {
                        ListWriter writer3 = new ListWriter();
                        this.lo.WriteLine("");
                        string[] properties = ListOutputContext.GetProperties(lve);
                        writer3.Initialize(properties, this.lo.ColumnNumber, this.lo.DisplayCells);
                        string[] values = ListOutputContext.GetValues(lve);
                        writer3.WriteProperties(values, this.lo);
                        this.lo.WriteLine("");
                    }
                }
            }
        }

        private void ProcessPayload(FormatEntryData fed, FormatMessagesContextManager.OutputContext c)
        {
            if (fed == null)
            {
                PSTraceSource.NewArgumentNullException("fed");
            }
            if (fed.formatEntryInfo == null)
            {
                PSTraceSource.NewArgumentNullException("fed.formatEntryInfo");
            }
            WriteStreamType writeStream = this.lo.WriteStream;
            try
            {
                this.lo.WriteStream = fed.writeStream;
                if (c == null)
                {
                    this.ProcessOutOfBandPayload(fed);
                }
                else
                {
                    ((GroupOutputContext) c).ProcessPayload(fed);
                }
            }
            finally
            {
                this.lo.WriteStream = writeStream;
            }
        }

        internal override void ProcessRecord()
        {
            PSObject so = this.ReadObject();
            if (((so != null) && (so != AutomationNull.Value)) && !this.ProcessObject(so))
            {
                Array array = this.ApplyFormatting(so);
                if (array != null)
                {
                    foreach (object obj3 in array)
                    {
                        PSObject obj4 = PSObjectHelper.AsPSObject(obj3);
                        obj4.IsHelpObject = so.IsHelpObject;
                        this.ProcessObject(obj4);
                    }
                }
            }
        }

        private FormattingHint RetrieveFormattingHint()
        {
            FormattingHint formattingHint = this.formattingHint;
            this.formattingHint = null;
            return formattingHint;
        }

        private void ValidateCurrentFormattingState(FormattingState expectedFormattingState, object obj)
        {
            if (this.currentFormattingState != expectedFormattingState)
            {
                string str = "format-*";
                StartData data = obj as StartData;
                if (data != null)
                {
                    if (data.shapeInfo.GetType() == typeof(WideViewHeaderInfo))
                    {
                        str = "format-wide";
                    }
                    else if (data.shapeInfo.GetType() == typeof(TableHeaderInfo))
                    {
                        str = "format-table";
                    }
                    else if (data.shapeInfo.GetType() == typeof(ListViewHeaderInfo))
                    {
                        str = "format-list";
                    }
                    else if (data.shapeInfo.GetType() == typeof(ComplexViewHeaderInfo))
                    {
                        str = "format-complex";
                    }
                }
                string message = StringUtil.Format(FormatAndOut_out_xxx.OutLineOutput_OutOfSequencePacket, obj.GetType().FullName, str);
                ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(), "ConsoleLineOutputOutOfSequencePacket", ErrorCategory.InvalidData, null) {
                    ErrorDetails = new ErrorDetails(message)
                };
                base.TerminatingErrorContext.ThrowTerminatingError(errorRecord);
            }
        }

        private FormatShape ActiveFormattingShape
        {
            get
            {
                FormatShape table = FormatShape.Table;
                FormatOutputContext formatContext = this.FormatContext;
                if ((formatContext != null) && (formatContext.Data.shapeInfo != null))
                {
                    if (formatContext.Data.shapeInfo is TableHeaderInfo)
                    {
                        return FormatShape.Table;
                    }
                    if (formatContext.Data.shapeInfo is ListViewHeaderInfo)
                    {
                        return FormatShape.List;
                    }
                    if (formatContext.Data.shapeInfo is WideViewHeaderInfo)
                    {
                        return FormatShape.Wide;
                    }
                    if (formatContext.Data.shapeInfo is ComplexViewHeaderInfo)
                    {
                        return FormatShape.Complex;
                    }
                }
                return table;
            }
        }

        private FormatOutputContext FormatContext
        {
            get
            {
                for (FormatMessagesContextManager.OutputContext context = this.ctxManager.ActiveOutputContext; context != null; context = context.ParentContext)
                {
                    FormatOutputContext context2 = context as FormatOutputContext;
                    if (context2 != null)
                    {
                        return context2;
                    }
                }
                return null;
            }
        }

        internal Microsoft.PowerShell.Commands.Internal.Format.LineOutput LineOutput
        {
            get
            {
                return this.lo;
            }
            set
            {
                this.lo = value;
            }
        }

        private ShapeInfo ShapeInfoOnFormatContext
        {
            get
            {
                FormatOutputContext formatContext = this.FormatContext;
                if (formatContext == null)
                {
                    return null;
                }
                return formatContext.Data.shapeInfo;
            }
        }

        private sealed class ComplexOutputContext : OutCommandInner.GroupOutputContext
        {
            private ComplexWriter writer;

            internal ComplexOutputContext(OutCommandInner cmd, FormatMessagesContextManager.OutputContext parentContext, GroupStartData formatData) : base(cmd, parentContext, formatData)
            {
                this.writer = new ComplexWriter();
            }

            internal override void Initialize()
            {
                this.writer.Initialize(base.InnerCommand.lo, base.InnerCommand.lo.ColumnNumber);
            }

            internal override void ProcessPayload(FormatEntryData fed)
            {
                ComplexViewEntry formatEntryInfo = fed.formatEntryInfo as ComplexViewEntry;
                if ((formatEntryInfo != null) && (formatEntryInfo.formatValueList != null))
                {
                    this.writer.WriteObject(formatEntryInfo.formatValueList);
                }
            }
        }

        private class FormatOutputContext : FormatMessagesContextManager.OutputContext
        {
            private FormatStartData formatData;

            internal FormatOutputContext(FormatMessagesContextManager.OutputContext parentContext, FormatStartData formatData) : base(parentContext)
            {
                this.formatData = formatData;
            }

            internal FormatStartData Data
            {
                get
                {
                    return this.formatData;
                }
            }
        }

        private abstract class FormattingHint
        {
            protected FormattingHint()
            {
            }
        }

        private enum FormattingState
        {
            Reset,
            Formatting,
            InsideGroup
        }

        private abstract class GroupOutputContext : FormatMessagesContextManager.OutputContext
        {
            private OutCommandInner cmd;
            private GroupStartData formatData;

            internal GroupOutputContext(OutCommandInner cmd, FormatMessagesContextManager.OutputContext parentContext, GroupStartData formatData) : base(parentContext)
            {
                this.cmd = cmd;
                this.formatData = formatData;
            }

            internal virtual void GroupEnd()
            {
            }

            internal virtual void GroupStart()
            {
            }

            internal virtual void Initialize()
            {
            }

            internal virtual void ProcessPayload(FormatEntryData fed)
            {
            }

            internal GroupStartData Data
            {
                get
                {
                    return this.formatData;
                }
            }

            protected OutCommandInner InnerCommand
            {
                get
                {
                    return this.cmd;
                }
            }
        }

        private sealed class ListOutputContext : OutCommandInner.GroupOutputContext
        {
            private ListWriter listWriter;
            private string[] properties;

            internal ListOutputContext(OutCommandInner cmd, FormatMessagesContextManager.OutputContext parentContext, GroupStartData formatData) : base(cmd, parentContext, formatData)
            {
                this.listWriter = new ListWriter();
            }

            internal static string[] GetProperties(ListViewEntry lve)
            {
                StringCollection strings = new StringCollection();
                foreach (ListViewField field in lve.listViewFieldList)
                {
                    strings.Add((field.label != null) ? field.label : field.propertyName);
                }
                if (strings.Count == 0)
                {
                    return null;
                }
                string[] array = new string[strings.Count];
                strings.CopyTo(array, 0);
                return array;
            }

            internal static string[] GetValues(ListViewEntry lve)
            {
                StringCollection strings = new StringCollection();
                foreach (ListViewField field in lve.listViewFieldList)
                {
                    strings.Add(field.formatPropertyField.propertyValue);
                }
                if (strings.Count == 0)
                {
                    return null;
                }
                string[] array = new string[strings.Count];
                strings.CopyTo(array, 0);
                return array;
            }

            internal override void GroupStart()
            {
                base.InnerCommand.lo.WriteLine("");
            }

            internal override void Initialize()
            {
            }

            private void InternalInitialize(ListViewEntry lve)
            {
                this.properties = GetProperties(lve);
                this.listWriter.Initialize(this.properties, base.InnerCommand.lo.ColumnNumber, base.InnerCommand.lo.DisplayCells);
            }

            internal override void ProcessPayload(FormatEntryData fed)
            {
                ListViewEntry formatEntryInfo = fed.formatEntryInfo as ListViewEntry;
                this.InternalInitialize(formatEntryInfo);
                string[] values = GetValues(formatEntryInfo);
                this.listWriter.WriteProperties(values, base.InnerCommand.lo);
                base.InnerCommand.lo.WriteLine("");
            }
        }

        private enum PreprocessingState
        {
            raw,
            processed,
            error
        }

        private sealed class TableFormattingHint : OutCommandInner.FormattingHint
        {
            internal int[] columnWidths;
        }

        private sealed class TableOutputContext : OutCommandInner.TableOutputContextBase
        {
            internal TableOutputContext(OutCommandInner cmd, FormatMessagesContextManager.OutputContext parentContext, GroupStartData formatData) : base(cmd, parentContext, formatData)
            {
            }

            internal override void GroupStart()
            {
                int count = this.CurrentTableHeaderInfo.tableColumnInfoList.Count;
                if (count != 0)
                {
                    string[] values = new string[count];
                    int num2 = 0;
                    foreach (TableColumnInfo info in this.CurrentTableHeaderInfo.tableColumnInfoList)
                    {
                        values[num2++] = (info.label != null) ? info.label : info.propertyName;
                    }
                    base.Writer.GenerateHeader(values, base.InnerCommand.lo);
                }
            }

            internal override void Initialize()
            {
                OutCommandInner.TableFormattingHint hint = base.InnerCommand.RetrieveFormattingHint() as OutCommandInner.TableFormattingHint;
                int[] columnWidths = null;
                if (hint != null)
                {
                    columnWidths = hint.columnWidths;
                }
                int columnNumber = base.InnerCommand.lo.ColumnNumber;
                int count = this.CurrentTableHeaderInfo.tableColumnInfoList.Count;
                if (count != 0)
                {
                    int[] numArray2 = new int[count];
                    int[] alignment = new int[count];
                    int index = 0;
                    foreach (TableColumnInfo info in this.CurrentTableHeaderInfo.tableColumnInfoList)
                    {
                        numArray2[index] = (columnWidths != null) ? columnWidths[index] : info.width;
                        alignment[index] = info.alignment;
                        index++;
                    }
                    base.Writer.Initialize(0, columnNumber, numArray2, alignment, this.CurrentTableHeaderInfo.hideHeader);
                }
            }

            internal override void ProcessPayload(FormatEntryData fed)
            {
                int count = this.CurrentTableHeaderInfo.tableColumnInfoList.Count;
                if (count != 0)
                {
                    TableRowEntry formatEntryInfo = fed.formatEntryInfo as TableRowEntry;
                    string[] values = new string[count];
                    int[] alignment = new int[count];
                    int num2 = formatEntryInfo.formatPropertyFieldList.Count;
                    for (int i = 0; i < count; i++)
                    {
                        if (i < num2)
                        {
                            values[i] = formatEntryInfo.formatPropertyFieldList[i].propertyValue;
                            alignment[i] = formatEntryInfo.formatPropertyFieldList[i].alignment;
                        }
                        else
                        {
                            values[i] = "";
                            alignment[i] = 1;
                        }
                    }
                    base.Writer.GenerateRow(values, base.InnerCommand.lo, formatEntryInfo.multiLine, alignment, base.InnerCommand.lo.DisplayCells);
                }
            }

            private TableHeaderInfo CurrentTableHeaderInfo
            {
                get
                {
                    return (TableHeaderInfo) base.InnerCommand.ShapeInfoOnFormatContext;
                }
            }
        }

        private class TableOutputContextBase : OutCommandInner.GroupOutputContext
        {
            private TableWriter tableWriter;

            internal TableOutputContextBase(OutCommandInner cmd, FormatMessagesContextManager.OutputContext parentContext, GroupStartData formatData) : base(cmd, parentContext, formatData)
            {
                this.tableWriter = new TableWriter();
            }

            protected TableWriter Writer
            {
                get
                {
                    return this.tableWriter;
                }
            }
        }

        private sealed class WideFormattingHint : OutCommandInner.FormattingHint
        {
            internal int maxWidth;
        }

        private sealed class WideOutputContext : OutCommandInner.TableOutputContextBase
        {
            private StringValuesBuffer buffer;

            internal WideOutputContext(OutCommandInner cmd, FormatMessagesContextManager.OutputContext parentContext, GroupStartData formatData) : base(cmd, parentContext, formatData)
            {
            }

            internal override void GroupEnd()
            {
                this.WriteStringBuffer();
            }

            internal override void GroupStart()
            {
                base.InnerCommand.lo.WriteLine("");
            }

            internal override void Initialize()
            {
                int size = 2;
                OutCommandInner.WideFormattingHint hint = base.InnerCommand.RetrieveFormattingHint() as OutCommandInner.WideFormattingHint;
                if ((hint != null) && (hint.maxWidth > 0))
                {
                    size = TableWriter.ComputeWideViewBestItemsPerRowFit(hint.maxWidth, base.InnerCommand.lo.ColumnNumber);
                }
                else if (this.CurrentWideHeaderInfo.columns > 0)
                {
                    size = this.CurrentWideHeaderInfo.columns;
                }
                this.buffer = new StringValuesBuffer(size);
                int[] columnWidths = new int[size];
                int[] alignment = new int[size];
                for (int i = 0; i < size; i++)
                {
                    columnWidths[i] = 0;
                    alignment[i] = 1;
                }
                base.Writer.Initialize(0, base.InnerCommand.lo.ColumnNumber, columnWidths, alignment, false);
            }

            internal override void ProcessPayload(FormatEntryData fed)
            {
                WideViewEntry formatEntryInfo = fed.formatEntryInfo as WideViewEntry;
                FormatPropertyField formatPropertyField = formatEntryInfo.formatPropertyField;
                this.buffer.Add(formatPropertyField.propertyValue);
                if (this.buffer.IsFull)
                {
                    this.WriteStringBuffer();
                }
            }

            private void WriteStringBuffer()
            {
                if (!this.buffer.IsEmpty)
                {
                    string[] values = new string[this.buffer.Lenght];
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (i < this.buffer.CurrentCount)
                        {
                            values[i] = this.buffer[i];
                        }
                        else
                        {
                            values[i] = "";
                        }
                    }
                    base.Writer.GenerateRow(values, base.InnerCommand.lo, false, null, base.InnerCommand.lo.DisplayCells);
                    this.buffer.Reset();
                }
            }

            private WideViewHeaderInfo CurrentWideHeaderInfo
            {
                get
                {
                    return (WideViewHeaderInfo) base.InnerCommand.ShapeInfoOnFormatContext;
                }
            }

            private class StringValuesBuffer
            {
                private string[] arr;
                private int lastEmptySpot;

                internal StringValuesBuffer(int size)
                {
                    this.arr = new string[size];
                    this.Reset();
                }

                internal void Add(string s)
                {
                    this.arr[this.lastEmptySpot++] = s;
                }

                internal void Reset()
                {
                    this.lastEmptySpot = 0;
                    for (int i = 0; i < this.arr.Length; i++)
                    {
                        this.arr[i] = null;
                    }
                }

                internal int CurrentCount
                {
                    get
                    {
                        return this.lastEmptySpot;
                    }
                }

                internal bool IsEmpty
                {
                    get
                    {
                        return (this.lastEmptySpot == 0);
                    }
                }

                internal bool IsFull
                {
                    get
                    {
                        return (this.lastEmptySpot == this.arr.Length);
                    }
                }

                internal string this[int k]
                {
                    get
                    {
                        return this.arr[k];
                    }
                }

                internal int Lenght
                {
                    get
                    {
                        return this.arr.Length;
                    }
                }
            }
        }
    }
}

