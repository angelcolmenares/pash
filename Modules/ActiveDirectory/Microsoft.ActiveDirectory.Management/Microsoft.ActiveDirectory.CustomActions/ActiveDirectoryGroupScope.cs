using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DataContract(Name="ActiveDirectoryGroupScope", Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions")]
	[GeneratedCode("System.Runtime.Serialization", "3.0.0.0")]
	internal enum ActiveDirectoryGroupScope
	{
		Unknown,
		DomainLocal,
		Global,
		Universal
	}
}