using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Library.Annotations
{
	internal class EdmDirectValueAnnotationsManager : IEdmDirectValueAnnotationsManager
	{
		private VersioningDictionary<IEdmElement, object> annotationsDictionary;

		private object annotationsDictionaryLock;

		private VersioningList<IEdmElement> unsortedElements;

		private object unsortedElementsLock;

		public EdmDirectValueAnnotationsManager()
		{
			this.annotationsDictionaryLock = new object();
			this.unsortedElements = VersioningList<IEdmElement>.Create();
			this.unsortedElementsLock = new object();
			this.annotationsDictionary = VersioningDictionary<IEdmElement, object>.Create(new Func<IEdmElement, IEdmElement, int>(this.CompareElements));
		}

		private int CompareElements(IEdmElement left, IEdmElement right)
		{
			int num;
			if (left != right)
			{
				int hashCode = left.GetHashCode();
				int hashCode1 = right.GetHashCode();
				if (hashCode >= hashCode1)
				{
					if (hashCode <= hashCode1)
					{
						IEdmNamedElement edmNamedElement = left as IEdmNamedElement;
						IEdmNamedElement edmNamedElement1 = right as IEdmNamedElement;
						if (edmNamedElement != null)
						{
							if (edmNamedElement1 != null)
							{
								int num1 = string.Compare(edmNamedElement.Name, edmNamedElement1.Name, StringComparison.Ordinal);
								if (num1 != 0)
								{
									return num1;
								}
							}
							else
							{
								return 1;
							}
						}
						else
						{
							if (edmNamedElement1 != null)
							{
								return -1;
							}
						}
						while (true)
						{
							foreach (IEdmElement unsortedElement in this.unsortedElements)
							{
								if (unsortedElement != left)
								{
									if (unsortedElement != right)
									{
										continue;
									}
									num = -1;
									return num;
								}
								else
								{
									num = 1;
									return num;
								}
							}
							lock (this.unsortedElementsLock)
							{
								this.unsortedElements = this.unsortedElements.Add(left);
							}
						}
						return num;
					}
					else
					{
						return 1;
					}
				}
				else
				{
					return -1;
				}
			}
			else
			{
				return 0;
			}
		}

		private static IEdmDirectValueAnnotation FindTransientAnnotation(object transientAnnotations, string namespaceName, string localName)
		{
			Func<IEdmDirectValueAnnotation, bool> func = null;
			if (transientAnnotations != null)
			{
				IEdmDirectValueAnnotation edmDirectValueAnnotation = transientAnnotations as IEdmDirectValueAnnotation;
				if (edmDirectValueAnnotation == null)
				{
					VersioningList<IEdmDirectValueAnnotation> edmDirectValueAnnotations = (VersioningList<IEdmDirectValueAnnotation>)transientAnnotations;
					VersioningList<IEdmDirectValueAnnotation> edmDirectValueAnnotations1 = edmDirectValueAnnotations;
					if (func == null)
					{
						func = (IEdmDirectValueAnnotation existingAnnotation) => {
							if (existingAnnotation.NamespaceUri != namespaceName)
							{
								return false;
							}
							else
							{
								return existingAnnotation.Name == localName;
							}
						}
						;
					}
					return edmDirectValueAnnotations1.FirstOrDefault<IEdmDirectValueAnnotation>(func);
				}
				else
				{
					if (edmDirectValueAnnotation.NamespaceUri == namespaceName && edmDirectValueAnnotation.Name == localName)
					{
						return edmDirectValueAnnotation;
					}
				}
			}
			return null;
		}

		public object GetAnnotationValue(IEdmElement element, string namespaceName, string localName)
		{
			VersioningDictionary<IEdmElement, object> versioningDictionary = this.annotationsDictionary;
			return this.GetAnnotationValue(element, namespaceName, localName, versioningDictionary);
		}

		private object GetAnnotationValue(IEdmElement element, string namespaceName, string localName, VersioningDictionary<IEdmElement, object> annotationsDictionary)
		{
			object value;
			IEdmDirectValueAnnotation edmDirectValueAnnotation = EdmDirectValueAnnotationsManager.FindTransientAnnotation(EdmDirectValueAnnotationsManager.GetTransientAnnotations(element, annotationsDictionary), namespaceName, localName);
			if (edmDirectValueAnnotation == null)
			{
				IEnumerable<IEdmDirectValueAnnotation> attachedAnnotations = this.GetAttachedAnnotations(element);
				if (attachedAnnotations != null)
				{
					IEnumerator<IEdmDirectValueAnnotation> enumerator = attachedAnnotations.GetEnumerator();
					using (enumerator)
					{
						while (enumerator.MoveNext())
						{
							IEdmDirectValueAnnotation current = enumerator.Current;
							if (!(current.NamespaceUri == namespaceName) || !(current.Name == localName))
							{
								continue;
							}
							value = current.Value;
							return value;
						}
						return null;
					}
					return value;
				}
				return null;
			}
			else
			{
				return edmDirectValueAnnotation.Value;
			}
		}

		public object[] GetAnnotationValues(IEnumerable<IEdmDirectValueAnnotationBinding> annotations)
		{
			VersioningDictionary<IEdmElement, object> versioningDictionary = this.annotationsDictionary;
			object[] annotationValue = new object[annotations.Count<IEdmDirectValueAnnotationBinding>()];
			int num = 0;
			foreach (IEdmDirectValueAnnotationBinding annotation in annotations)
			{
				int num1 = num;
				num = num1 + 1;
				annotationValue[num1] = this.GetAnnotationValue(annotation.Element, annotation.NamespaceUri, annotation.Name, versioningDictionary);
			}
			return annotationValue;
		}

		protected virtual IEnumerable<IEdmDirectValueAnnotation> GetAttachedAnnotations(IEdmElement element)
		{
			return null;
		}

		public IEnumerable<IEdmDirectValueAnnotation> GetDirectValueAnnotations(IEdmElement element)
		{
			VersioningDictionary<IEdmElement, object> versioningDictionary = this.annotationsDictionary;
			IEnumerable<IEdmDirectValueAnnotation> attachedAnnotations = this.GetAttachedAnnotations(element);
			object obj = EdmDirectValueAnnotationsManager.GetTransientAnnotations(element, versioningDictionary);
			if (attachedAnnotations != null)
			{
				foreach (IEdmDirectValueAnnotation attachedAnnotation in attachedAnnotations)
				{
					if (EdmDirectValueAnnotationsManager.IsDead(attachedAnnotation.NamespaceUri, attachedAnnotation.Name, obj))
					{
						continue;
					}
					yield return attachedAnnotation;
				}
			}
			foreach (IEdmDirectValueAnnotation edmDirectValueAnnotation in EdmDirectValueAnnotationsManager.TransientAnnotations(obj))
			{
				yield return edmDirectValueAnnotation;
			}
		}

		private static object GetTransientAnnotations(IEdmElement element, VersioningDictionary<IEdmElement, object> annotationsDictionary)
		{
			object obj = null;
			annotationsDictionary.TryGetValue(element, out obj);
			return obj;
		}

		private static bool IsDead(string namespaceName, string localName, object transientAnnotations)
		{
			return EdmDirectValueAnnotationsManager.FindTransientAnnotation(transientAnnotations, namespaceName, localName) != null;
		}

		private static void RemoveTransientAnnotation(ref object transientAnnotations, string namespaceName, string localName)
		{
			if (transientAnnotations != null)
			{
				IEdmDirectValueAnnotation edmDirectValueAnnotation = transientAnnotations as IEdmDirectValueAnnotation;
				if (edmDirectValueAnnotation == null)
				{
					VersioningList<IEdmDirectValueAnnotation> edmDirectValueAnnotations = (VersioningList<IEdmDirectValueAnnotation>)transientAnnotations;
					int num = 0;
					while (num < edmDirectValueAnnotations.Count)
					{
						IEdmDirectValueAnnotation item = edmDirectValueAnnotations[num];
						if (!(item.NamespaceUri == namespaceName) || !(item.Name == localName))
						{
							num++;
						}
						else
						{
							edmDirectValueAnnotations = edmDirectValueAnnotations.RemoveAt(num);
							if (edmDirectValueAnnotations.Count != 1)
							{
								transientAnnotations = edmDirectValueAnnotations;
								return;
							}
							else
							{
								transientAnnotations = edmDirectValueAnnotations.Single<IEdmDirectValueAnnotation>();
								return;
							}
						}
					}
				}
				else
				{
					if (edmDirectValueAnnotation.NamespaceUri == namespaceName && edmDirectValueAnnotation.Name == localName)
					{
						transientAnnotations = null;
						return;
					}
				}
			}
		}

		private static void SetAnnotation(IEnumerable<IEdmDirectValueAnnotation> immutableAnnotations, ref object transientAnnotations, string namespaceName, string localName, object value)
		{
			IEdmDirectValueAnnotation edmDirectValueAnnotation;
			Func<IEdmDirectValueAnnotation, bool> func = null;
			bool flag = false;
			if (immutableAnnotations != null)
			{
				IEnumerable<IEdmDirectValueAnnotation> edmDirectValueAnnotations = immutableAnnotations;
				if (func == null)
				{
					func = (IEdmDirectValueAnnotation existingAnnotation) => {
						if (existingAnnotation.NamespaceUri != namespaceName)
						{
							return false;
						}
						else
						{
							return existingAnnotation.Name == localName;
						}
					}
					;
				}
				if (edmDirectValueAnnotations.Any<IEdmDirectValueAnnotation>(func))
				{
					flag = true;
				}
			}
			if (value != null || flag)
			{
				if (!(namespaceName == "http://schemas.microsoft.com/ado/2011/04/edm/documentation") || value == null || value as IEdmDocumentation != null)
				{
					if (value != null)
					{
						edmDirectValueAnnotation = new EdmDirectValueAnnotation(namespaceName, localName, value);
					}
					else
					{
						edmDirectValueAnnotation = new EdmDirectValueAnnotation(namespaceName, localName);
					}
					IEdmDirectValueAnnotation edmDirectValueAnnotation1 = edmDirectValueAnnotation;
					if (transientAnnotations != null)
					{
						IEdmDirectValueAnnotation edmDirectValueAnnotation2 = transientAnnotations as IEdmDirectValueAnnotation;
						if (edmDirectValueAnnotation2 == null)
						{
							VersioningList<IEdmDirectValueAnnotation> edmDirectValueAnnotations1 = (VersioningList<IEdmDirectValueAnnotation>)transientAnnotations;
							int num = 0;
							while (num < edmDirectValueAnnotations1.Count)
							{
								IEdmDirectValueAnnotation item = edmDirectValueAnnotations1[num];
								if (!(item.NamespaceUri == namespaceName) || !(item.Name == localName))
								{
									num++;
								}
								else
								{
									edmDirectValueAnnotations1 = edmDirectValueAnnotations1.RemoveAt(num);
									break;
								}
							}
							transientAnnotations = edmDirectValueAnnotations1.Add(edmDirectValueAnnotation1);
							return;
						}
						else
						{
							if (!(edmDirectValueAnnotation2.NamespaceUri == namespaceName) || !(edmDirectValueAnnotation2.Name == localName))
							{
								transientAnnotations = VersioningList<IEdmDirectValueAnnotation>.Create().Add(edmDirectValueAnnotation2).Add(edmDirectValueAnnotation1);
								return;
							}
							else
							{
								transientAnnotations = edmDirectValueAnnotation1;
								return;
							}
						}
					}
					else
					{
						transientAnnotations = edmDirectValueAnnotation1;
						return;
					}
				}
				else
				{
					throw new InvalidOperationException(Strings.Annotations_DocumentationPun(value.GetType().Name));
				}
			}
			else
			{
				EdmDirectValueAnnotationsManager.RemoveTransientAnnotation(ref transientAnnotations, namespaceName, localName);
				return;
			}
		}

		public void SetAnnotationValue(IEdmElement element, string namespaceName, string localName, object value)
		{
			lock (this.annotationsDictionaryLock)
			{
				VersioningDictionary<IEdmElement, object> versioningDictionary = this.annotationsDictionary;
				this.SetAnnotationValue(element, namespaceName, localName, value, ref versioningDictionary);
				this.annotationsDictionary = versioningDictionary;
			}
		}

		private void SetAnnotationValue(IEdmElement element, string namespaceName, string localName, object value, ref VersioningDictionary<IEdmElement, object> annotationsDictionary)
		{
			object transientAnnotations = EdmDirectValueAnnotationsManager.GetTransientAnnotations(element, annotationsDictionary);
			object obj = transientAnnotations;
			EdmDirectValueAnnotationsManager.SetAnnotation(this.GetAttachedAnnotations(element), ref transientAnnotations, namespaceName, localName, value);
			if (transientAnnotations != obj)
			{
				annotationsDictionary = annotationsDictionary.Set(element, transientAnnotations);
			}
		}

		public void SetAnnotationValues(IEnumerable<IEdmDirectValueAnnotationBinding> annotations)
		{
			lock (this.annotationsDictionaryLock)
			{
				VersioningDictionary<IEdmElement, object> versioningDictionary = this.annotationsDictionary;
				foreach (IEdmDirectValueAnnotationBinding annotation in annotations)
				{
					this.SetAnnotationValue(annotation.Element, annotation.NamespaceUri, annotation.Name, annotation.Value, ref versioningDictionary);
				}
				this.annotationsDictionary = versioningDictionary;
			}
		}

		private static IEnumerable<IEdmDirectValueAnnotation> TransientAnnotations(object transientAnnotations)
		{
			if (transientAnnotations != null)
			{
				IEdmDirectValueAnnotation edmDirectValueAnnotation = transientAnnotations as IEdmDirectValueAnnotation;
				if (edmDirectValueAnnotation == null)
				{
					VersioningList<IEdmDirectValueAnnotation> edmDirectValueAnnotations = (VersioningList<IEdmDirectValueAnnotation>)transientAnnotations;
					foreach (IEdmDirectValueAnnotation edmDirectValueAnnotation1 in edmDirectValueAnnotations)
					{
						if (edmDirectValueAnnotation1.Value == null)
						{
							continue;
						}
						yield return edmDirectValueAnnotation1;
					}
				}
				else
				{
					if (edmDirectValueAnnotation.Value != null)
					{
						yield return edmDirectValueAnnotation;
					}
				}
			}
		}
	}
}