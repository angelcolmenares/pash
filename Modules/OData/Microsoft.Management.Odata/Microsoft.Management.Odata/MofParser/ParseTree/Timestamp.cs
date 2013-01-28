using System;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal sealed class Timestamp : CIMDateTime
	{
		private readonly int m_year;

		private readonly int m_month;

		private readonly int m_day;

		private readonly int m_hour;

		private readonly int m_minute;

		private readonly int m_second;

		private readonly int m_microsecond;

		private readonly int m_utcOffsetMinutes;

		public override bool IsInterval
		{
			get
			{
				return false;
			}
		}

		public override bool IsTimestamp
		{
			get
			{
				return true;
			}
		}

		internal Timestamp(int year, int month, int day, int hour, int minute, int second, int microsecond, int utcOffsetMinutes)
		{
			this.m_year = year;
			this.m_month = month;
			this.m_day = day;
			this.m_hour = hour;
			this.m_minute = minute;
			this.m_second = second;
			this.m_microsecond = microsecond;
			this.m_utcOffsetMinutes = utcOffsetMinutes;
		}

		public override bool Equals(object obj)
		{
			Timestamp timestamp = obj as Timestamp;
			if (timestamp != null)
			{
				if (timestamp.m_year != this.m_year || timestamp.m_month != this.m_month || timestamp.m_day != this.m_day || timestamp.m_hour != this.m_hour || timestamp.m_minute != this.m_minute || timestamp.m_second != this.m_second || timestamp.m_microsecond != this.m_microsecond)
				{
					return false;
				}
				else
				{
					return timestamp.m_utcOffsetMinutes == this.m_utcOffsetMinutes;
				}
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return this.m_year ^ this.m_month ^ this.m_day ^ this.m_hour ^ this.m_minute ^ this.m_second ^ this.m_microsecond ^ this.m_utcOffsetMinutes;
		}

		public override string ToString()
		{
			char chr;
			string str = "\"{0:0000}{1:00}{2:00}{3:00}{4:00}.{5:000000}{6}{7:000}\"";
			object[] mYear = new object[9];
			mYear[0] = this.m_year;
			mYear[1] = this.m_month;
			mYear[2] = this.m_day;
			mYear[3] = this.m_hour;
			mYear[4] = this.m_minute;
			mYear[5] = this.m_second;
			mYear[6] = this.m_microsecond;
			object[] objArray = mYear;
			int num = 7;
			if (this.m_utcOffsetMinutes < 0)
			{
				chr = '-';
			}
			else
			{
				chr = '+';
			}
			objArray[num] = chr;
			mYear[8] = Math.Abs(this.m_utcOffsetMinutes);
			return string.Format(str, mYear);
		}
	}
}