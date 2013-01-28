using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Tracing;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using System.Xml;

namespace Microsoft.PowerShell.ScheduledJob
{
	[Serializable]
	public sealed class ScheduledJobDefinition : ISerializable, IDisposable
	{
		private const string TaskExecutionPath = "powershell.exe";

		private const string TaskArguments = "-NoLogo -NonInteractive -WindowStyle Hidden -Command \"Import-Module PSScheduledJob; $jobDef = [Microsoft.PowerShell.ScheduledJob.ScheduledJobDefinition]::LoadFromStore('{0}', '{1}'); $jobDef.Run()\"";

		private JobInvocationInfo _invocationInfo;

		private ScheduledJobOptions _options;

		private PSCredential _credential;

		private Guid _globalId;

		private string _name;

		private int _id;

		private int _executionHistoryLength;

		private bool _enabled;

		private Dictionary<int, ScheduledJobTrigger> _triggers;

		private int _currentTriggerId;

		private string _definitionFilePath;

		private string _definitionOutputPath;

		private bool _isDisposed;

		private static object LockObject;

		private static int CurrentId;

		private static int DefaultExecutionHistoryLength;

		internal static ScheduledJobDefinitionRepository Repository;

		public string Command
		{
			get
			{
				return this._invocationInfo.Command;
			}
		}

		public PSCredential Credential
		{
			get
			{
				return this._credential;
			}
			internal set
			{
				this._credential = value;
			}
		}

		public JobDefinition Definition
		{
			get
			{
				return this._invocationInfo.Definition;
			}
		}

		public bool Enabled
		{
			get
			{
				return this._enabled;
			}
		}

		public int ExecutionHistoryLength
		{
			get
			{
				return this._executionHistoryLength;
			}
		}

		public Guid GlobalId
		{
			get
			{
				return this._globalId;
			}
		}

		public int Id
		{
			get
			{
				return this._id;
			}
		}

		public JobInvocationInfo InvocationInfo
		{
			get
			{
				return this._invocationInfo;
			}
		}

		public List<ScheduledJobTrigger> JobTriggers
		{
			get
			{
				List<int> nums = null;
				return this.GetTriggers(null, out nums);
			}
		}

		public string Name
		{
			get
			{
				return this._name;
			}
		}

		public ScheduledJobOptions Options
		{
			get
			{
				return new ScheduledJobOptions(this._options);
			}
		}

		internal string OutputPath
		{
			get
			{
				return this._definitionOutputPath;
			}
		}

		public string PSExecutionArgs
		{
			get
			{
				string str = this._invocationInfo.Name.Replace("'", "''");
				object[] objArray = new object[2];
				objArray[0] = str;
				objArray[1] = this._definitionFilePath;
				return string.Format(CultureInfo.InvariantCulture, "-NoLogo -NonInteractive -WindowStyle Hidden -Command \"Import-Module PSScheduledJob; $jobDef = [Microsoft.PowerShell.ScheduledJob.ScheduledJobDefinition]::LoadFromStore('{0}', '{1}'); $jobDef.Run()\"", objArray);
			}
		}

		public string PSExecutionPath
		{
			get
			{
				return "powershell.exe";
			}
		}

		static ScheduledJobDefinition()
		{
			ScheduledJobDefinition.LockObject = new object();
			ScheduledJobDefinition.CurrentId = 0;
			ScheduledJobDefinition.DefaultExecutionHistoryLength = 32;
			ScheduledJobDefinition.Repository = new ScheduledJobDefinitionRepository();
		}

		private ScheduledJobDefinition()
		{
			this._globalId = Guid.NewGuid();
			this._name = string.Empty;
			this._id = ScheduledJobDefinition.GetCurrentId();
			this._executionHistoryLength = ScheduledJobDefinition.DefaultExecutionHistoryLength;
			this._enabled = true;
			this._triggers = new Dictionary<int, ScheduledJobTrigger>();
		}

		public ScheduledJobDefinition(JobInvocationInfo invocationInfo, IEnumerable<ScheduledJobTrigger> triggers, ScheduledJobOptions options, PSCredential credential)
		{
			ScheduledJobOptions scheduledJobOption;
			this._globalId = Guid.NewGuid();
			this._name = string.Empty;
			this._id = ScheduledJobDefinition.GetCurrentId();
			this._executionHistoryLength = ScheduledJobDefinition.DefaultExecutionHistoryLength;
			this._enabled = true;
			this._triggers = new Dictionary<int, ScheduledJobTrigger>();
			if (invocationInfo != null)
			{
				this._name = invocationInfo.Name;
				this._invocationInfo = invocationInfo;
				this.SetTriggers(triggers, false);
				ScheduledJobDefinition scheduledJobDefinition = this;
				if (options != null)
				{
					scheduledJobOption = new ScheduledJobOptions(options);
				}
				else
				{
					scheduledJobOption = new ScheduledJobOptions();
				}
				scheduledJobDefinition._options = scheduledJobOption;
				this._options.JobDefinition = this;
				this._credential = credential;
				return;
			}
			else
			{
				throw new PSArgumentNullException("invocationInfo");
			}
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		private ScheduledJobDefinition(SerializationInfo info, StreamingContext context)
		{
			this._globalId = Guid.NewGuid();
			this._name = string.Empty;
			this._id = ScheduledJobDefinition.GetCurrentId();
			this._executionHistoryLength = ScheduledJobDefinition.DefaultExecutionHistoryLength;
			this._enabled = true;
			this._triggers = new Dictionary<int, ScheduledJobTrigger>();
			if (info != null)
			{
				this._options = (ScheduledJobOptions)info.GetValue("Options_Member", typeof(ScheduledJobOptions));
				this._globalId = Guid.Parse(info.GetString("GlobalId_Member"));
				this._name = info.GetString("Name_Member");
				this._executionHistoryLength = info.GetInt32("HistoryLength_Member");
				this._enabled = info.GetBoolean("Enabled_Member");
				this._triggers = (Dictionary<int, ScheduledJobTrigger>)info.GetValue("Triggers_Member", typeof(Dictionary<int, ScheduledJobTrigger>));
				this._currentTriggerId = info.GetInt32("CurrentTriggerId_Member");
				this._definitionFilePath = info.GetString("FilePath_Member");
				this._definitionOutputPath = info.GetString("OutputPath_Member");
				object value = info.GetValue("InvocationInfo_Member", typeof(object));
				this._invocationInfo = value as JobInvocationInfo;
				this._options.JobDefinition = this;
				foreach (ScheduledJobTrigger scheduledJobTrigger in this._triggers.Values)
				{
					scheduledJobTrigger.JobDefinition = this;
				}
				this._isDisposed = false;
				return;
			}
			else
			{
				throw new PSArgumentNullException("info");
			}
		}

		private void AddToJobStore()
		{
			FileStream fileStream = null;
			try
			{
				fileStream = ScheduledJobStore.CreateFileForJobDefinition(this.Name);
				this._definitionFilePath = ScheduledJobStore.GetJobDefinitionLocation();
				this._definitionOutputPath = ScheduledJobStore.GetJobRunOutputDirectory(this.Name);
				XmlObjectSerializer netDataContractSerializer = new NetDataContractSerializer();
				netDataContractSerializer.WriteObject(fileStream, this);
				fileStream.Flush();
			}
			finally
			{
				if (fileStream != null)
				{
					fileStream.Close();
				}
			}
			if (this.Credential != null)
			{
				this.UpdateFilePermissions(this.Credential.UserName);
			}
		}

		private void AddToWTS()
		{
			using (ScheduledJobWTS scheduledJobWT = new ScheduledJobWTS())
			{
				scheduledJobWT.CreateTask(this);
			}
		}

		public void AddTriggers(IEnumerable<ScheduledJobTrigger> triggers, bool save)
		{
			this.IsDisposed();
			if (triggers != null)
			{
				this.ValidateTriggers(triggers);
				Collection<int> nums = new Collection<int>();
				foreach (ScheduledJobTrigger trigger in triggers)
				{
					ScheduledJobTrigger scheduledJobTrigger = new ScheduledJobTrigger(trigger);
					ScheduledJobDefinition scheduledJobDefinition = this;
					int num = scheduledJobDefinition._currentTriggerId + 1;
					int num1 = num;
					scheduledJobDefinition._currentTriggerId = num;
					scheduledJobTrigger.Id = num1;
					nums.Add(scheduledJobTrigger.Id);
					scheduledJobTrigger.JobDefinition = this;
					this._triggers.Add(scheduledJobTrigger.Id, scheduledJobTrigger);
				}
				if (save)
				{
					this.Save();
				}
				return;
			}
			else
			{
				throw new PSArgumentNullException("triggers");
			}
		}

		public void ClearExecutionHistory()
		{
			this.IsDisposed();
			ScheduledJobStore.RemoveAllJobRuns(this.Name);
			ScheduledJobSourceAdapter.ClearRepositoryForDefinition(this.Name);
		}

		public void Dispose()
		{
			this._isDisposed = true;
			GC.SuppressFinalize(this);
		}

		private static int GetCurrentId()
		{
			int num;
			lock (ScheduledJobDefinition.LockObject)
			{
				int currentId = ScheduledJobDefinition.CurrentId + 1;
				ScheduledJobDefinition.CurrentId = currentId;
				num = currentId;
			}
			return num;
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info != null)
			{
				info.AddValue("Options_Member", this._options);
				info.AddValue("GlobalId_Member", this._globalId.ToString());
				info.AddValue("Name_Member", this._name);
				info.AddValue("HistoryLength_Member", this._executionHistoryLength);
				info.AddValue("Enabled_Member", this._enabled);
				info.AddValue("Triggers_Member", this._triggers);
				info.AddValue("CurrentTriggerId_Member", this._currentTriggerId);
				info.AddValue("FilePath_Member", this._definitionFilePath);
				info.AddValue("OutputPath_Member", this._definitionOutputPath);
				info.AddValue("InvocationInfo_Member", this._invocationInfo);
				return;
			}
			else
			{
				throw new PSArgumentNullException("info");
			}
		}

		public ScheduledJobTrigger GetTrigger(int triggerId)
		{
			this.IsDisposed();
			if (!this._triggers.ContainsKey(triggerId))
			{
				return null;
			}
			else
			{
				return new ScheduledJobTrigger(this._triggers[triggerId]);
			}
		}

		public List<ScheduledJobTrigger> GetTriggers(IEnumerable<int> triggerIds, out List<int> notFoundIds)
		{
			List<ScheduledJobTrigger> scheduledJobTriggers;
			this.IsDisposed();
			List<int> nums = new List<int>();
			if (triggerIds != null)
			{
				scheduledJobTriggers = new List<ScheduledJobTrigger>();
				foreach (int triggerId in triggerIds)
				{
					if (!this._triggers.ContainsKey(triggerId))
					{
						nums.Add(triggerId);
					}
					else
					{
						scheduledJobTriggers.Add(new ScheduledJobTrigger(this._triggers[triggerId]));
					}
				}
			}
			else
			{
				scheduledJobTriggers = new List<ScheduledJobTrigger>();
				foreach (ScheduledJobTrigger value in this._triggers.Values)
				{
					scheduledJobTriggers.Add(new ScheduledJobTrigger(value));
				}
			}
			notFoundIds = nums;
			List<ScheduledJobTrigger> scheduledJobTriggers1 = scheduledJobTriggers;
			scheduledJobTriggers1.Sort((ScheduledJobTrigger firstTrigger, ScheduledJobTrigger secondTrigger) => firstTrigger.Id - secondTrigger.Id);
			return scheduledJobTriggers;
		}

		private void IsDisposed()
		{
			if (!this._isDisposed)
			{
				return;
			}
			else
			{
				string str = StringUtil.Format(ScheduledJobErrorStrings.DefinitionObjectDisposed, this.Name);
				throw new RuntimeException(str);
			}
		}

		internal static ScheduledJobDefinition LoadDefFromStore(string definitionName, string definitionPath)
		{
			ScheduledJobDefinition scheduledJobDefinition = null;
			FileStream fileForJobDefinition = null;
			try
			{
				if (definitionPath != null)
				{
					fileForJobDefinition = ScheduledJobStore.GetFileForJobDefinition(definitionName, definitionPath, FileMode.Open, FileAccess.Read, FileShare.Read);
				}
				else
				{
					fileForJobDefinition = ScheduledJobStore.GetFileForJobDefinition(definitionName, FileMode.Open, FileAccess.Read, FileShare.Read);
				}
				XmlObjectSerializer netDataContractSerializer = new NetDataContractSerializer();
				scheduledJobDefinition = netDataContractSerializer.ReadObject(fileForJobDefinition) as ScheduledJobDefinition;
			}
			finally
			{
				if (fileForJobDefinition != null)
				{
					fileForJobDefinition.Close();
				}
			}
			return scheduledJobDefinition;
		}

		public static ScheduledJobDefinition LoadFromStore(string definitionName, string definitionPath)
		{
			if (!string.IsNullOrEmpty(definitionName))
			{
				ScheduledJobDefinition scheduledJobDefinition = null;
				bool flag = false;
				Exception exception = null;
				try
				{
					scheduledJobDefinition = ScheduledJobDefinition.LoadDefFromStore(definitionName, definitionPath);
				}
				catch (DirectoryNotFoundException directoryNotFoundException1)
				{
					DirectoryNotFoundException directoryNotFoundException = directoryNotFoundException1;
					exception = directoryNotFoundException;
				}
				catch (FileNotFoundException fileNotFoundException1)
				{
					FileNotFoundException fileNotFoundException = fileNotFoundException1;
					exception = fileNotFoundException;
					flag = true;
				}
				catch (UnauthorizedAccessException unauthorizedAccessException1)
				{
					UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
					exception = unauthorizedAccessException;
				}
				catch (IOException oException1)
				{
					IOException oException = oException1;
					exception = oException;
				}
				catch (XmlException xmlException1)
				{
					XmlException xmlException = xmlException1;
					exception = xmlException;
					flag = true;
				}
				catch (TypeInitializationException typeInitializationException1)
				{
					TypeInitializationException typeInitializationException = typeInitializationException1;
					exception = typeInitializationException;
					flag = true;
				}
				catch (ArgumentNullException argumentNullException1)
				{
					ArgumentNullException argumentNullException = argumentNullException1;
					exception = argumentNullException;
					flag = true;
				}
				catch (SerializationException serializationException1)
				{
					SerializationException serializationException = serializationException1;
					exception = serializationException;
					flag = true;
				}
				if (exception == null)
				{
					scheduledJobDefinition.SyncWithWTS();
					return scheduledJobDefinition;
				}
				else
				{
					if (!flag || definitionPath != null && !ScheduledJobStore.IsDefaultUserPath(definitionPath))
					{
						throw new ScheduledJobException(StringUtil.Format(ScheduledJobErrorStrings.CannotFindJobDefinition, definitionName), exception);
					}
					else
					{
						ScheduledJobDefinition.RemoveDefinition(definitionName);
						throw new ScheduledJobException(StringUtil.Format(ScheduledJobErrorStrings.CantLoadDefinitionFromStore, definitionName), exception);
					}
				}
			}
			else
			{
				throw new PSArgumentNullException("definitionName");
			}
		}

		private void LoadRepository()
		{
			ScheduledJobDefinition.RefreshRepositoryFromStore(null);
		}

		internal static Dictionary<string, Exception> RefreshRepositoryFromStore(Action<ScheduledJobDefinition> itemFound = null)
		{
			string str;
			Dictionary<string, Exception> strs = new Dictionary<string, Exception>();
			IEnumerable<string> jobDefinitions = ScheduledJobStore.GetJobDefinitions();
			HashSet<string> strs1 = new HashSet<string>();
			foreach (string jobDefinition in jobDefinitions)
			{
				int num = jobDefinition.LastIndexOf('\\');
				if (num != -1)
				{
					str = jobDefinition.Substring(num + 1);
				}
				else
				{
					str = jobDefinition;
				}
				string str1 = str;
				strs1.Add(str1);
			}
			foreach (ScheduledJobDefinition definition in ScheduledJobDefinition.Repository.Definitions)
			{
				if (strs1.Contains(definition.Name))
				{
					strs1.Remove(definition.Name);
					if (itemFound == null)
					{
						continue;
					}
					itemFound(definition);
				}
				else
				{
					ScheduledJobDefinition.Repository.Remove(definition);
				}
			}
			foreach (string str2 in strs1)
			{
				try
				{
					ScheduledJobDefinition scheduledJobDefinition = ScheduledJobDefinition.LoadDefFromStore(str2, null);
					ScheduledJobDefinition.Repository.AddOrReplace(scheduledJobDefinition);
					if (itemFound != null)
					{
						itemFound(scheduledJobDefinition);
					}
				}
				catch (IOException oException1)
				{
					IOException oException = oException1;
					strs.Add(str2, oException);
				}
				catch (XmlException xmlException1)
				{
					XmlException xmlException = xmlException1;
					strs.Add(str2, xmlException);
				}
				catch (TypeInitializationException typeInitializationException1)
				{
					TypeInitializationException typeInitializationException = typeInitializationException1;
					strs.Add(str2, typeInitializationException);
				}
				catch (SerializationException serializationException1)
				{
					SerializationException serializationException = serializationException1;
					strs.Add(str2, serializationException);
				}
				catch (ArgumentNullException argumentNullException1)
				{
					ArgumentNullException argumentNullException = argumentNullException1;
					strs.Add(str2, argumentNullException);
				}
				catch (UnauthorizedAccessException unauthorizedAccessException1)
				{
					UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
					strs.Add(str2, unauthorizedAccessException);
				}
			}
			return strs;
		}

		public void Register()
		{
			object message;
			this.IsDisposed();
			this.LoadRepository();
			ScheduledJobDefinition.ValidateName(this.Name);
			Exception exception = null;
			bool flag = false;
			try
			{
				this.AddToJobStore();
			}
			catch (ArgumentException argumentException1)
			{
				ArgumentException argumentException = argumentException1;
				exception = argumentException;
			}
			catch (DirectoryNotFoundException directoryNotFoundException1)
			{
				DirectoryNotFoundException directoryNotFoundException = directoryNotFoundException1;
				exception = directoryNotFoundException;
			}
			catch (FileNotFoundException fileNotFoundException1)
			{
				FileNotFoundException fileNotFoundException = fileNotFoundException1;
				exception = fileNotFoundException;
			}
			catch (UnauthorizedAccessException unauthorizedAccessException1)
			{
				UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
				exception = unauthorizedAccessException;
			}
			catch (IOException oException1)
			{
				IOException oException = oException1;
				exception = oException;
			}
			catch (SerializationException serializationException1)
			{
				SerializationException serializationException = serializationException1;
				flag = true;
				exception = serializationException;
			}
			catch (InvalidDataContractException invalidDataContractException1)
			{
				InvalidDataContractException invalidDataContractException = invalidDataContractException1;
				flag = true;
				exception = invalidDataContractException;
			}
			catch (ScheduledJobException scheduledJobException1)
			{
				ScheduledJobException scheduledJobException = scheduledJobException1;
				flag = !scheduledJobException.FQEID.Equals("ScheduledJobDefExists", StringComparison.OrdinalIgnoreCase);
				exception = scheduledJobException;
			}
			if (exception == null)
			{
				exception = null;
				try
				{
					this.AddToWTS();
				}
				catch (ArgumentException argumentException3)
				{
					ArgumentException argumentException2 = argumentException3;
					exception = argumentException2;
				}
				catch (DirectoryNotFoundException directoryNotFoundException3)
				{
					DirectoryNotFoundException directoryNotFoundException2 = directoryNotFoundException3;
					exception = directoryNotFoundException2;
				}
				catch (FileNotFoundException fileNotFoundException3)
				{
					FileNotFoundException fileNotFoundException2 = fileNotFoundException3;
					exception = fileNotFoundException2;
				}
				catch (UnauthorizedAccessException unauthorizedAccessException3)
				{
					UnauthorizedAccessException unauthorizedAccessException2 = unauthorizedAccessException3;
					exception = unauthorizedAccessException2;
				}
				catch (IOException oException3)
				{
					IOException oException2 = oException3;
					exception = oException2;
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					exception = cOMException;
				}
				if (exception == null)
				{
					ScheduledJobDefinition.Repository.AddOrReplace(this);
					return;
				}
				else
				{
					this.RemoveFromJobStore();
					string errorRegisteringDefinitionTask = ScheduledJobErrorStrings.ErrorRegisteringDefinitionTask;
					object[] name = new object[2];
					name[0] = this.Name;
					object[] objArray = name;
					int num = 1;
					if (!string.IsNullOrEmpty(exception.Message))
					{
						message = exception.Message;
					}
					else
					{
						message = string.Empty;
					}
					objArray[num] = message;
					string str = StringUtil.Format(errorRegisteringDefinitionTask, name);
					throw new ScheduledJobException(str, exception);
				}
			}
			else
			{
				if (flag)
				{
					try
					{
						ScheduledJobStore.RemoveJobDefinition(this.Name);
					}
					catch (DirectoryNotFoundException directoryNotFoundException4)
					{
					}
					catch (FileNotFoundException fileNotFoundException4)
					{
					}
					catch (UnauthorizedAccessException unauthorizedAccessException4)
					{
					}
					catch (IOException oException4)
					{
					}
				}
				if (exception as ScheduledJobException != null)
				{
					throw exception;
				}
				else
				{
					string str1 = StringUtil.Format(ScheduledJobErrorStrings.ErrorRegisteringDefinitionStore, this.Name);
					throw new ScheduledJobException(str1, exception);
				}
			}
		}

		public void Remove(bool force)
		{
			this.IsDisposed();
			try
			{
				this.RemoveFromWTS(force);
			}
			catch (DirectoryNotFoundException directoryNotFoundException)
			{
			}
			catch (FileNotFoundException fileNotFoundException)
			{
			}
			Exception exception = null;
			try
			{
				try
				{
					this.RemoveFromJobStore();
				}
				catch (DirectoryNotFoundException directoryNotFoundException1)
				{
				}
				catch (FileNotFoundException fileNotFoundException1)
				{
				}
				catch (ArgumentException argumentException1)
				{
					ArgumentException argumentException = argumentException1;
					exception = argumentException;
				}
				catch (UnauthorizedAccessException unauthorizedAccessException1)
				{
					UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
					exception = unauthorizedAccessException;
				}
				catch (IOException oException1)
				{
					IOException oException = oException1;
					exception = oException;
				}
			}
			finally
			{
				ScheduledJobDefinition.Repository.Remove(this);
				ScheduledJobSourceAdapter.ClearRepositoryForDefinition(this.Name);
				this.Dispose();
			}
			if (exception == null)
			{
				return;
			}
			else
			{
				string str = StringUtil.Format(ScheduledJobErrorStrings.ErrorRemovingDefinitionStore, this.Name);
				throw new ScheduledJobException(str, exception);
			}
		}

		internal static void RemoveDefinition(string definitionName)
		{
			try
			{
				ScheduledJobStore.RemoveJobDefinition(definitionName);
			}
			catch (DirectoryNotFoundException directoryNotFoundException)
			{
			}
			catch (FileNotFoundException fileNotFoundException)
			{
			}
			catch (UnauthorizedAccessException unauthorizedAccessException)
			{
			}
			catch (IOException oException)
			{
			}
			using (ScheduledJobWTS scheduledJobWT = new ScheduledJobWTS())
			{
				try
				{
					scheduledJobWT.RemoveTaskByName(definitionName, true, true);
				}
				catch (UnauthorizedAccessException unauthorizedAccessException1)
				{
				}
				catch (IOException oException1)
				{
				}
			}
		}

		private void RemoveFromJobStore()
		{
			ScheduledJobStore.RemoveJobDefinition(this.Name);
		}

		private void RemoveFromWTS(bool force)
		{
			using (ScheduledJobWTS scheduledJobWT = new ScheduledJobWTS())
			{
				scheduledJobWT.RemoveTask(this, force);
			}
		}

		public List<int> RemoveTriggers(IEnumerable<int> triggerIds, bool save)
		{
			this.IsDisposed();
			List<int> nums = new List<int>();
			bool flag = false;
			if (triggerIds != null)
			{
				foreach (int triggerId in triggerIds)
				{
					if (!this._triggers.ContainsKey(triggerId))
					{
						nums.Add(triggerId);
					}
					else
					{
						this._triggers[triggerId].JobDefinition = null;
						this._triggers[triggerId].Id = 0;
						this._triggers.Remove(triggerId);
						flag = true;
					}
				}
			}
			else
			{
				this._currentTriggerId = 0;
				if (this._triggers.Count > 0)
				{
					flag = true;
					foreach (ScheduledJobTrigger value in this._triggers.Values)
					{
						value.Id = 0;
						value.JobDefinition = null;
					}
					this._triggers = new Dictionary<int, ScheduledJobTrigger>();
				}
			}
			if (save && flag)
			{
				this.Save();
			}
			return nums;
		}

		internal void RenameAndSave(string newName)
		{
			string str;
			string str1;
			if (!this.InvocationInfo.Name.Equals(newName, StringComparison.OrdinalIgnoreCase))
			{
				ScheduledJobDefinition.ValidateName(newName);
				string name = this.InvocationInfo.Name;
				Exception exception = null;
				try
				{
					ScheduledJobStore.RenameScheduledJobDefDir(name, newName);
				}
				catch (ArgumentException argumentException1)
				{
					ArgumentException argumentException = argumentException1;
					exception = argumentException;
				}
				catch (DirectoryNotFoundException directoryNotFoundException1)
				{
					DirectoryNotFoundException directoryNotFoundException = directoryNotFoundException1;
					exception = directoryNotFoundException;
				}
				catch (FileNotFoundException fileNotFoundException1)
				{
					FileNotFoundException fileNotFoundException = fileNotFoundException1;
					exception = fileNotFoundException;
				}
				catch (UnauthorizedAccessException unauthorizedAccessException1)
				{
					UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
					exception = unauthorizedAccessException;
				}
				catch (IOException oException1)
				{
					IOException oException = oException1;
					exception = oException;
				}
				if (exception == null)
				{
					try
					{
						try
						{
							this.RemoveFromWTS(true);
							this._name = newName;
							this.InvocationInfo.Name = newName;
							this.InvocationInfo.Definition.Name = newName;
							this._definitionOutputPath = ScheduledJobStore.GetJobRunOutputDirectory(this.Name);
							this.UpdateJobStore();
							this.AddToWTS();
							this.UpdateJobRunNames(newName);
						}
						catch (ArgumentException argumentException3)
						{
							ArgumentException argumentException2 = argumentException3;
							exception = argumentException2;
						}
						catch (DirectoryNotFoundException directoryNotFoundException3)
						{
							DirectoryNotFoundException directoryNotFoundException2 = directoryNotFoundException3;
							exception = directoryNotFoundException2;
						}
						catch (FileNotFoundException fileNotFoundException3)
						{
							FileNotFoundException fileNotFoundException2 = fileNotFoundException3;
							exception = fileNotFoundException2;
						}
						catch (UnauthorizedAccessException unauthorizedAccessException3)
						{
							UnauthorizedAccessException unauthorizedAccessException2 = unauthorizedAccessException3;
							exception = unauthorizedAccessException2;
						}
						catch (IOException oException3)
						{
							IOException oException2 = oException3;
							exception = oException2;
						}
					}
					finally
					{
						ScheduledJobSourceAdapter.ClearRepository();
					}
					if (exception == null)
					{
						return;
					}
					else
					{
						try
						{
							this.Remove(true);
						}
						catch (ScheduledJobException scheduledJobException1)
						{
							ScheduledJobException scheduledJobException = scheduledJobException1;
							exception.Data.Add("SchedJobRemoveError", scheduledJobException);
						}
						if (string.IsNullOrEmpty(exception.Message))
						{
							object[] objArray = new object[2];
							objArray[0] = name;
							objArray[1] = newName;
							str1 = StringUtil.Format(ScheduledJobErrorStrings.BrokenRenamingScheduledJob, objArray);
						}
						else
						{
							object[] message = new object[3];
							message[0] = name;
							message[1] = newName;
							message[2] = exception.Message;
							str1 = StringUtil.Format(ScheduledJobErrorStrings.BrokenRenamingScheduledJobWithMessage, message);
						}
						throw new ScheduledJobException(str1, exception);
					}
				}
				else
				{
					if (string.IsNullOrEmpty(exception.Message))
					{
						object[] objArray1 = new object[2];
						objArray1[0] = name;
						objArray1[1] = newName;
						str = StringUtil.Format(ScheduledJobErrorStrings.ErrorRenamingScheduledJob, objArray1);
					}
					else
					{
						object[] message1 = new object[3];
						message1[0] = name;
						message1[1] = newName;
						message1[2] = exception.Message;
						str = StringUtil.Format(ScheduledJobErrorStrings.ErrorRenamingScheduledJobWithMessage, message1);
					}
					throw new ScheduledJobException(str, exception);
				}
			}
			else
			{
				return;
			}
		}

		public Job2 Run()
		{
			object message;
			object obj = null;
			Job2 job = null;
			PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
			using (traceSource)
			{
				Exception exception = null;
				try
				{
					JobManager jobManager = Runspace.DefaultRunspace.JobManager;
					job = jobManager.NewJob(this.InvocationInfo);
					ScheduledJob scheduledJob = job as ScheduledJob;
					if (scheduledJob != null)
					{
						scheduledJob.Definition = this;
						scheduledJob.AllowSetShouldExit = true;
					}
					job.StateChanged += (object sender, JobStateEventArgs e) => {
						if (e.JobStateInfo.State == JobState.Running)
						{
							jobManager.PersistJob(job, this.Definition);
						}
					}
					;
					job.StartJob();
					object[] name = new object[2];
					name[0] = job.Name;
					DateTime? pSBeginTime = job.PSBeginTime;
					name[1] = pSBeginTime.ToString();
					traceSource.WriteScheduledJobStartEvent(name);
					job.Finished.WaitOne();
					jobManager.PersistJob(job, this.Definition);
					System.Management.Automation.PowerShell powerShell = System.Management.Automation.PowerShell.Create();
					using (powerShell)
					{
						powerShell.AddCommand("Receive-Job").AddParameter("Job", job).AddParameter("Keep", true);
						powerShell.Invoke();
					}
					object[] str = new object[3];
					str[0] = job.Name;
					DateTime? pSEndTime = job.PSEndTime;
					str[1] = pSEndTime.ToString();
					str[2] = job.JobStateInfo.State.ToString();
					traceSource.WriteScheduledJobCompleteEvent(str);
				}
				catch (RuntimeException runtimeException1)
				{
					RuntimeException runtimeException = runtimeException1;
					exception = runtimeException;
				}
				catch (InvalidOperationException invalidOperationException1)
				{
					InvalidOperationException invalidOperationException = invalidOperationException1;
					exception = invalidOperationException;
				}
				catch (SecurityException securityException1)
				{
					SecurityException securityException = securityException1;
					exception = securityException;
				}
				catch (ThreadAbortException threadAbortException1)
				{
					ThreadAbortException threadAbortException = threadAbortException1;
					exception = threadAbortException;
				}
				catch (IOException oException1)
				{
					IOException oException = oException1;
					exception = oException;
				}
				catch (UnauthorizedAccessException unauthorizedAccessException1)
				{
					UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
					exception = unauthorizedAccessException;
				}
				catch (ArgumentException argumentException1)
				{
					ArgumentException argumentException = argumentException1;
					exception = argumentException;
				}
				catch (ScriptCallDepthException scriptCallDepthException1)
				{
					ScriptCallDepthException scriptCallDepthException = scriptCallDepthException1;
					exception = scriptCallDepthException;
				}
				catch (SerializationException serializationException1)
				{
					SerializationException serializationException = serializationException1;
					exception = serializationException;
				}
				catch (InvalidDataContractException invalidDataContractException1)
				{
					InvalidDataContractException invalidDataContractException = invalidDataContractException1;
					exception = invalidDataContractException;
				}
				catch (XmlException xmlException1)
				{
					XmlException xmlException = xmlException1;
					exception = xmlException;
				}
				catch (ScheduledJobException scheduledJobException1)
				{
					ScheduledJobException scheduledJobException = scheduledJobException1;
					exception = scheduledJobException;
				}
				if (exception != null)
				{
					PowerShellTraceSource powerShellTraceSource = traceSource;
					object[] objArray = new object[4];
					objArray[0] = this.Name;
					objArray[1] = exception.Message;
					objArray[2] = exception.StackTrace.ToString();
					object[] objArray1 = objArray;
					int num = 3;
					if (exception.InnerException != null)
					{
						message = exception.InnerException.Message;
					}
					else
					{
						message = string.Empty;
					}
					objArray1[num] = message;
					powerShellTraceSource.WriteScheduledJobErrorEvent(objArray);
					throw exception;
				}
			}
			return job;
		}

		public void Save()
		{
			this.IsDisposed();
			this.LoadRepository();
			ScheduledJobDefinition.ValidateName(this.Name);
			Exception exception = null;
			try
			{
				this.UpdateWTSFromDefinition();
			}
			catch (ArgumentException argumentException1)
			{
				ArgumentException argumentException = argumentException1;
				exception = argumentException;
			}
			catch (DirectoryNotFoundException directoryNotFoundException1)
			{
				DirectoryNotFoundException directoryNotFoundException = directoryNotFoundException1;
				exception = directoryNotFoundException;
			}
			catch (FileNotFoundException fileNotFoundException1)
			{
				FileNotFoundException fileNotFoundException = fileNotFoundException1;
				exception = fileNotFoundException;
			}
			catch (UnauthorizedAccessException unauthorizedAccessException1)
			{
				UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
				exception = unauthorizedAccessException;
			}
			catch (IOException oException1)
			{
				IOException oException = oException1;
				exception = oException;
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				exception = cOMException;
			}
			if (exception == null)
			{
				exception = null;
				try
				{
					this.UpdateJobStore();
				}
				catch (ArgumentException argumentException3)
				{
					ArgumentException argumentException2 = argumentException3;
					exception = argumentException2;
				}
				catch (DirectoryNotFoundException directoryNotFoundException3)
				{
					DirectoryNotFoundException directoryNotFoundException2 = directoryNotFoundException3;
					exception = directoryNotFoundException2;
				}
				catch (FileNotFoundException fileNotFoundException3)
				{
					FileNotFoundException fileNotFoundException2 = fileNotFoundException3;
					exception = fileNotFoundException2;
				}
				catch (UnauthorizedAccessException unauthorizedAccessException3)
				{
					UnauthorizedAccessException unauthorizedAccessException2 = unauthorizedAccessException3;
					exception = unauthorizedAccessException2;
				}
				catch (IOException oException3)
				{
					IOException oException2 = oException3;
					exception = oException2;
				}
				if (exception == null)
				{
					ScheduledJobDefinition.RefreshRepositoryFromStore(null);
					ScheduledJobDefinition.Repository.AddOrReplace(this);
					return;
				}
				else
				{
					this.RemoveFromWTS(true);
					string str = StringUtil.Format(ScheduledJobErrorStrings.ErrorUpdatingDefinitionStore, this.Name);
					throw new ScheduledJobException(str, exception);
				}
			}
			else
			{
				this.SyncWithWTS();
				string str1 = StringUtil.Format(ScheduledJobErrorStrings.ErrorUpdatingDefinitionTask, this.Name);
				throw new ScheduledJobException(str1, exception);
			}
		}

		internal void SaveToStore()
		{
			this.IsDisposed();
			this.UpdateJobStore();
		}

		public void SetEnabled(bool enabled, bool save)
		{
			this.IsDisposed();
			this._enabled = enabled;
			if (save)
			{
				this.Save();
			}
		}

		public void SetExecutionHistoryLength(int executionHistoryLength, bool save)
		{
			this.IsDisposed();
			this._executionHistoryLength = executionHistoryLength;
			if (save)
			{
				this.SaveToStore();
			}
		}

		public void SetName(string name, bool save)
		{
			string empty;
			this.IsDisposed();
			ScheduledJobDefinition scheduledJobDefinition = this;
			if (name != null)
			{
				empty = name;
			}
			else
			{
				empty = string.Empty;
			}
			scheduledJobDefinition._name = empty;
			if (save)
			{
				this.Save();
			}
		}

		public void SetTriggers(IEnumerable<ScheduledJobTrigger> newTriggers, bool save)
		{
			this.IsDisposed();
			this.ValidateTriggers(newTriggers);
			foreach (ScheduledJobTrigger value in this._triggers.Values)
			{
				value.JobDefinition = null;
			}
			this._currentTriggerId = 0;
			this._triggers = new Dictionary<int, ScheduledJobTrigger>();
			if (newTriggers != null)
			{
				foreach (ScheduledJobTrigger newTrigger in newTriggers)
				{
					ScheduledJobTrigger scheduledJobTrigger = new ScheduledJobTrigger(newTrigger);
					ScheduledJobDefinition scheduledJobDefinition = this;
					int num = scheduledJobDefinition._currentTriggerId + 1;
					int num1 = num;
					scheduledJobDefinition._currentTriggerId = num;
					scheduledJobTrigger.Id = num1;
					scheduledJobTrigger.JobDefinition = this;
					this._triggers.Add(scheduledJobTrigger.Id, scheduledJobTrigger);
				}
			}
			if (save)
			{
				this.Save();
			}
		}

		public ScheduledJob StartJob()
		{
			this.IsDisposed();
			ScheduledJob scheduledJob = new ScheduledJob(this._invocationInfo.Command, this._invocationInfo.Name, this);
			scheduledJob.StartJob();
			return scheduledJob;
		}

		public static Job2 StartJob(string DefinitionName)
		{
			ScheduledJobDefinition scheduledJobDefinition = ScheduledJobDefinition.LoadFromStore(DefinitionName, null);
			return scheduledJobDefinition.StartJob();
		}

		internal void SyncWithWTS()
		{
			Exception exception = null;
			try
			{
				if (this.UpdateDefintionFromWTS())
				{
					this.SaveToStore();
				}
			}
			catch (DirectoryNotFoundException directoryNotFoundException1)
			{
				DirectoryNotFoundException directoryNotFoundException = directoryNotFoundException1;
				exception = directoryNotFoundException;
			}
			catch (FileNotFoundException fileNotFoundException1)
			{
				FileNotFoundException fileNotFoundException = fileNotFoundException1;
				exception = fileNotFoundException;
			}
			if (exception == null)
			{
				return;
			}
			else
			{
				this.Remove(true);
				throw exception;
			}
		}

		private bool UpdateDefintionFromWTS()
		{
			ScheduledJobTrigger current = null;
			ScheduledJobTrigger item = null;
			bool valueOrDefault = false;
			bool flag = false;
			using (ScheduledJobWTS scheduledJobWT = new ScheduledJobWTS())
			{
				bool taskEnabled = scheduledJobWT.GetTaskEnabled(this._name);
				ScheduledJobOptions jobOptions = scheduledJobWT.GetJobOptions(this._name);
				Collection<ScheduledJobTrigger> jobTriggers = scheduledJobWT.GetJobTriggers(this._name);
				if (taskEnabled != this._enabled)
				{
					this._enabled = taskEnabled;
					flag = true;
				}
				if (jobOptions.DoNotAllowDemandStart != this._options.DoNotAllowDemandStart || jobOptions.IdleDuration != this._options.IdleDuration || jobOptions.IdleTimeout != this._options.IdleTimeout || jobOptions.MultipleInstancePolicy != this._options.MultipleInstancePolicy || jobOptions.RestartOnIdleResume != this._options.RestartOnIdleResume || jobOptions.RunElevated != this._options.RunElevated || jobOptions.RunWithoutNetwork != this._options.RunWithoutNetwork || jobOptions.ShowInTaskScheduler != this._options.ShowInTaskScheduler || jobOptions.StartIfNotIdle != this._options.StartIfNotIdle || jobOptions.StartIfOnBatteries != this._options.StartIfOnBatteries || jobOptions.StopIfGoingOffIdle != this._options.StopIfGoingOffIdle || jobOptions.StopIfGoingOnBatteries != this._options.StopIfGoingOnBatteries || jobOptions.WakeToRun != this._options.WakeToRun)
				{
					jobOptions.JobDefinition = this._options.JobDefinition;
					this._options = jobOptions;
					flag = true;
				}
				if (this._triggers.Count == jobTriggers.Count)
				{
					bool flag1 = false;
					IEnumerator<ScheduledJobTrigger> enumerator = jobTriggers.GetEnumerator();
					using (enumerator)
					{
						do
						{
							if (!enumerator.MoveNext())
							{
								continue;
							}
							current = enumerator.Current;
							if (this._triggers.ContainsKey(current.Id))
							{
								item = this._triggers[current.Id];
								if (item.DaysOfWeek != current.DaysOfWeek || item.Enabled != current.Enabled || item.Frequency != current.Frequency || item.Interval != current.Interval || item.RandomDelay != current.RandomDelay)
								{
									break;
								}
								DateTime? at = item.At;
								DateTime? nullable = current.At;
								if (at.HasValue != nullable.HasValue)
								{
									valueOrDefault = true;
								}
								else
								{
									if (!at.HasValue)
									{
										valueOrDefault = false;
									}
									else
									{
										valueOrDefault = at.GetValueOrDefault() != nullable.GetValueOrDefault();
									}
								}
							}
							else
							{
								flag1 = true;
								break;
							}
						}
						while (!valueOrDefault && !(item.User != current.User));
						flag1 = true;
					}
					if (flag1)
					{
						this.SetTriggers(jobTriggers, false);
						flag = true;
					}
				}
				else
				{
					this.SetTriggers(jobTriggers, false);
					flag = true;
				}
			}
			return flag;
		}

		private void UpdateFilePermissions(string user)
		{
			Exception exception = null;
			try
			{
				ScheduledJobStore.SetReadAccessOnDefinitionFile(this.Name, user);
				ScheduledJobStore.SetWriteAccessOnJobRunOutput(this.Name, user);
			}
			catch (IdentityNotMappedException identityNotMappedException1)
			{
				IdentityNotMappedException identityNotMappedException = identityNotMappedException1;
				exception = identityNotMappedException;
			}
			catch (IOException oException1)
			{
				IOException oException = oException1;
				exception = oException;
			}
			catch (UnauthorizedAccessException unauthorizedAccessException1)
			{
				UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
				exception = unauthorizedAccessException;
			}
			catch (ArgumentNullException argumentNullException1)
			{
				ArgumentNullException argumentNullException = argumentNullException1;
				exception = argumentNullException;
			}
			if (exception == null)
			{
				return;
			}
			else
			{
				object[] name = new object[2];
				name[0] = this.Name;
				name[1] = this.Credential.UserName;
				string str = StringUtil.Format(ScheduledJobErrorStrings.ErrorSettingAccessPermissions, name);
				throw new ScheduledJobException(str, exception);
			}
		}

		public void UpdateJobInvocationInfo(JobInvocationInfo jobInvocationInfo, bool save)
		{
			this.IsDisposed();
			if (jobInvocationInfo != null)
			{
				this._invocationInfo = jobInvocationInfo;
				this._name = jobInvocationInfo.Name;
				if (save)
				{
					this.SaveToStore();
				}
				return;
			}
			else
			{
				throw new PSArgumentNullException("jobInvocationInfo");
			}
		}

		private void UpdateJobRunNames(string newDefName)
		{
			Collection<DateTime> jobRuns = ScheduledJobSourceAdapter.GetJobRuns(newDefName);
			if (jobRuns != null)
			{
				ScheduledJobDefinition scheduledJobDefinition = ScheduledJobDefinition.LoadFromStore(newDefName, null);
				foreach (DateTime jobRun in jobRuns)
				{
					ScheduledJob scheduledJob = null;
					try
					{
						scheduledJob = ScheduledJobSourceAdapter.LoadJobFromStore(scheduledJobDefinition.Name, jobRun) as ScheduledJob;
					}
					catch (ScheduledJobException scheduledJobException)
					{
						continue;
					}
					catch (DirectoryNotFoundException directoryNotFoundException)
					{
						continue;
					}
					catch (FileNotFoundException fileNotFoundException)
					{
						continue;
					}
					catch (UnauthorizedAccessException unauthorizedAccessException)
					{
						continue;
					}
					catch (IOException oException)
					{
						continue;
					}
					if (scheduledJob == null)
					{
						continue;
					}
					scheduledJob.Name = newDefName;
					scheduledJob.Definition = scheduledJobDefinition;
					ScheduledJobSourceAdapter.SaveJobToStore(scheduledJob);
				}
				return;
			}
			else
			{
				return;
			}
		}

		private void UpdateJobStore()
		{
			FileStream fileForJobDefinition = null;
			try
			{
				fileForJobDefinition = ScheduledJobStore.GetFileForJobDefinition(this.Name, FileMode.Create, FileAccess.Write, FileShare.None);
				XmlObjectSerializer netDataContractSerializer = new NetDataContractSerializer();
				netDataContractSerializer.WriteObject(fileForJobDefinition, this);
				fileForJobDefinition.Flush();
			}
			finally
			{
				if (fileForJobDefinition != null)
				{
					fileForJobDefinition.Close();
				}
			}
			if (this.Credential != null)
			{
				this.UpdateFilePermissions(this.Credential.UserName);
			}
		}

		public void UpdateOptions(ScheduledJobOptions options, bool save)
		{
			ScheduledJobOptions scheduledJobOption;
			this.IsDisposed();
			this._options.JobDefinition = null;
			ScheduledJobDefinition scheduledJobDefinition = this;
			if (options != null)
			{
				scheduledJobOption = new ScheduledJobOptions(options);
			}
			else
			{
				scheduledJobOption = new ScheduledJobOptions();
			}
			scheduledJobDefinition._options = scheduledJobOption;
			this._options.JobDefinition = this;
			if (save)
			{
				this.Save();
			}
		}

		public List<int> UpdateTriggers(IEnumerable<ScheduledJobTrigger> triggers, bool save)
		{
			this.IsDisposed();
			if (triggers != null)
			{
				this.ValidateTriggers(triggers);
				List<int> nums = new List<int>();
				bool flag = false;
				foreach (ScheduledJobTrigger trigger in triggers)
				{
					if (!this._triggers.ContainsKey(trigger.Id))
					{
						nums.Add(trigger.Id);
					}
					else
					{
						this._triggers[trigger.Id].JobDefinition = null;
						ScheduledJobTrigger scheduledJobTrigger = new ScheduledJobTrigger(trigger);
						scheduledJobTrigger.Id = trigger.Id;
						scheduledJobTrigger.JobDefinition = this;
						this._triggers[scheduledJobTrigger.Id] = scheduledJobTrigger;
						flag = true;
					}
				}
				if (save && flag)
				{
					this.Save();
				}
				return nums;
			}
			else
			{
				throw new PSArgumentNullException("triggers");
			}
		}

		private void UpdateWTSFromDefinition()
		{
			using (ScheduledJobWTS scheduledJobWT = new ScheduledJobWTS())
			{
				scheduledJobWT.UpdateTask(this);
			}
		}

		private static void ValidateName(string name)
		{
			if (name.IndexOfAny(Path.GetInvalidFileNameChars()) == -1)
			{
				return;
			}
			else
			{
				string str = StringUtil.Format(ScheduledJobErrorStrings.InvalidJobDefName, name);
				throw new ScheduledJobException(str);
			}
		}

		private void ValidateTriggers(IEnumerable<ScheduledJobTrigger> triggers)
		{
			if (triggers != null)
			{
				foreach (ScheduledJobTrigger trigger in triggers)
				{
					trigger.Validate();
				}
			}
		}
	}
}