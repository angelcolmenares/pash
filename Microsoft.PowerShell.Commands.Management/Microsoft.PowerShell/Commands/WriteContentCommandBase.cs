using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace Microsoft.PowerShell.Commands
{
	public class WriteContentCommandBase : PassThroughContentCommandBase
	{
		private object[] content;

		private bool pipingPaths;

		private bool contentWritersOpen;

		[AllowEmptyCollection]
		[AllowNull]
		[Parameter(Mandatory=true, Position=1, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
		public object[] Value
		{
			get
			{
				return this.content;
			}
			set
			{
				this.content = value;
			}
		}

		public WriteContentCommandBase()
		{
		}

		internal virtual void BeforeOpenStreams(string[] paths)
		{
		}

		protected override void BeginProcessing()
		{
			if (base.Path == null || (int)base.Path.Length <= 0)
			{
				this.pipingPaths = true;
				return;
			}
			else
			{
				this.pipingPaths = false;
				return;
			}
		}

		protected override void EndProcessing()
		{
			base.Dispose(true);
		}

		private string[] GetAcceptedPaths(string[] unfilteredPaths, CmdletProviderContext currentContext)
		{
			Collection<PathInfo> pathInfos = base.ResolvePaths(unfilteredPaths, true, false, currentContext);
			ArrayList arrayLists = new ArrayList();
			foreach (PathInfo pathInfo in pathInfos)
			{
				if (!this.CallShouldProcess(pathInfo.Path))
				{
					continue;
				}
				arrayLists.Add(pathInfo.Path);
			}
			return (string[])arrayLists.ToArray(typeof(string));
		}

		internal List<ContentCommandBase.ContentHolder> GetContentWriters(string[] writerPaths, CmdletProviderContext currentCommandContext)
		{
			Collection<PathInfo> pathInfos = base.ResolvePaths(writerPaths, true, false, currentCommandContext);
			List<ContentCommandBase.ContentHolder> contentHolders = new List<ContentCommandBase.ContentHolder>();
			foreach (PathInfo pathInfo in pathInfos)
			{
				Collection<IContentWriter> writer = null;
				try
				{
					writer = base.InvokeProvider.Content.GetWriter(pathInfo.Path, currentCommandContext);
				}
				catch (PSNotSupportedException pSNotSupportedException1)
				{
					PSNotSupportedException pSNotSupportedException = pSNotSupportedException1;
					base.WriteError(new ErrorRecord(pSNotSupportedException.ErrorRecord, pSNotSupportedException));
					continue;
				}
				catch (DriveNotFoundException driveNotFoundException1)
				{
					DriveNotFoundException driveNotFoundException = driveNotFoundException1;
					base.WriteError(new ErrorRecord(driveNotFoundException.ErrorRecord, driveNotFoundException));
					continue;
				}
				catch (ProviderNotFoundException providerNotFoundException1)
				{
					ProviderNotFoundException providerNotFoundException = providerNotFoundException1;
					base.WriteError(new ErrorRecord(providerNotFoundException.ErrorRecord, providerNotFoundException));
					continue;
				}
				catch (ItemNotFoundException itemNotFoundException1)
				{
					ItemNotFoundException itemNotFoundException = itemNotFoundException1;
					base.WriteError(new ErrorRecord(itemNotFoundException.ErrorRecord, itemNotFoundException));
					continue;
				}
				if (writer == null || writer.Count <= 0 || writer.Count != 1 || writer[0] == null)
				{
					continue;
				}
				ContentCommandBase.ContentHolder contentHolder = new ContentCommandBase.ContentHolder(pathInfo, null, writer[0]);
				contentHolders.Add(contentHolder);
			}
			return contentHolders;
		}

		internal override object GetDynamicParameters(CmdletProviderContext context)
		{
			if (base.Path == null || (int)base.Path.Length <= 0)
			{
				return base.InvokeProvider.Content.GetContentWriterDynamicParameters(".", context);
			}
			else
			{
				return base.InvokeProvider.Content.GetContentWriterDynamicParameters(base.Path[0], context);
			}
		}

		protected override void ProcessRecord()
		{
			CmdletProviderContext currentContext = base.GetCurrentContext();
			if (this.content == null)
			{
				this.content = new object[0];
			}
			if (this.pipingPaths && this.contentStreams != null && this.contentStreams.Count > 0)
			{
				base.CloseContent(this.contentStreams, false);
				this.contentWritersOpen = false;
				this.contentStreams = new List<ContentCommandBase.ContentHolder>();
			}
			if (!this.contentWritersOpen)
			{
				string[] acceptedPaths = this.GetAcceptedPaths(base.Path, currentContext);
				if ((int)acceptedPaths.Length > 0)
				{
					this.BeforeOpenStreams(acceptedPaths);
					this.contentStreams = this.GetContentWriters(acceptedPaths, currentContext);
					this.SeekContentPosition(this.contentStreams);
				}
				this.contentWritersOpen = true;
			}
			try
			{
				foreach (ContentCommandBase.ContentHolder contentStream in this.contentStreams)
				{
					if (contentStream.Writer == null)
					{
						continue;
					}
					IList lists = null;
					try
					{
						lists = contentStream.Writer.Write(this.content);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						CommandsCommon.CheckForSevereException(this, exception);
						ProviderInvocationException providerInvocationException = new ProviderInvocationException("ProviderContentWriteError", SessionStateStrings.ProviderContentWriteError, contentStream.PathInfo.Provider, contentStream.PathInfo.Path, exception);
						MshLog.LogProviderHealthEvent(base.Context, contentStream.PathInfo.Provider.Name, providerInvocationException, Severity.Warning);
						base.WriteError(new ErrorRecord(providerInvocationException.ErrorRecord, providerInvocationException));
						continue;
					}
					if (lists == null || lists.Count <= 0 || !base.PassThru)
					{
						continue;
					}
					base.WriteContentObject(lists, (long)lists.Count, contentStream.PathInfo, currentContext);
				}
			}
			finally
			{
				if (this.pipingPaths)
				{
					base.CloseContent(this.contentStreams, false);
					this.contentWritersOpen = false;
					this.contentStreams = new List<ContentCommandBase.ContentHolder>();
				}
			}
		}

		internal virtual void SeekContentPosition(List<ContentCommandBase.ContentHolder> contentHolders)
		{
		}
	}
}