namespace System.Management.Automation
{
    using System;

    internal class ScriptRequiresSyntaxException : ScriptRequiresException
    {
        internal ScriptRequiresSyntaxException(string message) : base(message)
        {
        }
    }
}

