using System;

namespace Microsoft.Management.Odata.MofParser
{
	internal struct DocumentRange
	{
		private readonly DocumentCoordinate m_start;

		private readonly DocumentCoordinate m_end;

		private readonly string m_documentPath;

		public string DocumentPath
		{
			get
			{
				return this.m_documentPath;
			}
		}

		public DocumentCoordinate End
		{
			get
			{
				return this.m_end;
			}
		}

		public DocumentCoordinate Start
		{
			get
			{
				return this.m_start;
			}
		}

		internal DocumentRange(string documentPath, DocumentCoordinate rangeStart, DocumentCoordinate rangeEnd)
		{
			this.m_documentPath = documentPath;
			this.m_start = rangeStart;
			this.m_end = rangeEnd;
		}

		public override string ToString()
		{
			return string.Format("{0} {1}-{2}", this.DocumentPath, this.Start, this.End);
		}
	}
}