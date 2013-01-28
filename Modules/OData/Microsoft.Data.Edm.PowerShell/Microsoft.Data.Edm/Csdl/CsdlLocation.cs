using Microsoft.Data.Edm;
using System;
using System.Globalization;

namespace Microsoft.Data.Edm.Csdl
{
	internal class CsdlLocation : EdmLocation
	{
		public int LineNumber
		{
			get;
			private set;
		}

		public int LinePosition
		{
			get;
			private set;
		}

		internal CsdlLocation(int number, int position)
		{
			this.LineNumber = number;
			this.LinePosition = position;
		}

		public override string ToString()
		{
			string[] str = new string[5];
			str[0] = "(";
			str[1] = Convert.ToString(this.LineNumber, CultureInfo.InvariantCulture);
			str[2] = ", ";
			str[3] = Convert.ToString(this.LinePosition, CultureInfo.InvariantCulture);
			str[4] = ")";
			return string.Concat(str);
		}
	}
}