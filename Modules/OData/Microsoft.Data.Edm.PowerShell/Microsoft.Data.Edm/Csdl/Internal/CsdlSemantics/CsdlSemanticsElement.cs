using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal abstract class CsdlSemanticsElement : IEdmElement, IEdmLocatable
	{
		private readonly Cache<CsdlSemanticsElement, IEnumerable<IEdmVocabularyAnnotation>> inlineVocabularyAnnotationsCache;

		private readonly static Func<CsdlSemanticsElement, IEnumerable<IEdmVocabularyAnnotation>> ComputeInlineVocabularyAnnotationsFunc;

		private readonly Cache<CsdlSemanticsElement, IEnumerable<IEdmDirectValueAnnotation>> directValueAnnotationsCache;

		private readonly static Func<CsdlSemanticsElement, IEnumerable<IEdmDirectValueAnnotation>> ComputeDirectValueAnnotationsFunc;

		private readonly static IEnumerable<IEdmVocabularyAnnotation> emptyVocabularyAnnotations;

		public IEnumerable<IEdmDirectValueAnnotation> DirectValueAnnotations
		{
			get
			{
				if (this.directValueAnnotationsCache != null)
				{
					return this.directValueAnnotationsCache.GetValue(this, CsdlSemanticsElement.ComputeDirectValueAnnotationsFunc, null);
				}
				else
				{
					return null;
				}
			}
		}

		public abstract CsdlElement Element
		{
			get;
		}

		public IEnumerable<IEdmVocabularyAnnotation> InlineVocabularyAnnotations
		{
			get
			{
				if (this.inlineVocabularyAnnotationsCache != null)
				{
					return this.inlineVocabularyAnnotationsCache.GetValue(this, CsdlSemanticsElement.ComputeInlineVocabularyAnnotationsFunc, null);
				}
				else
				{
					return CsdlSemanticsElement.emptyVocabularyAnnotations;
				}
			}
		}

		public EdmLocation Location
		{
			get
			{
				if (this.Element == null || this.Element.Location == null)
				{
					return new ObjectLocation(this);
				}
				else
				{
					return this.Element.Location;
				}
			}
		}

		public abstract CsdlSemanticsModel Model
		{
			get;
		}

		static CsdlSemanticsElement()
		{
			CsdlSemanticsElement.ComputeInlineVocabularyAnnotationsFunc = (CsdlSemanticsElement me) => me.ComputeInlineVocabularyAnnotations();
			CsdlSemanticsElement.ComputeDirectValueAnnotationsFunc = (CsdlSemanticsElement me) => me.ComputeDirectValueAnnotations();
			CsdlSemanticsElement.emptyVocabularyAnnotations = Enumerable.Empty<IEdmVocabularyAnnotation>();
		}

		protected CsdlSemanticsElement(CsdlElement element)
		{
			if (element != null)
			{
				if (element.HasDirectValueAnnotations)
				{
					this.directValueAnnotationsCache = new Cache<CsdlSemanticsElement, IEnumerable<IEdmDirectValueAnnotation>>();
				}
				if (element.HasVocabularyAnnotations)
				{
					this.inlineVocabularyAnnotationsCache = new Cache<CsdlSemanticsElement, IEnumerable<IEdmVocabularyAnnotation>>();
				}
			}
		}

		protected static List<T> AllocateAndAdd<T>(List<T> list, T item)
		{
			if (list == null)
			{
				list = new List<T>();
			}
			list.Add(item);
			return list;
		}

		protected static List<T> AllocateAndAdd<T>(List<T> list, IEnumerable<T> items)
		{
			if (list == null)
			{
				list = new List<T>();
			}
			list.AddRange(items);
			return list;
		}

		protected IEnumerable<IEdmDirectValueAnnotation> ComputeDirectValueAnnotations()
		{
			CsdlDocumentation documentation;
			if (this.Element != null)
			{
				List<CsdlDirectValueAnnotation> list = this.Element.ImmediateValueAnnotations.ToList<CsdlDirectValueAnnotation>();
				CsdlElementWithDocumentation element = this.Element as CsdlElementWithDocumentation;
				if (element != null)
				{
					documentation = element.Documentation;
				}
				else
				{
					documentation = null;
				}
				CsdlDocumentation csdlDocumentation = documentation;
				if (csdlDocumentation != null || list.FirstOrDefault<CsdlDirectValueAnnotation>() != null)
				{
					List<IEdmDirectValueAnnotation> edmDirectValueAnnotations = new List<IEdmDirectValueAnnotation>();
					foreach (CsdlDirectValueAnnotation csdlDirectValueAnnotation in list)
					{
						edmDirectValueAnnotations.Add(new CsdlSemanticsDirectValueAnnotation(csdlDirectValueAnnotation, this.Model));
					}
					if (csdlDocumentation != null)
					{
						edmDirectValueAnnotations.Add(new CsdlSemanticsDocumentation(csdlDocumentation, this.Model));
					}
					return edmDirectValueAnnotations;
				}
				else
				{
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		protected virtual IEnumerable<IEdmVocabularyAnnotation> ComputeInlineVocabularyAnnotations()
		{
			return this.Model.WrapInlineVocabularyAnnotations(this, null);
		}
	}
}