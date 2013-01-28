using Microsoft.ActiveDirectory.Management;
using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal delegate void ToDirectoryFormatDelegate(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo);
}