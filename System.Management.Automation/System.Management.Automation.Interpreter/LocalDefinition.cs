namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Linq.Expressions;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct LocalDefinition
    {
        private readonly int _index;
        private readonly ParameterExpression _parameter;
        internal LocalDefinition(int localIndex, ParameterExpression parameter)
        {
            this._index = localIndex;
            this._parameter = parameter;
        }

        public int Index
        {
            get
            {
                return this._index;
            }
        }
        public ParameterExpression Parameter
        {
            get
            {
                return this._parameter;
            }
        }
        public override bool Equals(object obj)
        {
            if (!(obj is LocalDefinition))
            {
                return false;
            }
            LocalDefinition definition = (LocalDefinition) obj;
            return ((definition.Index == this.Index) && (definition.Parameter == this.Parameter));
        }

        public override int GetHashCode()
        {
            if (this._parameter == null)
            {
                return 0;
            }
            return (this._parameter.GetHashCode() ^ this._index.GetHashCode());
        }

        public static bool operator ==(LocalDefinition self, LocalDefinition other)
        {
            return ((self.Index == other.Index) && (self.Parameter == other.Parameter));
        }

        public static bool operator !=(LocalDefinition self, LocalDefinition other)
        {
            if (self.Index == other.Index)
            {
                return (self.Parameter != other.Parameter);
            }
            return true;
        }
    }
}

