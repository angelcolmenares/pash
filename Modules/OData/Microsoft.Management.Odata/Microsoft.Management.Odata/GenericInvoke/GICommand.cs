using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Core;
using Microsoft.Management.Odata.PS;
using System;
using System.Collections.Generic;
using System.Data.Services.Providers;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.Management.Odata.GenericInvoke
{
	internal class GICommand : ICommand, IDisposable
	{
		private bool disposed;

		private CommandType commandType;

		private Dictionary<string, object> parameters;

		private ResourceType entityType;

		private ExclusiveItemStore<PSRunspace, UserContext> runspaceStore;

		private UserContext userContext;

		private string membershipId;

		public GICommand(CommandType commandType, ExclusiveItemStore<PSRunspace, UserContext> runspaceStore, ResourceType entityType, UserContext userContext, string membershipId)
		{
			this.commandType = commandType;
			this.runspaceStore = runspaceStore;
			this.entityType = entityType;
			this.userContext = userContext;
			this.membershipId = membershipId;
			this.parameters = new Dictionary<string, object>();
		}

		public bool AddArrayFieldParameter(string parameter, IEnumerable<object> values)
		{
			object[] objArray = new object[2];
			objArray[0] = "AddArrayFieldParameter";
			objArray[1] = "GICommand";
			throw new NotImplementedException(ExceptionHelpers.GetExceptionMessage(Resources.NotImplementedExceptionMessage, objArray));
		}

		public bool AddFieldParameter(string parameter, object value)
		{
			if (this.commandType == CommandType.Create || string.Equals(parameter, "ID", StringComparison.Ordinal))
			{
				this.parameters[parameter] = value;
				return true;
			}
			else
			{
				return false;
			}
		}

		public void AddParameter(string parameter, object value, bool isOption = true)
		{
			throw new NotImplementedException();
		}

		public bool CanFieldBeAdded(string fieldName)
		{
			if (this.commandType == CommandType.Create || string.Equals(fieldName, "ID", StringComparison.Ordinal))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposeManagedResources)
		{
			if (!this.disposed && disposeManagedResources)
			{
				this.runspaceStore.Dispose();
			}
			this.disposed = true;
		}

		public IEnumerator<DSResource> InvokeAsync(Expression expression, bool noStreamingResponse)
		{
			object obj = null;
			PipelineInvocation pipelineInvocation = null;
			object obj1 = null;
			PipelineInvocation pipelineInvocation1 = null;
			IEnumerator<DSResource> enumerator;
			object[] name;
			CommandType commandType = this.commandType;
			if (commandType == CommandType.Create)
			{
				UserDataCache.UserDataEnvelope userDataEnvelope = DataServiceController.Current.UserDataCache.Get(this.userContext);
				using (userDataEnvelope)
				{
					Envelope<PSRunspace, UserContext> envelope = this.runspaceStore.Borrow(this.userContext, this.membershipId);
					PipelineInvocation pipelineInvocation2 = new PipelineInvocation(envelope, this.entityType);
					foreach (KeyValuePair<string, object> parameter in this.parameters)
					{
						pipelineInvocation2.AddFieldParameter(parameter.Key, parameter.Value);
					}
					PipelineInvocation pipelineInvocation3 = null;
					userDataEnvelope.Data.CommandInvocations.AddOrLockKey(pipelineInvocation2.ID, pipelineInvocation2, out pipelineInvocation3);
					try
					{
						List<DSResource> dSResources = new List<DSResource>();
						pipelineInvocation3.InvokeAsync(dSResources.AsQueryable<DSResource>().Expression, true);
						dSResources.Add(pipelineInvocation3.MakeDsResource());
						enumerator = dSResources.GetEnumerator();
					}
					finally
					{
						userDataEnvelope.Data.CommandInvocations.TryUnlockKey(pipelineInvocation3.ID);
					}
				}
			}
			else if (commandType == CommandType.Read)
			{
				UserDataCache.UserDataEnvelope userDataEnvelope1 = DataServiceController.Current.UserDataCache.Get(this.userContext);
				using (userDataEnvelope1)
				{
					List<DSResource> dSResources1 = new List<DSResource>();
					if (!this.parameters.TryGetValue("ID", out obj))
					{
						foreach (KeyValuePair<Guid, PipelineInvocation> list in userDataEnvelope1.Data.CommandInvocations.ToList())
						{
							dSResources1.Add(list.Value.MakeDsResource());
						}
					}
					else
					{
						Guid guid = (Guid)obj;
						if (userDataEnvelope1.Data.CommandInvocations.TryGetValue(guid, out pipelineInvocation))
						{
							dSResources1.Add(pipelineInvocation.MakeDsResource());
						}
					}
					enumerator = dSResources1.GetEnumerator();
				}
			}
			else if (commandType == CommandType.Update)
			{
				name = new object[2];
				name[0] = this.entityType.Name;
				name[1] = (object)this.commandType.ToString();
				throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.EntityDoesNotHaveCommand, name));
			}
			else if (commandType == CommandType.Delete)
			{
				UserDataCache.UserDataEnvelope userDataEnvelope2 = DataServiceController.Current.UserDataCache.Get(this.userContext);
				using (userDataEnvelope2)
				{
					if (!this.parameters.TryGetValue("ID", out obj1))
					{
						throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.KeysMissingInQuery, new object[0]));
					}
					else
					{
						if (userDataEnvelope2.Data.CommandInvocations.TryGetValue((Guid)obj1, out pipelineInvocation1))
						{
							bool flag = pipelineInvocation1.Interrupt();
							userDataEnvelope2.Data.CommandInvocations.TryRemove((Guid)obj1);
							if (flag)
							{
								pipelineInvocation1.Dispose();
							}
						}
						enumerator = (new List<DSResource>()).GetEnumerator();
					}
				}
			}
			else
			{
				name = new object[2];
				name[0] = this.entityType.Name;
				name[1] = (object)this.commandType.ToString();
				throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.EntityDoesNotHaveCommand, name));
			}
			return enumerator;
			name = new object[2];
			name[0] = this.entityType.Name;
			name[1] = this.commandType.ToString();
			throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.EntityDoesNotHaveCommand, name));
		}

		public static class Fields
		{
			public const string Id = "ID";

			public const string Command = "Command";

			public const string WaitMsec = "WaitMsec";

			public const string OutputFormat = "OutputFormat";

			public const string Status = "Status";

			public const string Output = "Output";

			public const string Errors = "Errors";

			public const string ExpirationTime = "ExpirationTime";

		}
	}
}