namespace System.Data.Services.Client.Metadata
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Data.Services.Client.Serializers;
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;

    [DebuggerDisplay("{ElementTypeName}")]
    internal sealed class ClientTypeAnnotation
    {
        private Dictionary<string, ClientPropertyAnnotation> clientPropertyCache;
        private IEdmProperty[] edmPropertyCache;
        internal readonly IEdmType EdmType;
        internal readonly Type ElementType;
        internal readonly string ElementTypeName;
        private EpmLazyLoader epmLazyLoader;
        private bool? isMediaLinkEntry;
        internal readonly DataServiceProtocolVersion MaxProtocolVersion;
        private ClientPropertyAnnotation mediaDataMember;
        private Version metadataVersion;

        internal ClientTypeAnnotation(IEdmType edmType, Type type, string qualifiedName, DataServiceProtocolVersion maxProtocolVersion)
        {
            this.EdmType = edmType;
            this.ElementTypeName = qualifiedName;
            this.ElementType = Nullable.GetUnderlyingType(type) ?? type;
            this.MaxProtocolVersion = maxProtocolVersion;
            this.epmLazyLoader = new EpmLazyLoader(this);
        }

        private void BuildPropertyCache()
        {
            lock (this)
            {
                if (this.clientPropertyCache == null)
                {
                    ClientEdmModel model = ClientEdmModel.GetModel(this.MaxProtocolVersion);
                    Dictionary<string, ClientPropertyAnnotation> dictionary = new Dictionary<string, ClientPropertyAnnotation>(StringComparer.Ordinal);
                    foreach (IEdmProperty property in this.EdmProperties())
                    {
                        dictionary.Add(property.Name, model.GetClientPropertyAnnotation(property));
                    }
                    this.clientPropertyCache = dictionary;
                }
            }
        }

        private void CheckMediaLinkEntry()
        {
            Func<ClientPropertyAnnotation, bool> predicate = null;
            this.isMediaLinkEntry = false;
            MediaEntryAttribute mediaEntryAttribute = (MediaEntryAttribute) this.ElementType.GetCustomAttributes(typeof(MediaEntryAttribute), true).SingleOrDefault<object>();
            if (mediaEntryAttribute != null)
            {
                this.isMediaLinkEntry = true;
                if (predicate == null)
                {
                    predicate = p => p.PropertyName == mediaEntryAttribute.MediaMemberName;
                }
                ClientPropertyAnnotation annotation = this.Properties().SingleOrDefault<ClientPropertyAnnotation>(predicate);
                if (annotation == null)
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.ClientType_MissingMediaEntryProperty(this.ElementTypeName, mediaEntryAttribute.MediaMemberName));
                }
                this.mediaDataMember = annotation;
            }
            if (this.ElementType.GetCustomAttributes(typeof(HasStreamAttribute), true).Any<object>())
            {
                this.isMediaLinkEntry = true;
            }
            if (this.isMediaLinkEntry.HasValue && this.isMediaLinkEntry.Value)
            {
                this.SetMediaLinkEntryAnnotation();
            }
        }

        private Version ComputeVersionForPropertyCollection(IEnumerable<IEdmProperty> propertyCollection, HashSet<IEdmType> visitedComplexTypes, ClientEdmModel model)
        {
            Version version = Util.DataServiceVersion1;
            foreach (IEdmProperty property in propertyCollection)
            {
                ClientPropertyAnnotation clientPropertyAnnotation = model.GetClientPropertyAnnotation(property);
                if (clientPropertyAnnotation.IsPrimitiveOrComplexCollection || clientPropertyAnnotation.IsSpatialType)
                {
                    WebUtil.RaiseVersion(ref version, Util.DataServiceVersion3);
                }
                else
                {
                    if ((property.Type.TypeKind() == EdmTypeKind.Complex) && !clientPropertyAnnotation.IsDictionary)
                    {
                        if (visitedComplexTypes == null)
                        {
                            visitedComplexTypes = new HashSet<IEdmType>(EqualityComparer<IEdmType>.Default);
                        }
                        else if (visitedComplexTypes.Contains(property.Type.Definition))
                        {
                            goto Label_00A6;
                        }
                        visitedComplexTypes.Add(property.Type.Definition);
                        WebUtil.RaiseVersion(ref version, this.ComputeVersionForPropertyCollection(model.GetClientTypeAnnotation(property).EdmProperties(), visitedComplexTypes, model));
                    }
                Label_00A6:;
                }
            }
            return version;
        }

        internal object CreateInstance()
        {
            return Util.ActivatorCreateInstance(this.ElementType, new object[0]);
        }

        private IEnumerable<IEdmProperty> DiscoverEdmProperties()
        {
            IEdmStructuredType edmType = this.EdmType as IEdmStructuredType;
            if (edmType != null)
            {
                HashSet<string> iteratorVariable1 = new HashSet<string>(EqualityComparer<string>.Default);
                do
                {
                    foreach (IEdmProperty iteratorVariable2 in edmType.DeclaredProperties)
                    {
                        string name = iteratorVariable2.Name;
                        if (!iteratorVariable1.Contains(name))
                        {
                            iteratorVariable1.Add(name);
                            yield return iteratorVariable2;
                        }
                    }
                    edmType = edmType.BaseType;
                }
                while (edmType != null);
            }
        }

        internal IEnumerable<IEdmProperty> EdmProperties()
        {
            if (this.edmPropertyCache == null)
            {
                this.edmPropertyCache = this.DiscoverEdmProperties().ToArray<IEdmProperty>();
            }
            return this.edmPropertyCache;
        }

        internal void EnsureEPMLoaded()
        {
            this.epmLazyLoader.EnsureEPMLoaded();
        }

        internal Version GetMetadataVersion()
        {
            if (this.metadataVersion == null)
            {
                Version version = Util.DataServiceVersion1;
                ClientEdmModel model = ClientEdmModel.GetModel(this.MaxProtocolVersion);
                WebUtil.RaiseVersion(ref version, this.ComputeVersionForPropertyCollection(this.EdmProperties(), null, model));
                this.metadataVersion = version;
            }
            return this.metadataVersion;
        }

        internal ClientPropertyAnnotation GetProperty(string propertyName, bool ignoreMissingProperties)
        {
            ClientPropertyAnnotation annotation;
            if (this.clientPropertyCache == null)
            {
                this.BuildPropertyCache();
            }
            if (!this.clientPropertyCache.TryGetValue(propertyName, out annotation) && !ignoreMissingProperties)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.ClientType_MissingProperty(this.ElementTypeName, propertyName));
            }
            return annotation;
        }

        internal IEnumerable<ClientPropertyAnnotation> Properties()
        {
            if (this.clientPropertyCache == null)
            {
                this.BuildPropertyCache();
            }
            return this.clientPropertyCache.Values;
        }

        private void SetMediaLinkEntryAnnotation()
        {
            ClientEdmModel.GetModel(this.MaxProtocolVersion).SetHasDefaultStream((IEdmEntityType) this.EdmType, true);
        }

        internal DataServiceProtocolVersion EpmMinimumDataServiceProtocolVersion
        {
            get
            {
                if (!this.HasEntityPropertyMappings)
                {
                    return DataServiceProtocolVersion.V1;
                }
                return this.EpmTargetTree.MinimumDataServiceProtocolVersion;
            }
        }

        internal System.Data.Services.Client.Serializers.EpmTargetTree EpmTargetTree
        {
            get
            {
                return this.epmLazyLoader.EpmTargetTree;
            }
        }

        internal bool HasEntityPropertyMappings
        {
            get
            {
                return (this.epmLazyLoader.EpmSourceTree.Root.SubProperties.Count > 0);
            }
        }

        internal bool IsEntityType
        {
            get
            {
                return (this.EdmType.TypeKind == EdmTypeKind.Entity);
            }
        }

        internal bool IsMediaLinkEntry
        {
            get
            {
                if (!this.isMediaLinkEntry.HasValue)
                {
                    this.CheckMediaLinkEntry();
                }
                return this.isMediaLinkEntry.Value;
            }
        }

        internal ClientPropertyAnnotation MediaDataMember
        {
            get
            {
                if (!this.isMediaLinkEntry.HasValue)
                {
                    this.CheckMediaLinkEntry();
                }
                return this.mediaDataMember;
            }
        }

        

        private class EpmLazyLoader
        {
            private ClientTypeAnnotation clientTypeAnnotation;
            private object epmDataLock = new object();
            private System.Data.Services.Client.Serializers.EpmSourceTree epmSourceTree;
            private System.Data.Services.Client.Serializers.EpmTargetTree epmTargetTree;

            internal EpmLazyLoader(ClientTypeAnnotation clientTypeAnnotation)
            {
                this.clientTypeAnnotation = clientTypeAnnotation;
            }

            private static void BuildEpmInfo(ClientTypeAnnotation clientTypeAnnotation, System.Data.Services.Client.Serializers.EpmSourceTree sourceTree)
            {
                BuildEpmInfo(clientTypeAnnotation.ElementType, clientTypeAnnotation, sourceTree);
            }

            private static void BuildEpmInfo(Type type, ClientTypeAnnotation clientTypeAnnotation, System.Data.Services.Client.Serializers.EpmSourceTree sourceTree)
            {
                if (clientTypeAnnotation.IsEntityType)
                {
                    Func<EntityPropertyMappingAttribute, bool> predicate = null;
                    Type baseType = type.BaseType;
                    ClientTypeAnnotation annotation = null;
                    ClientEdmModel model = ClientEdmModel.GetModel(clientTypeAnnotation.MaxProtocolVersion);
                    ODataEntityPropertyMappingCollection mappings = null;
                    if ((baseType != null) && (baseType != typeof(object)))
                    {
                        if (((EdmStructuredType) clientTypeAnnotation.EdmType).BaseType == null)
                        {
                            BuildEpmInfo(baseType, clientTypeAnnotation, sourceTree);
                            mappings = model.GetAnnotationValue<ODataEntityPropertyMappingCollection>(clientTypeAnnotation.EdmType);
                        }
                        else
                        {
                            annotation = model.GetClientTypeAnnotation(baseType);
                            BuildEpmInfo(baseType, annotation, sourceTree);
                        }
                    }
                    foreach (EntityPropertyMappingAttribute attribute in type.GetCustomAttributes(typeof(EntityPropertyMappingAttribute), false))
                    {
                        BuildEpmInfo(attribute, type, clientTypeAnnotation, sourceTree);
                        if (mappings == null)
                        {
                            mappings = new ODataEntityPropertyMappingCollection();
                        }
                        mappings.Add(attribute);
                    }
                    if (mappings != null)
                    {
                        ODataEntityPropertyMappingCollection annotationValue = model.GetAnnotationValue<ODataEntityPropertyMappingCollection>(clientTypeAnnotation.EdmType);
                        if (annotationValue != null)
                        {
                            if (predicate == null)
                            {
                                predicate = oldM => !mappings.Any<EntityPropertyMappingAttribute>(newM => (oldM.SourcePath == newM.SourcePath));
                            }
                            foreach (EntityPropertyMappingAttribute attribute2 in annotationValue.Where<EntityPropertyMappingAttribute>(predicate).ToList<EntityPropertyMappingAttribute>())
                            {
                                mappings.Add(attribute2);
                            }
                        }
                        model.SetAnnotationValue<ODataEntityPropertyMappingCollection>(clientTypeAnnotation.EdmType, mappings);
                    }
                }
            }

            private static void BuildEpmInfo(EntityPropertyMappingAttribute epmAttr, Type definingType, ClientTypeAnnotation clientTypeAnnotation, System.Data.Services.Client.Serializers.EpmSourceTree sourceTree)
            {
                sourceTree.Add(new System.Data.Services.Client.Serializers.EntityPropertyMappingInfo(epmAttr, definingType, clientTypeAnnotation));
            }

            internal void EnsureEPMLoaded()
            {
                if (this.EpmNeedsInitializing)
                {
                    this.InitializeAndBuildTree();
                }
            }

            private void InitializeAndBuildTree()
            {
                lock (this.epmDataLock)
                {
                    if (this.EpmNeedsInitializing)
                    {
                        System.Data.Services.Client.Serializers.EpmTargetTree epmTargetTree = new System.Data.Services.Client.Serializers.EpmTargetTree();
                        System.Data.Services.Client.Serializers.EpmSourceTree sourceTree = new System.Data.Services.Client.Serializers.EpmSourceTree(epmTargetTree);
                        BuildEpmInfo(this.clientTypeAnnotation, sourceTree);
                        sourceTree.Validate(this.clientTypeAnnotation);
                        this.epmTargetTree = epmTargetTree;
                        this.epmSourceTree = sourceTree;
                    }
                }
            }

            private bool EpmNeedsInitializing
            {
                get
                {
                    if (this.epmSourceTree != null)
                    {
                        return (this.epmTargetTree == null);
                    }
                    return true;
                }
            }

            internal System.Data.Services.Client.Serializers.EpmSourceTree EpmSourceTree
            {
                get
                {
                    if (this.EpmNeedsInitializing)
                    {
                        this.InitializeAndBuildTree();
                    }
                    return this.epmSourceTree;
                }
            }

            internal System.Data.Services.Client.Serializers.EpmTargetTree EpmTargetTree
            {
                get
                {
                    if (this.EpmNeedsInitializing)
                    {
                        this.InitializeAndBuildTree();
                    }
                    return this.epmTargetTree;
                }
            }
        }
    }
}

