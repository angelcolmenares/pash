namespace Microsoft.Data.Edm.Internal
{
	internal interface IDependent : IFlushCaches
	{
		HashSetInternal<IDependencyTrigger> DependsOn
		{
			get;
		}

	}
}