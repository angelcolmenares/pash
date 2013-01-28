namespace System.Data.Services
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Annotations;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal sealed class AnnotatedModel : IEdmModel, IEdmElement
    {
        private readonly IEnumerable<IEdmModel> annotationsModels;
        private readonly IEdmModel primaryModel;
        private IEnumerable<IEdmVocabularyAnnotation> vocabularyAnnotations;

        internal AnnotatedModel(IEdmModel primaryModel, IEnumerable<IEdmModel> annotationsModels)
        {
            this.primaryModel = primaryModel;
            this.annotationsModels = annotationsModels;
        }

        public IEdmEntityContainer FindDeclaredEntityContainer(string name)
        {
            return this.primaryModel.FindDeclaredEntityContainer(name);
        }

        public IEnumerable<IEdmFunction> FindDeclaredFunctions(string qualifiedName)
        {
            return this.primaryModel.FindDeclaredFunctions(qualifiedName);
        }

        public IEdmSchemaType FindDeclaredType(string qualifiedName)
        {
            return this.primaryModel.FindDeclaredType(qualifiedName);
        }

        public IEdmValueTerm FindDeclaredValueTerm(string qualifiedName)
        {
            return this.primaryModel.FindDeclaredValueTerm(qualifiedName);
        }

        public IEnumerable<IEdmVocabularyAnnotation> FindDeclaredVocabularyAnnotations(IEdmVocabularyAnnotatable element)
        {
            return (from a in this.VocabularyAnnotations
                where a.Target == element
                select a);
        }

        public IEnumerable<IEdmStructuredType> FindDirectlyDerivedTypes(IEdmStructuredType baseType)
        {
            return this.primaryModel.FindDirectlyDerivedTypes(baseType);
        }

        private static bool IsModelMember(IEdmVocabularyAnnotatable item, IEdmModel model)
        {
            if (item is IEdmSchemaElement)
            {
                return model.SchemaElements.Contains<IEdmVocabularyAnnotatable>(item);
            }
            IEdmProperty property = item as IEdmProperty;
            if (property != null)
            {
                return model.SchemaElements.Contains<IEdmSchemaElement>(((IEdmSchemaElement) property.DeclaringType));
            }
            if (item is IEdmEntityContainerElement)
            {
                return model.EntityContainers().Single<IEdmEntityContainer>().Elements.Contains<IEdmVocabularyAnnotatable>(item);
            }
            return ((item is IEdmEntityContainer) && model.EntityContainers().Contains<IEdmVocabularyAnnotatable>(item));
        }

        public IEdmDirectValueAnnotationsManager DirectValueAnnotationsManager
        {
            get
            {
                return this.primaryModel.DirectValueAnnotationsManager;
            }
        }

        public IEnumerable<IEdmModel> ReferencedModels
        {
            get
            {
                return this.primaryModel.ReferencedModels;
            }
        }

        public IEnumerable<IEdmSchemaElement> SchemaElements
        {
            get
            {
                return this.primaryModel.SchemaElements;
            }
        }

        public IEnumerable<IEdmVocabularyAnnotation> VocabularyAnnotations
        {
            get
            {
                if (this.vocabularyAnnotations == null)
                {
                    IEnumerable<IEdmVocabularyAnnotation> enumerable = this.primaryModel.VocabularyAnnotations.Concat<IEdmVocabularyAnnotation>((from m in this.annotationsModels select m.VocabularyAnnotations).SelectMany (x => x));
                    AnnotationDescriptorComparer comparer = new AnnotationDescriptorComparer();
                    Dictionary<AnnotationDescriptor, IEdmVocabularyAnnotation> dictionary = new Dictionary<AnnotationDescriptor, IEdmVocabularyAnnotation>(comparer);
                    foreach (IEdmVocabularyAnnotation annotation in enumerable)
                    {
                        IEdmVocabularyAnnotatable target = annotation.Target;
                        if ((target != null) && IsModelMember(target, this.primaryModel))
                        {
                            AnnotationDescriptor key = new AnnotationDescriptor(target, annotation.Term, annotation.Qualifier);
                            if (!dictionary.ContainsKey(key))
                            {
                                dictionary.Add(key, annotation);
                            }
                        }
                    }
                    this.vocabularyAnnotations = dictionary.Values;
                }
                return this.vocabularyAnnotations;
            }
        }

        private class AnnotationDescriptor
        {
            public AnnotationDescriptor(IEdmVocabularyAnnotatable target, IEdmTerm term, string qualifier)
            {
                this.Target = target;
                this.Term = term;
                this.Qualifier = qualifier;
            }

            public string Qualifier { get; private set; }

            public IEdmVocabularyAnnotatable Target { get; private set; }

            public IEdmTerm Term { get; private set; }
        }

        private class AnnotationDescriptorComparer : IEqualityComparer<AnnotatedModel.AnnotationDescriptor>
        {
            public bool Equals(AnnotatedModel.AnnotationDescriptor x, AnnotatedModel.AnnotationDescriptor y)
            {
                if ((x == null) || (y == null))
                {
                    return ((x == null) && (y == null));
                }
                return ((((x.Target == y.Target) && (x.Term.Namespace == y.Term.Namespace)) && (x.Term.Name == y.Term.Name)) && (x.Qualifier == y.Qualifier));
            }

            public int GetHashCode(AnnotatedModel.AnnotationDescriptor obj)
            {
                if (obj == null)
                {
                    return 0;
                }
                int num = (obj.Target.GetHashCode() ^ obj.Term.Namespace.GetHashCode()) ^ obj.Term.Name.GetHashCode();
                if (obj.Qualifier == null)
                {
                    return num;
                }
                return (num ^ obj.Qualifier.GetHashCode());
            }
        }
    }
}

