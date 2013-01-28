using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Runtime.Serialization;
using System.Xml;

namespace Microsoft.PowerShell.ScheduledJob
{
	public abstract class ScheduleJobCmdletBase : PSCmdlet
	{
		protected const string ModuleName = "PSScheduledJob";

		protected ScheduleJobCmdletBase()
		{
		}

		internal void FindAllJobDefinitions(Action<ScheduledJobDefinition> itemFound)
		{
			Dictionary<string, Exception> strs = ScheduledJobDefinition.RefreshRepositoryFromStore((ScheduledJobDefinition definition) => {
				if (this.ValidateJobDefinition(definition))
				{
					itemFound(definition);
				}
			}
			);
			this.HandleAllLoadErrors(strs);
		}

		internal void FindJobDefinitionsById(int[] ids, Action<ScheduledJobDefinition> itemFound, bool writeErrorsAndWarnings = true)
		{
			HashSet<int> nums = new HashSet<int>(ids);
			Dictionary<string, Exception> strs = ScheduledJobDefinition.RefreshRepositoryFromStore((ScheduledJobDefinition definition) => {
				if (nums.Contains(definition.Id) && this.ValidateJobDefinition(definition))
				{
					itemFound(definition);
					nums.Remove(definition.Id);
				}
			}
			);
			this.HandleAllLoadErrors(strs);
			if (writeErrorsAndWarnings)
			{
				foreach (int num in nums)
				{
					this.WriteDefinitionNotFoundByIdError(num);
				}
			}
		}

		internal void FindJobDefinitionsByName(string[] names, Action<ScheduledJobDefinition> itemFound, bool writeErrorsAndWarnings = true)
		{
			HashSet<string> strs = new HashSet<string>(names);
			Dictionary<string, WildcardPattern> strs1 = new Dictionary<string, WildcardPattern>();
			string[] strArrays = names;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				if (!strs1.ContainsKey(str))
				{
					strs1.Add(str, new WildcardPattern(str, WildcardOptions.IgnoreCase));
				}
			}
			Dictionary<string, Exception> strs2 = ScheduledJobDefinition.RefreshRepositoryFromStore((ScheduledJobDefinition definition) => {
				foreach (KeyValuePair<string, WildcardPattern> pattern in strs1)
				{
					if (!pattern.Value.IsMatch(definition.Name) || !this.ValidateJobDefinition(definition))
					{
						continue;
					}
					itemFound(definition);
					if (!strs.Contains(pattern.Key))
					{
						continue;
					}
					strs.Remove(pattern.Key);
				}
			}
			);
			foreach (KeyValuePair<string, Exception> keyValuePair in strs2)
		{
				Dictionary<string, WildcardPattern>.Enumerator enumerator = strs1.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						var keyValuePair1 = enumerator.Current;
						if (!keyValuePair1.Value.IsMatch(keyValuePair.Key))
						{
							continue;
						}
						this.HandleLoadError(keyValuePair.Key, keyValuePair.Value);
					}
				}
				finally
				{
					enumerator.Dispose();
				}
			}
			if (writeErrorsAndWarnings)
			{
				foreach (string str1 in strs)
				{
					this.WriteDefinitionNotFoundByNameError(str1);
				}
			}
		}

		internal List<ScheduledJobDefinition> GetAllJobDefinitions()
		{
			Dictionary<string, Exception> strs = ScheduledJobDefinition.RefreshRepositoryFromStore(null);
			this.HandleAllLoadErrors(strs);
			this.ValidateJobDefinitions();
			return ScheduledJobDefinition.Repository.Definitions;
		}

		internal ScheduledJobDefinition GetJobDefinitionById(int id, bool writeErrorsAndWarnings = true)
		{
			ScheduledJobDefinition scheduledJobDefinition;
			Dictionary<string, Exception> strs = ScheduledJobDefinition.RefreshRepositoryFromStore(null);
			this.HandleAllLoadErrors(strs);
			List<ScheduledJobDefinition>.Enumerator enumerator = ScheduledJobDefinition.Repository.Definitions.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					ScheduledJobDefinition current = enumerator.Current;
					if (current.Id != id || !this.ValidateJobDefinition(current))
					{
						continue;
					}
					scheduledJobDefinition = current;
					return scheduledJobDefinition;
				}
				if (writeErrorsAndWarnings)
				{
					this.WriteDefinitionNotFoundByIdError(id);
				}
				return null;
			}
			finally
			{
				enumerator.Dispose();
			}
			return scheduledJobDefinition;
		}

		internal ScheduledJobDefinition GetJobDefinitionByName(string name, bool writeErrorsAndWarnings = true)
		{
			ScheduledJobDefinition scheduledJobDefinition;
			Dictionary<string, Exception> strs = ScheduledJobDefinition.RefreshRepositoryFromStore(null);
			WildcardPattern wildcardPattern = new WildcardPattern(name, WildcardOptions.IgnoreCase);
			List<ScheduledJobDefinition>.Enumerator enumerator = ScheduledJobDefinition.Repository.Definitions.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					ScheduledJobDefinition current = enumerator.Current;
					if (!wildcardPattern.IsMatch(current.Name) || !this.ValidateJobDefinition(current))
					{
						continue;
					}
					scheduledJobDefinition = current;
					return scheduledJobDefinition;
				}
				Dictionary<string, Exception>.Enumerator enumerator1 = strs.GetEnumerator();
				try
				{
					while (enumerator1.MoveNext())
					{
						KeyValuePair<string, Exception> keyValuePair = enumerator1.Current;
						if (!wildcardPattern.IsMatch(keyValuePair.Key))
						{
							continue;
						}
						this.HandleLoadError(keyValuePair.Key, keyValuePair.Value);
					}
				}
				finally
				{
					enumerator1.Dispose();
				}
				if (writeErrorsAndWarnings)
				{
					this.WriteDefinitionNotFoundByNameError(name);
				}
				return null;
			}
			finally
			{
				enumerator.Dispose();
			}
			return scheduledJobDefinition;
		}

		internal List<ScheduledJobDefinition> GetJobDefinitionsById(int[] ids, bool writeErrorsAndWarnings = true)
		{
			Dictionary<string, Exception> strs = ScheduledJobDefinition.RefreshRepositoryFromStore(null);
			this.HandleAllLoadErrors(strs);
			List<ScheduledJobDefinition> scheduledJobDefinitions = new List<ScheduledJobDefinition>();
			HashSet<int> nums = new HashSet<int>(ids);
			foreach (ScheduledJobDefinition definition in ScheduledJobDefinition.Repository.Definitions)
			{
				if (!nums.Contains(definition.Id) || !this.ValidateJobDefinition(definition))
				{
					continue;
				}
				scheduledJobDefinitions.Add(definition);
				nums.Remove(definition.Id);
			}
			if (writeErrorsAndWarnings)
			{
				foreach (int num in nums)
				{
					this.WriteDefinitionNotFoundByIdError(num);
				}
			}
			return scheduledJobDefinitions;
		}

		internal List<ScheduledJobDefinition> GetJobDefinitionsByName(string[] names, bool writeErrorsAndWarnings = true)
		{
			bool flag = false;
			Dictionary<string, Exception> strs = ScheduledJobDefinition.RefreshRepositoryFromStore(null);
			List<ScheduledJobDefinition> scheduledJobDefinitions = new List<ScheduledJobDefinition>();
			string[] strArrays = names;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				WildcardPattern wildcardPattern = new WildcardPattern(str, WildcardOptions.IgnoreCase);
				foreach (ScheduledJobDefinition definition in ScheduledJobDefinition.Repository.Definitions)
				{
					if (!wildcardPattern.IsMatch(definition.Name) || !this.ValidateJobDefinition(definition))
					{
						continue;
					}
					flag = true;
					scheduledJobDefinitions.Add(definition);
				}
				foreach (KeyValuePair<string, Exception> keyValuePair in strs)
				{
					if (!wildcardPattern.IsMatch(keyValuePair.Key))
					{
						continue;
					}
					this.HandleLoadError(keyValuePair.Key, keyValuePair.Value);
				}
				if (!flag && writeErrorsAndWarnings)
				{
					this.WriteDefinitionNotFoundByNameError(str);
				}
			}
			return scheduledJobDefinitions;
		}

		private void HandleAllLoadErrors(Dictionary<string, Exception> errors)
		{
			foreach (KeyValuePair<string, Exception> error in errors)
			{
				this.HandleLoadError(error.Key, error.Value);
			}
		}

		private void HandleLoadError(string name, Exception e)
		{
			if (e as IOException != null || e as XmlException != null || e as TypeInitializationException != null || e as SerializationException != null || e as ArgumentNullException != null)
			{
				ScheduledJobDefinition.RemoveDefinition(name);
				this.WriteErrorLoadingDefinition(name, e);
			}
		}

		private bool ValidateJobDefinition(ScheduledJobDefinition definition)
		{
			Exception exception = null;
			try
			{
				definition.SyncWithWTS();
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
			catch (ArgumentNullException argumentNullException1)
			{
				ArgumentNullException argumentNullException = argumentNullException1;
				exception = argumentNullException;
			}
			if (exception != null)
			{
				this.WriteErrorLoadingDefinition(definition.Name, exception);
			}
			return exception == null;
		}

		private void ValidateJobDefinitions()
		{
			foreach (ScheduledJobDefinition definition in ScheduledJobDefinition.Repository.Definitions)
			{
				this.ValidateJobDefinition(definition);
			}
		}

		internal void WriteDefinitionNotFoundByIdError(int defId)
		{
			string str = StringUtil.Format(ScheduledJobErrorStrings.DefinitionNotFoundById, defId);
			Exception runtimeException = new RuntimeException(str);
			ErrorRecord errorRecord = new ErrorRecord(runtimeException, "ScheduledJobDefinitionNotFoundById", ErrorCategory.ObjectNotFound, null);
			base.WriteError(errorRecord);
		}

		internal void WriteDefinitionNotFoundByNameError(string name)
		{
			string str = StringUtil.Format(ScheduledJobErrorStrings.DefinitionNotFoundByName, name);
			Exception runtimeException = new RuntimeException(str);
			ErrorRecord errorRecord = new ErrorRecord(runtimeException, "ScheduledJobDefinitionNotFoundByName", ErrorCategory.ObjectNotFound, null);
			base.WriteError(errorRecord);
		}

		internal void WriteErrorLoadingDefinition(string name, Exception error)
		{
			string str = StringUtil.Format(ScheduledJobErrorStrings.CantLoadDefinitionFromStore, name);
			Exception runtimeException = new RuntimeException(str, error);
			ErrorRecord errorRecord = new ErrorRecord(runtimeException, "CantLoadScheduledJobDefinitionFromStore", ErrorCategory.InvalidOperation, null);
			base.WriteError(errorRecord);
		}

		internal void WriteTriggerNotFoundError(int notFoundId, string definitionName, object errorObject)
		{
			object[] objArray = new object[2];
			objArray[0] = notFoundId;
			objArray[1] = definitionName;
			string str = StringUtil.Format(ScheduledJobErrorStrings.TriggerNotFound, objArray);
			Exception runtimeException = new RuntimeException(str);
			ErrorRecord errorRecord = new ErrorRecord(runtimeException, "ScheduledJobTriggerNotFound", ErrorCategory.ObjectNotFound, errorObject);
			base.WriteError(errorRecord);
		}
	}
}