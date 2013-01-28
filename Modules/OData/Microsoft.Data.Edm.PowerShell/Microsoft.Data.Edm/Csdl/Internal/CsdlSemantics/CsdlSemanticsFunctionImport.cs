using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library.Expressions;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsFunctionImport : CsdlSemanticsFunctionBase, IEdmFunctionImport, IEdmFunctionBase, IEdmEntityContainerElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly CsdlFunctionImport functionImport;

		private readonly CsdlSemanticsEntityContainer container;

		private readonly Cache<CsdlSemanticsFunctionImport, IEdmExpression> entitySetCache;

		private readonly static Func<CsdlSemanticsFunctionImport, IEdmExpression> ComputeEntitySetFunc;

		public IEdmEntityContainer Container
		{
			get
			{
				return this.container;
			}
		}

		public IEdmExpression EntitySet
		{
			get
			{
				return this.entitySetCache.GetValue(this, CsdlSemanticsFunctionImport.ComputeEntitySetFunc, null);
			}
		}

		public bool IsBindable
		{
			get
			{
				return this.functionImport.Bindable;
			}
		}

		public bool IsComposable
		{
			get
			{
				return this.functionImport.Composable;
			}
		}

		public bool IsSideEffecting
		{
			get
			{
				return this.functionImport.SideEffecting;
			}
		}

		static CsdlSemanticsFunctionImport()
		{
			CsdlSemanticsFunctionImport.ComputeEntitySetFunc = (CsdlSemanticsFunctionImport me) => me.ComputeEntitySet();
		}

		public CsdlSemanticsFunctionImport(CsdlSemanticsEntityContainer container, CsdlFunctionImport functionImport) : base(container.Context, functionImport)
		{
			this.entitySetCache = new Cache<CsdlSemanticsFunctionImport, IEdmExpression>();
			this.container = container;
			this.functionImport = functionImport;
		}

		private IEdmExpression ComputeEntitySet()
		{
			if (this.functionImport.EntitySet == null)
			{
				if (this.functionImport.EntitySetPath == null)
				{
					return null;
				}
				else
				{
					CsdlSemanticsFunctionImport.FunctionImportPathExpression functionImportPathExpression = new CsdlSemanticsFunctionImport.FunctionImportPathExpression(this.functionImport.EntitySetPath);
					functionImportPathExpression.Location = base.Location;
					return functionImportPathExpression;
				}
			}
			else
			{
				IEdmEntitySet edmEntitySet = this.container.FindEntitySet(this.functionImport.EntitySet);
				IEdmEntitySet unresolvedEntitySet = edmEntitySet;
				if (edmEntitySet == null)
				{
					unresolvedEntitySet = new UnresolvedEntitySet(this.functionImport.EntitySet, this.Container, base.Location);
				}
				IEdmEntitySet edmEntitySet1 = unresolvedEntitySet;
				CsdlSemanticsFunctionImport.FunctionImportEntitySetReferenceExpression functionImportEntitySetReferenceExpression = new CsdlSemanticsFunctionImport.FunctionImportEntitySetReferenceExpression(edmEntitySet1);
				functionImportEntitySetReferenceExpression.Location = base.Location;
				return functionImportEntitySetReferenceExpression;
			}
		}

		private sealed class FunctionImportEntitySetReferenceExpression : EdmEntitySetReferenceExpression, IEdmLocatable
		{
			private EdmLocation JustDecompileGenerated_k__BackingField;

			public EdmLocation JustDecompileGenerated_get_Location()
			{
				return this.JustDecompileGenerated_k__BackingField;
			}

			public void JustDecompileGenerated_set_Location(EdmLocation value)
			{
				this.JustDecompileGenerated_k__BackingField = value;
			}

			public EdmLocation Location
			{
				get
				{
					return JustDecompileGenerated_get_Location();
				}
				set
				{
					JustDecompileGenerated_set_Location(value);
				}
			}

			internal FunctionImportEntitySetReferenceExpression(IEdmEntitySet referencedEntitySet) : base(referencedEntitySet)
			{
			}
		}

		private sealed class FunctionImportPathExpression : EdmPathExpression, IEdmLocatable
		{
			private EdmLocation JustDecompileGenerated_k__BackingField;

			public EdmLocation JustDecompileGenerated_get_Location()
			{
				return this.JustDecompileGenerated_k__BackingField;
			}

			public void JustDecompileGenerated_set_Location(EdmLocation value)
			{
				this.JustDecompileGenerated_k__BackingField = value;
			}

			public EdmLocation Location
			{
				get
				{
					return JustDecompileGenerated_get_Location();
				}
				set
				{
					JustDecompileGenerated_set_Location(value);
				}
			}

			internal FunctionImportPathExpression(string path) : base(path)
			{
			}
		}
	}
}