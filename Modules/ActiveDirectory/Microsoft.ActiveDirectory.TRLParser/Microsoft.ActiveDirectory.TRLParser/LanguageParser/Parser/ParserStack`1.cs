using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser.Parser
{
	internal class ParserStack<T>
	{
		private T[] array;

		private int _top;

		public IList<T> Elements
		{
			get
			{
				return this.array;
			}
		}

		public int Top
		{
			get
			{
				return this._top;
			}
		}

		public ParserStack()
		{
			this.array = new T[1];
		}

		public bool IsEmpty()
		{
			return this._top == 0;
		}

		public T Pop()
		{
			ParserStack<T> parserStack = this;
			int num = parserStack._top - 1;
			int num1 = num;
			parserStack._top = num;
			return this.array[num1];
		}

		public void Push(T value)
		{
			if (this._top >= (int)this.array.Length)
			{
				T[] tArray = new T[(int)this.array.Length * 2];
				Array.Copy(this.array, tArray, this._top);
				this.array = tArray;
			}
			ParserStack<T> parserStack = this;
			int num = parserStack._top;
			int num1 = num;
			parserStack._top = num + 1;
			this.array[num1] = value;
		}

		public T TopElement()
		{
			return this.array[this._top - 1];
		}
	}
}