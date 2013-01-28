using Microsoft.Management.Odata;
using System;

namespace Microsoft.Management.Odata.Common
{
	internal class PerTimeslotCounter
	{
		private PerTimeslotCounter.PerTimeSlotWindow timeSlotWindow;

		private int @value;

		public virtual int TimeSlot
		{
			get
			{
				return this.timeSlotWindow.TimeSlot.Value;
			}
		}

		public virtual int Value
		{
			get
			{
				if (this.timeSlotWindow.InsideWindow(DateTimeHelper.Now))
				{
					return this.@value;
				}
				else
				{
					return 0;
				}
			}
		}

		public PerTimeslotCounter(int timeSlot)
		{
			this.timeSlotWindow = new PerTimeslotCounter.PerTimeSlotWindow(DateTimeHelper.Now, timeSlot);
			this.@value = 0;
		}

		public virtual void Increment(int newTimeSlot)
		{
			DateTime now = DateTimeHelper.Now;
			if (newTimeSlot != this.timeSlotWindow.TimeSlot.Value || !this.timeSlotWindow.InsideWindow(now))
			{
				if (this.PreResetEventHandler != null && this.@value != 0)
				{
					this.PreResetEventHandler(this, new PerTimeslotCounter.PreResetEventArgs(this.timeSlotWindow.BaseTime, this.@value, this.timeSlotWindow.TimeSlot.Value, newTimeSlot));
				}
				this.@value = 1;
				this.timeSlotWindow.Set(now, newTimeSlot);
				return;
			}
			else
			{
				PerTimeslotCounter perTimeslotCounter = this;
				perTimeslotCounter.@value = perTimeslotCounter.@value + 1;
				return;
			}
		}

		public override string ToString()
		{
			object[] str = new object[6];
			str[0] = "Per time slot requests = ";
			int value = this.Value;
			str[1] = value.ToString();
			str[2] = " Time slot = ";
			str[3] = this.TimeSlot;
			str[4] = "Base time = ";
			str[5] = this.timeSlotWindow.BaseTime;
			return string.Concat(str);
		}

		public event EventHandler<PerTimeslotCounter.PreResetEventArgs> PreResetEventHandler;
		internal class PerTimeSlotWindow
		{
			public DateTime BaseTime
			{
				get;
				private set;
			}

			public BoundedInteger TimeSlot
			{
				get;
				private set;
			}

			public PerTimeSlotWindow(DateTime baseTime, int timeSlot)
			{
				this.TimeSlot = new BoundedInteger(timeSlot, 1, 0x7fffffff);
				this.Reset(baseTime, timeSlot);
			}

			public bool InsideWindow(DateTime other)
			{
				ExceptionHelpers.ThrowArgumentExceptionIf("other", other < this.BaseTime, Resources.TimeShouldBeGreater, new object[0]);
				TimeSpan timeSpan = other - this.BaseTime;
				if (timeSpan.TotalSeconds >= (double)this.TimeSlot.Value)
				{
					return false;
				}
				else
				{
					return true;
				}
			}

			private void Reset(DateTime baseTime, int timeSlot)
			{
				this.BaseTime = new DateTime(baseTime.Year, baseTime.Month, baseTime.Day, baseTime.Hour, baseTime.Minute, baseTime.Second);
				this.TimeSlot.Value = timeSlot;
			}

			public void Set(DateTime newTime, int timeSlot)
			{
				ExceptionHelpers.ThrowArgumentExceptionIf("newTime", newTime < this.BaseTime, Resources.TimeShouldBeGreater, new object[0]);
				if (this.TimeSlot.Value == timeSlot)
				{
					TimeSpan timeSpan = newTime - this.BaseTime;
					int num = (int)Math.Floor(timeSpan.TotalSeconds / (double)timeSlot);
					DateTime baseTime = this.BaseTime;
					this.BaseTime = baseTime.AddSeconds((double)(num * timeSlot));
					return;
				}
				else
				{
					this.Reset(newTime, timeSlot);
					return;
				}
			}
		}

		internal class PreResetEventArgs : EventArgs
		{
			public int Counter
			{
				get;
				private set;
			}

			public int NewTimeSlot
			{
				get;
				private set;
			}

			public int OldTimeSlot
			{
				get;
				private set;
			}

			public DateTime Time
			{
				get;
				private set;
			}

			public PreResetEventArgs(DateTime time, int counter, int oldTimeSlot, int newTimeSlot)
			{
				this.Counter = counter;
				this.Time = time;
				this.OldTimeSlot = oldTimeSlot;
				this.NewTimeSlot = newTimeSlot;
			}
		}
	}
}