using Microsoft.PowerShell;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Management.Automation.Internal.Host;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Start", "Transcript", SupportsShouldProcess=true, DefaultParameterSetName="ByPath", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113408")]
	[OutputType(new Type[] { typeof(string) })]
	public sealed class StartTranscriptCommand : PSCmdlet
	{
		private const string resBaseName = "TranscriptStrings";

		private bool isLiteralPath;

		private bool force;

		private bool noclobber;

		private bool shouldAppend;

		private string outFilename;

		private bool isFilenameSet;

		[Parameter]
		public SwitchParameter Append
		{
			get
			{
				return this.shouldAppend;
			}
			set
			{
				this.shouldAppend = value;
			}
		}

		[Parameter]
		public SwitchParameter Force
		{
			get
			{
				return this.force;
			}
			set
			{
				this.force = value;
			}
		}

		[Alias(new string[] { "PSPath" })]
		[Parameter(Position=0, ParameterSetName="ByLiteralPath")]
		[ValidateNotNullOrEmpty]
		public string LiteralPath
		{
			get
			{
				return this.outFilename;
			}
			set
			{
				this.isFilenameSet = true;
				this.outFilename = value;
				this.isLiteralPath = true;
			}
		}

		[Alias(new string[] { "NoOverwrite" })]
		[Parameter]
		public SwitchParameter NoClobber
		{
			get
			{
				return this.noclobber;
			}
			set
			{
				this.noclobber = value;
			}
		}

		[Parameter(Position=0, ParameterSetName="ByPath")]
		[ValidateNotNullOrEmpty]
		public string Path
		{
			get
			{
				return this.outFilename;
			}
			set
			{
				this.isFilenameSet = true;
				this.outFilename = value;
			}
		}

		public StartTranscriptCommand()
		{
		}

		protected override void BeginProcessing()
		{
			InternalHost host = base.Host as InternalHost;
			if (host != null)
			{
				ConsoleHost externalHost = host.ExternalHost as ConsoleHost;
				if (externalHost != null)
				{
					if (!externalHost.IsTranscribing)
					{
						if (!this.isFilenameSet)
						{
							object variableValue = base.GetVariableValue("global:TRANSCRIPT", null);
							if (variableValue != null)
							{
								this.outFilename = (string)variableValue;
							}
							else
							{
								this.outFilename = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), StringUtil.Format("PowerShell_transcript.{0:yyyyMMddHHmmss}.txt", DateTime.Now));
							}
						}
						try
						{
							string str = this.ResolveFilePath(this.Path, this.isLiteralPath);
							if (base.ShouldProcess(this.Path))
							{
								if (File.Exists(str))
								{
									if (this.NoClobber && !this.Append)
									{
										string str1 = StringUtil.Format(TranscriptStrings.TranscriptFileExistsNoClobber, str, "NoClobber");
										Exception unauthorizedAccessException = new UnauthorizedAccessException(str1);
										ErrorRecord errorRecord = new ErrorRecord(unauthorizedAccessException, "NoClobber", ErrorCategory.ResourceExists, str);
										base.ThrowTerminatingError(errorRecord);
									}
									FileInfo fileInfo = new FileInfo(str);
									if ((fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
									{
										if (!this.Force)
										{
											object[] objArray = new object[1];
											objArray[0] = str;
											Exception exception = PSTraceSource.NewArgumentException(str, "TranscriptStrings", "TranscriptFileReadOnly", objArray);
											base.ThrowTerminatingError(new ErrorRecord(exception, "FileReadOnly", ErrorCategory.InvalidArgument, str));
										}
										else
										{
											FileInfo attributes = fileInfo;
											attributes.Attributes = attributes.Attributes & (FileAttributes.Hidden | FileAttributes.System | FileAttributes.Directory | FileAttributes.Archive | FileAttributes.Device | FileAttributes.Normal | FileAttributes.Temporary | FileAttributes.SparseFile | FileAttributes.ReparsePoint | FileAttributes.Compressed | FileAttributes.Offline | FileAttributes.NotContentIndexed | FileAttributes.Encrypted
#if !MONO
											                                                 | FileAttributes.IntegrityStream | FileAttributes.NoScrubData
#endif
											                                                 );
										}
									}
								}
								externalHost.StartTranscribing(str, this.Append);
								base.WriteObject(StringUtil.Format(TranscriptStrings.TranscriptionStarted, this.Path));
							}
						}
						catch (Exception exception2)
						{
							Exception exception1 = exception2;
							string str2 = "CannotStartTranscription";
							ErrorRecord errorRecord1 = new ErrorRecord(PSTraceSource.NewInvalidOperationException(exception1, "TranscriptStrings", str2, new object[0]), str2, ErrorCategory.InvalidOperation, null);
							base.ThrowTerminatingError(errorRecord1);
						}
						return;
					}
					else
					{
						throw new InvalidOperationException(TranscriptStrings.TranscriptionInProgress);
					}
				}
				else
				{
					throw new PSNotSupportedException(StringUtil.Format(TranscriptStrings.HostDoesNotSupportTranscript, new object[0]));
				}
			}
			else
			{
				throw new PSNotSupportedException(StringUtil.Format(TranscriptStrings.HostDoesNotSupportTranscript, new object[0]));
			}
		}

		private void ReportMultipleFilesNotSupported()
		{
			string str = "MultipleFilesNotSupported";
			ErrorRecord errorRecord = new ErrorRecord(PSTraceSource.NewInvalidOperationException("TranscriptStrings", str, new object[0]), str, ErrorCategory.InvalidArgument, null);
			base.ThrowTerminatingError(errorRecord);
		}

		private void ReportWrongProviderType(string providerId)
		{
			string str = "ReadWriteFileNotFileSystemProvider";
			object[] objArray = new object[1];
			objArray[0] = providerId;
			ErrorRecord errorRecord = new ErrorRecord(PSTraceSource.NewInvalidOperationException("TranscriptStrings", str, objArray), str, ErrorCategory.InvalidArgument, null);
			base.ThrowTerminatingError(errorRecord);
		}

		private string ResolveFilePath(string filePath, bool isLiteralPath)
		{
			string item = null;
			try
			{
				if (!isLiteralPath)
				{
					ProviderInfo providerInfo = null;
					Collection<string> resolvedProviderPathFromPSPath = base.SessionState.Path.GetResolvedProviderPathFromPSPath(filePath, out providerInfo);
					if (!providerInfo.NameEquals(base.Context.ProviderNames.FileSystem))
					{
						this.ReportWrongProviderType(providerInfo.FullName);
					}
					if (resolvedProviderPathFromPSPath.Count > 1)
					{
						this.ReportMultipleFilesNotSupported();
					}
					item = resolvedProviderPathFromPSPath[0];
				}
				else
				{
					item = base.SessionState.Path.GetUnresolvedProviderPathFromPSPath(filePath);
				}
			}
			catch (ItemNotFoundException itemNotFoundException)
			{
				item = null;
			}
			if (string.IsNullOrEmpty(item))
			{
				CmdletProviderContext cmdletProviderContext = new CmdletProviderContext(this);
				ProviderInfo providerInfo1 = null;
				PSDriveInfo pSDriveInfo = null;
				item = base.SessionState.Path.GetUnresolvedProviderPathFromPSPath(filePath, cmdletProviderContext, out providerInfo1, out pSDriveInfo);
				cmdletProviderContext.ThrowFirstErrorOrDoNothing();
				if (!providerInfo1.NameEquals(base.Context.ProviderNames.FileSystem))
				{
					this.ReportWrongProviderType(providerInfo1.FullName);
				}
			}
			return item;
		}
	}
}