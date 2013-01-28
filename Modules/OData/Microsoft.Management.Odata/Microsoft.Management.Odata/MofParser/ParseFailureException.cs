using System;

namespace Microsoft.Management.Odata.MofParser
{
	public sealed class ParseFailureException : Exception
	{
		private readonly DocumentRange m_location;

		public override string Message
		{
			get
			{
				return string.Format("{0} {1}", base.Message, this.m_location);
			}
		}

		internal ParseFailureException(string message, DocumentRange location) : base(message)
		{
			this.m_location = location;
		}
	}
}