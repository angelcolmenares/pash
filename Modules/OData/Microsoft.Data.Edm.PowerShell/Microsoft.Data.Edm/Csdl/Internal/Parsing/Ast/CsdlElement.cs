using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal abstract class CsdlElement
	{
		protected List<object> annotations;

		protected EdmLocation location;

		public virtual bool HasDirectValueAnnotations
		{
			get
			{
				return this.HasAnnotations<CsdlDirectValueAnnotation>();
			}
		}

		public bool HasVocabularyAnnotations
		{
			get
			{
				return this.HasAnnotations<CsdlVocabularyAnnotationBase>();
			}
		}

		public IEnumerable<CsdlDirectValueAnnotation> ImmediateValueAnnotations
		{
			get
			{
				return this.GetAnnotations<CsdlDirectValueAnnotation>();
			}
		}

		public EdmLocation Location
		{
			get
			{
				return this.location;
			}
		}

		public IEnumerable<CsdlVocabularyAnnotationBase> VocabularyAnnotations
		{
			get
			{
				return this.GetAnnotations<CsdlVocabularyAnnotationBase>();
			}
		}

		public CsdlElement(CsdlLocation location)
		{
			this.location = location;
		}

		public void AddAnnotation(CsdlDirectValueAnnotation annotation)
		{
			this.AddUntypedAnnotation(annotation);
		}

		public void AddAnnotation(CsdlVocabularyAnnotationBase annotation)
		{
			this.AddUntypedAnnotation(annotation);
		}

		private void AddUntypedAnnotation(object annotation)
		{
			if (this.annotations == null)
			{
				this.annotations = new List<object>();
			}
			this.annotations.Add(annotation);
		}

		private IEnumerable<T> GetAnnotations<T>()
		where T : class
		{
			if (this.annotations != null)
			{
				return this.annotations.OfType<T>();
			}
			else
			{
				return Enumerable.Empty<T>();
			}
		}

		private bool HasAnnotations<T>()
		{
			bool flag;
			if (this.annotations != null)
			{
				List<object>.Enumerator enumerator = this.annotations.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object current = enumerator.Current;
						if (!(current is T))
						{
							continue;
						}
						flag = true;
						return flag;
					}
					return false;
				}
				finally
				{
					enumerator.Dispose();
				}
				return flag;
			}
			else
			{
				return false;
			}
		}
	}
}