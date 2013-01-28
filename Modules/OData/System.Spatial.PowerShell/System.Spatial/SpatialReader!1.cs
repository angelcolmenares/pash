namespace System.Spatial
{
    using System;
    using System.Runtime.CompilerServices;

    internal abstract class SpatialReader<TSource>
    {
        protected SpatialReader(SpatialPipeline destination)
        {
            Util.CheckArgumentNull(destination, "destination");
            this.Destination = destination;
        }

        public void ReadGeography(TSource input)
        {
            Util.CheckArgumentNull(input, "input");
            try
            {
                this.ReadGeographyImplementation(input);
            }
            catch (Exception exception)
            {
                if (Util.IsCatchableExceptionType(exception))
                {
                    throw new ParseErrorException(exception.Message, exception);
                }
                throw;
            }
        }

        protected abstract void ReadGeographyImplementation(TSource input);
        public void ReadGeometry(TSource input)
        {
            Util.CheckArgumentNull(input, "input");
            try
            {
                this.ReadGeometryImplementation(input);
            }
            catch (Exception exception)
            {
                if (Util.IsCatchableExceptionType(exception))
                {
                    throw new ParseErrorException(exception.Message, exception);
                }
                throw;
            }
        }

        protected abstract void ReadGeometryImplementation(TSource input);

        public virtual void Reset()
        {
            this.Destination.Reset();
            this.Destination.Reset();
        }

        protected SpatialPipeline Destination { get; set; }
    }
}

