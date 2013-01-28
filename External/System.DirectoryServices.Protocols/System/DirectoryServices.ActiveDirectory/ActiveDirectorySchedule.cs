using System;
using System.ComponentModel;
using System.DirectoryServices;
using System.Collections;
using System.Linq;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ActiveDirectorySchedule
	{
		private bool[] scheduleArray;

		private long utcOffSet;

		public bool[,,] RawSchedule
		{
			get
			{
				bool[,,] flagArray = new bool[7, 24, 4];
				for (int i = 0; i < 7; i++)
				{
					for (int j = 0; j < 24; j++)
					{
						for (int k = 0; k < 4; k++)
						{
							flagArray[i, j, k] = this.scheduleArray[i * 24 * 4 + j * 4 + k];
						}
					}
				}
				return flagArray;
			}
			set
			{
				if (value != null)
				{
					this.ValidateRawArray(value);
					for (int i = 0; i < 7; i++)
					{
						for (int j = 0; j < 24; j++)
						{
							for (int k = 0; k < 4; k++)
							{
								this.scheduleArray[i * 24 * 4 + j * 4 + k] = value[i, j, k];
							}
						}
					}
					return;
				}
				else
				{
					throw new ArgumentNullException("value");
				}
			}
		}

		public ActiveDirectorySchedule()
		{
			this.scheduleArray = new bool[0x2a0];
			TimeSpan utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
			this.utcOffSet = utcOffset.Ticks / 0x861c46800L;
		}

		public ActiveDirectorySchedule(ActiveDirectorySchedule schedule) : this()
		{
			if (schedule != null)
			{
				bool[] flagArray = schedule.scheduleArray;
				for (int i = 0; i < 0x2a0; i++)
				{
					this.scheduleArray[i] = flagArray[i];
				}
				return;
			}
			else
			{
				throw new ArgumentNullException();
			}
		}

		internal ActiveDirectorySchedule(bool[] schedule) : this()
		{
			for (int i = 0; i < 0x2a0; i++)
			{
				this.scheduleArray[i] = schedule[i];
			}
		}

		internal byte[] GetUnmanagedSchedule()
		{
			byte num = 0;
			byte[] numArray = new byte[188];
			numArray[0] = 188;
			numArray[8] = 1;
			numArray[16] = 20;
			for (int i = 20; i < 188; i++)
			{
				num = 0;
				int num1 = (i - 20) * 4;
				if (this.scheduleArray[num1])
				{
					num = (byte)(num | 1);
				}
				if (this.scheduleArray[num1 + 1])
				{
					num = (byte)(num | 2);
				}
				if (this.scheduleArray[num1 + 2])
				{
					num = (byte)(num | 4);
				}
				if (this.scheduleArray[num1 + 3])
				{
					num = (byte)(num | 8);
				}
				int num2 = i - (int)this.utcOffSet;
				if (num2 < 188)
				{
					if (num2 < 20)
					{
						num2 = 188 - 20 - num2;
					}
				}
				else
				{
					num2 = num2 - 188 + 20;
				}
				numArray[num2] = num;
			}
			return numArray;
		}

		public void ResetSchedule()
		{
			for (int i = 0; i < 0x2a0; i++)
			{
				this.scheduleArray[i] = false;
			}
		}

		public void SetDailySchedule(HourOfDay fromHour, MinuteOfHour fromMinute, HourOfDay toHour, MinuteOfHour toMinute)
		{
			for (int i = 0; i < 7; i++)
			{
				this.SetSchedule((DayOfWeek)i, fromHour, fromMinute, toHour, toMinute);
			}
		}

		public void SetSchedule(DayOfWeek day, HourOfDay fromHour, MinuteOfHour fromMinute, HourOfDay toHour, MinuteOfHour toMinute)
		{
			if (day < DayOfWeek.Sunday || day > DayOfWeek.Saturday)
			{
				throw new InvalidEnumArgumentException("day", (int)day, typeof(DayOfWeek));
			}
			else
			{
				if (fromHour < HourOfDay.Zero || fromHour > HourOfDay.TwentyThree)
				{
					throw new InvalidEnumArgumentException("fromHour", (int)fromHour, typeof(HourOfDay));
				}
				else
				{
					if (fromMinute == MinuteOfHour.Zero || fromMinute == MinuteOfHour.Fifteen || fromMinute == MinuteOfHour.Thirty || fromMinute == MinuteOfHour.FortyFive)
					{
						if (toHour < HourOfDay.Zero || toHour > HourOfDay.TwentyThree)
						{
							throw new InvalidEnumArgumentException("toHour", (int)toHour, typeof(HourOfDay));
						}
						else
						{
							if (toMinute == MinuteOfHour.Zero || toMinute == MinuteOfHour.Fifteen || toMinute == MinuteOfHour.Thirty || toMinute == MinuteOfHour.FortyFive)
							{
								if ((int)fromHour * (int)(HourOfDay.Four | HourOfDay.Eight | HourOfDay.Twelve | HourOfDay.Sixteen | HourOfDay.Twenty) + (int)fromMinute <= (int)toHour * (int)(HourOfDay.Four | HourOfDay.Eight | HourOfDay.Twelve | HourOfDay.Sixteen | HourOfDay.Twenty) + (int)toMinute)
								{
									int num = (int)day * 24 * (int)DayOfWeek.Thursday + (int)((int)fromHour * (int)HourOfDay.Four) + (int)((int)fromMinute / (int)MinuteOfHour.Fifteen);
									int num1 = (int)day * 24 * (int)DayOfWeek.Thursday + (int)((int)toHour * (int)HourOfDay.Four) + (int)((int)toMinute / (int)MinuteOfHour.Fifteen);
									for (int i = num; i <= num1; i++)
									{
										this.scheduleArray[i] = true;
									}
									return;
								}
								else
								{
									throw new ArgumentException(Res.GetString("InvalidTime"));
								}
							}
							else
							{
								throw new InvalidEnumArgumentException("toMinute", (int)toMinute, typeof(MinuteOfHour));
							}
						}
					}
					else
					{
						throw new InvalidEnumArgumentException("fromMinute", (int)fromMinute, typeof(MinuteOfHour));
					}
				}
			}
		}

		public void SetSchedule(DayOfWeek[] days, HourOfDay fromHour, MinuteOfHour fromMinute, HourOfDay toHour, MinuteOfHour toMinute)
		{
			if (!days.Contains (DayOfWeek.Sunday))
			{
				int num = 0;
				while (num < (int)days.Length)
				{
					if (days[num] < DayOfWeek.Sunday || days[num] > DayOfWeek.Saturday)
					{
						throw new InvalidEnumArgumentException("days", (int)days[num], typeof(DayOfWeek));
					}
					else
					{
						num++;
					}
				}
				for (int i = 0; i < (int)days.Length; i++)
				{
					this.SetSchedule(days[i], fromHour, fromMinute, toHour, toMinute);
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("days");
			}
		}

		internal void SetUnmanagedSchedule(byte[] unmanagedSchedule)
		{
			for (int i = 20; i < 188; i++)
			{
				int num = (i - 20) * 4;
				int num1 = i - (int)this.utcOffSet;
				if (num1 < 188)
				{
					if (num1 < 20)
					{
						num1 = 188 - 20 - num1;
					}
				}
				else
				{
					num1 = num1 - 188 + 20;
				}
				int num2 = unmanagedSchedule[num1];
				if ((num2 & 1) != 0)
				{
					this.scheduleArray[num] = true;
				}
				if ((num2 & 2) != 0)
				{
					this.scheduleArray[num + 1] = true;
				}
				if ((num2 & 4) != 0)
				{
					this.scheduleArray[num + 2] = true;
				}
				if ((num2 & 8) != 0)
				{
					this.scheduleArray[num + 3] = true;
				}
			}
		}

		private void ValidateRawArray(bool[,,] array)
		{
			if (array.Length == 0x2a0)
			{
				int length = array.GetLength(0);
				int num = array.GetLength(1);
				int length1 = array.GetLength(2);
				if (length != 7 || num != 24 || length1 != 4)
				{
					throw new ArgumentException("value");
				}
				else
				{
					return;
				}
			}
			else
			{
				throw new ArgumentException("value");
			}
		}
	}
}