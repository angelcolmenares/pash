namespace System.Data.Services.Client
{
    using Microsoft.Data.Edm;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services.Client.Metadata;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class LoadPropertyResult : QueryResult
    {
        private readonly object entity;
        private readonly ProjectionPlan plan;
        private readonly string propertyName;

        internal LoadPropertyResult(object entity, string propertyName, DataServiceContext context, ODataRequestMessageWrapper request, AsyncCallback callback, object state, DataServiceRequest dataServiceRequest, ProjectionPlan plan) : base(context, "LoadProperty", dataServiceRequest, request, new RequestInfo(context), callback, state)
        {
            this.entity = entity;
            this.propertyName = propertyName;
            this.plan = plan;
        }

        private object GetCollectionInstance(ClientPropertyAnnotation property, out bool instanceCreated)
        {
            instanceCreated = false;
            object obj2 = property.GetValue(this.entity);
            if (obj2 != null)
            {
                return obj2;
            }
            instanceCreated = true;
            Type propertyType = property.PropertyType;
            if (BindingEntityInfo.IsDataServiceCollection(propertyType, base.RequestInfo.MaxProtocolVersion))
            {
                object[] args = new object[2];
                args[1] = TrackingMode.None;
                return Activator.CreateInstance(WebUtil.GetDataServiceCollectionOfT(new Type[] { property.EntityCollectionItemType }), args);
            }
            Type c = typeof(List<>).MakeGenericType(new Type[] { property.EntityCollectionItemType });
            if (!propertyType.IsAssignableFrom(c))
            {
                c = propertyType;
            }
            return Activator.CreateInstance(c);
        }

        internal QueryOperationResponse LoadProperty()
        {
            MaterializeAtom results = null;
            QueryOperationResponse responseWithType;
            DataServiceContext source = (DataServiceContext) base.Source;
            ClientEdmModel model = ClientEdmModel.GetModel(source.MaxProtocolVersion);
            ClientTypeAnnotation clientTypeAnnotation = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(this.entity.GetType()));
            EntityDescriptor entityDescriptor = source.GetEntityDescriptor(this.entity);
            if (EntityStates.Added == entityDescriptor.State)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_NoLoadWithInsertEnd);
            }
            ClientPropertyAnnotation property = clientTypeAnnotation.GetProperty(this.propertyName, false);
            Type elementType = property.EntityCollectionItemType ?? property.NullablePropertyType;
            try
            {
                if (clientTypeAnnotation.MediaDataMember == property)
                {
                    results = this.ReadPropertyFromRawData(property);
                }
                else
                {
                    results = this.ReadPropertyFromAtom(entityDescriptor, property);
                }
                responseWithType = base.GetResponseWithType(results, elementType);
            }
            catch (InvalidOperationException exception)
            {
                QueryOperationResponse response = base.GetResponseWithType(results, elementType);
                if (response != null)
                {
                    response.Error = exception;
                    throw new DataServiceQueryException(System.Data.Services.Client.Strings.DataServiceException_GeneralError, exception, response);
                }
                throw;
            }
            return responseWithType;
        }

        private static byte[] ReadByteArrayChunked(Stream responseStream)
        {
            byte[] buffer = null;
            using (MemoryStream stream = new MemoryStream())
            {
                byte[] buffer2 = new byte[0x1000];
                int count = 0;
                int num2 = 0;
                while (true)
                {
                    count = responseStream.Read(buffer2, 0, buffer2.Length);
                    if (count <= 0)
                    {
                        break;
                    }
                    stream.Write(buffer2, 0, count);
                    num2 += count;
                }
                buffer = new byte[num2];
                stream.Position = 0L;
                count = stream.Read(buffer, 0, buffer.Length);
            }
            return buffer;
        }

        private static byte[] ReadByteArrayWithContentLength(Stream responseStream, int totalLength)
        {
            int num2;
            byte[] buffer = new byte[totalLength];
            for (int i = 0; i < totalLength; i += num2)
            {
                num2 = responseStream.Read(buffer, i, totalLength - i);
                if (num2 <= 0)
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_UnexpectedZeroRawRead);
                }
            }
            return buffer;
        }

        private MaterializeAtom ReadPropertyFromAtom(EntityDescriptor box, ClientPropertyAnnotation property)
        {
            MaterializeAtom atom2;
            DataServiceContext source = (DataServiceContext) base.Source;
            bool applyingChanges = source.ApplyingChanges;
            try
            {
                source.ApplyingChanges = true;
                bool flag2 = EntityStates.Deleted == box.State;
                bool instanceCreated = false;
                object instance = null;
                if (property.IsEntityCollection)
                {
                    instance = this.GetCollectionInstance(property, out instanceCreated);
                }
                Type type = property.IsEntityCollection ? property.EntityCollectionItemType : property.NullablePropertyType;
                IList results = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(new Type[] { type }));
                DataServiceQueryContinuation continuation = null;
                using (MaterializeAtom atom = base.GetMaterializer(this.plan))
                {
                    bool flag4 = property.EdmProperty.PropertyKind == EdmPropertyKind.Navigation;
                    int num = 0;
                    foreach (object obj3 in atom)
                    {
                        if (property.IsEntityCollection)
                        {
                            property.SetValue(instance, obj3, this.propertyName, true);
                            results.Add(obj3);
                        }
                        else if (property.IsPrimitiveOrComplexCollection)
                        {
                            object obj4 = property.GetValue(this.entity);
                            if (obj4 == null)
                            {
                                obj4 = Activator.CreateInstance(obj3.GetType());
                                property.SetValue(this.entity, obj4, this.propertyName, false);
                            }
                            else
                            {
                                property.ClearBackingICollectionInstance(obj4);
                            }
                            foreach (object obj5 in (IEnumerable) obj3)
                            {
                                property.AddValueToBackingICollectionInstance(obj4, obj5);
                            }
                            results.Add(obj4);
                        }
                        else
                        {
                            property.SetValue(this.entity, obj3, this.propertyName, false);
                            results.Add(obj3);
                        }
                        num++;
                        if (((obj3 != null) && (MergeOption.NoTracking != atom.MergeOptionValue)) && flag4)
                        {
                            if (flag2)
                            {
                                source.DeleteLink(this.entity, this.propertyName, obj3);
                            }
                            else
                            {
                                source.AttachLink(this.entity, this.propertyName, obj3, atom.MergeOptionValue);
                            }
                        }
                    }
                    continuation = atom.GetContinuation(null);
                    Util.SetNextLinkForCollection(property.IsEntityCollection ? instance : this.entity, continuation);
                }
                if (instanceCreated)
                {
                    property.SetValue(this.entity, instance, this.propertyName, false);
                }
                atom2 = MaterializeAtom.CreateWrapper(source, results, continuation);
            }
            finally
            {
                source.ApplyingChanges = applyingChanges;
            }
            return atom2;
        }

        private MaterializeAtom ReadPropertyFromRawData(ClientPropertyAnnotation property)
        {
            MaterializeAtom atom;
            DataServiceContext source = (DataServiceContext) base.Source;
            bool applyingChanges = source.ApplyingChanges;
            try
            {
                source.ApplyingChanges = true;
                string mime = null;
                Encoding encoding = null;
                Type type = property.EntityCollectionItemType ?? property.NullablePropertyType;
                IList results = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(new Type[] { type }));
                HttpProcessUtility.ReadContentType(base.ContentType, out mime, out encoding);
                using (Stream stream = base.GetResponseStream())
                {
                    if (property.PropertyType == typeof(byte[]))
                    {
                        int contentLength = (int) base.ContentLength;
                        byte[] buffer = null;
                        if (contentLength >= 0)
                        {
                            buffer = ReadByteArrayWithContentLength(stream, contentLength);
                        }
                        else
                        {
                            buffer = ReadByteArrayChunked(stream);
                        }
                        results.Add(buffer);
                        property.SetValue(this.entity, buffer, this.propertyName, false);
                    }
                    else
                    {
                        StreamReader reader = new StreamReader(stream, encoding);
                        object obj2 = (property.PropertyType == typeof(string)) ? reader.ReadToEnd() : ClientConvert.ChangeType(reader.ReadToEnd(), property.PropertyType);
                        results.Add(obj2);
                        property.SetValue(this.entity, obj2, this.propertyName, false);
                    }
                }
                if (property.MimeTypeProperty != null)
                {
                    property.MimeTypeProperty.SetValue(this.entity, mime, null, false);
                }
                atom = MaterializeAtom.CreateWrapper(source, results);
            }
            finally
            {
                source.ApplyingChanges = applyingChanges;
            }
            return atom;
        }
    }
}

