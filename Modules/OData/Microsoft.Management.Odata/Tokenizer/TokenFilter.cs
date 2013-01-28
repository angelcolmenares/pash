using System;
using System.Collections;
using System.Collections.Generic;

namespace Tokenizer
{
	internal sealed class TokenFilter : IEnumerable<Token>, IEnumerable
	{
		private readonly IEnumerable<Token> m_tokenSource;

		private readonly Predicate<Token> m_predicate;

		public TokenFilter(IEnumerable<Token> tokenSource, Predicate<Token> predicate)
		{
			this.m_tokenSource = tokenSource;
			this.m_predicate = predicate;
		}

		public IEnumerator<Token> GetEnumerator()
		{
			return new TokenFilter.Enumerator(this.m_tokenSource.GetEnumerator(), this.m_predicate);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		private sealed class Enumerator : IEnumerator<Token>, IDisposable, IEnumerator
		{
			private readonly IEnumerator<Token> m_wrappedEnumerator;

			private readonly Predicate<Token> m_predicate;

			private Token m_current;

			public Token Current
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

			public Enumerator(IEnumerator<Token> wrappedEnumerator, Predicate<Token> predicate)
			{
				this.m_wrappedEnumerator = wrappedEnumerator;
				this.m_predicate = predicate;
			}

			public void Dispose()
			{
				this.m_wrappedEnumerator.Dispose();
			}

			public bool MoveNext()
			{
				Token current;
				do
				{
					if (this.m_wrappedEnumerator.MoveNext())
					{
						current = this.m_wrappedEnumerator.Current;
					}
					else
					{
						return false;
					}
				}
				while (!this.m_predicate(current));
				this.m_current = current;
				return true;
			}

			public void Reset()
			{
				throw new NotSupportedException();
			}
		}
	}
}