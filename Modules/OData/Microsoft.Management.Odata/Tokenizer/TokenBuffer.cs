using Microsoft.Management.Odata.MofParser;
using System;
using System.Collections.Generic;

namespace Tokenizer
{
	internal sealed class TokenBuffer
	{
		private readonly IEnumerator<CharacterAndCoordinate> m_enumerator;

		private readonly List<CharacterAndCoordinate> m_buffer;

		private DocumentCoordinate m_lastReadPosition;

		private readonly string m_documentPath;

		public string DocumentPath
		{
			get
			{
				return this.m_documentPath;
			}
		}

		public char this[int offset]
		{
			get
			{
				CharacterAndCoordinate at = this.GetAt(offset);
				return at.Character;
			}
		}

		public TokenBuffer(IEnumerator<CharacterAndCoordinate> enumerator, string documentPath)
		{
			this.m_buffer = new List<CharacterAndCoordinate>();
			this.m_enumerator = enumerator;
			this.m_documentPath = documentPath;
		}

		public void Discard(int numberOfCharactersToDiscard)
		{
			int count = this.m_buffer.Count;
			if (count >= numberOfCharactersToDiscard)
			{
				this.m_buffer.RemoveRange(0, numberOfCharactersToDiscard);
				return;
			}
			else
			{
				this.m_buffer.Clear();
				int num = 0;
				while (num < numberOfCharactersToDiscard - count)
				{
					if (this.m_enumerator.MoveNext())
					{
						num++;
					}
					else
					{
						return;
					}
				}
				return;
			}
		}

		private CharacterAndCoordinate GetAt(int offset)
		{
			while (offset > this.m_buffer.Count - 1)
			{
				if (!this.m_enumerator.MoveNext())
				{
					CharacterAndCoordinate mLastReadPosition = new CharacterAndCoordinate();
					mLastReadPosition.Coordinate = this.m_lastReadPosition;
					return mLastReadPosition;
				}
				else
				{
					CharacterAndCoordinate current = this.m_enumerator.Current;
					this.m_buffer.Add(current);
					this.m_lastReadPosition = current.Coordinate;
				}
			}
			return this.m_buffer[offset];
		}

		public DocumentRange GetRange(int length)
		{
			CharacterAndCoordinate at = this.GetAt(0);
			DocumentCoordinate coordinate = at.Coordinate;
			CharacterAndCoordinate characterAndCoordinate = this.GetAt(length);
			DocumentCoordinate documentCoordinate = characterAndCoordinate.Coordinate;
			for (int i = length - 1; documentCoordinate.Column == 0 && i >= 0; i--)
			{
				CharacterAndCoordinate at1 = this.GetAt(i);
				char character = at1.Character;
				if (character != 0)
				{
					if (character == '\r' || character == '\n')
					{
						documentCoordinate = at1.Coordinate;
						break;
					}
					else
					{
						DocumentCoordinate coordinate1 = at1.Coordinate;
						documentCoordinate = coordinate1.NextColumn();
					}
				}
			}
			return new DocumentRange(this.m_documentPath, coordinate, documentCoordinate);
		}

		public string GetString(int length)
		{
			char[] item = new char[length];
			for (int i = 0; i < length; i++)
			{
				item[i] = this[i];
			}
			return new string(item);
		}

		public bool IsMatchAt(char ch, int offset)
		{
			char item = this[offset];
			return char.ToLower(ch) == char.ToLower(item);
		}
	}
}