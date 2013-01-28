namespace System.Management.Automation.Interpreter
{
    using System;

    internal sealed class InstructionFactory<T> : InstructionFactory
    {
        private Instruction _defaultValue;
        private Instruction _getArrayItem;
        private Instruction _newArray;
        private Instruction _setArrayItem;
        private Instruction _typeAs;
        private Instruction _typeIs;
        public static readonly InstructionFactory Factory;

        static InstructionFactory()
        {
            InstructionFactory<T>.Factory = new InstructionFactory<T>();
        }

        private InstructionFactory()
        {
        }

        protected internal override Instruction DefaultValue()
        {
            return (this._defaultValue ?? (this._defaultValue = new DefaultValueInstruction<T>()));
        }

        protected internal override Instruction GetArrayItem()
        {
            return (this._getArrayItem ?? (this._getArrayItem = new GetArrayItemInstruction<T>()));
        }

        protected internal override Instruction NewArray()
        {
            return (this._newArray ?? (this._newArray = new NewArrayInstruction<T>()));
        }

        protected internal override Instruction NewArrayInit(int elementCount)
        {
            return new NewArrayInitInstruction<T>(elementCount);
        }

        protected internal override Instruction SetArrayItem()
        {
            return (this._setArrayItem ?? (this._setArrayItem = new SetArrayItemInstruction<T>()));
        }

        protected internal override Instruction TypeAs()
        {
            return (this._typeAs ?? (this._typeAs = new TypeAsInstruction<T>()));
        }

        protected internal override Instruction TypeIs()
        {
            return (this._typeIs ?? (this._typeIs = new TypeIsInstruction<T>()));
        }
    }
}

