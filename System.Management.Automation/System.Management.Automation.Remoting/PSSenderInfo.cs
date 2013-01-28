namespace System.Management.Automation.Remoting
{
    using Microsoft.PowerShell;
    using System;
    using System.Management.Automation;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class PSSenderInfo : ISerializable
    {
        private PSPrimitiveDictionary applicationArguments;
        private TimeZone clientTimeZone;
        private string connectionString;
        private PSPrincipal userPrinicpal;

        public PSSenderInfo(PSPrincipal userPrincipal, string httpUrl)
        {
            this.userPrinicpal = userPrincipal;
            this.connectionString = httpUrl;
        }

        private PSSenderInfo(SerializationInfo info, StreamingContext context)
        {
            if (info != null)
            {
                string source = null;
                try
                {
                    source = info.GetValue("CliXml", typeof(string)) as string;
                }
                catch (Exception)
                {
                    return;
                }
                if (source != null)
                {
                    try
                    {
                        PSSenderInfo info2 = DeserializingTypeConverter.RehydratePSSenderInfo(PSObject.AsPSObject(PSSerializer.Deserialize(source)));
                        this.userPrinicpal = info2.userPrinicpal;
                        this.clientTimeZone = info2.ClientTimeZone;
                        this.connectionString = info2.connectionString;
                        this.applicationArguments = info2.applicationArguments;
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            PSObject.AsPSObject(this).GetObjectData(info, context);
        }

        public PSPrimitiveDictionary ApplicationArguments
        {
            get
            {
                return this.applicationArguments;
            }
            internal set
            {
                this.applicationArguments = value;
            }
        }

        public TimeZone ClientTimeZone
        {
            get
            {
                return this.clientTimeZone;
            }
            internal set
            {
                this.clientTimeZone = value;
            }
        }

        public string ConnectionString
        {
            get
            {
                return this.connectionString;
            }
        }

        public PSPrincipal UserInfo
        {
            get
            {
                return this.userPrinicpal;
            }
        }
    }
}

