using Microsoft.Management.Odata.MofParser;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Tokenizer
{
	internal sealed class Coordinatizer : IEnumerable<CharacterAndCoordinate>, IEnumerable
	{
		private readonly IEnumerable<char> m_enumerable;

		private readonly string m_documentPath;

		public Coordinatizer(IEnumerable<char> enumerable, string documentPath)
		{
			this.m_enumerable = enumerable;
			this.m_documentPath = documentPath;
		}

		public IEnumerator<CharacterAndCoordinate> GetEnumerator()
		{
			return new Coordinatizer.Enumerator(this.m_enumerable.GetEnumerator(), this.m_documentPath);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		private sealed class Enumerator : IEnumerator<CharacterAndCoordinate>, IDisposable, IEnumerator
		{
			private readonly IEnumerator<char> m_wrappedEnumerator;

			private readonly string m_documentPath;

			private CharacterAndCoordinate m_current;

			public CharacterAndCoordinate Current
			{
				get
				{
					return this.m_current;
				}
			}

			object System.Collections.IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}

			internal Enumerator(IEnumerator<char> wrappedEnumerator, string documentPath)
			{
				this.m_wrappedEnumerator = wrappedEnumerator;
				this.m_documentPath = documentPath;
			}

			public void Dispose()
			{
				this.m_wrappedEnumerator.Dispose();
			}

			public bool MoveNext()
			{
				bool flag = this.m_wrappedEnumerator.MoveNext();
				if (!flag)
				{
					this.m_current = new CharacterAndCoordinate();
				}
				else
				{
					this.m_current.DocumentPath = this.m_documentPath;
					char current = this.m_wrappedEnumerator.Current;
					char character = this.m_current.Character;
					char chr = character;
					if (chr == '\0' || chr == '\n')
					{
						DocumentCoordinate coordinate = this.m_current.Coordinate;
						this.m_current.Coordinate = coordinate.NextLine();
					}
					else
					{
						if (chr == '\r')
						{
							if (current != '\n')
							{
								DocumentCoordinate documentCoordinate = this.m_current.Coordinate;
								this.m_current.Coordinate = documentCoordinate.NextLine();
							}
						}
						else
						{
							DocumentCoordinate coordinate1 = this.m_current.Coordinate;
							this.m_current.Coordinate = coordinate1.NextColumn();
						}
					}
					this.m_current.Character = this.m_wrappedEnumerator.Current;
				}
				return flag;
			}

			public void Reset()
			{
				throw new NotSupportedException();
			}
		}
	}
}