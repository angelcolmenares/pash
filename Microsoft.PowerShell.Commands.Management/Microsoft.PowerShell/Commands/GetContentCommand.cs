using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Management.Automation.Provider;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Get", "Content", DefaultParameterSetName="Path", SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113310")]
	public class GetContentCommand : ContentCommandBase
	{
		private bool _totalCountSpecified;

		private int _backCount;

		private bool _tailSpecified;

		private long readCount;

		private long totalCount;

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public long ReadCount
		{
			get
			{
				return this.readCount;
			}
			set
			{
				this.readCount = value;
			}
		}

		[Alias(new string[] { "Last" })]
		[Parameter(ValueFromPipelineByPropertyName=true)]
		public int Tail
		{
			get
			{
				return this._backCount;
			}
			set
			{
				this._backCount = value;
				this._tailSpecified = true;
			}
		}

		[Alias(new string[] { "First", "Head" })]
		[Parameter(ValueFromPipelineByPropertyName=true)]
		public long TotalCount
		{
			get
			{
				return this.totalCount;
			}
			set
			{
				this.totalCount = value;
				this._totalCountSpecified = true;
			}
		}

		public GetContentCommand()
		{
			this._backCount = -1;
			this.readCount = (long)1;
			this.totalCount = (long)-1;
		}

		protected override void EndProcessing()
		{
			base.Dispose(true);
		}

		internal override object GetDynamicParameters(CmdletProviderContext context)
		{
			if (base.Path == null || (int)base.Path.Length <= 0)
			{
				return base.InvokeProvider.Content.GetContentReaderDynamicParameters(".", context);
			}
			else
			{
				return base.InvokeProvider.Content.GetContentReaderDynamicParameters(base.Path[0], context);
			}
		}

		protected override void ProcessRecord()
		{
			if (!this._totalCountSpecified || !this._tailSpecified)
			{
				if (this.TotalCount != (long)0)
				{
					CmdletProviderContext cmdletProviderContext = this.CmdletProviderContext;
					this.contentStreams = base.GetContentReaders(base.Path, cmdletProviderContext);
					try
					{
					Label0:
						foreach (ContentCommandBase.ContentHolder contentStream in this.contentStreams)
						{
							long count = (long)0;
							if (!this._tailSpecified || contentStream.Reader as FileSystemContentReaderWriter != null)
							{
								if (this.Tail >= 0)
								{
									bool flag = false;
									try
									{
										flag = this.SeekPositionForTail(contentStream.Reader);
									}
									catch (Exception exception1)
									{
										Exception exception = exception1;
										CommandsCommon.CheckForSevereException(this, exception);
										ProviderInvocationException providerInvocationException = new ProviderInvocationException("ProviderContentReadError", SessionStateStrings.ProviderContentReadError, contentStream.PathInfo.Provider, contentStream.PathInfo.Path, exception);
										MshLog.LogProviderHealthEvent(base.Context, contentStream.PathInfo.Provider.Name, providerInvocationException, Severity.Warning);
										base.WriteError(new ErrorRecord(providerInvocationException.ErrorRecord, providerInvocationException));
										continue;
									}
									if (!flag && !this.ScanForwardsForTail(contentStream, cmdletProviderContext))
									{
										continue;
									}
								}
								if (this.TotalCount == (long)0)
								{
									continue;
								}
								IList lists = null;
								do
								{
									long readCount = this.ReadCount;
									if (this.TotalCount > (long)0 && this.TotalCount - readCount < count)
									{
										readCount = this.TotalCount - count;
									}
									try
									{
										lists = contentStream.Reader.Read(readCount);
									}
									catch (Exception exception3)
									{
										Exception exception2 = exception3;
										CommandsCommon.CheckForSevereException(this, exception2);
										ProviderInvocationException providerInvocationException1 = new ProviderInvocationException("ProviderContentReadError", SessionStateStrings.ProviderContentReadError, contentStream.PathInfo.Provider, contentStream.PathInfo.Path, exception2);
										MshLog.LogProviderHealthEvent(base.Context, contentStream.PathInfo.Provider.Name, providerInvocationException1, Severity.Warning);
										base.WriteError(new ErrorRecord(providerInvocationException1.ErrorRecord, providerInvocationException1));
										goto Label0;
									}
									if (lists == null || lists.Count <= 0)
									{
										continue;
									}
									count = count + (long)lists.Count;
									if (this.ReadCount != (long)1)
									{
										base.WriteContentObject(lists, count, contentStream.PathInfo, cmdletProviderContext);
									}
									else
									{
										base.WriteContentObject(lists[0], count, contentStream.PathInfo, cmdletProviderContext);
									}
								}
								while (lists != null && lists.Count > 0 && (this.TotalCount < (long)0 || count < this.TotalCount));
							}
							else
							{
								string getContentTailNotSupported = SessionStateStrings.GetContent_TailNotSupported;
								ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(getContentTailNotSupported), "TailNotSupported", ErrorCategory.InvalidOperation, (object)this.Tail);
								base.WriteError(errorRecord);
							}
						}
					}
					finally
					{
						base.CloseContent(this.contentStreams, false);
						this.contentStreams = new List<ContentCommandBase.ContentHolder>();
					}
					return;
				}
				else
				{
					return;
				}
			}
			else
			{
				string str = StringUtil.Format(SessionStateStrings.GetContent_TailAndHeadCannotCoexist, "TotalCount", "Tail");
				ErrorRecord errorRecord1 = new ErrorRecord(new InvalidOperationException(str), "TailAndHeadCannotCoexist", ErrorCategory.InvalidOperation, null);
				base.WriteError(errorRecord1);
				return;
			}
		}

		private bool ScanForwardsForTail(ContentCommandBase.ContentHolder holder, CmdletProviderContext currentContext)
		{
			FileSystemContentReaderWriter reader = holder.Reader as FileSystemContentReaderWriter;
			Queue<object> objs = new Queue<object>();
			IList lists = null;
			ErrorRecord errorRecord = null;
			do
			{
				try
				{
					lists = reader.ReadWithoutWaitingChanges(this.ReadCount);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					CommandsCommon.CheckForSevereException(this, exception);
					ProviderInvocationException providerInvocationException = new ProviderInvocationException("ProviderContentReadError", SessionStateStrings.ProviderContentReadError, holder.PathInfo.Provider, holder.PathInfo.Path, exception);
					MshLog.LogProviderHealthEvent(base.Context, holder.PathInfo.Provider.Name, providerInvocationException, Severity.Warning);
					errorRecord = new ErrorRecord(providerInvocationException.ErrorRecord, providerInvocationException);
					break;
				}
				if (lists == null || lists.Count <= 0)
				{
					continue;
				}
				foreach (object obj in lists)
				{
					if (objs.Count == this.Tail)
					{
						objs.Dequeue();
					}
					objs.Enqueue(obj);
				}
			}
			while (lists != null && lists.Count > 0);
			if (objs.Count > 0)
			{
				int count = 0;
				if (this.ReadCount <= (long)0 || this.ReadCount >= (long)objs.Count && this.ReadCount != (long)1)
				{
					count = objs.Count;
					ArrayList arrayLists = new ArrayList();
					while (objs.Count > 0)
					{
						arrayLists.Add(objs.Dequeue());
					}
					base.WriteContentObject(arrayLists.ToArray(), (long)count, holder.PathInfo, currentContext);
				}
				else
				{
					if (this.ReadCount != (long)1)
					{
						while ((long)objs.Count >= this.ReadCount)
						{
							ArrayList arrayLists1 = new ArrayList();
							int num = 0;
							while ((long)num < this.ReadCount)
							{
								arrayLists1.Add(objs.Dequeue());
								num++;
								count++;
							}
							base.WriteContentObject(arrayLists1.ToArray(), (long)count, holder.PathInfo, currentContext);
						}
						int count1 = objs.Count;
						if (count1 > 0)
						{
							ArrayList arrayLists2 = new ArrayList();
							while (count1 > 0)
							{
								arrayLists2.Add(objs.Dequeue());
								count1--;
								count++;
							}
							base.WriteContentObject(arrayLists2.ToArray(), (long)count, holder.PathInfo, currentContext);
						}
					}
					else
					{
						while (objs.Count > 0)
						{
							int num1 = count;
							count = num1 + 1;
							base.WriteContentObject(objs.Dequeue(), (long)num1, holder.PathInfo, currentContext);
						}
					}
				}
			}
			if (errorRecord == null)
			{
				return true;
			}
			else
			{
				base.WriteError(errorRecord);
				return false;
			}
		}

		private bool SeekPositionForTail(IContentReader reader)
		{
			bool flag;
			FileSystemContentReaderWriter fileSystemContentReaderWriter = reader as FileSystemContentReaderWriter;
			try
			{
				fileSystemContentReaderWriter.SeekItemsBackward(this.Tail);
				flag = true;
			}
			catch (BackReaderEncodingNotSupportedException backReaderEncodingNotSupportedException)
			{
				fileSystemContentReaderWriter.Seek((long)0, SeekOrigin.Begin);
				flag = false;
			}
			return flag;
		}
	}
}