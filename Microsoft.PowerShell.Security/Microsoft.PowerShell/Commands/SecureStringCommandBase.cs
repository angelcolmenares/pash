using System;
using System.Management.Automation;
using System.Security;

namespace Microsoft.PowerShell.Commands
{
	public abstract class SecureStringCommandBase : PSCmdlet
	{
		private SecureString ss;

		private string commandName;

		protected SecureString SecureStringData
		{
			get
			{
				return this.ss;
			}
			set
			{
				this.ss = value;
			}
		}

		protected SecureStringCommandBase(string name)
		{
			this.commandName = name;
		}

		private SecureStringCommandBase()
		{
		}
	}
}