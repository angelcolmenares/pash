using Microsoft.Data.Edm.Values;
using System;

namespace Microsoft.Data.Edm.EdmToClrConversion
{
	internal delegate bool TryCreateObjectInstance(IEdmStructuredValue edmValue, Type clrType, EdmToClrConverter converter, out object objectInstance, out bool objectInstanceInitialized);
}