using System;
using System.Collections.Generic;

namespace Microsoft.Management.Odata.Core
{
	internal interface IUpdateInstance
	{
		void Delete();

		Dictionary<string, object> GetKeyValues();

		void InvokeCommand();

		void Reset();

		object Resolve();

		void SetReference(string propertyName, IUpdateInstance instance);

		void SetValue(string propertyName, object value);

		TestHookCommandInvocationData TestHookGetInvocationData();

		void VerifyConcurrencyValues(IEnumerable<KeyValuePair<string, object>> values);
	}
}