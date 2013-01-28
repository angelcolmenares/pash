namespace System.Management.Automation.Remoting
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;

    internal class RemoteDataObject<T>
    {
        private T data;
        private RemotingDataType dataType;
        private const int dataTypeOffset = 4;
        private RemotingDestination destination;
        private const int destinationOffset = 0;
        private const int headerLength = 40;
        private Guid powerShellId;
        private const int PowerShellMask = 0x41000;
        private const int psIdOffset = 0x18;
        private const int rsPoolIdOffset = 8;
        private Guid runspacePoolId;
        private const int RunspacePoolMask = 0x21000;
        private const int SessionMask = 0x10000;

        protected RemoteDataObject(RemotingDestination destination, RemotingDataType dataType, Guid runspacePoolId, Guid powerShellId, T data)
        {
            this.destination = destination;
            this.dataType = dataType;
            this.runspacePoolId = runspacePoolId;
            this.powerShellId = powerShellId;
            this.data = data;
        }

        internal static RemoteDataObject<T> CreateFrom(Stream serializedDataStream, Fragmentor defragmentor)
        {
            if ((serializedDataStream.Length - serializedDataStream.Position) < 40L)
            {
                PSRemotingTransportException exception = new PSRemotingTransportException(PSRemotingErrorId.NotEnoughHeaderForRemoteDataObject, RemotingErrorIdStrings.NotEnoughHeaderForRemoteDataObject, new object[] { 0x3d });
                throw exception;
            }
            RemotingDestination destination = (RemotingDestination) RemoteDataObject<T>.DeserializeUInt(serializedDataStream);
            RemotingDataType dataType = (RemotingDataType) RemoteDataObject<T>.DeserializeUInt(serializedDataStream);
            Guid runspacePoolId = RemoteDataObject<T>.DeserializeGuid(serializedDataStream);
            Guid powerShellId = RemoteDataObject<T>.DeserializeGuid(serializedDataStream);
            object valueToConvert = null;
            if ((serializedDataStream.Length - 40L) > 0L)
            {
                valueToConvert = defragmentor.DeserializeToPSObject(serializedDataStream);
            }
            return new RemoteDataObject<T>(destination, dataType, runspacePoolId, powerShellId, (T) LanguagePrimitives.ConvertTo(valueToConvert, typeof(T), CultureInfo.CurrentCulture));
        }

        internal static RemoteDataObject<T> CreateFrom(RemotingDestination destination, RemotingDataType dataType, Guid runspacePoolId, Guid powerShellId, T data)
        {
            return new RemoteDataObject<T>(destination, dataType, runspacePoolId, powerShellId, data);
        }

        private static Guid DeserializeGuid(Stream serializedDataStream)
        {
            byte[] b = new byte[0x10];
            for (int i = 0; i < 0x10; i++)
            {
                b[i] = (byte) serializedDataStream.ReadByte();
            }
            return new Guid(b);
        }

        private static int DeserializeUInt(Stream serializedDataStream)
        {
            int num = 0;
            num |= (int) (serializedDataStream.ReadByte() & 0xff);
            num |= (int) ((serializedDataStream.ReadByte() << 8) & 0xff00);
            num |= (int) ((serializedDataStream.ReadByte() << 0x10) & 0xff0000);
            return (num | ((int) ((serializedDataStream.ReadByte() << 0x18) & -16777216)));
        }

        internal virtual void Serialize(Stream streamToWriteTo, Fragmentor fragmentor)
        {
            this.SerializeHeader(streamToWriteTo);
            if (this.data != null)
            {
                fragmentor.SerializeToBytes(this.data, streamToWriteTo);
            }
        }

        private void SerializeGuid(Guid guid, Stream streamToWriteTo)
        {
            byte[] buffer = guid.ToByteArray();
            streamToWriteTo.Write(buffer, 0, buffer.Length);
        }

        private void SerializeHeader(Stream streamToWriteTo)
        {
            this.SerializeUInt((int) this.Destination, streamToWriteTo);
            this.SerializeUInt((int) this.DataType, streamToWriteTo);
            this.SerializeGuid(this.runspacePoolId, streamToWriteTo);
            this.SerializeGuid(this.powerShellId, streamToWriteTo);
        }

        private void SerializeUInt(int data, Stream streamToWriteTo)
        {
            byte[] buffer = new byte[4];
            int num = 0;
            buffer[num++] = (byte) (data & 0xff);
            buffer[num++] = (byte) ((data >> 8) & 0xff);
            buffer[num++] = (byte) ((data >> 0x10) & 0xff);
            buffer[num++] = (byte) ((data >> 0x18) & 0xff);
            streamToWriteTo.Write(buffer, 0, 4);
        }

        internal T Data
        {
            get
            {
                return this.data;
            }
        }

        internal RemotingDataType DataType
        {
            get
            {
                return this.dataType;
            }
        }

        internal RemotingDestination Destination
        {
            get
            {
                return this.destination;
            }
        }

        internal Guid PowerShellId
        {
            get
            {
                return this.powerShellId;
            }
        }

        internal Guid RunspacePoolId
        {
            get
            {
                return this.runspacePoolId;
            }
        }

        internal RemotingTargetInterface TargetInterface
        {
            get
            {
                int dataType = (int) this.dataType;
                if ((dataType & 0x41000) == 0x41000)
                {
                    return RemotingTargetInterface.PowerShell;
                }
                if ((dataType & 0x21000) == 0x21000)
                {
                    return RemotingTargetInterface.RunspacePool;
                }
                if ((dataType & 0x10000) == 0x10000)
                {
                    return RemotingTargetInterface.Session;
                }
                return RemotingTargetInterface.InvalidTargetInterface;
            }
        }
    }
}

