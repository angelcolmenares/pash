namespace System.Spatial
{
    using System;
    using System.Collections.Generic;

    internal abstract class SpatialFormatter<TReaderStream, TWriterStream>
    {
        private readonly SpatialImplementation creator;

        protected SpatialFormatter(SpatialImplementation creator)
        {
            Util.CheckArgumentNull(creator, "creator");
            this.creator = creator;
        }

        public abstract SpatialPipeline CreateWriter(TWriterStream writerStream);
        protected KeyValuePair<SpatialPipeline, IShapeProvider> MakeValidatingBuilder()
        {
            SpatialBuilder destination = this.creator.CreateBuilder();
            SpatialPipeline key = this.creator.CreateValidator();
            key.ChainTo(destination);
            return new KeyValuePair<SpatialPipeline, IShapeProvider>(key, destination);
        }

        public TResult Read<TResult>(TReaderStream input) where TResult: class, ISpatial
        {
            KeyValuePair<SpatialPipeline, IShapeProvider> pair = this.MakeValidatingBuilder();
            IShapeProvider provider = pair.Value;
            this.Read<TResult>(input, pair.Key);
            if (typeof(Geometry).IsAssignableFrom(typeof(TResult)))
            {
                return (TResult) ((object)provider.ConstructedGeometry);
            }
			return (TResult) ((object)provider.ConstructedGeography);
        }

        public void Read<TResult>(TReaderStream input, SpatialPipeline pipeline) where TResult: class, ISpatial
        {
            if (typeof(Geometry).IsAssignableFrom(typeof(TResult)))
            {
                this.ReadGeometry(input, pipeline);
            }
            else
            {
                this.ReadGeography(input, pipeline);
            }
        }

        protected abstract void ReadGeography(TReaderStream readerStream, SpatialPipeline pipeline);
        protected abstract void ReadGeometry(TReaderStream readerStream, SpatialPipeline pipeline);
        public void Write(ISpatial spatial, TWriterStream writerStream)
        {
            SpatialPipeline destination = this.CreateWriter(writerStream);
            spatial.SendTo(destination);
        }
    }
}

