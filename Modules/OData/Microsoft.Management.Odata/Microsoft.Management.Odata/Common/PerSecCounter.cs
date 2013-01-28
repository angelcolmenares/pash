using Microsoft.Management.Odata;
using System;

namespace Microsoft.Management.Odata.Common
{
	internal class PerSecCounter : PerTimeslotCounter
	{
		public PerSecCounter() : base(1)
		{
		}

		public virtual void Increment()
		{
			base.Increment(1);
		}

		public override void Increment(int newTimeSlot)
		{
			object[] objArray = new object[2];
			objArray[0] = newTimeSlot;
			objArray[1] = 1;
			ExceptionHelpers.ThrowArgumentExceptionIf("newTimeSlot", newTimeSlot != 1, Resources.InvalidArgMessage, objArray);
			base.Increment(newTimeSlot);
		}
	}
}