namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Reflection;

    internal sealed class NewInstruction : Instruction
    {
        private readonly int _argCount;
        private readonly ConstructorInfo _constructor;

        public NewInstruction(ConstructorInfo constructor)
        {
            this._constructor = constructor;
            this._argCount = constructor.GetParameters().Length;
        }

        public override int Run(InterpretedFrame frame)
        {
            object obj2;
            object[] parameters = new object[this._argCount];
            for (int i = this._argCount - 1; i >= 0; i--)
            {
                parameters[i] = frame.Pop();
            }
            try
            {
                obj2 = this._constructor.Invoke(parameters);
            }
            catch (TargetInvocationException exception)
            {
                ExceptionHelpers.UpdateForRethrow(exception.InnerException);
                throw exception.InnerException;
            }
            frame.Push(obj2);
            return 1;
        }

        public override string ToString()
        {
            return string.Concat(new object[] { "New ", this._constructor.DeclaringType.Name, "(", this._constructor, ")" });
        }

        public override int ConsumedStack
        {
            get
            {
                return this._argCount;
            }
        }

        public override int ProducedStack
        {
            get
            {
                return 1;
            }
        }
    }
}

