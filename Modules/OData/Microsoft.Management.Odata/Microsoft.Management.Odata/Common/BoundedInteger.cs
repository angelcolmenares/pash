using Microsoft.Management.Odata;
using System;

namespace Microsoft.Management.Odata.Common
{
	internal class BoundedInteger
	{
		private int @value;

		public int LowerBound
		{
			get;
			private set;
		}

		public bool NoThrowForInvalidSet
		{
			get;
			private set;
		}

		public int UpperBound
		{
			get;
			private set;
		}

		public int Value
		{
			get
			{
				return this.@value;
			}
			set
			{
				if (!this.NoThrowForInvalidSet)
				{
					if (value < this.LowerBound || value > this.UpperBound)
					{
						object[] lowerBound = new object[3];
						lowerBound[0] = value;
						lowerBound[1] = this.LowerBound;
						lowerBound[2] = this.UpperBound;
						throw new ArgumentOutOfRangeException(ExceptionHelpers.GetExceptionMessage(Resources.IntegerOutOfRange, lowerBound));
					}
					else
					{
						this.@value = value;
						return;
					}
				}
				else
				{
					this.@value = this.GetValueInRange(value);
					return;
				}
			}
		}

		public BoundedInteger(int currentValue, int lowerBound, int upperBound) : this(currentValue, lowerBound, upperBound, false)
		{
		}

		public BoundedInteger(int currentValue, int lowerBound, int upperBound, bool noThrowForInvalidSet)
		{
			if (lowerBound <= upperBound)
			{
				this.LowerBound = lowerBound;
				this.UpperBound = upperBound;
				this.NoThrowForInvalidSet = noThrowForInvalidSet;
				this.Value = currentValue;
				return;
			}
			else
			{
				object[] objArray = new object[2];
				objArray[0] = lowerBound;
				objArray[1] = upperBound;
				throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.LowerBoundGreaterThanUpper, objArray));
			}
		}

		private int GetValueInRange(int newValue)
		{
			if (this.LowerBound <= newValue)
			{
				if (this.UpperBound >= newValue)
				{
					return newValue;
				}
				else
				{
					object[] lowerBound = new object[4];
					lowerBound[0] = "Value passed is greater than upper bound. Returning upper bound value.\nValue ";
					lowerBound[1] = newValue;
					lowerBound[2] = "\nUpper limit ";
					lowerBound[3] = this.LowerBound;
					TraceHelper.Current.DebugMessage(string.Concat(lowerBound));
					return this.UpperBound;
				}
			}
			else
			{
				object[] objArray = new object[4];
				objArray[0] = "Value passed is less than lower bound. Returning lower bound.\nValue ";
				objArray[1] = newValue;
				objArray[2] = "\nLower limit ";
				objArray[3] = this.LowerBound;
				TraceHelper.Current.DebugMessage(string.Concat(objArray));
				return this.LowerBound;
			}
		}
	}
}