using Microsoft.Management.Infrastructure;
using System;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CimSessionWrapper
	{
		private uint sessionId;

		private Guid instanceId;

		private string name;

		private string computerName;

		private CimSession cimSession;

		private ProtocolType protocol;

		private PSObject psObject;

		public CimSession CimSession
		{
			get
			{
				return this.cimSession;
			}
		}

		public string ComputerName
		{
			get
			{
				return this.computerName;
			}
		}

		public Guid InstanceId
		{
			get
			{
				return this.instanceId;
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public string Protocol
		{
			get
			{
				ProtocolType protocolType = this.protocol;
				switch (protocolType)
				{
					case ProtocolType.Default:
					case ProtocolType.Wsman:
					{
						return "WSMAN";
					}
					case ProtocolType.Dcom:
					{
						return "DCOM";
					}
					default:
					{
						return "WSMAN";
					}
				}
			}
		}

		public uint SessionId
		{
			get
			{
				return this.sessionId;
			}
		}

		internal CimSessionWrapper(uint theSessionId, Guid theInstanceId, string theName, string theComputerName, CimSession theCimSession, ProtocolType theProtocol)
		{
			this.sessionId = theSessionId;
			this.instanceId = theInstanceId;
			this.name = theName;
			this.computerName = theComputerName;
			this.cimSession = theCimSession;
			this.psObject = null;
			this.protocol = theProtocol;
		}

		internal ProtocolType GetProtocolType()
		{
			return this.protocol;
		}

		internal PSObject GetPSObject()
		{
			if (this.psObject != null)
			{
				this.psObject.Properties[CimSessionState.idPropName].Value = this.SessionId;
				this.psObject.Properties[CimSessionState.namePropName].Value = this.name;
				this.psObject.Properties[CimSessionState.instanceidPropName].Value = this.instanceId;
				this.psObject.Properties[CimSessionState.computernamePropName].Value = this.ComputerName;
				this.psObject.Properties[CimSessionState.protocolPropName].Value = this.Protocol;
			}
			else
			{
				this.psObject = new PSObject(this.cimSession);
				this.psObject.Properties.Add(new PSNoteProperty(CimSessionState.idPropName, (object)this.sessionId));
				this.psObject.Properties.Add(new PSNoteProperty(CimSessionState.namePropName, this.name));
				this.psObject.Properties.Add(new PSNoteProperty(CimSessionState.instanceidPropName, (object)this.instanceId));
				this.psObject.Properties.Add(new PSNoteProperty(CimSessionState.computernamePropName, this.ComputerName));
				this.psObject.Properties.Add(new PSNoteProperty(CimSessionState.protocolPropName, this.Protocol));
			}
			return this.psObject;
		}
	}
}