using Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics;

namespace Microsoft.Data.Edm.Internal
{
	internal static class TupleInternal
	{
		public static TupleInternal<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
		{
			return new TupleInternal<T1, T2>(item1, item2);
		}
	}
}