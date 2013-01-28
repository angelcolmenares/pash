namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class RuntimeVariables : IRuntimeVariables
    {
        private readonly IStrongBox[] _boxes;

        private RuntimeVariables(IStrongBox[] boxes)
        {
            this._boxes = boxes;
        }

        internal static IRuntimeVariables Create(IStrongBox[] boxes)
        {
            return new System.Management.Automation.Interpreter.RuntimeVariables(boxes);
        }

        int IRuntimeVariables.Count
        {
            get
            {
                return this._boxes.Length;
            }
        }

        object IRuntimeVariables.this[int index]
        {
            get
            {
                return this._boxes[index].Value;
            }
            set
            {
                this._boxes[index].Value = value;
            }
        }
    }
}

