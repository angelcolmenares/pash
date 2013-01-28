using Microsoft.Management.Odata.Schema;
using System;
using System.Collections.Generic;
using System.Data.Services.Providers;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.Management.Odata.Core
{
	internal class ReferredResourceExtractor : ExpressionVisitor
	{
		private ReferredResourceExtractor.ExtractionState currentState;

		private Dictionary<string, object> referredEntityKeys;

		private Dictionary<string, object> referringEntityKeys;

		private IQueryable<DSResource> resourceRoot;

		private ResourceProperty navigationProperty;

		private EntityMetadata entityMetadata;

		public DSResource ReferredResource
		{
			get;
			private set;
		}

		public ReferredResourceExtractor()
		{
		}

		public bool Extract(Expression tree, IQueryable<DSResource> resourceRoot, ResourceType resourceType, EntityMetadata entityMetadata)
		{
			this.resourceRoot = resourceRoot;
			this.entityMetadata = entityMetadata;
			this.navigationProperty = null;
			this.referredEntityKeys = new Dictionary<string, object>();
			this.referringEntityKeys = new Dictionary<string, object>();
			this.currentState = ReferredResourceExtractor.ExtractionState.ExtractingReferredEntityInfo;
			this.Visit(tree);
			if (this.currentState == ReferredResourceExtractor.ExtractionState.ExtractingReferringEntityInfo)
			{
				DSResource dSResource = ResourceTypeExtensions.CreateKeyOnlyResource(resourceType, this.referringEntityKeys);
				if (dSResource != null)
				{
					this.ReferredResource = ResourceTypeExtensions.CreateKeyOnlyResource(this.navigationProperty.ResourceType, this.referredEntityKeys);
					if (this.ReferredResource != null)
					{
						this.currentState = ReferredResourceExtractor.ExtractionState.ExtractionDone;
					}
				}
			}
			if (this.currentState != ReferredResourceExtractor.ExtractionState.ExtractionDone)
			{
				this.currentState = ReferredResourceExtractor.ExtractionState.ExtractionFailed;
			}
			return this.currentState == ReferredResourceExtractor.ExtractionState.ExtractionDone;
		}

		protected override Expression VisitMethodCall(MethodCallExpression expression)
		{
			ResourceProperty resourceProperty = null;
			object obj = null;
			if (this.currentState != ReferredResourceExtractor.ExtractionState.ExtractionFailed)
			{
				if (expression.Method.Name != "Where")
				{
					if (expression.Method.Name != "SelectMany")
					{
						this.currentState = ReferredResourceExtractor.ExtractionState.ExtractionFailed;
					}
					else
					{
						if (this.currentState == ReferredResourceExtractor.ExtractionState.ExtractingReferredEntityInfo)
						{
							this.navigationProperty = ExpressionHelper.GetResourcePropertyFromSequence(expression.Arguments[1]);
							if (!this.navigationProperty.IsReferenceSetProperty())
							{
								this.currentState = ReferredResourceExtractor.ExtractionState.ExtractionFailed;
							}
							else
							{
								this.currentState = ReferredResourceExtractor.ExtractionState.ExtractingReferringEntityInfo;
							}
						}
						else
						{
							this.currentState = ReferredResourceExtractor.ExtractionState.ExtractionFailed;
							return expression;
						}
					}
				}
				else
				{
					if (ExpressionHelper.IsResourceRoot(expression.Arguments[0], this.resourceRoot) || ExpressionHelper.IsNestedWhereClause(expression.Arguments[0], this.resourceRoot))
					{
						if (this.currentState != ReferredResourceExtractor.ExtractionState.ExtractingReferringEntityInfo || this.navigationProperty == null)
						{
							this.currentState = ReferredResourceExtractor.ExtractionState.ExtractionFailed;
							return expression;
						}
					}
					else
					{
						if (this.currentState != ReferredResourceExtractor.ExtractionState.ExtractingReferredEntityInfo || this.navigationProperty != null)
						{
							this.currentState = ReferredResourceExtractor.ExtractionState.ExtractionFailed;
							return expression;
						}
					}
					if (ExpressionHelper.GetResourcePropertyAndValueFromLambda(expression.Arguments[1], out resourceProperty, out obj))
					{
						if (this.currentState != ReferredResourceExtractor.ExtractionState.ExtractingReferringEntityInfo)
						{
							this.referredEntityKeys.Add(resourceProperty.Name, obj);
						}
						else
						{
							this.referringEntityKeys.Add(resourceProperty.Name, obj);
						}
					}
					else
					{
						this.currentState = ReferredResourceExtractor.ExtractionState.ExtractionFailed;
						return expression;
					}
				}
				this.Visit(expression.Arguments[0]);
				return expression;
			}
			else
			{
				return expression;
			}
		}

		private enum ExtractionState
		{
			ExtractingReferredEntityInfo,
			ExtractingReferringEntityInfo,
			ExtractionDone,
			ExtractionFailed
		}
	}
}