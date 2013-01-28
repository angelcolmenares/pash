using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADAccountExpiration", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219354", SupportsShouldProcess=true)]
	public class SetADAccountExpiration : ADSetCmdletBase<SetADAccountExpirationParameterSet, ADAccountFactory<ADAccount>, ADAccount>
	{
		public SetADAccountExpiration()
		{
			base.BeginProcessPipeline.Clear();
			base.BeginProcessPipeline.InsertAtEnd(new CmdletSubroutine(this.SetADAccountExpirationBeginCSRoutine));
		}

		private bool SetADAccountExpirationBeginCSRoutine()
		{
			DateTime? item;
			bool flag = this._cmdletParameters.Contains("TimeSpan");
			bool flag1 = this._cmdletParameters.Contains("DateTime");
			if (flag || flag1)
			{
				if (!flag1 || !flag)
				{
					if (!flag)
					{
						item = (DateTime?)(this._cmdletParameters["DateTime"] as DateTime?);
					}
					else
					{
						TimeSpan timeSpan = (TimeSpan)this._cmdletParameters["TimeSpan"];
						item = new DateTime?(DateTime.Now + timeSpan);
					}
					this._cmdletParameters.RemoveParameter("TimeSpan");
					this._cmdletParameters.RemoveParameter("DateTime");
					this._cmdletParameters["AccountExpirationDate"] = item;
					return true;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = string.Format("{0}, {1}", "TimeSpan", "DateTime");
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ParameterRequiredOnlyOne, objArray));
				}
			}
			else
			{
				object[] objArray1 = new object[1];
				objArray1[0] = string.Format("{0}, {1}", "TimeSpan", "DateTime");
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ParameterRequiredMultiple, objArray1));
			}
		}
	}
}