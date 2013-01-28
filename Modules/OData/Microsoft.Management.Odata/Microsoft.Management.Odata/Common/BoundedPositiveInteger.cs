using System;

namespace Microsoft.Management.Odata.Common
{
	internal class BoundedPositiveInteger : BoundedInteger
	{
		public BoundedPositiveInteger(int currentValue, int upperBound) : base(currentValue, 0, upperBound)
		{
		}

		public BoundedPositiveInteger(int currentValue, int upperBound, bool noThrowForInvalidSet) : base(currentValue, 0, upperBound, noThrowForInvalidSet)
		{
		}
	}
}