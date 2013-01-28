namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class LightLambdaCompileEventArgs : EventArgs
    {
        internal LightLambdaCompileEventArgs(Delegate compiled)
        {
            this.Compiled = compiled;
        }

        public Delegate Compiled { get; private set; }
    }
}

