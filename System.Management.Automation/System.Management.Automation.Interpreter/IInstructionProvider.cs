namespace System.Management.Automation.Interpreter
{
    using System;

    internal interface IInstructionProvider
    {
        void AddInstructions(LightCompiler compiler);
    }
}

