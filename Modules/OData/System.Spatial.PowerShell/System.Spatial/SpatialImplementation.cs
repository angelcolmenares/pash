using System;
using Microsoft.Data.Spatial;

namespace System.Spatial
{

    internal abstract class SpatialImplementation
    {
        private static SpatialImplementation spatialImplementation = new DataServicesSpatialImplementation();

        protected SpatialImplementation()
        {
        }

        public abstract SpatialBuilder CreateBuilder();
        public abstract GeoJsonObjectFormatter CreateGeoJsonObjectFormatter();
        public abstract GmlFormatter CreateGmlFormatter();
        public abstract SpatialPipeline CreateValidator();
        public abstract WellKnownTextSqlFormatter CreateWellKnownTextSqlFormatter();
        public abstract WellKnownTextSqlFormatter CreateWellKnownTextSqlFormatter(bool allowOnlyTwoDimensions);
        internal SpatialOperations VerifyAndGetNonNullOperations()
        {
            SpatialOperations operations = this.Operations;
            if (operations == null)
            {
                throw new NotImplementedException(Strings.SpatialImplementation_NoRegisteredOperations);
            }
            return operations;
        }

        public static SpatialImplementation CurrentImplementation
        {
            get
            {
                return spatialImplementation;
            }
            internal set
            {
                spatialImplementation = value;
            }
        }

        public abstract SpatialOperations Operations { get; set; }
    }
}

