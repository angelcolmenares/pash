using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.Edm.Library.Internal;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal abstract class CsdlSemanticsVocabularyAnnotation : CsdlSemanticsElement, IEdmVocabularyAnnotation, IEdmElement, IEdmCheckable
	{
		protected readonly CsdlVocabularyAnnotationBase Annotation;

		private readonly CsdlSemanticsSchema schema;

		private readonly string qualifier;

		private readonly IEdmVocabularyAnnotatable targetContext;

		private readonly CsdlSemanticsAnnotations annotationsContext;

		private readonly Cache<CsdlSemanticsVocabularyAnnotation, IEdmTerm> termCache;

		private readonly static Func<CsdlSemanticsVocabularyAnnotation, IEdmTerm> ComputeTermFunc;

		private readonly Cache<CsdlSemanticsVocabularyAnnotation, IEdmVocabularyAnnotatable> targetCache;

		private readonly static Func<CsdlSemanticsVocabularyAnnotation, IEdmVocabularyAnnotatable> ComputeTargetFunc;

		public override CsdlElement Element
		{
			get
			{
				return this.Annotation;
			}
		}

		public IEnumerable<EdmError> Errors
		{
			get
			{
				if (this.Term as IUnresolvedElement == null)
				{
					if (this.Target as IUnresolvedElement == null)
					{
						return Enumerable.Empty<EdmError>();
					}
					else
					{
						return this.Target.Errors();
					}
				}
				else
				{
					return this.Term.Errors();
				}
			}
		}

		public override CsdlSemanticsModel Model
		{
			get
			{
				return this.schema.Model;
			}
		}

		public string Qualifier
		{
			get
			{
				return this.qualifier;
			}
		}

		public CsdlSemanticsSchema Schema
		{
			get
			{
				return this.schema;
			}
		}

		public IEdmVocabularyAnnotatable Target
		{
			get
			{
				return this.targetCache.GetValue(this, CsdlSemanticsVocabularyAnnotation.ComputeTargetFunc, null);
			}
		}

		public IEdmEntityType TargetBindingContext
		{
			get
			{
				IEdmVocabularyAnnotatable target = this.Target;
				IEdmEntityType elementType = target as IEdmEntityType;
				if (elementType == null)
				{
					IEdmEntitySet edmEntitySet = target as IEdmEntitySet;
					if (edmEntitySet != null)
					{
						elementType = edmEntitySet.ElementType;
					}
				}
				return elementType;
			}
		}

		public IEdmTerm Term
		{
			get
			{
				return this.termCache.GetValue(this, CsdlSemanticsVocabularyAnnotation.ComputeTermFunc, null);
			}
		}

		static CsdlSemanticsVocabularyAnnotation()
		{
			CsdlSemanticsVocabularyAnnotation.ComputeTermFunc = (CsdlSemanticsVocabularyAnnotation me) => me.ComputeTerm();
			CsdlSemanticsVocabularyAnnotation.ComputeTargetFunc = (CsdlSemanticsVocabularyAnnotation me) => me.ComputeTarget();
		}

		protected CsdlSemanticsVocabularyAnnotation(CsdlSemanticsSchema schema, IEdmVocabularyAnnotatable targetContext, CsdlSemanticsAnnotations annotationsContext, CsdlVocabularyAnnotationBase annotation, string qualifier) : base(annotation)
		{
			this.termCache = new Cache<CsdlSemanticsVocabularyAnnotation, IEdmTerm>();
			this.targetCache = new Cache<CsdlSemanticsVocabularyAnnotation, IEdmVocabularyAnnotatable>();
			this.schema = schema;
			this.Annotation = annotation;
			string str = qualifier;
			string str1 = str;
			if (str == null)
			{
				str1 = annotation.Qualifier;
			}
			this.qualifier = str1;
			this.targetContext = targetContext;
			this.annotationsContext = annotationsContext;
		}

		private IEdmVocabularyAnnotatable ComputeTarget()
		{
			IEdmEntityContainer edmEntityContainer;
			if (this.targetContext == null)
			{
				string target = this.annotationsContext.Annotations.Target;
				char[] chrArray = new char[1];
				chrArray[0] = '/';
				string[] strArrays = target.Split(chrArray);
				int num = strArrays.Count<string>();
				if (num != 1)
				{
					if (num != 2)
					{
						if (num == 3)
						{
							edmEntityContainer = this.Model.FindEntityContainer(strArrays[0]);
							if (edmEntityContainer != null)
							{
								IEdmEntityContainer edmEntityContainer1 = edmEntityContainer;
								IEdmFunctionImport edmFunctionImport = this.FindParameterizedFunction<IEdmFunctionImport>(strArrays[1], new Func<string, IEnumerable<IEdmFunctionImport>>(edmEntityContainer1.FindFunctionImports), new Func<IEnumerable<IEdmFunctionImport>, IEdmFunctionImport>(this.CreateAmbiguousFunctionImport));
								if (edmFunctionImport != null)
								{
									IEdmFunctionParameter edmFunctionParameter = edmFunctionImport.FindParameter(strArrays[2]);
									if (edmFunctionParameter == null)
									{
										return new UnresolvedParameter(edmFunctionImport, strArrays[1], base.Location);
									}
									else
									{
										return edmFunctionParameter;
									}
								}
							}
						}
						EdmError[] edmError = new EdmError[1];
						edmError[0] = new EdmError(base.Location, EdmErrorCode.ImpossibleAnnotationsTarget, Strings.CsdlSemantics_ImpossibleAnnotationsTarget(target));
						return new BadElement(edmError);
					}
					else
					{
						edmEntityContainer = this.schema.FindEntityContainer(strArrays[0]);
						if (edmEntityContainer == null)
						{
							IEdmStructuredType edmStructuredType = this.schema.FindType(strArrays[0]) as IEdmStructuredType;
							if (edmStructuredType == null)
							{
								IEdmFunction edmFunction = this.FindParameterizedFunction<IEdmFunction>(strArrays[0], new Func<string, IEnumerable<IEdmFunction>>(this.Schema.FindFunctions), new Func<IEnumerable<IEdmFunction>, IEdmFunction>(this.CreateAmbiguousFunction));
								if (edmFunction == null)
								{
									return new UnresolvedProperty(new UnresolvedEntityType(this.Schema.UnresolvedName(strArrays[0]), base.Location), strArrays[1], base.Location);
								}
								else
								{
									IEdmFunctionParameter edmFunctionParameter1 = edmFunction.FindParameter(strArrays[1]);
									if (edmFunctionParameter1 == null)
									{
										return new UnresolvedParameter(edmFunction, strArrays[1], base.Location);
									}
									else
									{
										return edmFunctionParameter1;
									}
								}
							}
							else
							{
								IEdmProperty edmProperty = edmStructuredType.FindProperty(strArrays[1]);
								if (edmProperty == null)
								{
									return new UnresolvedProperty(edmStructuredType, strArrays[1], base.Location);
								}
								else
								{
									return edmProperty;
								}
							}
						}
						else
						{
							IEdmEntityContainerElement edmEntityContainerElement = edmEntityContainer.FindEntitySet(strArrays[1]);
							if (edmEntityContainerElement == null)
							{
								IEdmEntityContainer edmEntityContainer2 = edmEntityContainer;
								IEdmFunctionImport edmFunctionImport1 = this.FindParameterizedFunction<IEdmFunctionImport>(strArrays[1], new Func<string, IEnumerable<IEdmFunctionImport>>(edmEntityContainer2.FindFunctionImports), new Func<IEnumerable<IEdmFunctionImport>, IEdmFunctionImport>(this.CreateAmbiguousFunctionImport));
								if (edmFunctionImport1 == null)
								{
									return new UnresolvedEntitySet(strArrays[1], edmEntityContainer, base.Location);
								}
								else
								{
									return edmFunctionImport1;
								}
							}
							else
							{
								return edmEntityContainerElement;
							}
						}
					}
				}
				else
				{
					string str = strArrays[0];
					IEdmSchemaType edmSchemaType = this.schema.FindType(str);
					if (edmSchemaType == null)
					{
						IEdmValueTerm edmValueTerm = this.schema.FindValueTerm(str);
						if (edmValueTerm == null)
						{
							IEdmFunction edmFunction1 = this.FindParameterizedFunction<IEdmFunction>(str, new Func<string, IEnumerable<IEdmFunction>>(this.Schema.FindFunctions), new Func<IEnumerable<IEdmFunction>, IEdmFunction>(this.CreateAmbiguousFunction));
							if (edmFunction1 == null)
							{
								edmEntityContainer = this.schema.FindEntityContainer(str);
								if (edmEntityContainer == null)
								{
									return new UnresolvedType(this.Schema.UnresolvedName(strArrays[0]), base.Location);
								}
								else
								{
									return edmEntityContainer;
								}
							}
							else
							{
								return edmFunction1;
							}
						}
						else
						{
							return edmValueTerm;
						}
					}
					else
					{
						return edmSchemaType;
					}
				}
			}
			else
			{
				return this.targetContext;
			}
		}

		protected abstract IEdmTerm ComputeTerm();

		private IEdmFunction CreateAmbiguousFunction(IEnumerable<IEdmFunction> functions)
		{
			IEnumerator<IEdmFunction> enumerator = functions.GetEnumerator();
			enumerator.MoveNext();
			IEdmFunction current = enumerator.Current;
			enumerator.MoveNext();
			IEdmFunction edmFunction = enumerator.Current;
			AmbiguousFunctionBinding ambiguousFunctionBinding = new AmbiguousFunctionBinding(current, edmFunction);
			while (enumerator.MoveNext())
			{
				ambiguousFunctionBinding.AddBinding(enumerator.Current);
			}
			return ambiguousFunctionBinding;
		}

		private IEdmFunctionImport CreateAmbiguousFunctionImport(IEnumerable<IEdmFunctionImport> functions)
		{
			IEnumerator<IEdmFunctionImport> enumerator = functions.GetEnumerator();
			enumerator.MoveNext();
			IEdmFunctionImport current = enumerator.Current;
			enumerator.MoveNext();
			IEdmFunctionImport edmFunctionImport = enumerator.Current;
			AmbiguousFunctionImportBinding ambiguousFunctionImportBinding = new AmbiguousFunctionImportBinding(current, edmFunctionImport);
			while (enumerator.MoveNext())
			{
				ambiguousFunctionImportBinding.AddBinding(enumerator.Current);
			}
			return ambiguousFunctionImportBinding;
		}

		private T FindParameterizedFunction<T>(string parameterizedName, Func<string, IEnumerable<T>> findFunctions, Func<IEnumerable<T>, T> ambiguityCreator)
			where T : class, IEdmFunctionBase
		{
			int num = parameterizedName.IndexOf('(');
			int num1 = parameterizedName.LastIndexOf(')');
			if (num >= 0)
			{
				string str = parameterizedName.Substring(0, num);
				string[] strArrays = new string[1];
				strArrays[0] = ", ";
				string[] strArrays1 = parameterizedName.Substring(num + 1, num1 - num + 1).Split(strArrays, StringSplitOptions.RemoveEmptyEntries);
				IEnumerable<T> ts = this.FindParameterizedFunctionFromList(findFunctions(str).Cast<IEdmFunctionBase>(), strArrays1).Cast<T>();
				if (ts.Count<T>() != 0)
				{
					if (ts.Count<T>() != 1)
					{
						T t = ambiguityCreator(ts);
						return t;
					}
					else
					{
						return ts.First<T>();
					}
				}
				else
				{
					T t1 = default(T);
					return t1;
				}
			}
			else
			{
				T t2 = default(T);
				return t2;
			}
		}

		private IEnumerable<IEdmFunctionBase> FindParameterizedFunctionFromList(IEnumerable<IEdmFunctionBase> functions, string[] parameters)
		{
			bool flag;
			bool flag1;
			bool flag2;
			List<IEdmFunctionBase> edmFunctionBases = new List<IEdmFunctionBase>();
			foreach (IEdmFunctionBase parameter in functions)
			{
				if (parameter.Parameters.Count<IEdmFunctionParameter>() != parameters.Count<string>())
				{
					continue;
				}
				bool flag3 = true;
				IEnumerator<string> enumerator = ((IEnumerable<string>)parameters).GetEnumerator();
				IEnumerator<IEdmFunctionParameter> enumerator1 = parameter.Parameters.GetEnumerator();
				using (enumerator1)
				{
					do
					{
						if (!enumerator1.MoveNext())
						{
							break;
						}
						IEdmFunctionParameter edmFunctionParameter = enumerator1.Current;
						enumerator.MoveNext();
						char[] chrArray = new char[2];
						chrArray[0] = '(';
						chrArray[1] = ')';
						string[] strArrays = enumerator.Current.Split(chrArray);
						string str = strArrays[0];
						string str1 = str;
						if (str != null)
						{
							if (str1 == "Collection")
							{
								if (!edmFunctionParameter.Type.IsCollection())
								{
									flag1 = false;
								}
								else
								{
									flag1 = this.Schema.FindType(strArrays[1]).IsEquivalentTo(edmFunctionParameter.Type.AsCollection().ElementType().Definition);
								}
								flag3 = flag1;
								continue;
							}
							else
							{
								if (str1 == "Ref")
								{
									if (!edmFunctionParameter.Type.IsEntityReference())
									{
										flag2 = false;
									}
									else
									{
										flag2 = this.Schema.FindType(strArrays[1]).IsEquivalentTo(edmFunctionParameter.Type.AsEntityReference().EntityType());
									}
									flag3 = flag2;
									continue;
								}
							}
						}
						if (EdmCoreModel.Instance.FindDeclaredType(enumerator.Current).IsEquivalentTo(edmFunctionParameter.Type.Definition))
						{
							flag = true;
						}
						else
						{
							flag = this.Schema.FindType(enumerator.Current).IsEquivalentTo(edmFunctionParameter.Type.Definition);
						}
						flag3 = flag;
					}
					while (flag3);
				}
				if (!flag3)
				{
					continue;
				}
				edmFunctionBases.Add(parameter);
			}
			return edmFunctionBases;
		}
	}
}