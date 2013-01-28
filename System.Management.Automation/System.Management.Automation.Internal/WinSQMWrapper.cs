namespace System.Management.Automation.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Management.Automation;
    using System.Runtime.InteropServices;

    internal static class WinSQMWrapper
    {
        private static readonly IntPtr HGLOBALSESSION = IntPtr.Zero;
		private static Guid WINDOWS_SQM_GLOBALSESSION = new Guid("95baba28-ed26-49c9-b74f-93b170e1b849"); // new Guid("{ 0x95baba28, 0xed26, 0x49c9, { 0xb7, 0x4f, 0x93, 0xb1, 0x70, 0xe1, 0xb8, 0x49 }}");

        private static void FireWinSQMEvent(EventDescriptor eventDescriptor, Dictionary<int, int> dataToWrite)
        {
            Guid empty = Guid.Empty;
            if (WinSqmEventEnabled(ref eventDescriptor, ref empty))
            {
                IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(WINDOWS_SQM_GLOBALSESSION));
                IntPtr ptr2 = Marshal.AllocHGlobal(4);
                IntPtr ptr3 = Marshal.AllocHGlobal(4);
                try
                {
                    Marshal.StructureToPtr(WINDOWS_SQM_GLOBALSESSION, ptr, true);
                    foreach (int num in dataToWrite.Keys)
                    {
                        int structure = dataToWrite[num];
                        Marshal.StructureToPtr(num, ptr2, true);
                        Marshal.StructureToPtr(structure, ptr3, true);
                        FireWinSQMEvent(eventDescriptor, ptr, ptr2, ptr3);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                    Marshal.FreeHGlobal(ptr2);
                    Marshal.FreeHGlobal(ptr3);
                }
            }
        }

        private static void FireWinSQMEvent(EventDescriptor eventDescriptor, int dataPointID, int dataPointValue)
        {
            Guid empty = Guid.Empty;
            if (WinSqmEventEnabled(ref eventDescriptor, ref empty))
            {
                IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(WINDOWS_SQM_GLOBALSESSION));
                IntPtr ptr2 = Marshal.AllocHGlobal(4);
                IntPtr ptr3 = Marshal.AllocHGlobal(4);
                try
                {
                    Marshal.StructureToPtr(WINDOWS_SQM_GLOBALSESSION, ptr, true);
                    Marshal.StructureToPtr(dataPointID, ptr2, true);
                    Marshal.StructureToPtr(dataPointValue, ptr3, true);
                    FireWinSQMEvent(eventDescriptor, ptr, ptr2, ptr3);
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                    Marshal.FreeHGlobal(ptr2);
                    Marshal.FreeHGlobal(ptr3);
                }
            }
        }

        private static void FireWinSQMEvent(EventDescriptor eventDescriptor, IntPtr sessionHandle, IntPtr dataPointIDHandle, IntPtr dataValueHandle)
        {
            EventDataDescriptor descriptor = new EventDataDescriptor(sessionHandle, Marshal.SizeOf(WINDOWS_SQM_GLOBALSESSION));
            EventDataDescriptor descriptor2 = new EventDataDescriptor(dataPointIDHandle, 4);
            EventDataDescriptor descriptor3 = new EventDataDescriptor(dataValueHandle, 4);
            EventDataDescriptor[] userData = new EventDataDescriptor[] { descriptor, descriptor2, descriptor3 };
            WinSqmEventWrite(ref eventDescriptor, userData.Length, userData);
        }

        public static bool IsWinSqmOptedIn()
        {
            try
            {
                return WinSqmIsOptedIn();
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
            }
            return false;
        }

        public static void WinSqmAddToStream(int dataPointID, string stringData)
        {
            SqmStreamEntry entry = SqmStreamEntry.CreateStringSqmStreamEntry(stringData);
            SqmStreamEntry[] streamEntries = new SqmStreamEntry[] { entry };
            WinSqmAddToStream(HGLOBALSESSION, dataPointID, streamEntries.Length, streamEntries);
        }

        public static void WinSqmAddToStream(int dataPointID, string stringData, int numericalData)
        {
            SqmStreamEntry entry = SqmStreamEntry.CreateStringSqmStreamEntry(stringData);
            SqmStreamEntry entry2 = SqmStreamEntry.CreateStringSqmStreamEntry(numericalData.ToString(CultureInfo.InvariantCulture));
            SqmStreamEntry[] streamEntries = new SqmStreamEntry[] { entry, entry2 };
            WinSqmAddToStream(HGLOBALSESSION, dataPointID, streamEntries.Length, streamEntries);
        }

        [DllImport("ntdll.dll")]
        private static extern void WinSqmAddToStream(IntPtr sessionGuid, int dataPointID, int sqmStreamEntries, SqmStreamEntry[] streamEntries);
        [DllImport("ntdll.dll")]
        private static extern bool WinSqmEventEnabled(ref EventDescriptor eventDescriptor, ref Guid guid);
        [DllImport("ntdll.dll")]
        private static extern int WinSqmEventWrite(ref EventDescriptor eventDescriptor, int userDataCount, EventDataDescriptor[] userData);
        public static void WinSqmIncrement(Dictionary<int, int> dataToWrite)
        {
            EventDescriptor eventDescriptor = new EventDescriptor(6, 0, 0, 4, 2, 0, 0x8000000000000L);
            FireWinSQMEvent(eventDescriptor, dataToWrite);
        }

        public static void WinSqmIncrement(int dataPointID, int incAmount)
        {
            EventDescriptor eventDescriptor = new EventDescriptor(6, 0, 0, 4, 2, 0, 0x8000000000000L);
            FireWinSQMEvent(eventDescriptor, dataPointID, incAmount);
        }
		/*
        [DllImport("ntdll.dll")]
        private static extern bool WinSqmIsOptedIn();
		*/

		private static bool WinSqmIsOptedIn()
		{
			return true;
		}


		public static void WinSqmSet(int dataPointID, int dataPointValue)
        {
            EventDescriptor eventDescriptor = new EventDescriptor(5, 0, 0, 4, 0, 0, 0x8000000000000L);
            FireWinSQMEvent(eventDescriptor, dataPointID, dataPointValue);
        }

        [StructLayout(LayoutKind.Explicit, Size=0x10)]
        private struct EventDataDescriptor
        {
            [FieldOffset(0)]
            private IntPtr dataPointer;
            [FieldOffset(12)]
            private int reserved;
            [FieldOffset(8)]
            private int size;

            internal EventDataDescriptor(IntPtr dp, int sz)
            {
                this.reserved = 0;
                this.size = sz;
                this.dataPointer = dp;
            }
        }

        [StructLayout(LayoutKind.Explicit, Size=0x10)]
        private struct EventDescriptor
        {
            [FieldOffset(3)]
            private byte channel;
            [FieldOffset(0)]
            private ushort id;
            [FieldOffset(8)]
            private ulong keywords;
            [FieldOffset(4)]
            private byte level;
            [FieldOffset(5)]
            private byte opcode;
            [FieldOffset(6)]
            private ushort task;
            [FieldOffset(2)]
            private byte version;

            internal EventDescriptor(ushort eventId, byte eventVersion, byte eventChannel, byte eventLevel, byte eventOpcode, ushort eventTask, ulong eventKeywords)
            {
                this.id = eventId;
                this.version = eventVersion;
                this.channel = eventChannel;
                this.level = eventLevel;
                this.opcode = eventOpcode;
                this.task = eventTask;
                this.keywords = eventKeywords;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SqmStreamEntry
        {
            private int type;
            [MarshalAs(UnmanagedType.LPWStr)]
            private string stringValue;
            internal static WinSQMWrapper.SqmStreamEntry CreateStringSqmStreamEntry(string value)
            {
                return new WinSQMWrapper.SqmStreamEntry { type = 2, stringValue = value };
            }
        }
    }
}

