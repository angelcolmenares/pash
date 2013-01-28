namespace System.Data.Services.Providers
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal sealed class MetadataProviderEdmEntityContainer : EdmElement, IEdmEntityContainer, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
    {
        private readonly string containerName;
        private readonly string containerNamespace;
        private readonly Dictionary<string, IEdmEntitySet> entitySetCache;
        private readonly Dictionary<string, List<MetadataProviderEdmFunctionImport>> functionImportCache;
        private readonly MetadataProviderEdmModel model;

        internal MetadataProviderEdmEntityContainer(MetadataProviderEdmModel model, string containerName, string entityContainerSchemaNamespace)
        {
            this.model = model;
            this.containerName = containerName;
            this.containerNamespace = entityContainerSchemaNamespace;
            this.entitySetCache = new Dictionary<string, IEdmEntitySet>(StringComparer.Ordinal);
            this.functionImportCache = new Dictionary<string, List<MetadataProviderEdmFunctionImport>>(StringComparer.Ordinal);
        }

        internal void AddEntitySet(string entitySetName, ResourceSetWrapper resourceSet)
        {
            IEdmEntityType elementType = (IEdmEntityType) this.model.EnsureSchemaType(resourceSet.ResourceType);
            IEdmEntitySet target = new EdmEntitySet(this, entitySetName, elementType);
            MetadataProviderUtils.ConvertCustomAnnotations(this.model, resourceSet.CustomAnnotations, target);
            this.entitySetCache.Add(entitySetName, target);
        }

        internal IEdmFunctionImport EnsureFunctionImport(OperationWrapper serviceOperation)
        {
            List<MetadataProviderEdmFunctionImport> list;
            string name = serviceOperation.Name;
            if (!this.functionImportCache.TryGetValue(name, out list))
            {
                list = new List<MetadataProviderEdmFunctionImport>();
                this.functionImportCache.Add(name, list);
            }
            MetadataProviderEdmFunctionImport item = list.Find(f => f.ServiceOperation == serviceOperation);
            if (item == null)
            {
                item = new MetadataProviderEdmFunctionImport(this.model, this, serviceOperation);
                list.Add(item);
            }
            return item;
        }

        public IEdmEntitySet FindEntitySet(string name)
        {
            IEdmEntitySet set;
            WebUtil.CheckStringArgumentNullOrEmpty(name, "name");
            if (!this.entitySetCache.TryGetValue(name, out set))
            {
                return null;
            }
            return set;
        }

        public IEnumerable<IEdmFunctionImport> FindFunctionImports(string name)
        {
            List<MetadataProviderEdmFunctionImport> list;
            WebUtil.CheckStringArgumentNullOrEmpty(name, "name");
            if (!this.functionImportCache.TryGetValue(name, out list))
            {
                return Enumerable.Empty<IEdmFunctionImport>();
            }
            return (IEnumerable<IEdmFunctionImport>) list.AsReadOnly();
        }

        public IEnumerable<IEdmEntityContainerElement> Elements
        {
            get
            {
                foreach (IEdmEntitySet iteratorVariable0 in this.entitySetCache.Values)
                {
                    yield return iteratorVariable0;
                }
                foreach (IEdmFunctionImport iteratorVariable1 in from v in this.functionImportCache.Values select v)
                {
                    yield return iteratorVariable1;
                }
            }
        }

        public string Name
        {
            get
            {
                return this.containerName;
            }
        }

        public string Namespace
        {
            get
            {
                return this.containerNamespace;
            }
        }

        public EdmSchemaElementKind SchemaElementKind
        {
            get
            {
                return EdmSchemaElementKind.EntityContainer;
            }
        }

        
    }
}

