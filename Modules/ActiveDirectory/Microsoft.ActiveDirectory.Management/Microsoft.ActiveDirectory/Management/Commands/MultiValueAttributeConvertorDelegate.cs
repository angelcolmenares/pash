using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal delegate object MultiValueAttributeConvertorDelegate(object entity, string extendedAttribute, CmdletSessionInfo cmdletSessionInfo);
}