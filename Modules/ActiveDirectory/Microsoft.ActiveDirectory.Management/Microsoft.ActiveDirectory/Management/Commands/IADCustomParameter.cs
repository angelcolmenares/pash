using Microsoft.ActiveDirectory.Management;
using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal interface IADCustomParameter
	{
		ADPropertyValueCollection ConvertToADPropertyValueCollection(string parameterName);

		object GetOriginalValue();
	}
}