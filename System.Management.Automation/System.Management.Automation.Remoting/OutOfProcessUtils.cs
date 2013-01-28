namespace System.Management.Automation.Remoting
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation.Tracing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal static class OutOfProcessUtils
    {
        internal const int EXITCODE_UNHANDLED_EXCEPTION = 0xfa0;
        internal const string PS_OUT_OF_PROC_CLOSE_ACK_TAG = "CloseAck";
        internal const string PS_OUT_OF_PROC_CLOSE_TAG = "Close";
        internal const string PS_OUT_OF_PROC_COMMAND_ACK_TAG = "CommandAck";
        internal const string PS_OUT_OF_PROC_COMMAND_TAG = "Command";
        internal const string PS_OUT_OF_PROC_DATA_ACK_TAG = "DataAck";
        internal const string PS_OUT_OF_PROC_DATA_TAG = "Data";
        internal const string PS_OUT_OF_PROC_PSGUID_ATTRIBUTE = "PSGuid";
        internal const string PS_OUT_OF_PROC_SIGNAL_ACK_TAG = "SignalAck";
        internal const string PS_OUT_OF_PROC_SIGNAL_TAG = "Signal";
        internal const string PS_OUT_OF_PROC_STREAM_ATTRIBUTE = "Stream";
        internal static System.Xml.XmlReaderSettings XmlReaderSettings = new System.Xml.XmlReaderSettings();

        static OutOfProcessUtils()
        {
            XmlReaderSettings.CheckCharacters = false;
            XmlReaderSettings.IgnoreComments = true;
            XmlReaderSettings.IgnoreProcessingInstructions = true;
            XmlReaderSettings.XmlResolver = null;
            XmlReaderSettings.ConformanceLevel = ConformanceLevel.Fragment;
        }

        internal static string CreateCloseAckPacket(Guid psGuid)
        {
            return CreatePSGuidPacket("CloseAck", psGuid);
        }

        internal static string CreateClosePacket(Guid psGuid)
        {
            return CreatePSGuidPacket("Close", psGuid);
        }

        internal static string CreateCommandAckPacket(Guid psGuid)
        {
            return CreatePSGuidPacket("CommandAck", psGuid);
        }

        internal static string CreateCommandPacket(Guid psGuid)
        {
            return CreatePSGuidPacket("Command", psGuid);
        }

        internal static string CreateDataAckPacket(Guid psGuid)
        {
            return CreatePSGuidPacket("DataAck", psGuid);
        }

        internal static string CreateDataPacket(byte[] data, DataPriorityType streamType, Guid psGuid)
        {
            return string.Format(CultureInfo.InvariantCulture, "<{0} {1}='{2}' {3}='{4}'>{5}</{0}>", new object[] { "Data", "Stream", streamType.ToString(), "PSGuid", psGuid.ToString(), Convert.ToBase64String(data) });
        }

        private static string CreatePSGuidPacket(string element, Guid psGuid)
        {
            return string.Format(CultureInfo.InvariantCulture, "<{0} {1}='{2}' />", new object[] { element, "PSGuid", psGuid.ToString() });
        }

        internal static string CreateSignalAckPacket(Guid psGuid)
        {
            return CreatePSGuidPacket("SignalAck", psGuid);
        }

        internal static string CreateSignalPacket(Guid psGuid)
        {
            return CreatePSGuidPacket("Signal", psGuid);
        }

        internal static void ProcessData(string data, DataProcessingDelegates callbacks)
        {
            if (!string.IsNullOrEmpty(data))
            {
                XmlReader xmlReader = XmlReader.Create(new StringReader(data), XmlReaderSettings);
                while (xmlReader.Read())
                {
                    switch (xmlReader.NodeType)
                    {
                        case XmlNodeType.Element:
                        {
                            ProcessElement(xmlReader, callbacks);
                            continue;
                        }
                        case XmlNodeType.EndElement:
                        {
                            continue;
                        }
                    }
                    throw new PSRemotingTransportException(PSRemotingErrorId.IPCUnknownNodeType, RemotingErrorIdStrings.IPCUnknownNodeType, new object[] { xmlReader.NodeType.ToString(), XmlNodeType.Element.ToString(), XmlNodeType.EndElement.ToString() });
                }
            }
        }

        private static void ProcessElement(XmlReader xmlReader, DataProcessingDelegates callbacks)
        {
            PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
            switch (xmlReader.LocalName)
            {
                case "Data":
                {
                    if (xmlReader.AttributeCount != 2)
                    {
                        throw new PSRemotingTransportException(PSRemotingErrorId.IPCWrongAttributeCountForDataElement, RemotingErrorIdStrings.IPCWrongAttributeCountForDataElement, new object[] { "Stream", "PSGuid", "Data" });
                    }
                    string attribute = xmlReader.GetAttribute("Stream");
                    string g = xmlReader.GetAttribute("PSGuid");
                    Guid psGuid = new Guid(g);
                    if (!xmlReader.Read())
                    {
                        throw new PSRemotingTransportException(PSRemotingErrorId.IPCInsufficientDataforElement, RemotingErrorIdStrings.IPCInsufficientDataforElement, new object[] { "Data" });
                    }
                    if (xmlReader.NodeType != XmlNodeType.Text)
                    {
                        throw new PSRemotingTransportException(PSRemotingErrorId.IPCOnlyTextExpectedInDataElement, RemotingErrorIdStrings.IPCOnlyTextExpectedInDataElement, new object[] { xmlReader.NodeType, "Data", XmlNodeType.Text });
                    }
                    string s = xmlReader.Value;
                    traceSource.WriteMessage("OutOfProcessUtils.ProcessElement : PS_OUT_OF_PROC_DATA received, psGuid : " + psGuid.ToString());
                    byte[] rawData = Convert.FromBase64String(s);
                    callbacks.DataPacketReceived(rawData, attribute, psGuid);
                    return;
                }
                case "DataAck":
                {
                    if (xmlReader.AttributeCount != 1)
                    {
                        throw new PSRemotingTransportException(PSRemotingErrorId.IPCWrongAttributeCountForElement, RemotingErrorIdStrings.IPCWrongAttributeCountForElement, new object[] { "PSGuid", "DataAck" });
                    }
                    string str4 = xmlReader.GetAttribute("PSGuid");
                    Guid guid2 = new Guid(str4);
                    traceSource.WriteMessage("OutOfProcessUtils.ProcessElement : PS_OUT_OF_PROC_DATA_ACK received, psGuid : " + guid2.ToString());
                    callbacks.DataAckPacketReceived(guid2);
                    return;
                }
                case "Command":
                {
                    if (xmlReader.AttributeCount != 1)
                    {
                        throw new PSRemotingTransportException(PSRemotingErrorId.IPCWrongAttributeCountForElement, RemotingErrorIdStrings.IPCWrongAttributeCountForElement, new object[] { "PSGuid", "Command" });
                    }
                    string str5 = xmlReader.GetAttribute("PSGuid");
                    Guid guid3 = new Guid(str5);
                    traceSource.WriteMessage("OutOfProcessUtils.ProcessElement : PS_OUT_OF_PROC_COMMAND received, psGuid : " + guid3.ToString());
                    callbacks.CommandCreationPacketReceived(guid3);
                    return;
                }
                case "CommandAck":
                {
                    if (xmlReader.AttributeCount != 1)
                    {
                        throw new PSRemotingTransportException(PSRemotingErrorId.IPCWrongAttributeCountForElement, RemotingErrorIdStrings.IPCWrongAttributeCountForElement, new object[] { "PSGuid", "CommandAck" });
                    }
                    string str6 = xmlReader.GetAttribute("PSGuid");
                    Guid guid4 = new Guid(str6);
                    traceSource.WriteMessage("OutOfProcessUtils.ProcessElement : PS_OUT_OF_PROC_COMMAND_ACK received, psGuid : " + guid4.ToString());
                    callbacks.CommandCreationAckReceived(guid4);
                    return;
                }
                case "Close":
                {
                    if (xmlReader.AttributeCount != 1)
                    {
                        throw new PSRemotingTransportException(PSRemotingErrorId.IPCWrongAttributeCountForElement, RemotingErrorIdStrings.IPCWrongAttributeCountForElement, new object[] { "PSGuid", "Close" });
                    }
                    string str7 = xmlReader.GetAttribute("PSGuid");
                    Guid guid5 = new Guid(str7);
                    traceSource.WriteMessage("OutOfProcessUtils.ProcessElement : PS_OUT_OF_PROC_CLOSE received, psGuid : " + guid5.ToString());
                    callbacks.ClosePacketReceived(guid5);
                    return;
                }
                case "CloseAck":
                {
                    if (xmlReader.AttributeCount != 1)
                    {
                        throw new PSRemotingTransportException(PSRemotingErrorId.IPCWrongAttributeCountForElement, RemotingErrorIdStrings.IPCWrongAttributeCountForElement, new object[] { "PSGuid", "CloseAck" });
                    }
                    string str8 = xmlReader.GetAttribute("PSGuid");
                    Guid guid6 = new Guid(str8);
                    traceSource.WriteMessage("OutOfProcessUtils.ProcessElement : PS_OUT_OF_PROC_CLOSE_ACK received, psGuid : " + guid6.ToString());
                    callbacks.CloseAckPacketReceived(guid6);
                    return;
                }
                case "Signal":
                {
                    if (xmlReader.AttributeCount != 1)
                    {
                        throw new PSRemotingTransportException(PSRemotingErrorId.IPCWrongAttributeCountForElement, RemotingErrorIdStrings.IPCWrongAttributeCountForElement, new object[] { "PSGuid", "Signal" });
                    }
                    string str9 = xmlReader.GetAttribute("PSGuid");
                    Guid guid7 = new Guid(str9);
                    traceSource.WriteMessage("OutOfProcessUtils.ProcessElement : PS_OUT_OF_PROC_SIGNAL received, psGuid : " + guid7.ToString());
                    callbacks.SignalPacketReceived(guid7);
                    return;
                }
                case "SignalAck":
                {
                    if (xmlReader.AttributeCount != 1)
                    {
                        throw new PSRemotingTransportException(PSRemotingErrorId.IPCWrongAttributeCountForElement, RemotingErrorIdStrings.IPCWrongAttributeCountForElement, new object[] { "PSGuid", "SignalAck" });
                    }
                    string str10 = xmlReader.GetAttribute("PSGuid");
                    Guid guid8 = new Guid(str10);
                    traceSource.WriteMessage("OutOfProcessUtils.ProcessElement : PS_OUT_OF_PROC_SIGNAL_ACK received, psGuid : " + guid8.ToString());
                    callbacks.SignalAckPacketReceived(guid8);
                    return;
                }
            }
            throw new PSRemotingTransportException(PSRemotingErrorId.IPCUnknownElementReceived, RemotingErrorIdStrings.IPCUnknownElementReceived, new object[] { xmlReader.LocalName });
        }

        internal delegate void CloseAckPacketReceived(Guid psGuid);

        internal delegate void ClosePacketReceived(Guid psGuid);

        internal delegate void CommandCreationAckReceived(Guid psGuid);

        internal delegate void CommandCreationPacketReceived(Guid psGuid);

        internal delegate void DataAckPacketReceived(Guid psGuid);

        internal delegate void DataPacketReceived(byte[] rawData, string stream, Guid psGuid);

        [StructLayout(LayoutKind.Sequential)]
        internal struct DataProcessingDelegates
        {
            internal System.Management.Automation.Remoting.OutOfProcessUtils.DataPacketReceived DataPacketReceived;
            internal System.Management.Automation.Remoting.OutOfProcessUtils.DataAckPacketReceived DataAckPacketReceived;
            internal System.Management.Automation.Remoting.OutOfProcessUtils.CommandCreationPacketReceived CommandCreationPacketReceived;
            internal System.Management.Automation.Remoting.OutOfProcessUtils.CommandCreationAckReceived CommandCreationAckReceived;
            internal System.Management.Automation.Remoting.OutOfProcessUtils.SignalPacketReceived SignalPacketReceived;
            internal System.Management.Automation.Remoting.OutOfProcessUtils.SignalAckPacketReceived SignalAckPacketReceived;
            internal System.Management.Automation.Remoting.OutOfProcessUtils.ClosePacketReceived ClosePacketReceived;
            internal System.Management.Automation.Remoting.OutOfProcessUtils.CloseAckPacketReceived CloseAckPacketReceived;
        }

        internal delegate void SignalAckPacketReceived(Guid psGuid);

        internal delegate void SignalPacketReceived(Guid psGuid);
    }
}

