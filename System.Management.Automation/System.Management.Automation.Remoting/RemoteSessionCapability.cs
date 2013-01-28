namespace System.Management.Automation.Remoting
{
    using System;
    using System.IO;
    using System.Management.Automation;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security;

    internal class RemoteSessionCapability
    {
        private Version _protocolVersion;
        private Version _psversion;
        private System.Management.Automation.RemotingDestination _remotingDestination;
        private Version _serversion;
        private System.TimeZone _timeZone;
        private static byte[] _timeZoneInByteFormat;

        internal RemoteSessionCapability(System.Management.Automation.RemotingDestination remotingDestination)
        {
            this._protocolVersion = RemotingConstants.ProtocolVersion;
            this._psversion = PSVersionInfo.PSVersion;
            this._serversion = PSVersionInfo.SerializationVersion;
            this._remotingDestination = remotingDestination;
        }

        internal RemoteSessionCapability(System.Management.Automation.RemotingDestination remotingDestination, Version protocolVersion, Version psVersion, Version serVersion)
        {
            this._protocolVersion = protocolVersion;
            this._psversion = psVersion;
            this._serversion = serVersion;
            this._remotingDestination = remotingDestination;
        }

        internal static System.TimeZone ConvertFromByteToTimeZone(byte[] data)
        {
            System.TimeZone result = null;
            if (data != null)
            {
                try
                {
                    MemoryStream serializationStream = new MemoryStream(data);
                    BinaryFormatter formatter = new BinaryFormatter();
                    LanguagePrimitives.TryConvertTo<System.TimeZone>(formatter.Deserialize(serializationStream), out result);
                    return result;
                }
                catch (ArgumentNullException)
                {
                }
                catch (SerializationException)
                {
                }
                catch (SecurityException)
                {
                }
            }
            return result;
        }

        internal static RemoteSessionCapability CreateClientCapability()
        {
            return new RemoteSessionCapability(System.Management.Automation.RemotingDestination.InvalidDestination | System.Management.Automation.RemotingDestination.Server);
        }

        internal static RemoteSessionCapability CreateServerCapability()
        {
            return new RemoteSessionCapability(System.Management.Automation.RemotingDestination.InvalidDestination | System.Management.Automation.RemotingDestination.Client);
        }

        internal static byte[] GetCurrentTimeZoneInByteFormat()
        {
            if (_timeZoneInByteFormat == null)
            {
                Exception exception = null;
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    using (MemoryStream stream = new MemoryStream())
                    {
                        formatter.Serialize(stream, System.TimeZone.CurrentTimeZone);
                        stream.Seek(0L, SeekOrigin.Begin);
                        byte[] buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, (int) stream.Length);
                        _timeZoneInByteFormat = buffer;
                    }
                }
                catch (ArgumentNullException exception2)
                {
                    exception = exception2;
                }
                catch (SerializationException exception3)
                {
                    exception = exception3;
                }
                catch (SecurityException exception4)
                {
                    exception = exception4;
                }
                if (exception != null)
                {
                    _timeZoneInByteFormat = new byte[0];
                }
            }
            return _timeZoneInByteFormat;
        }

        internal Version ProtocolVersion
        {
            get
            {
                return this._protocolVersion;
            }
            set
            {
                this._protocolVersion = value;
            }
        }

        internal Version PSVersion
        {
            get
            {
                return this._psversion;
            }
        }

        internal System.Management.Automation.RemotingDestination RemotingDestination
        {
            get
            {
                return this._remotingDestination;
            }
        }

        internal Version SerializationVersion
        {
            get
            {
                return this._serversion;
            }
        }

        internal System.TimeZone TimeZone
        {
            get
            {
                return this._timeZone;
            }
            set
            {
                this._timeZone = value;
            }
        }
    }
}

