using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.Edm.Library.Values;
using Microsoft.Data.Edm.Validation;
using Microsoft.Data.Edm.Values;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Data.Edm.Evaluation
{
	internal class EdmExpressionEvaluator
	{
		private readonly IDictionary<IEdmFunction, Func<IEdmValue[], IEdmValue>> builtInFunctions;

		private readonly Dictionary<IEdmLabeledExpression, EdmExpressionEvaluator.DelayedValue> labeledValues;

		private readonly Func<string, IEdmValue[], IEdmValue> lastChanceFunctionApplier;

		public EdmExpressionEvaluator(IDictionary<IEdmFunction, Func<IEdmValue[], IEdmValue>> builtInFunctions)
		{
			this.labeledValues = new Dictionary<IEdmLabeledExpression, EdmExpressionEvaluator.DelayedValue>();
			this.builtInFunctions = builtInFunctions;
		}

		public EdmExpressionEvaluator(IDictionary<IEdmFunction, Func<IEdmValue[], IEdmValue>> builtInFunctions, Func<string, IEdmValue[], IEdmValue> lastChanceFunctionApplier) : this(builtInFunctions)
		{
			this.lastChanceFunctionApplier = lastChanceFunctionApplier;
		}

		private static bool AssertOrMatchStructuredType(IEdmStructuredTypeReference structuredTargetType, IEdmStructuredValue structuredValue, bool testPropertyTypes, List<IEdmPropertyValue> newProperties)
		{
			bool flag;
			IEdmTypeReference type = structuredValue.Type;
			if (type == null || type.TypeKind() == EdmTypeKind.Row || structuredTargetType.StructuredDefinition().InheritsFrom(type.AsStructured().StructuredDefinition()))
			{
				HashSetInternal<IEdmPropertyValue> edmPropertyValues = new HashSetInternal<IEdmPropertyValue>();
				IEnumerator<IEdmStructuralProperty> enumerator = structuredTargetType.StructuralProperties().GetEnumerator();
				using (enumerator)
				{
					while (enumerator.MoveNext())
					{
						IEdmProperty current = enumerator.Current;
						IEdmPropertyValue edmPropertyValue = structuredValue.FindPropertyValue(current.Name);
						if (edmPropertyValue != null)
						{
							edmPropertyValues.Add(edmPropertyValue);
							if (!testPropertyTypes)
							{
								continue;
							}
							if (newProperties == null)
							{
								if (EdmExpressionEvaluator.MatchesType(current.Type, edmPropertyValue.Value))
								{
									continue;
								}
								flag = false;
								return flag;
							}
							else
							{
								newProperties.Add(new EdmPropertyValue(edmPropertyValue.Name, EdmExpressionEvaluator.AssertType(current.Type, edmPropertyValue.Value)));
							}
						}
						else
						{
							flag = false;
							return flag;
						}
					}
					if (structuredTargetType.IsEntity())
					{
						IEnumerator<IEdmNavigationProperty> enumerator1 = structuredTargetType.AsEntity().NavigationProperties().GetEnumerator();
						using (enumerator1)
						{
							while (enumerator1.MoveNext())
							{
								IEdmNavigationProperty edmNavigationProperty = enumerator1.Current;
								IEdmPropertyValue edmPropertyValue1 = structuredValue.FindPropertyValue(edmNavigationProperty.Name);
								if (edmPropertyValue1 != null)
								{
									if (!testPropertyTypes || EdmExpressionEvaluator.MatchesType(edmNavigationProperty.Type, edmPropertyValue1.Value, false))
									{
										edmPropertyValues.Add(edmPropertyValue1);
										if (newProperties == null)
										{
											continue;
										}
										newProperties.Add(edmPropertyValue1);
									}
									else
									{
										flag = false;
										return flag;
									}
								}
								else
								{
									flag = false;
									return flag;
								}
							}
						}
					}
					if (newProperties != null)
					{
						IEnumerator<IEdmPropertyValue> enumerator2 = structuredValue.PropertyValues.GetEnumerator();
						using (enumerator2)
						{
							while (enumerator2.MoveNext())
							{
								IEdmPropertyValue current1 = enumerator2.Current;
								if (edmPropertyValues.Contains(current1))
								{
									continue;
								}
								newProperties.Add(current1);
							}
						}
					}
					return true;
				}
				return flag;
			}
			else
			{
				return false;
			}
		}

		private static IEdmValue AssertType(IEdmTypeReference targetType, IEdmValue operand)
		{
			IEdmTypeReference type = operand.Type;
			EdmValueKind valueKind = operand.ValueKind;
			if ((type == null || valueKind == EdmValueKind.Null || !type.Definition.IsOrInheritsFrom(targetType.Definition)) && targetType.TypeKind() != EdmTypeKind.None)
			{
				bool flag = true;
				EdmValueKind edmValueKind = valueKind;
				if (edmValueKind == EdmValueKind.Collection)
				{
					if (!targetType.IsCollection())
					{
						flag = false;
					}
					else
					{
						return new EdmExpressionEvaluator.AssertTypeCollectionValue(targetType.AsCollection(), (IEdmCollectionValue)operand);
					}
				}
				else
				{
					if (edmValueKind == EdmValueKind.Structured)
					{
						if (!targetType.IsStructured())
						{
							flag = false;
						}
						else
						{
							IEdmStructuredTypeReference edmStructuredTypeReference = targetType.AsStructured();
							List<IEdmPropertyValue> edmPropertyValues = new List<IEdmPropertyValue>();
							flag = EdmExpressionEvaluator.AssertOrMatchStructuredType(edmStructuredTypeReference, (IEdmStructuredValue)operand, true, edmPropertyValues);
							if (flag)
							{
								return new EdmStructuredValue(edmStructuredTypeReference, edmPropertyValues);
							}
						}
					}
					else
					{
						flag = EdmExpressionEvaluator.MatchesType(targetType, operand);
					}
				}
				if (flag)
				{
					return operand;
				}
				else
				{
					throw new InvalidOperationException(Strings.Edm_Evaluator_FailedTypeAssertion(targetType.ToTraceString()));
				}
			}
			else
			{
				return operand;
			}
		}

		private IEdmValue Eval(IEdmExpression expression, IEdmStructuredValue context)
		{
			Func<IEdmValue[], IEdmValue> func = null;
			IEdmStructuredTypeReference edmStructuredTypeReference;
			IEdmCollectionTypeReference edmCollectionTypeReference;
			object traceString;
			EdmExpressionKind expressionKind = expression.ExpressionKind;
			switch (expressionKind)
			{
				case EdmExpressionKind.BinaryConstant:
				{
					return (IEdmBinaryConstantExpression)expression;
				}
				case EdmExpressionKind.BooleanConstant:
				{
					return (IEdmBooleanConstantExpression)expression;
				}
				case EdmExpressionKind.DateTimeConstant:
				{
					return (IEdmDateTimeConstantExpression)expression;
				}
				case EdmExpressionKind.DateTimeOffsetConstant:
				{
					return (IEdmDateTimeOffsetConstantExpression)expression;
				}
				case EdmExpressionKind.DecimalConstant:
				{
					return (IEdmDecimalConstantExpression)expression;
				}
				case EdmExpressionKind.FloatingConstant:
				{
					return (IEdmFloatingConstantExpression)expression;
				}
				case EdmExpressionKind.GuidConstant:
				{
					return (IEdmGuidConstantExpression)expression;
				}
				case EdmExpressionKind.IntegerConstant:
				{
					return (IEdmIntegerConstantExpression)expression;
				}
				case EdmExpressionKind.StringConstant:
				{
					return (IEdmStringConstantExpression)expression;
				}
				case EdmExpressionKind.TimeConstant:
				{
					return (IEdmTimeConstantExpression)expression;
				}
				case EdmExpressionKind.Null:
				{
					return (IEdmNullExpression)expression;
				}
				case EdmExpressionKind.Record:
				{
					IEdmRecordExpression edmRecordExpression = (IEdmRecordExpression)expression;
					EdmExpressionEvaluator.DelayedExpressionContext delayedExpressionContext = new EdmExpressionEvaluator.DelayedExpressionContext(this, context);
					List<IEdmPropertyValue> edmPropertyValues = new List<IEdmPropertyValue>();
					foreach (IEdmPropertyConstructor property in edmRecordExpression.Properties)
					{
						edmPropertyValues.Add(new EdmExpressionEvaluator.DelayedRecordProperty(delayedExpressionContext, property));
					}
					if (edmRecordExpression.DeclaredType != null)
					{
						edmStructuredTypeReference = edmRecordExpression.DeclaredType.AsStructured();
					}
					else
					{
						edmStructuredTypeReference = null;
					}
					EdmStructuredValue edmStructuredValue = new EdmStructuredValue(edmStructuredTypeReference, edmPropertyValues);
					return edmStructuredValue;
				}
				case EdmExpressionKind.Collection:
				{
					IEdmCollectionExpression edmCollectionExpression = (IEdmCollectionExpression)expression;
					EdmExpressionEvaluator.DelayedExpressionContext delayedExpressionContext1 = new EdmExpressionEvaluator.DelayedExpressionContext(this, context);
					List<IEdmDelayedValue> edmDelayedValues = new List<IEdmDelayedValue>();
					foreach (IEdmExpression element in edmCollectionExpression.Elements)
					{
						edmDelayedValues.Add(this.MapLabeledExpressionToDelayedValue(element, delayedExpressionContext1, context));
					}
					if (edmCollectionExpression.DeclaredType != null)
					{
						edmCollectionTypeReference = edmCollectionExpression.DeclaredType.AsCollection();
					}
					else
					{
						edmCollectionTypeReference = null;
					}
					EdmCollectionValue edmCollectionValue = new EdmCollectionValue(edmCollectionTypeReference, edmDelayedValues);
					return edmCollectionValue;
				}
				case EdmExpressionKind.Path:
				{
					EdmUtil.CheckArgumentNull<IEdmStructuredValue>(context, "context");
					IEdmPathExpression edmPathExpression = (IEdmPathExpression)expression;
					IEdmValue edmValue = context;
					foreach (string path in edmPathExpression.Path)
					{
						edmValue = this.FindProperty(path, edmValue);
						if (edmValue != null)
						{
							continue;
						}
						throw new InvalidOperationException(Strings.Edm_Evaluator_UnboundPath(path));
					}
					return edmValue;
				}
				case EdmExpressionKind.ParameterReference:
				case EdmExpressionKind.FunctionReference:
				case EdmExpressionKind.PropertyReference:
				case EdmExpressionKind.ValueTermReference:
				case EdmExpressionKind.EntitySetReference:
				case EdmExpressionKind.EnumMemberReference:
				{
					throw new InvalidOperationException(string.Concat("Not yet implemented: evaluation of ", expression.ExpressionKind.ToString(), " expressions."));
				}
				case EdmExpressionKind.If:
				{
					IEdmIfExpression edmIfExpression = (IEdmIfExpression)expression;
					if (!((IEdmBooleanValue)this.Eval(edmIfExpression.TestExpression, context)).Value)
					{
						return this.Eval(edmIfExpression.FalseExpression, context);
					}
					else
					{
						return this.Eval(edmIfExpression.TrueExpression, context);
					}
				}
				case EdmExpressionKind.AssertType:
				{
					IEdmAssertTypeExpression edmAssertTypeExpression = (IEdmAssertTypeExpression)expression;
					IEdmValue edmValue1 = this.Eval(edmAssertTypeExpression.Operand, context);
					IEdmTypeReference type = edmAssertTypeExpression.Type;
					return EdmExpressionEvaluator.AssertType(type, edmValue1);
				}
				case EdmExpressionKind.IsType:
				{
					IEdmIsTypeExpression edmIsTypeExpression = (IEdmIsTypeExpression)expression;
					IEdmValue edmValue2 = this.Eval(edmIsTypeExpression.Operand, context);
					IEdmTypeReference edmTypeReference = edmIsTypeExpression.Type;
					return new EdmBooleanConstant(EdmExpressionEvaluator.MatchesType(edmTypeReference, edmValue2));
				}
				case EdmExpressionKind.FunctionApplication:
				{
					IEdmApplyExpression edmApplyExpression = (IEdmApplyExpression)expression;
					IEdmExpression appliedFunction = edmApplyExpression.AppliedFunction;
					IEdmFunctionReferenceExpression edmFunctionReferenceExpression = appliedFunction as IEdmFunctionReferenceExpression;
					if (edmFunctionReferenceExpression != null)
					{
						IList<IEdmExpression> list = edmApplyExpression.Arguments.ToList<IEdmExpression>();
						IEdmValue[] edmValueArray = new IEdmValue[list.Count<IEdmExpression>()];
						int num = 0;
						foreach (IEdmExpression edmExpression in list)
						{
							int num1 = num;
							num = num1 + 1;
							edmValueArray[num1] = this.Eval(edmExpression, context);
						}
						IEdmFunction referencedFunction = edmFunctionReferenceExpression.ReferencedFunction;
						if (referencedFunction.IsBad() || !this.builtInFunctions.TryGetValue(referencedFunction, out func))
						{
							if (this.lastChanceFunctionApplier != null)
							{
								return this.lastChanceFunctionApplier(referencedFunction.FullName(), edmValueArray);
							}
						}
						else
						{
							return func(edmValueArray);
						}
					}
					if (edmFunctionReferenceExpression != null)
					{
						traceString = edmFunctionReferenceExpression.ReferencedFunction.ToTraceString();
					}
					else
					{
						traceString = string.Empty;
					}
					throw new InvalidOperationException(Strings.Edm_Evaluator_UnboundFunction(traceString));
				}
				case EdmExpressionKind.LabeledExpressionReference:
				{
					return this.MapLabeledExpressionToDelayedValue(((IEdmLabeledExpressionReferenceExpression)expression).ReferencedLabeledExpression, null, context).Value;
				}
				case EdmExpressionKind.Labeled:
				{
					return this.MapLabeledExpressionToDelayedValue(expression, new EdmExpressionEvaluator.DelayedExpressionContext(this, context), context).Value;
				}
			}
			int expressionKind1 = (int)expression.ExpressionKind;
			throw new InvalidOperationException(Strings.Edm_Evaluator_UnrecognizedExpressionKind(expressionKind1.ToString(CultureInfo.InvariantCulture)));
		}

		public IEdmValue Evaluate(IEdmExpression expression)
		{
			EdmUtil.CheckArgumentNull<IEdmExpression>(expression, "expression");
			return this.Eval(expression, null);
		}

		public IEdmValue Evaluate(IEdmExpression expression, IEdmStructuredValue context)
		{
			EdmUtil.CheckArgumentNull<IEdmExpression>(expression, "expression");
			return this.Eval(expression, context);
		}

		public IEdmValue Evaluate(IEdmExpression expression, IEdmStructuredValue context, IEdmTypeReference targetType)
		{
			EdmUtil.CheckArgumentNull<IEdmExpression>(expression, "expression");
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(targetType, "targetType");
			return EdmExpressionEvaluator.AssertType(targetType, this.Eval(expression, context));
		}

		private IEdmValue FindProperty(string name, IEdmValue context)
		{
			IEdmValue value = null;
			IEdmStructuredValue edmStructuredValue = context as IEdmStructuredValue;
			if (edmStructuredValue != null)
			{
				IEdmPropertyValue edmPropertyValue = edmStructuredValue.FindPropertyValue(name);
				if (edmPropertyValue != null)
				{
					value = edmPropertyValue.Value;
				}
			}
			return value;
		}

		private static bool FitsInSingle(double value)
		{
			if (value < -3.40282346638529E+38)
			{
				return false;
			}
			else
			{
				return value <= 3.40282346638529E+38;
			}
		}

		private static bool InRange(long value, long min, long max)
		{
			if (value < min)
			{
				return false;
			}
			else
			{
				return value <= max;
			}
		}

		private IEdmDelayedValue MapLabeledExpressionToDelayedValue(IEdmExpression expression, EdmExpressionEvaluator.DelayedExpressionContext delayedContext, IEdmStructuredValue context)
		{
			EdmExpressionEvaluator.DelayedValue delayedCollectionElement = null;
			IEdmLabeledExpression edmLabeledExpression = expression as IEdmLabeledExpression;
			if (edmLabeledExpression != null)
			{
				if (!this.labeledValues.TryGetValue(edmLabeledExpression, out delayedCollectionElement))
				{
					EdmExpressionEvaluator.DelayedExpressionContext delayedExpressionContext = delayedContext;
					EdmExpressionEvaluator.DelayedExpressionContext delayedExpressionContext1 = delayedExpressionContext;
					if (delayedExpressionContext == null)
					{
						delayedExpressionContext1 = new EdmExpressionEvaluator.DelayedExpressionContext(this, context);
					}
					delayedCollectionElement = new EdmExpressionEvaluator.DelayedCollectionElement(delayedExpressionContext1, edmLabeledExpression.Expression);
					this.labeledValues[edmLabeledExpression] = delayedCollectionElement;
					return delayedCollectionElement;
				}
				else
				{
					return delayedCollectionElement;
				}
			}
			else
			{
				return new EdmExpressionEvaluator.DelayedCollectionElement(delayedContext, expression);
			}
		}

		private static bool MatchesType(IEdmTypeReference targetType, IEdmValue operand)
		{
			return EdmExpressionEvaluator.MatchesType(targetType, operand, true);
		}

		private static bool MatchesType(IEdmTypeReference targetType, IEdmValue operand, bool testPropertyTypes)
		{
			bool flag;
			IEdmTypeReference type = operand.Type;
			EdmValueKind valueKind = operand.ValueKind;
			if (type == null || valueKind == EdmValueKind.Null || !type.Definition.IsOrInheritsFrom(targetType.Definition))
			{
				EdmValueKind edmValueKind = valueKind;
				switch (edmValueKind)
				{
					case EdmValueKind.Binary:
					{
						if (!targetType.IsBinary())
						{
							break;
						}
						IEdmBinaryTypeReference edmBinaryTypeReference = targetType.AsBinary();
						if (!edmBinaryTypeReference.IsUnbounded)
						{
							int? maxLength = edmBinaryTypeReference.MaxLength;
							if (maxLength.HasValue)
							{
								int? nullable = edmBinaryTypeReference.MaxLength;
								return nullable.Value >= (int)((IEdmBinaryValue)operand).Value.Length;
							}
						}
						return true;
					}
					case EdmValueKind.Boolean:
					{
						return targetType.IsBoolean();
					}
					case EdmValueKind.Collection:
					{
						if (!targetType.IsCollection())
						{
							break;
						}
						IEdmTypeReference edmTypeReference = targetType.AsCollection().ElementType();
						IEnumerator<IEdmDelayedValue> enumerator = ((IEdmCollectionValue)operand).Elements.GetEnumerator();
						using (enumerator)
						{
							while (enumerator.MoveNext())
							{
								IEdmDelayedValue current = enumerator.Current;
								if (EdmExpressionEvaluator.MatchesType(edmTypeReference, current.Value))
								{
									continue;
								}
								flag = false;
								return flag;
							}
							return true;
						}
						return flag;
					}
					case EdmValueKind.DateTimeOffset:
					{
						return targetType.IsDateTimeOffset();
					}
					case EdmValueKind.DateTime:
					{
						return targetType.IsDateTime();
					}
					case EdmValueKind.Decimal:
					{
						return targetType.IsDecimal();
					}
					case EdmValueKind.Enum:
					{
						return ((IEdmEnumValue)operand).Type.Definition.IsEquivalentTo(targetType.Definition);
					}
					case EdmValueKind.Floating:
					{
						if (targetType.IsDouble())
						{
							return true;
						}
						else
						{
							if (!targetType.IsSingle())
							{
								return false;
							}
							else
							{
								return EdmExpressionEvaluator.FitsInSingle(((IEdmFloatingValue)operand).Value);
							}
						}
					}
					case EdmValueKind.Guid:
					{
						return targetType.IsGuid();
					}
					case EdmValueKind.Integer:
					{
						if (targetType.TypeKind() != EdmTypeKind.Primitive)
						{
							break;
						}
						EdmPrimitiveTypeKind edmPrimitiveTypeKind = targetType.AsPrimitive().PrimitiveKind();
						if (edmPrimitiveTypeKind == EdmPrimitiveTypeKind.Byte)
						{
							return EdmExpressionEvaluator.InRange(((IEdmIntegerValue)operand).Value, (long)0, (long)0xff);
						}
						else if (edmPrimitiveTypeKind == EdmPrimitiveTypeKind.DateTime || edmPrimitiveTypeKind == EdmPrimitiveTypeKind.DateTimeOffset || edmPrimitiveTypeKind == EdmPrimitiveTypeKind.Decimal || edmPrimitiveTypeKind == EdmPrimitiveTypeKind.Guid)
						{
							break;
						}
						else if (edmPrimitiveTypeKind == EdmPrimitiveTypeKind.Double || edmPrimitiveTypeKind == EdmPrimitiveTypeKind.Int64 || edmPrimitiveTypeKind == EdmPrimitiveTypeKind.Single)
						{
							return true;
						}
						else if (edmPrimitiveTypeKind == EdmPrimitiveTypeKind.Int16)
						{
							return EdmExpressionEvaluator.InRange(((IEdmIntegerValue)operand).Value, (long)-32768, (long)0x7fff);
						}
						else if (edmPrimitiveTypeKind == EdmPrimitiveTypeKind.Int32)
						{
							return EdmExpressionEvaluator.InRange(((IEdmIntegerValue)operand).Value, (long)-2147483648, (long)0x7fffffff);
						}
						else if (edmPrimitiveTypeKind == EdmPrimitiveTypeKind.SByte)
						{
							return EdmExpressionEvaluator.InRange(((IEdmIntegerValue)operand).Value, (long)-128, (long)127);
						}
						break;
					}
					case EdmValueKind.Null:
					{
						return targetType.IsNullable;
					}
					case EdmValueKind.String:
					{
						if (!targetType.IsString())
						{
							break;
						}
						IEdmStringTypeReference edmStringTypeReference = targetType.AsString();
						if (!edmStringTypeReference.IsUnbounded)
						{
							int? maxLength1 = edmStringTypeReference.MaxLength;
							if (maxLength1.HasValue)
							{
								int? nullable1 = edmStringTypeReference.MaxLength;
								return nullable1.Value >= ((IEdmStringValue)operand).Value.Length;
							}
						}
						return true;
					}
					case EdmValueKind.Structured:
					{
						if (!targetType.IsStructured())
						{
							break;
						}
						return EdmExpressionEvaluator.AssertOrMatchStructuredType(targetType.AsStructured(), (IEdmStructuredValue)operand, testPropertyTypes, null);
					}
					case EdmValueKind.Time:
					{
						return targetType.IsTime();
					}
				}
				return false;
			}
			else
			{
				return true;
			}
			return true;
		}

		private class AssertTypeCollectionValue : EdmElement, IEdmCollectionValue, IEdmValue, IEdmElement, IEnumerable<IEdmDelayedValue>, IEnumerable
		{
			private readonly IEdmCollectionTypeReference targetCollectionType;

			private readonly IEdmCollectionValue collectionValue;

			IEnumerable<IEdmDelayedValue> Microsoft.Data.Edm.Values.IEdmCollectionValue.Elements
			{
				get
				{
					return this;
				}
			}

			IEdmTypeReference Microsoft.Data.Edm.Values.IEdmValue.Type
			{
				get
				{
					return this.targetCollectionType;
				}
			}

			EdmValueKind Microsoft.Data.Edm.Values.IEdmValue.ValueKind
			{
				get
				{
					return EdmValueKind.Collection;
				}
			}

			public AssertTypeCollectionValue(IEdmCollectionTypeReference targetCollectionType, IEdmCollectionValue collectionValue)
			{
				this.targetCollectionType = targetCollectionType;
				this.collectionValue = collectionValue;
			}

			IEnumerator<IEdmDelayedValue> System.Collections.Generic.IEnumerable<Microsoft.Data.Edm.Values.IEdmDelayedValue>.GetEnumerator()
			{
				return new EdmExpressionEvaluator.AssertTypeCollectionValue.AssertTypeCollectionValueEnumerator(this);
			}

			IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return new EdmExpressionEvaluator.AssertTypeCollectionValue.AssertTypeCollectionValueEnumerator(this);
			}

			private class AssertTypeCollectionValueEnumerator : IEnumerator<IEdmDelayedValue>, IDisposable, IEnumerator
			{
				private readonly EdmExpressionEvaluator.AssertTypeCollectionValue @value;

				private readonly IEnumerator<IEdmDelayedValue> enumerator;

				public IEdmDelayedValue Current
				{
					get
					{
						return new EdmExpressionEvaluator.AssertTypeCollectionValue.AssertTypeCollectionValueEnumerator.DelayedAssertType(this.@value.targetCollectionType.ElementType(), this.enumerator.Current);
					}
				}

				object System.Collections.IEnumerator.Current
				{
					get
					{
						return this.Current;
					}
				}

				public AssertTypeCollectionValueEnumerator(EdmExpressionEvaluator.AssertTypeCollectionValue value)
				{
					this.@value = value;
					this.enumerator = value.collectionValue.Elements.GetEnumerator();
				}

				bool System.Collections.IEnumerator.MoveNext()
				{
					return this.enumerator.MoveNext();
				}

				void System.Collections.IEnumerator.Reset()
				{
					this.enumerator.Reset();
				}

				void System.IDisposable.Dispose()
				{
					this.enumerator.Dispose();
				}

				private class DelayedAssertType : IEdmDelayedValue
				{
					private readonly IEdmDelayedValue delayedValue;

					private readonly IEdmTypeReference targetType;

					private IEdmValue @value;

					public IEdmValue Value
					{
						get
						{
							if (this.@value == null)
							{
								this.@value = EdmExpressionEvaluator.AssertType(this.targetType, this.delayedValue.Value);
							}
							return this.@value;
						}
					}

					public DelayedAssertType(IEdmTypeReference targetType, IEdmDelayedValue value)
					{
						this.delayedValue = value;
						this.targetType = targetType;
					}
				}
			}
		}

		private class DelayedCollectionElement : EdmExpressionEvaluator.DelayedValue
		{
			private readonly IEdmExpression expression;

			public override IEdmExpression Expression
			{
				get
				{
					return this.expression;
				}
			}

			public DelayedCollectionElement(EdmExpressionEvaluator.DelayedExpressionContext context, IEdmExpression expression) : base(context)
			{
				this.expression = expression;
			}
		}

		private class DelayedExpressionContext
		{
			private readonly EdmExpressionEvaluator expressionEvaluator;

			private readonly IEdmStructuredValue context;

			public DelayedExpressionContext(EdmExpressionEvaluator expressionEvaluator, IEdmStructuredValue context)
			{
				this.expressionEvaluator = expressionEvaluator;
				this.context = context;
			}

			public IEdmValue Eval(IEdmExpression expression)
			{
				return this.expressionEvaluator.Eval(expression, this.context);
			}
		}

		private class DelayedRecordProperty : EdmExpressionEvaluator.DelayedValue, IEdmPropertyValue, IEdmDelayedValue
		{
			private readonly IEdmPropertyConstructor constructor;

			public override IEdmExpression Expression
			{
				get
				{
					return this.constructor.Value;
				}
			}

			public string Name
			{
				get
				{
					return this.constructor.Name;
				}
			}

			public DelayedRecordProperty(EdmExpressionEvaluator.DelayedExpressionContext context, IEdmPropertyConstructor constructor) : base(context)
			{
				this.constructor = constructor;
			}
		}

		private abstract class DelayedValue : IEdmDelayedValue
		{
			private readonly EdmExpressionEvaluator.DelayedExpressionContext context;

			private IEdmValue @value;

			public abstract IEdmExpression Expression
			{
				get;
			}

			public IEdmValue Value
			{
				get
				{
					if (this.@value == null)
					{
						this.@value = this.context.Eval(this.Expression);
					}
					return this.@value;
				}
			}

			public DelayedValue(EdmExpressionEvaluator.DelayedExpressionContext context)
			{
				this.context = context;
			}
		}
	}
}