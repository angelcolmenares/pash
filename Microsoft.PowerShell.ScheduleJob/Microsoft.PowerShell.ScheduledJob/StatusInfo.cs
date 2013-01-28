using System;
using System.Management.Automation;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.PowerShell.ScheduledJob
{
	[Serializable]
	internal class StatusInfo : ISerializable
	{
		private Guid _instanceId;

		private string _name;

		private string _location;

		private string _command;

		private string _statusMessage;

		private JobState _jobState;

		private bool _hasMoreData;

		private DateTime? _startTime;

		private DateTime? _stopTime;

		private ScheduledJobDefinition _definition;

		internal string Command
		{
			get
			{
				return this._command;
			}
		}

		internal ScheduledJobDefinition Definition
		{
			get
			{
				return this._definition;
			}
		}

		internal bool HasMoreData
		{
			get
			{
				return this._hasMoreData;
			}
		}

		internal Guid InstanceId
		{
			get
			{
				return this._instanceId;
			}
		}

		internal string Location
		{
			get
			{
				return this._location;
			}
		}

		internal string Name
		{
			get
			{
				return this._name;
			}
		}

		internal DateTime? StartTime
		{
			get
			{
				return this._startTime;
			}
		}

		internal JobState State
		{
			get
			{
				return this._jobState;
			}
		}

		internal string StatusMessage
		{
			get
			{
				return this._statusMessage;
			}
		}

		internal DateTime? StopTime
		{
			get
			{
				return this._stopTime;
			}
		}

		internal StatusInfo(Guid instanceId, string name, string location, string command, string statusMessage, JobState jobState, bool hasMoreData, DateTime? startTime, DateTime? stopTime, ScheduledJobDefinition definition)
		{
			if (definition != null)
			{
				this._instanceId = instanceId;
				this._name = name;
				this._location = location;
				this._command = command;
				this._statusMessage = statusMessage;
				this._jobState = jobState;
				this._hasMoreData = hasMoreData;
				this._startTime = startTime;
				this._stopTime = stopTime;
				this._definition = definition;
				return;
			}
			else
			{
				throw new PSArgumentNullException("definition");
			}
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		private StatusInfo(SerializationInfo info, StreamingContext context)
		{
			if (info != null)
			{
				this._instanceId = Guid.Parse(info.GetString("Status_InstanceId"));
				this._name = info.GetString("Status_Name");
				this._location = info.GetString("Status_Location");
				this._command = info.GetString("Status_Command");
				this._statusMessage = info.GetString("Status_Message");
				this._jobState = (JobState)info.GetValue("Status_State", typeof(JobState));
				this._hasMoreData = info.GetBoolean("Status_MoreData");
				this._definition = (ScheduledJobDefinition)info.GetValue("Status_Definition", typeof(ScheduledJobDefinition));
				DateTime dateTime = info.GetDateTime("Status_StartTime");
				if (dateTime == DateTime.MinValue)
				{
					this._startTime = null;
				}
				else
				{
					this._startTime = new DateTime?(dateTime);
				}
				DateTime dateTime1 = info.GetDateTime("Status_StopTime");
				if (dateTime1 == DateTime.MinValue)
				{
					this._stopTime = null;
					return;
				}
				else
				{
					this._stopTime = new DateTime?(dateTime1);
					return;
				}
			}
			else
			{
				throw new PSArgumentNullException("info");
			}
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info != null)
			{
				info.AddValue("Status_InstanceId", this._instanceId);
				info.AddValue("Status_Name", this._name);
				info.AddValue("Status_Location", this._location);
				info.AddValue("Status_Command", this._command);
				info.AddValue("Status_Message", this._statusMessage);
				info.AddValue("Status_State", this._jobState);
				info.AddValue("Status_MoreData", this._hasMoreData);
				info.AddValue("Status_Definition", this._definition);
				if (!this._startTime.HasValue)
				{
					info.AddValue("Status_StartTime", DateTime.MinValue);
				}
				else
				{
					info.AddValue("Status_StartTime", this._startTime);
				}
				if (!this._stopTime.HasValue)
				{
					info.AddValue("Status_StopTime", DateTime.MinValue);
					return;
				}
				else
				{
					info.AddValue("Status_StopTime", this._stopTime);
					return;
				}
			}
			else
			{
				throw new PSArgumentNullException("info");
			}
		}
	}
}