using Microsoft.Management.Odata.MofParser;
using System;

namespace Tokenizer
{
	internal abstract class Token
	{
		private readonly DocumentRange m_location;

		public DocumentRange Location
		{
			get
			{
				return this.m_location;
			}
		}

		public abstract TokenType Type
		{
			get;
		}

		protected Token(DocumentRange location)
		{
			this.m_location = location;
		}

		public override string ToString()
		{
			DocumentRange location = this.Location;
			DocumentRange documentRange = this.Location;
			return string.Format("{0} {1}@{2}", this.Type, location.DocumentPath, documentRange.Start);
		}
	}
}