using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    internal class PSArrayAssignmentRHSBinder : DynamicMetaObjectBinder
    {
        private readonly static List<PSArrayAssignmentRHSBinder> _binders;

        private readonly int _elements;

        public override Type ReturnType
        {
            get
            {
                return typeof(IList);
            }
        }

        static PSArrayAssignmentRHSBinder()
        {
            PSArrayAssignmentRHSBinder._binders = new List<PSArrayAssignmentRHSBinder>();
        }

        private PSArrayAssignmentRHSBinder(int elements)
        {
            this._elements = elements;
        }

        public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            int i;
            if (target.HasValue)
            {
                if (target.Value as PSObject == null || PSObject.Base(target.Value) == target.Value)
                {
                    IList value = target.Value as IList;
                    if (value == null)
                    {
                        return new DynamicMetaObject(Expression.NewArrayInit(typeof(object), Enumerable.Repeat<Expression>(ExpressionCache.NullConstant, this._elements - 1).Prepend<Expression>(target.Expression.Cast(typeof(object)))), target.PSGetTypeRestriction()).WriteToDebugLog(this);
                    }
                    else
                    {
                        MemberExpression memberExpression = Expression.Property(target.Expression.Cast(typeof(ICollection)), CachedReflectionInfo.ICollection_Count);
                        BindingRestrictions bindingRestriction = target.PSGetTypeRestriction().Merge(BindingRestrictions.GetExpressionRestriction(Expression.Equal(memberExpression, ExpressionCache.Constant(value.Count))));
                        if (value.Count != this._elements)
                        {
                            Expression[] nullConstant = new Expression[this._elements];
                            ParameterExpression parameterExpression = Expression.Variable(typeof(IList));
                            if (value.Count >= this._elements)
                            {
                                for (i = 0; i < this._elements - 1; i++)
                                {
                                    Expression[] expressionArray = new Expression[1];
                                    expressionArray[0] = ExpressionCache.Constant(i);
                                    nullConstant[i] = Expression.Call(parameterExpression, CachedReflectionInfo.IList_get_Item, expressionArray);
                                }
                                nullConstant[this._elements - 1] = Expression.Call(CachedReflectionInfo.EnumerableOps_GetSlice, parameterExpression, ExpressionCache.Constant(this._elements - 1)).Cast(typeof(object));
                            }
                            else
                            {
                                for (i = 0; i < value.Count; i++)
                                {
                                    Expression[] expressionArray1 = new Expression[1];
                                    expressionArray1[0] = ExpressionCache.Constant(i);
                                    nullConstant[i] = Expression.Call(parameterExpression, CachedReflectionInfo.IList_get_Item, expressionArray1);
                                }
                                while (i < this._elements)
                                {
                                    nullConstant[i] = ExpressionCache.NullConstant;
                                    i++;
                                }
                            }
                            ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
                            parameterExpressionArray[0] = parameterExpression;
                            Expression[] expressionArray2 = new Expression[2];
                            expressionArray2[0] = Expression.Assign(parameterExpression, target.Expression.Cast(typeof(IList)));
                            expressionArray2[1] = Expression.NewArrayInit(typeof(object), nullConstant);
                            return new DynamicMetaObject(Expression.Block(parameterExpressionArray, expressionArray2), bindingRestriction).WriteToDebugLog(this);
                        }
                        else
                        {
                            return new DynamicMetaObject(target.Expression.Cast(typeof(IList)), bindingRestriction).WriteToDebugLog(this);
                        }
                    }
                }
                else
                {
                    DynamicMetaObject[] dynamicMetaObjectArray = new DynamicMetaObject[1];
                    dynamicMetaObjectArray[0] = target;
                    return this.DeferForPSObject(dynamicMetaObjectArray).WriteToDebugLog(this);
                }
            }
            else
            {
                return base.Defer(target, new DynamicMetaObject[0]).WriteToDebugLog(this);
            }
        }

        internal static PSArrayAssignmentRHSBinder Get(int i)
        {
            PSArrayAssignmentRHSBinder pSArrayAssignmentRHSBinder;
            lock (PSArrayAssignmentRHSBinder._binders)
            {
                while (PSArrayAssignmentRHSBinder._binders.Count <= i)
                {
                    PSArrayAssignmentRHSBinder._binders.Add(null);
                }
                PSArrayAssignmentRHSBinder item = PSArrayAssignmentRHSBinder._binders[i];
                PSArrayAssignmentRHSBinder pSArrayAssignmentRHSBinder1 = item;
                if (item == null)
                {
                    PSArrayAssignmentRHSBinder pSArrayAssignmentRHSBinder2 = new PSArrayAssignmentRHSBinder(i);
                    PSArrayAssignmentRHSBinder pSArrayAssignmentRHSBinder3 = pSArrayAssignmentRHSBinder2;
                    PSArrayAssignmentRHSBinder._binders[i] = pSArrayAssignmentRHSBinder2;
                    pSArrayAssignmentRHSBinder1 = pSArrayAssignmentRHSBinder3;
                }
                pSArrayAssignmentRHSBinder = pSArrayAssignmentRHSBinder1;
            }
            return pSArrayAssignmentRHSBinder;
        }

        public override string ToString()
        {
            object[] objArray = new object[1];
            objArray[0] = this._elements;
            return string.Format(CultureInfo.InvariantCulture, "MultiAssignRHSBinder {0}", objArray);
        }
    }
}