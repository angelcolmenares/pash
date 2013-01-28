using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.Management.Odata.Core
{
	internal interface ICommand : IDisposable
	{
		bool AddArrayFieldParameter(string parameter, IEnumerable<object> values);

		bool AddFieldParameter(string parameter, object value);

		void AddParameter(string parameter, object value, bool isOption = true);

		bool CanFieldBeAdded(string fieldName);

		IEnumerator<DSResource> InvokeAsync(Expression expression, bool noStreamingResponse);
	}
}