using System;
using System.Runtime.InteropServices;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	[ComVisible(false)]
	internal interface IObjectPreProcess
	{
		object Process(object resultObject);
	}
}