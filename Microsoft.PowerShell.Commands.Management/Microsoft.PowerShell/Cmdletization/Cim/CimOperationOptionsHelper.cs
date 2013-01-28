using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using Microsoft.PowerShell.Cim;
using System;
using System.Collections.Generic;

namespace Microsoft.PowerShell.Cmdletization.Cim
{
	internal static class CimOperationOptionsHelper
	{
		internal static void SetCustomOption(CimOperationOptions operationOptions, string optionName, object optionValue)
		{
			if (optionValue != null)
			{
				object cim = CimValueConverter.ConvertFromDotNetToCim(optionValue);
				CimType cimType = CimConverter.GetCimType(CimValueConverter.GetCimType(optionValue.GetType()));
				operationOptions.SetCustomOption(optionName, cim, cimType, false);
				return;
			}
			else
			{
				return;
			}
		}

		internal static void SetCustomOptions(CimOperationOptions operationOptions, IEnumerable<KeyValuePair<string, object>> customOptions)
		{
			if (customOptions != null)
			{
				foreach (KeyValuePair<string, object> customOption in customOptions)
				{
					CimOperationOptionsHelper.SetCustomOption(operationOptions, customOption.Key, customOption.Value);
				}
			}
		}
	}
}