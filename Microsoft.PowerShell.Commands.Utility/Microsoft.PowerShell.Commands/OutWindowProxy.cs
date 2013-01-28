namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Reflection;
    using System.Threading;

    internal class OutWindowProxy : IDisposable
    {
        private AutoResetEvent closedEvent;
        private GraphicalHostReflectionWrapper graphicalHostReflectionWrapper;
        private HeaderInfo headerInfo;
        private int index;
        private const string IndexPropertyName = "IndexValue";
        private bool IsWindowStarted;
        internal const string OriginalObjectPropertyName = "OutGridViewOriginalObject";
        private const string OriginalTypePropertyName = "OriginalType";
        private const string OutGridViewWindowClassName = "Microsoft.Management.UI.Internal.OutGridViewWindow";
        private OutputModeOption outputMode;
        private OutGridViewCommand parentCmdlet;
        private string title;
        private const string ToStringValuePropertyName = "ToStringValue";

        internal OutWindowProxy(string title, OutputModeOption outPutMode, OutGridViewCommand parentCmdlet)
        {
            this.title = title;
            this.outputMode = outPutMode;
            this.parentCmdlet = parentCmdlet;
            this.graphicalHostReflectionWrapper = GraphicalHostReflectionWrapper.GetGraphicalHostReflectionWrapper(parentCmdlet, "Microsoft.Management.UI.Internal.OutGridViewWindow");
        }

        internal void AddColumns(string[] propertyNames, string[] displayNames, Type[] types)
        {
            if (propertyNames == null)
            {
                throw new ArgumentNullException("propertyNames");
            }
            if (displayNames == null)
            {
                throw new ArgumentNullException("displayNames");
            }
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }
            try
            {
                this.graphicalHostReflectionWrapper.CallMethod("AddColumns", new object[] { propertyNames, displayNames, types });
            }
            catch (TargetInvocationException exception)
            {
                FileNotFoundException innerException = exception.InnerException as FileNotFoundException;
                if ((innerException == null) || !innerException.FileName.Contains("System.Core"))
                {
                    throw;
                }
                this.parentCmdlet.ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(StringUtil.Format(FormatAndOut_out_gridview.RestartPowerShell, this.parentCmdlet.CommandInfo.Name), exception.InnerException), "ErrorLoadingAssembly", ErrorCategory.ObjectNotFound, null));
            }
        }

        internal void AddColumnsAndItem(PSObject liveObject)
        {
            this.headerInfo = new HeaderInfo();
            this.headerInfo.AddColumn(new ScalarTypeColumnInfo(liveObject.BaseObject.GetType()));
            this.AddColumnsAndItemEnd(liveObject);
        }

        internal void AddColumnsAndItem(PSObject liveObject, TableView tableView)
        {
            this.headerInfo = tableView.GenerateHeaderInfo(liveObject, this.parentCmdlet);
            this.AddColumnsAndItemEnd(liveObject);
        }

        internal void AddColumnsAndItem(PSObject liveObject, TableView tableView, TableControlBody tableBody)
        {
            this.headerInfo = tableView.GenerateHeaderInfo(liveObject, tableBody, this.parentCmdlet);
            this.AddColumnsAndItemEnd(liveObject);
        }

        private void AddColumnsAndItemEnd(PSObject liveObject)
        {
            PSObject staleObject = this.headerInfo.AddColumnsToWindow(this, liveObject);
            this.AddExtraProperties(staleObject, liveObject);
            this.graphicalHostReflectionWrapper.CallMethod("AddItem", new object[] { staleObject });
        }

        private void AddExtraProperties(PSObject staleObject, PSObject liveObject)
        {
            staleObject.Properties.Add(new PSNoteProperty("IndexValue", this.index++));
            staleObject.Properties.Add(new PSNoteProperty("OriginalType", liveObject.BaseObject.GetType().FullName));
            staleObject.Properties.Add(new PSNoteProperty("OutGridViewOriginalObject", liveObject));
            staleObject.Properties.Add(new PSNoteProperty("ToStringValue", this.parentCmdlet.ConvertToString(liveObject)));
        }

        internal void AddHeteroViewColumnsAndItem(PSObject liveObject)
        {
            this.headerInfo = new HeaderInfo();
            this.headerInfo.AddColumn(new IndexColumnInfo("IndexValue", StringUtil.Format(FormatAndOut_out_gridview.IndexColumnName, new object[0]), this.index));
            this.headerInfo.AddColumn(new ToStringColumnInfo("ToStringValue", StringUtil.Format(FormatAndOut_out_gridview.ValueColumnName, new object[0]), this.parentCmdlet));
            this.headerInfo.AddColumn(new TypeNameColumnInfo("OriginalType", StringUtil.Format(FormatAndOut_out_gridview.TypeColumnName, new object[0])));
            PSObject obj2 = this.headerInfo.AddColumnsToWindow(this, liveObject);
            this.graphicalHostReflectionWrapper.CallMethod("AddItem", new object[] { obj2 });
        }

        internal void AddHeteroViewItem(PSObject livePSObject)
        {
            if (livePSObject == null)
            {
                throw new ArgumentNullException("livePSObject");
            }
            if (this.headerInfo == null)
            {
                throw new InvalidOperationException();
            }
            PSObject obj2 = this.headerInfo.CreateStalePSObject(livePSObject);
            this.graphicalHostReflectionWrapper.CallMethod("AddItem", new object[] { obj2 });
        }

        internal void AddItem(PSObject livePSObject)
        {
            if (livePSObject == null)
            {
                throw new ArgumentNullException("livePSObject");
            }
            if (this.headerInfo == null)
            {
                throw new InvalidOperationException();
            }
            PSObject staleObject = this.headerInfo.CreateStalePSObject(livePSObject);
            this.AddExtraProperties(staleObject, livePSObject);
            this.graphicalHostReflectionWrapper.CallMethod("AddItem", new object[] { staleObject });
        }

        internal void BlockUntillClosed()
        {
            if (this.closedEvent != null)
            {
                this.closedEvent.WaitOne();
            }
        }

        internal void CloseWindow()
        {
            if (this.IsWindowStarted)
            {
                this.graphicalHostReflectionWrapper.CallMethod("CloseWindow", new object[0]);
                this.IsWindowStarted = false;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isDisposing)
        {
            if (isDisposing && (this.closedEvent != null))
            {
                this.closedEvent.Dispose();
                this.closedEvent = null;
            }
        }

        internal Exception GetLastException()
        {
            return (Exception) this.graphicalHostReflectionWrapper.CallMethod("GetLastException", new object[0]);
        }

        internal List<PSObject> GetSelectedItems()
        {
            return (List<PSObject>) this.graphicalHostReflectionWrapper.CallMethod("SelectedItems", new object[0]);
        }

        internal bool IsWindowClosed()
        {
            return (bool) this.graphicalHostReflectionWrapper.CallMethod("GetWindowClosedStatus", new object[0]);
        }

        internal void ShowWindow()
        {
            if (!this.IsWindowStarted)
            {
                this.closedEvent = new AutoResetEvent(false);
                this.graphicalHostReflectionWrapper.CallMethod("StartWindow", new object[] { this.title, this.outputMode.ToString(), this.closedEvent });
                this.IsWindowStarted = true;
            }
        }
    }
}

