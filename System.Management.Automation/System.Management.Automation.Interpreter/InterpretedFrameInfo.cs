namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal struct InterpretedFrameInfo
    {
        public readonly string MethodName;
        public readonly System.Management.Automation.Interpreter.DebugInfo DebugInfo;
        public InterpretedFrameInfo(string methodName, System.Management.Automation.Interpreter.DebugInfo info)
        {
            this.MethodName = methodName;
            this.DebugInfo = info;
        }

        public override string ToString()
        {
            return (this.MethodName + ((this.DebugInfo != null) ? (": " + this.DebugInfo) : null));
        }
    }
}

