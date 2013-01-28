namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal interface IADCustomExceptionFiltering
	{
		IADExceptionFilter ExceptionFilter
		{
			get;
		}

	}
}