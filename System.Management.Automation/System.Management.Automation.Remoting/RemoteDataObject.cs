namespace System.Management.Automation.Remoting
{
    using System;
    using System.Management.Automation;

    internal class RemoteDataObject : RemoteDataObject<object>
    {
        private RemoteDataObject(RemotingDestination destination, RemotingDataType dataType, Guid runspacePoolId, Guid powerShellId, object data) : base(destination, dataType, runspacePoolId, powerShellId, data)
        {
        }

        internal static RemoteDataObject CreateFrom(RemotingDestination destination, RemotingDataType dataType, Guid runspacePoolId, Guid powerShellId, object data)
        {
            return new RemoteDataObject(destination, dataType, runspacePoolId, powerShellId, data);
        }
    }
}

