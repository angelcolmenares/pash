using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Tracing;
using System;
using System.Collections.Generic;
using System.Data.Services.Providers;
using System.Linq;

namespace Microsoft.Management.Odata.Core
{
	internal class DataServiceQueryProvider : IDataServiceQueryProvider
	{
		private DataContext dataContext;

		private DataServiceQueryProvider.ResultSetCollection resultSets;

		public object CurrentDataSource
		{
			get
			{
				return this.dataContext;
			}
			set
			{
				this.dataContext = (DataContext)value;
			}
		}


		public bool IsNullPropagationRequired
		{
			get;set;
		}

		public DataServiceQueryProvider()
		{
			this.IsNullPropagationRequired = false;
			this.resultSets = new DataServiceQueryProvider.ResultSetCollection();
		}

		internal static string GetInputVariableToODataMethodNullMessage()
		{
			return Resources.InputVariableToODataMethodNull;
		}

		public object GetOpenPropertyValue(object target, string propertyName)
		{
			return null;
		}

		public IEnumerable<KeyValuePair<string, object>> GetOpenPropertyValues(object target)
		{
			return null;
		}

		public object GetPropertyValue(object resourceObject, ResourceProperty resourceProperty)
		{
			object value;
			object[] objArray = new object[2];
			objArray[0] = "resourceObject";
			objArray[1] = "DataServiceQueryProvider.GetPropertyValue";
			resourceObject.ThrowIfNull("resourceObject", new ParameterExtensions.MessageLoader(DataServiceQueryProvider.GetInputVariableToODataMethodNullMessage), objArray);
			object[] objArray1 = new object[2];
			objArray1[0] = "resourceProperty";
			objArray1[1] = "DataServiceQueryProvider.GetPropertyValue";
			resourceProperty.ThrowIfNull("resourceProperty", new ParameterExtensions.MessageLoader(DataServiceQueryProvider.GetInputVariableToODataMethodNullMessage), objArray1);
			DSResource dSResource = resourceObject as DSResource;
			if (dSResource != null)
			{
				try
				{
					value = dSResource.GetValue(resourceProperty.Name, this.resultSets);
				}
				catch (PowerShellWebServiceException powerShellWebServiceException1)
				{
					PowerShellWebServiceException powerShellWebServiceException = powerShellWebServiceException1;
					powerShellWebServiceException.Trace(null);
					value = null;
				}
				return value;
			}
			else
			{
				object[] assemblyQualifiedName = new object[3];
				assemblyQualifiedName[0] = "resourceObject";
				assemblyQualifiedName[1] = resourceObject.GetType().AssemblyQualifiedName;
				assemblyQualifiedName[2] = typeof(DSResource).AssemblyQualifiedName;
				string exceptionMessage = ExceptionHelpers.GetExceptionMessage(Resources.InvalidArgClrType, assemblyQualifiedName);
				throw new ArgumentException(exceptionMessage, "resourceObject");
			}
		}

		public IQueryable GetQueryRootForResourceSet(ResourceSet resourceSet)
		{
			IQueryable queryables;
			string name;
			Tracer current = TraceHelper.Current;
			string str = "DataServiceQueryProvider";
			string str1 = "GetQueryRootForResourceSet";
			if (resourceSet == null)
			{
				name = "<null>";
			}
			else
			{
				name = resourceSet.Name;
			}
			current.MethodCall1(str, str1, name);
			object[] objArray = new object[2];
			objArray[0] = "resourceProperty";
			objArray[1] = "DataServiceQueryProvider.GetQueryRootForResourceSet";
			resourceSet.ThrowIfNull("resourceSet", new ParameterExtensions.MessageLoader(DataServiceQueryProvider.GetInputVariableToODataMethodNullMessage), objArray);
			try
			{
				using (OperationTracer operationTracer = new OperationTracer("GetQueryRoot"))
				{
					queryables = DSLinqQueryProvider.CreateQuery(this.dataContext.UserSchema, resourceSet.ResourceType, this.dataContext.UserContext, this.dataContext.MembershipId, this.resultSets);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				exception.Trace(null);
				TraceHelper.Current.QueryGetQueryRootForResourceFailed(this.dataContext.UserContext.Name, resourceSet.Name);
				throw;
			}
			return queryables;
		}

		public ResourceType GetResourceType(object resourceObject)
		{
			TraceHelper.Current.MethodCall0("DataServiceQueryProvider", "GetResourceType");
			object[] objArray = new object[2];
			objArray[0] = "resourceObject";
			objArray[1] = "DataServiceQueryProvider.GetResourceType";
			resourceObject.ThrowIfNull("resourceObject", new ParameterExtensions.MessageLoader(DataServiceQueryProvider.GetInputVariableToODataMethodNullMessage), objArray);
			DSResource dSResource = resourceObject as DSResource;
			if (dSResource != null)
			{
				return dSResource.ResourceType;
			}
			else
			{
				object[] assemblyQualifiedName = new object[3];
				assemblyQualifiedName[0] = "resourceObject";
				assemblyQualifiedName[1] = resourceObject.GetType().AssemblyQualifiedName;
				assemblyQualifiedName[2] = typeof(DSResource).AssemblyQualifiedName;
				string exceptionMessage = ExceptionHelpers.GetExceptionMessage(Resources.InvalidArgClrType, assemblyQualifiedName);
				throw new ArgumentException(exceptionMessage, "resourceObject");
			}
		}

		public object InvokeServiceOperation(ServiceOperation serviceOperation, object[] parameters)
		{
			return null;
		}

		internal class ResultSet : List<DSResource>
		{
			public ResourceType ResourceType
			{
				get;
				private set;
			}

			public ResultSet(ResourceType type)
			{
				this.ResourceType = type;
			}

			public override string ToString()
			{
				return this.ResourceType.ToString();
			}
		}

		internal class ResultSetCollection : Dictionary<string, DataServiceQueryProvider.ResultSet>
		{
			public ResultSetCollection()
			{
			}
		}
	}
}