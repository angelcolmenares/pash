using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Schema;
using Microsoft.Management.Odata.Tracing;
using System;
using System.Collections.Generic;
using System.Data.Services.Providers;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Management.Odata.Core
{
	internal class ComplexTypeInstance : IUpdateInstance
	{
		private Dictionary<string, object> properties;

		private ResourceType resourceType;

		private WellKnownTypeFactory factory;

		public ComplexTypeInstance(ResourceType resourceType)
		{
			this.resourceType = resourceType;
			this.properties = new Dictionary<string, object>();
			WellKnownTypes.TryGetFactory(resourceType.FullName, out this.factory);
		}

		public void Delete()
		{
			object[] objArray = new object[2];
			objArray[0] = "Delete";
			objArray[1] = "EntityUpdate";
			throw new NotImplementedException(ExceptionHelpers.GetExceptionMessage(Resources.NotImplementedExceptionMessage, objArray));
		}

		public Dictionary<string, object> GetKeyValues()
		{
			throw new NotImplementedException();
		}

		public void InvokeCommand()
		{
			object[] objArray = new object[2];
			objArray[0] = "InvokeCommand";
			objArray[1] = "EntityUpdate";
			throw new NotImplementedException(ExceptionHelpers.GetExceptionMessage(Resources.NotImplementedExceptionMessage, objArray));
		}

		public void Reset()
		{
			object[] objArray = new object[2];
			objArray[0] = "Reset";
			objArray[1] = "EntityUpdate";
			throw new NotImplementedException(ExceptionHelpers.GetExceptionMessage(Resources.NotImplementedExceptionMessage, objArray));
		}

		public object Resolve()
		{
			if (this.factory == null)
			{
				ResourceCustomState customState = this.resourceType.CustomState as ResourceCustomState;
				if (customState != null)
				{
					foreach (string list in this.properties.Keys.ToList<string>())
					{
						object item = this.properties[list];
						item = EntityUpdate.ResolveUpdatableObject(item);
						item = EntityUpdate.ResolveUpdatableObjectList(item);
						this.properties[list] = item;
					}
					return ComplexTypeInstance.ClrInstanceForComplexTypeBuilder.Build(customState.ClrType, this.properties);
				}
				else
				{
					throw new InvalidOperationException(ExceptionHelpers.GetExceptionMessage(Resources.ResourceCustomStateNull, new object[0]));
				}
			}
			else
			{
				return this.factory(this.properties);
			}
		}

		public void SetReference(string propertyName, IUpdateInstance instance)
		{
			throw new NotImplementedException();
		}

		public void SetValue(string propertyName, object value)
		{
			string str;
			Tracer current = TraceHelper.Current;
			string[] fullName = new string[6];
			fullName[0] = "ComplexUpdate SetValue ComplexType name = ";
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
			this.properties[propertyName] = value;
		}

		public TestHookCommandInvocationData TestHookGetInvocationData()
		{
			object[] objArray = new object[2];
			objArray[0] = "GetInvocationData";
			objArray[1] = "EntityUpdate";
			throw new NotImplementedException(ExceptionHelpers.GetExceptionMessage(Resources.NotImplementedExceptionMessage, objArray));
		}

		public void VerifyConcurrencyValues(IEnumerable<KeyValuePair<string, object>> values)
		{
			object[] objArray = new object[2];
			objArray[0] = "VerifyConcurrencyValues";
			objArray[1] = "EntityUpdate";
			throw new NotImplementedException(ExceptionHelpers.GetExceptionMessage(Resources.NotImplementedExceptionMessage, objArray));
		}

		internal static class ClrInstanceForComplexTypeBuilder
		{
			public static object Build(Type type, Dictionary<string, object> properties)
			{
				Dictionary<string, object> strs = ComplexTypeInstance.ClrInstanceForComplexTypeBuilder.CreateResolvedDictionary(properties);
				if (type != typeof(PSObject))
				{
					return ClrInstanceBuilder.Build(type, strs);
				}
				else
				{
					return PSObjectBuilder.Build(strs);
				}
			}

			internal static Dictionary<string, object> CreateResolvedDictionary(Dictionary<string, object> properties)
			{
				Dictionary<string, object> strs = new Dictionary<string, object>();
				foreach (string key in properties.Keys)
				{
					object item = properties[key];
					IUpdateInstance updateInstance = item as IUpdateInstance;
					if (updateInstance != null)
					{
						item = updateInstance.Resolve();
					}
					strs.Add(key, item);
				}
				return strs;
			}
		}
	}
}