using Microsoft.PowerShell;
using Microsoft.PowerShell.Security;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	public abstract class SignatureCommandsBase : PSCmdlet
	{
		private string[] path;

		private bool isLiteralPath;

		private Signature signature;

		private string commandName;

		[Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ByPath")]
		public string[] FilePath
		{
			get
			{
				return this.path;
			}
			set
			{
				this.path = value;
			}
		}

		[Alias(new string[] { "PSPath" })]
		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ByLiteralPath")]
		public string[] LiteralPath
		{
			get
			{
				return this.path;
			}
			set
			{
				this.path = value;
				this.isLiteralPath = true;
			}
		}

		protected Signature Signature
		{
			get
			{
				return this.signature;
			}
			set
			{
				this.signature = value;
			}
		}

		protected SignatureCommandsBase(string name)
		{
			this.commandName = name;
		}

		private SignatureCommandsBase()
		{
		}

		protected abstract Signature PerformAction(string filePath);

		protected override void ProcessRecord()
		{
			bool flag = false;
			string[] filePath = this.FilePath;
			for (int i = 0; i < (int)filePath.Length; i++)
			{
				string str = filePath[i];
				Collection<string> strs = new Collection<string>();
				if (!this.isLiteralPath)
				{
					try
					{
						foreach (PathInfo resolvedPSPathFromPSPath in base.SessionState.Path.GetResolvedPSPathFromPSPath(str))
						{
							strs.Add(resolvedPSPathFromPSPath.ProviderPath);
						}
					}
					catch (ItemNotFoundException itemNotFoundException)
					{
						object[] objArray = new object[1];
						objArray[0] = str;
						base.WriteError(SecurityUtils.CreateFileNotFoundErrorRecord(SignatureCommands.FileNotFound, "SignatureCommandsBaseFileNotFound", objArray));
					}
				}
				else
				{
					strs.Add(base.SessionState.Path.GetUnresolvedProviderPathFromPSPath(str));
				}
				if (strs.Count != 0)
				{
					foreach (string str1 in strs)
					{
						if (Directory.Exists(str1))
						{
							continue;
						}
						flag = true;
						string filePathOfExistingFile = SecurityUtils.GetFilePathOfExistingFile(this, str1);
						if (filePathOfExistingFile != null)
						{
							Signature signature = this.PerformAction(filePathOfExistingFile);
							Signature signature1 = signature;
							this.Signature = signature;
							if (signature1 == null)
							{
								continue;
							}
							base.WriteObject(this.Signature);
						}
						else
						{
							object[] objArray1 = new object[1];
							objArray1[0] = str1;
							base.WriteError(SecurityUtils.CreateFileNotFoundErrorRecord(SignatureCommands.FileNotFound, "SignatureCommandsBaseFileNotFound", objArray1));
						}
					}
					if (!flag)
					{
						base.WriteError(SecurityUtils.CreateFileNotFoundErrorRecord(SignatureCommands.CannotRetrieveFromContainer, "SignatureCommandsBaseCannotRetrieveFromContainer", new object[0]));
					}
				}
			}
		}
	}
}