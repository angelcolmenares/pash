namespace System.Data.Services.Serializers
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Data.Services.Providers;
    using System.Linq;

    internal sealed class ParameterDeserializer : ODataMessageReaderDeserializer
    {
        internal ParameterDeserializer(bool update, IDataService dataService, UpdateTracker tracker, RequestDescription requestDescription) : base(update, dataService, tracker, requestDescription, false)
        {
        }

        protected override ContentFormat GetContentFormat()
        {
            if (ODataUtils.GetReadFormat(base.MessageReader) != ODataFormat.VerboseJson)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.DataServiceException_GeneralError);
            }
            return ContentFormat.VerboseJson;
        }

        protected override object Read(System.Data.Services.SegmentInfo segmentInfo)
        {
            Func<OperationParameter, bool> predicate = null;
            Func<OperationParameter, bool> func2 = null;
            IEdmFunctionImport functionImport = base.GetFunctionImport(segmentInfo.Operation);
            ODataParameterReader reader = base.MessageReader.CreateODataParameterReader(functionImport);
            Dictionary<string, object> dictionary = new Dictionary<string, object>(EqualityComparer<string>.Default);
            while (reader.Read())
            {
                ResourceType parameterType;
                object obj2;
                switch (reader.State)
                {
                    case ODataParameterReaderState.Value:
                        if (predicate == null)
                        {
                            predicate = p => p.Name == reader.Name;
                        }
                        parameterType = segmentInfo.Operation.Parameters.Single<OperationParameter>(predicate).ParameterType;
                        obj2 = base.ConvertValue(reader.Value, ref parameterType);
                        break;

                    case ODataParameterReaderState.Collection:
                    {
                        ODataCollectionReader collectionReader = reader.CreateCollectionReader();
                        if (func2 == null)
                        {
                            func2 = p => p.Name == reader.Name;
                        }
                        parameterType = segmentInfo.Operation.Parameters.Single<OperationParameter>(func2).ParameterType;
                        obj2 = base.ConvertValue(ReadCollectionParameterValue(collectionReader), ref parameterType);
                        break;
                    }
                    case ODataParameterReaderState.Completed:
                        return dictionary;

                    default:
                        throw new InvalidOperationException(System.Data.Services.Strings.DataServiceException_GeneralError);
                }
                dictionary.Add(reader.Name, obj2);
            }
            return dictionary;
        }

        private static ODataCollectionValue ReadCollectionParameterValue(ODataCollectionReader collectionReader)
        {
            ODataCollectionValue value2;
            List<object> list = new List<object>();
            while (collectionReader.Read())
            {
                switch (collectionReader.State)
                {
                    case ODataCollectionReaderState.CollectionStart:
                    case ODataCollectionReaderState.CollectionEnd:
                    {
                        continue;
                    }
                    case ODataCollectionReaderState.Value:
                    {
                        list.Add(collectionReader.Item);
                        continue;
                    }
                    case ODataCollectionReaderState.Completed:
                        goto Label_004F;
                }
                throw new InvalidOperationException(System.Data.Services.Strings.DataServiceException_GeneralError);
            }
        Label_004F:
            value2 = new ODataCollectionValue();
            value2.Items = list;
            return value2;
        }
    }
}

