using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Core;
using System;
using System.Collections.Generic;
using System.Data.Services.Providers;

namespace Microsoft.Management.Odata.PS
{
	internal class PSReferenceCommand : PSCommand, IReferenceSetCommand, ICommand, IDisposable
	{
		private PSReferenceSetCmdletInfo referenceCmdletInfo;

		public PSReferenceCommand(Envelope<PSRunspace, UserContext> runspace, ResourceType entityType, PSReferenceSetCmdletInfo referenceCmdletInfo) : base(runspace, entityType, referenceCmdletInfo, (CommandType)1)
		{
			this.referenceCmdletInfo = referenceCmdletInfo;
		}

		public void AddReferredObject(Dictionary<string, object> keys)
		{
			foreach (KeyValuePair<string, object> key in keys)
			{
				string item = this.referenceCmdletInfo.ReferredObjectParameterMapping[key.Key];
				try
				{
					base.AddParameterInternal(item, key.Value, false);
				}
				catch (ArgumentException argumentException1)
				{
					ArgumentException argumentException = argumentException1;
					argumentException.Trace("Ignoring exception in AddReferredObject");
				}
			}
		}

		public void AddReferringObject(Dictionary<string, object> keys)
		{
			foreach (KeyValuePair<string, object> key in keys)
			{
				string item = this.referenceCmdletInfo.ReferringObjectParameterMapping[key.Key];
				try
				{
					base.AddParameterInternal(item, key.Value, false);
				}
				catch (ArgumentException argumentException1)
				{
					ArgumentException argumentException = argumentException1;
					argumentException.Trace("Ignoring exception in AddReferringObject");
				}
			}
		}
	}
}