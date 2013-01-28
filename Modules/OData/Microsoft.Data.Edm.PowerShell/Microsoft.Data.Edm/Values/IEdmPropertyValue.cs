using System;

namespace Microsoft.Data.Edm.Values
{
	internal interface IEdmPropertyValue : IEdmDelayedValue
	{
		string Name
		{
			get;
		}

	}
}