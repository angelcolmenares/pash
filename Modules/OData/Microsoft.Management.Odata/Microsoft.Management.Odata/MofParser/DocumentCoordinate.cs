using System;

namespace Microsoft.Management.Odata.MofParser
{
	internal struct DocumentCoordinate
	{
		private readonly int m_line;

		private readonly int m_column;

		public int Column
		{
			get
			{
				return this.m_column;
			}
		}

		public int Line
		{
			get
			{
				return this.m_line;
			}
		}

		internal DocumentCoordinate(int line, int column)
		{
			this.m_line = line;
			this.m_column = column;
		}

		internal DocumentCoordinate NextColumn()
		{
			return new DocumentCoordinate(this.Line, this.Column + 1);
		}

		internal DocumentCoordinate NextLine()
		{
			return new DocumentCoordinate(this.Line + 1, 1);
		}

		public override string ToString()
		{
			return string.Format("({0},{1})", this.Line, this.Column);
		}
	}
}