using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADReplicationAttributeMetadata", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216351")]
	public class GetADReplicationAttributeMetadata : ADFactoryCmdletBase<GetADReplicationAttributeMetadataParameterSet, ADReplicationAttributeMetadataFactory<ADReplicationAttributeMetadata>, ADReplicationAttributeMetadata>
	{
		protected internal string[] _propertiesRequested;

		protected bool _showDeleted;

		public GetADReplicationAttributeMetadata()
		{
			base.BeginProcessPipeline.InsertAtEnd(new CmdletSubroutine(this.GetADReplicationAttributeMetadataBeginCSRoutine));
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.GetADReplicationAttributeMetadataProcessCSRoutine));
		}

		protected internal void BuildPropertySet()
		{
			if (!this._cmdletParameters.Contains("Properties"))
			{
				this._propertiesRequested = null;
				return;
			}
			else
			{
				object item = this._cmdletParameters["Properties"];
				if (item as object[] == null)
				{
					string[] strArrays = new string[1];
					strArrays[0] = this._cmdletParameters["Properties"] as string;
					this._propertiesRequested = strArrays;
					return;
				}
				else
				{
					this._propertiesRequested = this._cmdletParameters["Properties"] as string[];
					return;
				}
			}
		}

		private bool GetADReplicationAttributeMetadataBeginCSRoutine()
		{
			string item = this._cmdletParameters["Server"] as string;
			if (!base.DoesServerNameRepresentDomainName(item))
			{
				this._showDeleted = this._cmdletParameters.GetSwitchParameterBooleanValue("IncludeDeletedObjects");
				this.BuildPropertySet();
				if (this._propertiesRequested != null)
				{
					Array.Resize<string>(ref this._propertiesRequested, (int)this._propertiesRequested.Length + 2);
					this._propertiesRequested[(int)this._propertiesRequested.Length - 2] = "msDS-ReplAttributeMetaData";
					this._propertiesRequested[(int)this._propertiesRequested.Length - 1] = "msDS-ReplValueMetaData";
				}
				else
				{
					string[] strArrays = new string[3];
					strArrays[0] = "*";
					strArrays[1] = "msDS-ReplAttributeMetaData";
					strArrays[2] = "msDS-ReplValueMetaData";
					this._propertiesRequested = strArrays;
				}
				return true;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = item;
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ServerIsNotDirectoryServer, objArray));
			}
		}

		protected bool GetADReplicationAttributeMetadataProcessCSRoutine()
		{
			ADObjectFactory<ADObject> aDObjectFactory = new ADObjectFactory<ADObject>();
			ADObject item = this._cmdletParameters["Object"] as ADObject;
			this.SetPipelinedSessionInfo(item.SessionInfo);
			CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
			this._factory.SetCmdletSessionInfo(cmdletSessionInfo);
			aDObjectFactory.SetCmdletSessionInfo(cmdletSessionInfo);
			item = aDObjectFactory.GetExtendedObjectFromIdentity(item, cmdletSessionInfo.DefaultPartitionPath, this._propertiesRequested, this._showDeleted);
			foreach (ADReplicationAttributeMetadata extendedObjectFromDirectoryObject in this._factory.GetExtendedObjectFromDirectoryObject(item, "msDS-ReplAttributeMetaData", "DS_REPL_ATTR_META_DATA"))
			{
				base.WriteObject(extendedObjectFromDirectoryObject);
			}
			foreach (ADReplicationAttributeMetadata aDReplicationAttributeMetadatum in this._factory.GetExtendedObjectFromDirectoryObject(item, "msDS-ReplValueMetaData", "DS_REPL_VALUE_META_DATA"))
			{
				base.WriteObject(aDReplicationAttributeMetadatum);
			}
			return true;
		}
	}
}