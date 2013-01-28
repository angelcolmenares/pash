namespace System.Spatial
{
    using System;

    internal abstract class SpatialOperations
    {
        protected SpatialOperations()
        {
        }

        public virtual double Distance(Geography operand1, Geography operand2)
        {
            throw new NotImplementedException();
        }

        public virtual double Distance(Geometry operand1, Geometry operand2)
        {
            throw new NotImplementedException();
        }
    }
}

