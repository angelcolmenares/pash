using Microsoft.Data.Edm;
using System;
using System.Globalization;

namespace Microsoft.Data.Edm.Validation
{
	internal class EdmError
	{
		public EdmErrorCode ErrorCode
		{
			get;
			private set;
		}

		public EdmLocation ErrorLocation
		{
			get;
			private set;
		}

		public string ErrorMessage
		{
			get;
			private set;
		}

		public EdmError(EdmLocation errorLocation, EdmErrorCode errorCode, string errorMessage)
		{
			this.ErrorLocation = errorLocation;
			this.ErrorCode = errorCode;
			this.ErrorMessage = errorMessage;
		}

		public override string ToString()
		{
			if (this.ErrorLocation == null || this.ErrorLocation as ObjectLocation != null)
			{
				return string.Concat(Convert.ToString(this.ErrorCode, CultureInfo.InvariantCulture), " : ", this.ErrorMessage);
			}
			else
			{
				string[] str = new string[5];
				str[0] = Convert.ToString(this.ErrorCode, CultureInfo.InvariantCulture);
				str[1] = " : ";
				str[2] = this.ErrorMessage;
				str[3] = " : ";
				str[4] = this.ErrorLocation.ToString();
				return string.Concat(str);
			}
		}
	}
}