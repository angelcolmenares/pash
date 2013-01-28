namespace Microsoft.Data.Edm.Internal
{
	internal class TupleInternal<T1, T2>
	{
		public T1 Item1
		{
			get;set;
		}

		public T2 Item2
		{
			get;set;
		}

		public TupleInternal(T1 item1, T2 item2)
		{
			this.Item1 = item1;
			this.Item2 = item2;
		}
	}
}