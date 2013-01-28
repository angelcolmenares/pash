namespace Microsoft.Management.Odata.Core
{
	internal enum ExpressionCategory
	{
		ResourceRoot,
		WhereOfResourceRoot,
		WhereInsideNavPropertyWithGetRefCmdlet,
		NestedPropertyComparisons,
		NestedPropertyComparisonsInsideNavPropertyWithGetRefCmdlet,
		WhereOfResultSet,
		SelectNavProperty,
		SelectExpansion,
		Unhandled
	}
}