namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections.ObjectModel;
    using System.Spatial;

    internal class GeographyCollectionImplementation : GeographyCollection
    {
        private Geography[] geographyArray;

        internal GeographyCollectionImplementation(SpatialImplementation creator, params Geography[] geography) : this(CoordinateSystem.DefaultGeography, creator, geography)
        {
        }

        internal GeographyCollectionImplementation(CoordinateSystem coordinateSystem, SpatialImplementation creator, params Geography[] geography) : base(coordinateSystem, creator)
        {
            this.geographyArray = geography ?? new Geography[0];
        }

        public override void SendTo(GeographyPipeline pipeline)
        {
            base.SendTo(pipeline);
            pipeline.BeginGeography(SpatialType.Collection);
            for (int i = 0; i < this.geographyArray.Length; i++)
            {
                this.geographyArray[i].SendTo(pipeline);
            }
            pipeline.EndGeography();
        }

        public override ReadOnlyCollection<Geography> Geographies
        {
            get
            {
                return this.geographyArray.AsReadOnly<Geography>();
            }
        }

        public override bool IsEmpty
        {
            get
            {
                return (this.geographyArray.Length == 0);
            }
        }
    }
}

