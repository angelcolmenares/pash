namespace Microsoft.Data.Edm.Internal
{
	internal interface IDependencyTrigger
	{
		HashSetInternal<IDependent> Dependents
		{
			get;
		}

	}
}