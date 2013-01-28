using Microsoft.Management.Odata.MofParser;
using System;

namespace Tokenizer
{
	internal struct CharacterAndCoordinate
	{
		public char Character
		{
			get;
			internal set;
		}

		public DocumentCoordinate Coordinate
		{
			get;
			internal set;
		}

		public string DocumentPath
		{
			get;
			internal set;
		}

		public override string ToString()
		{
			return string.Format("'{0}' {1}", this.Character, this.Coordinate);
		}
	}
}