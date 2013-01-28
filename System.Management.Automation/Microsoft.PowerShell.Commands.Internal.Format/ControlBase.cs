namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal abstract class ControlBase
    {
        protected ControlBase()
        {
        }

        internal virtual ControlBase Copy()
        {
            return this;
        }

        internal static string GetControlShapeName(ControlBase control)
        {
            if (control is TableControlBody)
            {
                return FormatShape.Table.ToString();
            }
            if (control is ListControlBody)
            {
                return FormatShape.List.ToString();
            }
            if (control is WideControlBody)
            {
                return FormatShape.Wide.ToString();
            }
            if (control is ComplexControlBody)
            {
                return FormatShape.Complex.ToString();
            }
            return "";
        }
    }
}

