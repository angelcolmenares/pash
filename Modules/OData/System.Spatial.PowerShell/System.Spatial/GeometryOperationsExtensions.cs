namespace System.Spatial
{
    using Microsoft.Data.Spatial;
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal static class GeometryOperationsExtensions
    {
        public static double? Distance(this Geometry operand1, Geometry operand2)
        {
            return OperationsFor(new Geometry[] { operand1, operand2 }).IfValidReturningNullable<SpatialOperations, double>(ops => ops.Distance(operand1, operand2));
        }

        private static SpatialOperations OperationsFor(params Geometry[] operands)
        {
            if (operands.Any<Geometry>(operand => operand == null))
            {
                return null;
            }
            return operands[0].Creator.VerifyAndGetNonNullOperations();
        }
    }
}

