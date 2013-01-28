using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.PS;
using Microsoft.Management.Odata.Schema;
using Microsoft.Management.Odata.Tracing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Services;
using System.Data.Services.Providers;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Data.Entity.Core;

namespace Microsoft.Management.Odata.Core
{
	internal class EntityUpdate : IUpdateInstance, ICommand, IDisposable
	{
		private IQueryable query;

		private UserContext userContext;

		private string membershipId;

		private ResourceType resourceType;

		private EntityMetadata metadata;

		private CommandType commandType;

		private SortedDictionary<string, object> propertyUpdates;

		private DSResource updatedResource;

		private DSResource resolveResource;

		private ResourceProperty referringProperty;

		private IUpdateInstance referredInstance;

		public EntityUpdate(CommandType commandType, UserContext userContext, ResourceType type, EntityMetadata metadata, IQueryable query, string membershipId)
		{
			ExceptionHelpers.ThrowArgumentExceptionIf("commandType", commandType != CommandType.Update, Resources.InternalErrorOccurred, new object[0]);
			this.query = query;
			this.userContext = userContext;
			this.membershipId = membershipId;
			this.resourceType = type;
			this.metadata = metadata;
			this.commandType = commandType;
			this.propertyUpdates = new SortedDictionary<string, object>();
			this.updatedResource = null;
			this.resolveResource = null;
			CommandArgumentVisitor commandArgumentVisitor = new CommandArgumentVisitor(this);
			commandArgumentVisitor.Visit(query.Expression);
			if (this.AreAllKeyFieldsSpecified())
			{
				return;
			}
			else
			{
				throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.KeysMissingInQuery, new object[0]));
			}
		}

		public EntityUpdate(UserContext userContext, ResourceType type, EntityMetadata metadata, string membershipId)
		{
			this.userContext = userContext;
			this.resourceType = type;
			this.metadata = metadata;
			this.commandType = CommandType.Create;
			this.query = null;
			this.membershipId = membershipId;
			this.propertyUpdates = new SortedDictionary<string, object>();
			this.updatedResource = null;
			this.resolveResource = null;
		}

		public bool AddArrayFieldParameter(string parameter, IEnumerable<object> values)
		{
			object[] objArray = new object[2];
			objArray[0] = "AddArrayFieldParameter";
			objArray[1] = "EntityUpdate";
			throw new NotImplementedException(ExceptionHelpers.GetExceptionMessage(Resources.NotImplementedExceptionMessage, objArray));
		}

		public bool AddFieldParameter(string parameter, object value)
		{
			ResourceProperty resourceProperty = this.resourceType.KeyProperties.SingleOrDefault<ResourceProperty>((ResourceProperty p) => p.Name == parameter);
			object[] objArray = new object[1];
			objArray[0] = parameter;
			ExceptionHelpers.ThrowArgumentExceptionIf("parameter", resourceProperty == null, Resources.FieldIsNotAKey, objArray);
			this.propertyUpdates.Add(parameter, value);
			return true;
		}

		public void AddParameter(string parameter, object value, bool isOption)
		{
			object[] objArray = new object[2];
			objArray[0] = "AddParameter";
			objArray[1] = "EntityUpdate";
			throw new NotImplementedException(ExceptionHelpers.GetExceptionMessage(Resources.NotImplementedExceptionMessage, objArray));
		}

		private void AddPropertyUpdates(ICommand command)
		{
			foreach (KeyValuePair<string, object> propertyUpdate in this.propertyUpdates)
			{
				Func<ResourceProperty, bool> func = null;
				KeyValuePair<string, object> keyValuePair1 = propertyUpdate;
				if (!command.CanFieldBeAdded(keyValuePair1.Key))
				{
					continue;
				}
				KeyValuePair<string, object> keyValuePair2 = propertyUpdate;
				object value = keyValuePair2.Value;
				value = EntityUpdate.ResolveUpdatableObject(value);
				value = EntityUpdate.ResolveUpdatableObjectList(value);
				if (value == null && this.commandType == CommandType.Create)
				{
					ReadOnlyCollection<ResourceProperty> properties = this.resourceType.Properties;
					if (func == null)
					{
						func = (ResourceProperty item) => {
							KeyValuePair<string, object> keyValuePair = propertyUpdate;
							return item.Name == keyValuePair.Key;
						}
						;
					}
					ResourceProperty resourceProperty = properties.First<ResourceProperty>(func);
					if (resourceProperty.ResourceType.IsNullable())
					{
						continue;
					}
				}
				KeyValuePair<string, object> keyValuePair3 = propertyUpdate;
				command.AddFieldParameter(keyValuePair3.Key, value);
			}
		}

		public void AddReference(string propertyName, IUpdateInstance instance)
		{
			this.referringProperty = this.resourceType.Properties.FirstOrDefault<ResourceProperty>((ResourceProperty it) => string.Equals(propertyName, it.Name, StringComparison.Ordinal));
			if (this.referringProperty != null)
			{
				this.commandType = CommandType.AddReference;
				this.referredInstance = instance;
				return;
			}
			else
			{
				throw new ArgumentException("not a reference property");
			}
		}

		private bool AreAllKeyFieldsSpecified()
		{
			bool flag;
			IEnumerator<ResourceProperty> enumerator = this.resourceType.KeyProperties.GetEnumerator();
			using (enumerator)
			{
				while (enumerator.MoveNext())
				{
					ResourceProperty current = enumerator.Current;
					if (this.propertyUpdates.ContainsKey(current.Name))
					{
						continue;
					}
					flag = false;
					return flag;
				}
				return true;
			}
			return flag;
		}

		public bool CanFieldBeAdded(string fieldName)
		{
			throw new NotImplementedException();
		}

		public void Delete()
		{
			this.commandType = CommandType.Delete;
			foreach (ResourceProperty property in this.resourceType.Properties)
			{
				if ((property.Kind & ResourcePropertyKind.Key) == ResourcePropertyKind.Key)
				{
					continue;
				}
				this.propertyUpdates.Remove(property.Name);
			}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		public Dictionary<string, object> GetKeyValues()
		{
			Dictionary<string, object> strs = new Dictionary<string, object>();
			foreach (ResourceProperty keyProperty in this.resourceType.KeyProperties)
			{
				strs.Add(keyProperty.Name, this.propertyUpdates[keyProperty.Name]);
			}
			return strs;
		}

		private DSResource GetOriginalResource()
		{
			DSResource dSResource;
			ICommand command = DataServiceController.Current.GetCommand(CommandType.Read, this.userContext, this.resourceType, this.metadata, this.membershipId);
			using (command)
			{
				UriParametersHelper.AddParametersToCommand(command, DataServiceController.Current.GetCurrentResourceUri());
				CommandArgumentVisitor commandArgumentVisitor = new CommandArgumentVisitor(command);
				commandArgumentVisitor.Visit(this.query.Expression);
				List<DSResource> dSResources = new List<DSResource>();
				DataServiceController.Current.QuotaSystem.CheckCmdletExecutionQuota(this.userContext);
				IEnumerator<DSResource> enumerator = command.InvokeAsync(dSResources.AsQueryable<DSResource>().Expression, true);
				while (enumerator.MoveNext())
				{
					dSResources.Add(enumerator.Current);
				}
				if (dSResources.Count != 0)
				{
					dSResource = dSResources.First<DSResource>();
				}
				else
				{
					throw new DataServiceException(0x194, ExceptionHelpers.GetDataServiceExceptionMessage(HttpStatusCode.NotFound, Resources.ResourceInstanceNotFound, new object[0]));
				}
			}
			return dSResource;
		}

		public IEnumerator<DSResource> InvokeAsync(Expression expression, bool noStreamingResponse)
		{
			object[] objArray = new object[2];
			objArray[0] = "Invoke";
			objArray[1] = "EntityUpdate";
			throw new NotImplementedException(ExceptionHelpers.GetExceptionMessage(Resources.NotImplementedExceptionMessage, objArray));
		}

		public void InvokeCommand()
		{
			if (PSCommandManager.IsReferenceCmdlet(this.commandType))
			{
				IReferenceSetCommand referenceSetCommand = DataServiceController.Current.GetReferenceSetCommand(this.commandType, this.userContext, this.referringProperty, this.metadata, this.membershipId, null);
				using (referenceSetCommand)
				{
					UriParametersHelper.AddParametersToCommand(referenceSetCommand, DataServiceController.Current.GetCurrentResourceUri());
					this.AddPropertyUpdates(referenceSetCommand);
					referenceSetCommand.AddReferredObject(this.referredInstance.GetKeyValues());
					referenceSetCommand.AddReferringObject(this.GetKeyValues());
					List<DSResource> dSResources = new List<DSResource>();
					DataServiceController.Current.QuotaSystem.CheckCmdletExecutionQuota(this.userContext);
					IEnumerator<DSResource> enumerator = referenceSetCommand.InvokeAsync(dSResources.AsQueryable<DSResource>().Expression, true);
					while (enumerator.MoveNext())
					{
					}
				}
			}
			else
			{
				ICommand command = DataServiceController.Current.GetCommand(this.commandType, this.userContext, this.resourceType, this.metadata, this.membershipId);
				using (command)
				{
					UriParametersHelper.AddParametersToCommand(command, DataServiceController.Current.GetCurrentResourceUri());
					this.AddPropertyUpdates(command);
					List<DSResource> dSResources1 = new List<DSResource>();
					IEnumerator<DSResource> enumerator1 = command.InvokeAsync(dSResources1.AsQueryable<DSResource>().Expression, true);
					while (enumerator1.MoveNext())
					{
						dSResources1.Add(enumerator1.Current);
					}
					if (this.commandType == CommandType.Delete || dSResources1.Count < 1)
					{
						if (this.commandType != CommandType.Create || dSResources1.Count > 0)
						{
							this.updatedResource = null;
						}
						else
						{
							throw new DataServiceException(string.Format(Resources.CreateCommandNotReturnedInstance, this.resourceType.Name));
						}
					}
					else
					{
						this.updatedResource = dSResources1.First<DSResource>();
					}
				}
			}
		}

		public void RemoveReference(string propertyName, IUpdateInstance instance)
		{
			this.referringProperty = this.resourceType.Properties.FirstOrDefault<ResourceProperty>((ResourceProperty it) => string.Equals(propertyName, it.Name, StringComparison.Ordinal));
			if (this.referringProperty != null)
			{
				this.commandType = CommandType.RemoveReference;
				this.referredInstance = instance;
				return;
			}
			else
			{
				throw new ArgumentException("not a reference property");
			}
		}

		public void Reset()
		{
			foreach (ResourceProperty property in this.resourceType.Properties)
			{
				if ((property.Kind & ResourcePropertyKind.Key) == ResourcePropertyKind.Key)
				{
					continue;
				}
				PropertyCustomState customState = property.GetCustomState();
				if (!customState.IsUpdatable)
				{
					continue;
				}
				this.propertyUpdates.Remove(property.Name);
			}
		}

		public object Resolve()
		{
			if (this.commandType != CommandType.Delete)
			{
				if (this.updatedResource == null)
				{
					if (this.resolveResource == null)
					{
						this.resolveResource = new DSResource(this.resourceType, true);
					}
					return this.resolveResource;
				}
				else
				{
					return this.updatedResource;
				}
			}
			else
			{
				throw new NotImplementedException(ExceptionHelpers.GetExceptionMessage(Resources.ResolveResourceAfterDelete, new object[0]));
			}
		}

		internal static object ResolveUpdatableObject(object value)
		{
			IUpdateInstance updateInstance = value as IUpdateInstance;
			if (updateInstance == null)
			{
				return value;
			}
			else
			{
				return updateInstance.Resolve();
			}
		}

		internal static object ResolveUpdatableObjectList(object list)
		{
			if (list == null || !TypeSystem.ContainsEnumerableInterface(list.GetType()))
			{
				return list;
			}
			else
			{
				IEnumerable enumerable = list as IEnumerable;
				ArrayList arrayLists = new ArrayList();
				foreach (object obj in enumerable)
				{
					arrayLists.Add(EntityUpdate.ResolveUpdatableObject(obj));
				}
				return arrayLists;
			}
		}

		public void SetReference(string propertyName, IUpdateInstance instance)
		{
			Func<ResourceProperty, bool> func = null;
			if (instance != null)
			{
				Dictionary<string, object> keyValues = instance.GetKeyValues();
				if (keyValues.Count <= 1)
				{
					foreach (KeyValuePair<string, object> keyValue in keyValues)
					{
						this.propertyUpdates[propertyName] = keyValue.Value;
					}
					return;
				}
				else
				{
					throw new NotImplementedException();
				}
			}
			else
			{
				SortedDictionary<string, object> defaultValue = this.propertyUpdates;
				string str = propertyName;
				ReadOnlyCollection<ResourceProperty> properties = this.resourceType.Properties;
				if (func == null)
				{
					func = (ResourceProperty p) => p.Name == propertyName;
				}
				defaultValue[str] = properties.Single<ResourceProperty>(func).GetCustomState().DefaultValue;
				return;
			}
		}

		public void SetValue(string propertyName, object value)
		{
			string str;
			Tracer current = TraceHelper.Current;
			string[] fullName = new string[6];
			fullName[0] = "EntityUpdate SetValue Entity name = ";
			fullName[1] = this.resourceType.FullName;
			fullName[2] = " property name = ";
			fullName[3] = propertyName;
			fullName[4] = " value = ";
			string[] strArrays = fullName;
			int num = 5;
			if (value != null)
			{
				str = value.ToString();
			}
			else
			{
				str = "<null value>";
			}
			strArrays[num] = str;
			current.DebugMessage(string.Concat(fullName));
			if (value != null || this.commandType != CommandType.Update)
			{
				this.propertyUpdates[propertyName] = value;
				return;
			}
			else
			{
				return;
			}
		}

		public TestHookCommandInvocationData TestHookGetInvocationData()
		{
			TestHookCommandInvocationData testHookCommandInvocationDatum = new TestHookCommandInvocationData();
			testHookCommandInvocationDatum.CommandType = this.commandType;
			testHookCommandInvocationDatum.Parameters = new Dictionary<string, object>(this.propertyUpdates, StringComparer.OrdinalIgnoreCase);
			return testHookCommandInvocationDatum;
		}

		public void VerifyConcurrencyValues(IEnumerable<KeyValuePair<string, object>> values)
		{
			DSResource originalResource = this.GetOriginalResource();
			if (originalResource != null)
			{
				foreach (KeyValuePair<string, object> value in values)
				{
					object obj = originalResource.GetValue(value.Key, null);
					if (obj.Equals(value.Value))
					{
						continue;
					}
					object[] key = new object[2];
					key[0] = value.Key;
					key[1] = value.Value;
					throw new DataServiceException(0x19c, ExceptionHelpers.GetDataServiceExceptionMessage(HttpStatusCode.PreconditionFailed, Resources.PropertyKey, key));
				}
				return;
			}
			else
			{
				throw new OptimisticConcurrencyException(ExceptionHelpers.GetExceptionMessage(Resources.GetCmdletNotReturningAnObject, new object[0]));
			}
		}
	}
}