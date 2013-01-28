namespace System.Spatial
{
    using System;

    internal class SpatialPipeline
    {
        private System.Spatial.GeographyPipeline geographyPipeline;
        private System.Spatial.GeometryPipeline geometryPipeline;
        private SpatialPipeline startingLink;

        public SpatialPipeline()
        {
            this.startingLink = this;
        }

        public SpatialPipeline(System.Spatial.GeographyPipeline geographyPipeline, System.Spatial.GeometryPipeline geometryPipeline)
        {
            this.geographyPipeline = geographyPipeline;
            this.geometryPipeline = geometryPipeline;
            this.startingLink = this;
        }

        public virtual SpatialPipeline ChainTo(SpatialPipeline destination)
        {
            throw new NotImplementedException();
        }

        public static implicit operator System.Spatial.GeographyPipeline(SpatialPipeline spatialPipeline)
        {
            if (spatialPipeline != null)
            {
                return spatialPipeline.GeographyPipeline;
            }
            return null;
        }

        public static implicit operator System.Spatial.GeometryPipeline(SpatialPipeline spatialPipeline)
        {
            if (spatialPipeline != null)
            {
                return spatialPipeline.GeometryPipeline;
            }
            return null;
        }

        public virtual System.Spatial.GeographyPipeline GeographyPipeline
        {
            get
            {
                return this.geographyPipeline;
            }
        }

        public virtual System.Spatial.GeometryPipeline GeometryPipeline
        {
            get
            {
                return this.geometryPipeline;
            }
        }

        public SpatialPipeline StartingLink
        {
            get
            {
                return this.startingLink;
            }
            set
            {
                this.startingLink = value;
            }
        }

		public void Reset ()
		{
			if (this.geographyPipeline != null) {
				geographyPipeline.Reset ();
			}
			if (this.geometryPipeline != null) {
				this.geometryPipeline.Reset ();
			}
		}
    }
}

