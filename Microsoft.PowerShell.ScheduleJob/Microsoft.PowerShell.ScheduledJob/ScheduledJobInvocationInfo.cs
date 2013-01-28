using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.ScheduledJob
{
	[Serializable]
	public sealed class ScheduledJobInvocationInfo : JobInvocationInfo
	{
		public const string ScriptBlockParameter = "ScriptBlock";

		public const string FilePathParameter = "FilePath";

		public const string RunAs32Parameter = "RunAs32";

		public const string AuthenticationParameter = "Authentication";

		public const string InitializationScriptParameter = "InitializationScript";

		public const string ArgumentListParameter = "ArgumentList";

		public ScheduledJobInvocationInfo(JobDefinition definition, Dictionary<string, object> parameters) : base(definition, parameters)
		{
			if (definition != null)
			{
				base.Name = definition.Name;
				return;
			}
			else
			{
				throw new PSArgumentNullException("definition");
			}
		}

		internal ScheduledJobInvocationInfo(SerializationInfo info, StreamingContext context)
		{
			if (info != null)
			{
				this.DeserializeInvocationInfo(info);
				return;
			}
			else
			{
				throw new PSArgumentNullException("info");
			}
		}

		private void DeserializeInvocationInfo(SerializationInfo info)
		{
			string str = info.GetString("InvocationInfo_Command");
			string str1 = info.GetString("InvocationInfo_Name");
			string str2 = info.GetString("InvocationInfo_ModuleName");
			string str3 = info.GetString("InvocationInfo_AdapterTypeName");
			Dictionary<string, object> strs = new Dictionary<string, object>();
			string str4 = info.GetString("InvocationParam_ScriptBlock");
			if (str4 != null)
			{
				strs.Add("ScriptBlock", ScriptBlock.Create(str4));
			}
			string str5 = info.GetString("InvocationParam_FilePath");
			if (!string.IsNullOrEmpty(str5))
			{
				strs.Add("FilePath", str5);
			}
			str4 = info.GetString("InvocationParam_InitScript");
			if (!string.IsNullOrEmpty(str4))
			{
				strs.Add("InitializationScript", ScriptBlock.Create(str4));
			}
			bool flag = info.GetBoolean("InvocationParam_RunAs32");
			strs.Add("RunAs32", flag);
			AuthenticationMechanism value = (AuthenticationMechanism)info.GetValue("InvocationParam_Authentication", typeof(AuthenticationMechanism));
			strs.Add("Authentication", value);
			object[] objArray = (object[])info.GetValue("InvocationParam_ArgList", typeof(object[]));
			if (objArray != null)
			{
				strs.Add("ArgumentList", objArray);
			}
			JobDefinition jobDefinition = new JobDefinition(null, str, str1);
			jobDefinition.ModuleName = str2;
			jobDefinition.JobSourceAdapterTypeName = str3;
			CommandParameterCollection commandParameterCollection = new CommandParameterCollection();
			foreach (KeyValuePair<string, object> keyValuePair in strs)
			{
				CommandParameter commandParameter = new CommandParameter(keyValuePair.Key, keyValuePair.Value);
				commandParameterCollection.Add(commandParameter);
			}
			base.Definition = jobDefinition;
			base.Name = str1;
			base.Command = str;
			base.Parameters.Add(commandParameterCollection);
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info != null)
			{
				this.SerializeInvocationInfo(info);
				return;
			}
			else
			{
				throw new PSArgumentNullException("info");
			}
		}

		private void SerializeInvocationInfo(SerializationInfo info)
		{
			info.AddValue("InvocationInfo_Command", base.Command);
			info.AddValue("InvocationInfo_Name", base.Name);
			info.AddValue("InvocationInfo_AdapterType", base.Definition.JobSourceAdapterType);
			info.AddValue("InvocationInfo_ModuleName", base.Definition.ModuleName);
			info.AddValue("InvocationInfo_AdapterTypeName", base.Definition.JobSourceAdapterTypeName);
			Dictionary<string, object> strs = new Dictionary<string, object>();
			foreach (CommandParameter item in base.Parameters[0])
			{
				if (strs.ContainsKey(item.Name))
				{
					continue;
				}
				strs.Add(item.Name, item.Value);
			}
			if (!strs.ContainsKey("ScriptBlock"))
			{
				info.AddValue("InvocationParam_ScriptBlock", null);
			}
			else
			{
				ScriptBlock scriptBlock = (ScriptBlock)strs["ScriptBlock"];
				info.AddValue("InvocationParam_ScriptBlock", scriptBlock.ToString());
			}
			if (!strs.ContainsKey("FilePath"))
			{
				info.AddValue("InvocationParam_FilePath", string.Empty);
			}
			else
			{
				string str = (string)strs["FilePath"];
				info.AddValue("InvocationParam_FilePath", str);
			}
			if (!strs.ContainsKey("InitializationScript"))
			{
				info.AddValue("InvocationParam_InitScript", string.Empty);
			}
			else
			{
				ScriptBlock item1 = (ScriptBlock)strs["InitializationScript"];
				info.AddValue("InvocationParam_InitScript", item1.ToString());
			}
			if (!strs.ContainsKey("RunAs32"))
			{
				info.AddValue("InvocationParam_RunAs32", false);
			}
			else
			{
				bool flag = (bool)strs["RunAs32"];
				info.AddValue("InvocationParam_RunAs32", flag);
			}
			if (!strs.ContainsKey("Authentication"))
			{
				info.AddValue("InvocationParam_Authentication", AuthenticationMechanism.Default);
			}
			else
			{
				AuthenticationMechanism authenticationMechanism = (AuthenticationMechanism)strs["Authentication"];
				info.AddValue("InvocationParam_Authentication", authenticationMechanism);
			}
			if (!strs.ContainsKey("ArgumentList"))
			{
				info.AddValue("InvocationParam_ArgList", null);
				return;
			}
			else
			{
				object[] objArray = (object[])strs["ArgumentList"];
				info.AddValue("InvocationParam_ArgList", objArray);
				return;
			}
		}
	}
}