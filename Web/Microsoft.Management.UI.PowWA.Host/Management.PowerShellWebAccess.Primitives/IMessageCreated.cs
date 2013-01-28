using System;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	internal interface IMessageCreated
	{
		event EventHandler<MessageCreatedEventArgs> MessageCreated;
	}
}