namespace System.Spatial
{
    using Microsoft.Data.Spatial;
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal static class GeographyOperationsExtensions
    {
        public static double? Distance(this Geography operand1, Geography operand2)
        {
            return OperationsFor(new Geography[] { operand1, operand2 }).IfValidReturningNullable<SpatialOperations, double>(ops => ops.Distance(operand1, operand2));
        }

        private static SpatialOperations OperationsFor(params Geography[] operands)
        {
            if (operands.Any<Geography>(operand => operand == null))
            {
                return null;
            }
            return operands[0].Creator.VerifyAndGetNonNullOperations();
        }
    }
}

