using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.WSMan.Management
{
	[SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
	[TypeLibType((short)0)]
	public enum WSManEnumFlags
	{
		WSManFlagReturnObject = 0,
		WSManFlagHierarchyDeep = 0,
		WSManFlagNonXmlText = 1,
		WSManFlagReturnEPR = 2,
		WSManFlagReturnObjectAndEPR = 4,
		WSManFlagHierarchyShallow = 32,
		WSManFlagHierarchyDeepBasePropsOnly = 64,
		WSManFlagAssociationInstance = 128
	}
}