namespace System.Management.Automation.PerformanceData
{
    using System;
    using System.Diagnostics.PerformanceData;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct CounterInfo
    {
        private string _Name;
        private int _Id;
        private CounterType _Type;
        public CounterInfo(int id, CounterType type, string name)
        {
            this._Id = id;
            this._Type = type;
            this._Name = name;
        }

        public CounterInfo(int id, CounterType type)
        {
            this._Id = id;
            this._Type = type;
            this._Name = null;
        }

        public string Name
        {
            get
            {
                return this._Name;
            }
        }
        public int Id
        {
            get
            {
                return this._Id;
            }
        }
        public CounterType Type
        {
            get
            {
                return this._Type;
            }
        }
    }
}

