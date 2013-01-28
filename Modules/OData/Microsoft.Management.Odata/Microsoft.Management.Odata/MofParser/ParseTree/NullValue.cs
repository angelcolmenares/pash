using System;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal sealed class NullValue
	{
		private readonly static NullValue s_instance;

		internal static NullValue Instance
		{
			get
			{
				return NullValue.s_instance;
			}
		}

		static NullValue()
		{
			NullValue.s_instance = new NullValue();
		}

		public NullValue()
		{
		}

		public override string ToString()
		{
			return "null";
		}
	}
}