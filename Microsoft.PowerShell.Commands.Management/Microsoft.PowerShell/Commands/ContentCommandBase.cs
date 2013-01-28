using Microsoft.PowerShell.Commands.Management;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Management.Automation.Provider;

namespace Microsoft.PowerShell.Commands
{
	public class ContentCommandBase : CoreCommandWithCredentialsBase, IDisposable
	{
		private string[] paths;

		internal List<ContentCommandBase.ContentHolder> contentStreams;

		private ContentCommandBase.ContentPathsCache currentContentItem;

		[Parameter]
		public override string[] Exclude
		{
			get
			{
				return base.Exclude;
			}
			set
			{
				base.Exclude = value;
			}
		}

		[Parameter]
		public override string Filter
		{
			get
			{
				return base.Filter;
			}
			set
			{
				base.Filter = value;
			}
		}

		[Parameter]
		public override SwitchParameter Force
		{
			get
			{
				return base.Force;
			}
			set
			{
				base.Force = value;
			}
		}

		[Parameter]
		public override string[] Include
		{
			get
			{
				return base.Include;
			}
			set
			{
				base.Include = value;
			}
		}

		[Alias(new string[] { "PSPath" })]
		[Parameter(ParameterSetName="LiteralPath", Mandatory=true, ValueFromPipeline=false, ValueFromPipelineByPropertyName=true)]
		public string[] LiteralPath
		{
			get
			{
				return this.paths;
			}
			set
			{
				base.SuppressWildcardExpansion = true;
				this.paths = value;
			}
		}

		[Parameter(Position=0, ParameterSetName="Path", Mandatory=true, ValueFromPipelineByPropertyName=true)]
		public string[] Path
		{
			get
			{
				return this.paths;
			}
			set
			{
				this.paths = value;
			}
		}

		public ContentCommandBase()
		{
			this.contentStreams = new List<ContentCommandBase.ContentHolder>();
		}

		internal virtual bool CallShouldProcess(string path)
		{
			return true;
		}

		internal void CloseContent(List<ContentCommandBase.ContentHolder> contentHolders, bool disposing)
		{
			if (contentHolders != null)
			{
				foreach (ContentCommandBase.ContentHolder contentHolder in contentHolders)
				{
					try
					{
						if (contentHolder.Writer != null)
						{
							contentHolder.Writer.Close();
						}
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						CommandsCommon.CheckForSevereException(this, exception);
						ProviderInvocationException providerInvocationException = new ProviderInvocationException("ProviderContentCloseError", SessionStateStrings.ProviderContentCloseError, contentHolder.PathInfo.Provider, contentHolder.PathInfo.Path, exception);
						MshLog.LogProviderHealthEvent(base.Context, contentHolder.PathInfo.Provider.Name, providerInvocationException, Severity.Warning);
						if (!disposing)
						{
							base.WriteError(new ErrorRecord(providerInvocationException.ErrorRecord, providerInvocationException));
						}
					}
					try
					{
						if (contentHolder.Reader != null)
						{
							contentHolder.Reader.Close();
						}
					}
					catch (Exception exception3)
					{
						Exception exception2 = exception3;
						CommandsCommon.CheckForSevereException(this, exception2);
						ProviderInvocationException providerInvocationException1 = new ProviderInvocationException("ProviderContentCloseError", SessionStateStrings.ProviderContentCloseError, contentHolder.PathInfo.Provider, contentHolder.PathInfo.Path, exception2);
						MshLog.LogProviderHealthEvent(base.Context, contentHolder.PathInfo.Provider.Name, providerInvocationException1, Severity.Warning);
						if (!disposing)
						{
							base.WriteError(new ErrorRecord(providerInvocationException1.ErrorRecord, providerInvocationException1));
						}
					}
				}
				return;
			}
			else
			{
				throw PSTraceSource.NewArgumentNullException("contentHolders");
			}
		}

		internal void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				this.CloseContent(this.contentStreams, true);
				this.contentStreams = new List<ContentCommandBase.ContentHolder>();
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

        ~ContentCommandBase()
		{
			try
			{
				this.Dispose(false);
			}
			finally
			{
				
			}
		}

		internal List<ContentCommandBase.ContentHolder> GetContentReaders(string[] readerPaths, CmdletProviderContext currentCommandContext)
		{
			Collection<PathInfo> pathInfos = this.ResolvePaths(readerPaths, false, true, currentCommandContext);
			List<ContentCommandBase.ContentHolder> contentHolders = new List<ContentCommandBase.ContentHolder>();
			foreach (PathInfo pathInfo in pathInfos)
			{
				Collection<IContentReader> reader = null;
				try
				{
					string path = WildcardPattern.Escape(pathInfo.Path);
					if (currentCommandContext.SuppressWildcardExpansion)
					{
						path = pathInfo.Path;
					}
					reader = base.InvokeProvider.Content.GetReader(path, currentCommandContext);
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
				if (reader == null || reader.Count <= 0 || reader.Count != 1 || reader[0] == null)
				{
					continue;
				}
				ContentCommandBase.ContentHolder contentHolder = new ContentCommandBase.ContentHolder(pathInfo, reader[0], null);
				contentHolders.Add(contentHolder);
			}
			return contentHolders;
		}

		internal Collection<PathInfo> ResolvePaths(string[] pathsToResolve, bool allowNonexistingPaths, bool allowEmptyResult, CmdletProviderContext currentCommandContext)
		{
			Collection<PathInfo> pathInfos = new Collection<PathInfo>();
			string[] strArrays = pathsToResolve;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				bool flag = false;
				bool flag1 = false;
				ErrorRecord errorRecord = null;
				try
				{
					Collection<PathInfo> resolvedPSPathFromPSPath = base.SessionState.Path.GetResolvedPSPathFromPSPath(str, currentCommandContext);
					if (resolvedPSPathFromPSPath.Count == 0)
					{
						flag = true;
						if (!currentCommandContext.SuppressWildcardExpansion)
						{
							flag1 = true;
						}
					}
					foreach (PathInfo pathInfo in resolvedPSPathFromPSPath)
					{
						pathInfos.Add(pathInfo);
					}
				}
				catch (PSNotSupportedException pSNotSupportedException1)
				{
					PSNotSupportedException pSNotSupportedException = pSNotSupportedException1;
					base.WriteError(new ErrorRecord(pSNotSupportedException.ErrorRecord, pSNotSupportedException));
				}
				catch (DriveNotFoundException driveNotFoundException1)
				{
					DriveNotFoundException driveNotFoundException = driveNotFoundException1;
					base.WriteError(new ErrorRecord(driveNotFoundException.ErrorRecord, driveNotFoundException));
				}
				catch (ProviderNotFoundException providerNotFoundException1)
				{
					ProviderNotFoundException providerNotFoundException = providerNotFoundException1;
					base.WriteError(new ErrorRecord(providerNotFoundException.ErrorRecord, providerNotFoundException));
				}
				catch (ItemNotFoundException itemNotFoundException1)
				{
					ItemNotFoundException itemNotFoundException = itemNotFoundException1;
					flag = true;
					errorRecord = new ErrorRecord(itemNotFoundException.ErrorRecord, itemNotFoundException);
				}
				if (flag)
				{
					if (!allowNonexistingPaths || flag1 || !currentCommandContext.SuppressWildcardExpansion && WildcardPattern.ContainsWildcardCharacters(str))
					{
						if (errorRecord == null)
						{
							string str1 = StringUtil.Format(NavigationResources.ItemNotFound, this.Path);
							Exception exception = new Exception(str1);
							errorRecord = new ErrorRecord(exception, "ItemNotFound", ErrorCategory.ObjectNotFound, this.Path);
						}
						base.WriteError(errorRecord);
					}
					else
					{
						ProviderInfo providerInfo = null;
						PSDriveInfo pSDriveInfo = null;
						string unresolvedProviderPathFromPSPath = base.SessionState.Path.GetUnresolvedProviderPathFromPSPath(str, currentCommandContext, out providerInfo, out pSDriveInfo);
						PathInfo pathInfo1 = new PathInfo(pSDriveInfo, providerInfo, unresolvedProviderPathFromPSPath, base.SessionState);
						pathInfos.Add(pathInfo1);
					}
				}
			}
			return pathInfos;
		}

		internal void WriteContentObject(object content, long readCount, PathInfo pathInfo, CmdletProviderContext context)
		{
			PSNoteProperty pSNoteProperty;
			string str;
			PSObject pSObject = PSObject.AsPSObject(content);
			if (this.currentContentItem == null || this.currentContentItem.PathInfo != pathInfo && string.Compare(pathInfo.Path, this.currentContentItem.PathInfo.Path, StringComparison.OrdinalIgnoreCase) != 0)
			{
				this.currentContentItem = new ContentCommandBase.ContentPathsCache(pathInfo);
				string path = pathInfo.Path;
				pSNoteProperty = new PSNoteProperty("PSPath", path);
				pSObject.Properties.Add(pSNoteProperty, true);
				object[] objArray = new object[2];
				objArray[0] = "PSPath";
				objArray[1] = path;
				CoreCommandBase.tracer.WriteLine("Attaching {0} = {1}", objArray);
				this.currentContentItem.PSPath = path;
				try
				{
					if (pathInfo.Drive == null)
					{
						str = base.SessionState.Path.ParseParent(pathInfo.Path, string.Empty, context);
					}
					else
					{
						str = base.SessionState.Path.ParseParent(pathInfo.Path, pathInfo.Drive.Root, context);
					}
					pSNoteProperty = new PSNoteProperty("PSParentPath", str);
					pSObject.Properties.Add(pSNoteProperty, true);
					object[] objArray1 = new object[2];
					objArray1[0] = "PSParentPath";
					objArray1[1] = str;
					CoreCommandBase.tracer.WriteLine("Attaching {0} = {1}", objArray1);
					this.currentContentItem.ParentPath = str;
					string str1 = base.SessionState.Path.ParseChildName(pathInfo.Path, context);
					pSNoteProperty = new PSNoteProperty("PSChildName", str1);
					pSObject.Properties.Add(pSNoteProperty, true);
					object[] objArray2 = new object[2];
					objArray2[0] = "PSChildName";
					objArray2[1] = str1;
					CoreCommandBase.tracer.WriteLine("Attaching {0} = {1}", objArray2);
					this.currentContentItem.ChildName = str1;
				}
				catch (NotSupportedException notSupportedException)
				{
				}
				if (pathInfo.Drive != null)
				{
					PSDriveInfo drive = pathInfo.Drive;
					pSNoteProperty = new PSNoteProperty("PSDrive", drive);
					pSObject.Properties.Add(pSNoteProperty, true);
					object[] objArray3 = new object[2];
					objArray3[0] = "PSDrive";
					objArray3[1] = drive;
					CoreCommandBase.tracer.WriteLine("Attaching {0} = {1}", objArray3);
					this.currentContentItem.Drive = drive;
				}
				ProviderInfo provider = pathInfo.Provider;
				pSNoteProperty = new PSNoteProperty("PSProvider", provider);
				pSObject.Properties.Add(pSNoteProperty, true);
				object[] objArray4 = new object[2];
				objArray4[0] = "PSProvider";
				objArray4[1] = provider;
				CoreCommandBase.tracer.WriteLine("Attaching {0} = {1}", objArray4);
				this.currentContentItem.Provider = provider;
			}
			else
			{
				pSObject = this.currentContentItem.AttachNotes(pSObject);
			}
			pSNoteProperty = new PSNoteProperty("ReadCount", (object)readCount);
			pSObject.Properties.Add(pSNoteProperty, true);
			base.WriteObject(pSObject);
		}

		internal struct ContentHolder
		{
			private PathInfo _pathInfo;

			private IContentReader _reader;

			private IContentWriter _writer;

			internal PathInfo PathInfo
			{
				get
				{
					return this._pathInfo;
				}
			}

			internal IContentReader Reader
			{
				get
				{
					return this._reader;
				}
			}

			internal IContentWriter Writer
			{
				get
				{
					return this._writer;
				}
			}

			internal ContentHolder(PathInfo pathInfo, IContentReader reader, IContentWriter writer)
			{
				if (pathInfo != null)
				{
					this._pathInfo = pathInfo;
					this._reader = reader;
					this._writer = writer;
					return;
				}
				else
				{
					throw PSTraceSource.NewArgumentNullException("pathInfo");
				}
			}
		}

		internal class ContentPathsCache
		{
			private PathInfo pathInfo;

			private string psPath;

			private string parentPath;

			private PSDriveInfo drive;

			private ProviderInfo provider;

			private string childName;

			public string ChildName
			{
				get
				{
					return this.childName;
				}
				set
				{
					this.childName = value;
				}
			}

			public PSDriveInfo Drive
			{
				get
				{
					return this.drive;
				}
				set
				{
					this.drive = value;
				}
			}

			public string ParentPath
			{
				get
				{
					return this.parentPath;
				}
				set
				{
					this.parentPath = value;
				}
			}

			public PathInfo PathInfo
			{
				get
				{
					return this.pathInfo;
				}
			}

			public ProviderInfo Provider
			{
				get
				{
					return this.provider;
				}
				set
				{
					this.provider = value;
				}
			}

			public string PSPath
			{
				get
				{
					return this.psPath;
				}
				set
				{
					this.psPath = value;
				}
			}

			public ContentPathsCache(PathInfo pathInfo)
			{
				this.pathInfo = pathInfo;
			}

			public PSObject AttachNotes(PSObject content)
			{
				PSNoteProperty pSNoteProperty = new PSNoteProperty("PSPath", this.PSPath);
				content.Properties.Add(pSNoteProperty, true);
				object[] pSPath = new object[2];
				pSPath[0] = "PSPath";
				pSPath[1] = this.PSPath;
				CoreCommandBase.tracer.WriteLine("Attaching {0} = {1}", pSPath);
				pSNoteProperty = new PSNoteProperty("PSParentPath", this.ParentPath);
				content.Properties.Add(pSNoteProperty, true);
				object[] parentPath = new object[2];
				parentPath[0] = "PSParentPath";
				parentPath[1] = this.ParentPath;
				CoreCommandBase.tracer.WriteLine("Attaching {0} = {1}", parentPath);
				pSNoteProperty = new PSNoteProperty("PSChildName", this.ChildName);
				content.Properties.Add(pSNoteProperty, true);
				object[] childName = new object[2];
				childName[0] = "PSChildName";
				childName[1] = this.ChildName;
				CoreCommandBase.tracer.WriteLine("Attaching {0} = {1}", childName);
				if (this.pathInfo.Drive != null)
				{
					pSNoteProperty = new PSNoteProperty("PSDrive", this.Drive);
					content.Properties.Add(pSNoteProperty, true);
					object[] drive = new object[2];
					drive[0] = "PSDrive";
					drive[1] = this.Drive;
					CoreCommandBase.tracer.WriteLine("Attaching {0} = {1}", drive);
				}
				pSNoteProperty = new PSNoteProperty("PSProvider", this.Provider);
				content.Properties.Add(pSNoteProperty, true);
				object[] provider = new object[2];
				provider[0] = "PSProvider";
				provider[1] = this.Provider;
				CoreCommandBase.tracer.WriteLine("Attaching {0} = {1}", provider);
				return content;
			}
		}
	}
}