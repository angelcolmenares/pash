namespace Microsoft.PowerShell.Commands.Internal.Format
{
    internal sealed class FrameToken : FormatToken
    {
        internal FrameInfoDefinition frameInfoDefinition = new FrameInfoDefinition();
        internal ComplexControlItemDefinition itemDefinition = new ComplexControlItemDefinition();
    }
}

