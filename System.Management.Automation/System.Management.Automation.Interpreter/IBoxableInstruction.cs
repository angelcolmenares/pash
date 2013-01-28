namespace System.Management.Automation.Interpreter
{
    using System;

    internal interface IBoxableInstruction
    {
        Instruction BoxIfIndexMatches(int index);
    }
}

