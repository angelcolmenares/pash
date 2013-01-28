using Microsoft.ActiveDirectory;
using System;
using System.Globalization;

namespace Microsoft.ActiveDirectory.Management
{
	[Serializable]
	public class ADFilterParsingException : FormatException
	{
		private int _position;

		public int Position
		{
			get
			{
				return this._position;
			}
		}

		public ADFilterParsingException()
		{
			this._position = -1;
		}

		public ADFilterParsingException(string message) : base(message)
		{
			this._position = -1;
		}

		public ADFilterParsingException(string message, Exception innerException) : base(message, innerException)
		{
			this._position = -1;
		}

		public ADFilterParsingException(string query, string errorMessage, int position)

			: base(string.Format(CultureInfo.CurrentCulture, StringResources.ADFilterParsingErrorMessage, new object[] { query, errorMessage, position }))
		{
			this._position = position;
		}
	}
}