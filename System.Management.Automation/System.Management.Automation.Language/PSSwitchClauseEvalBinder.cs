using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace System.Management.Automation.Language
{
    internal class PSSwitchClauseEvalBinder : DynamicMetaObjectBinder
    {
        private readonly static PSSwitchClauseEvalBinder[] _binderCache;

        private readonly SwitchFlags _flags;

        public override Type ReturnType
        {
            get
            {
                return typeof(bool);
            }
        }

        static PSSwitchClauseEvalBinder()
        {
            PSSwitchClauseEvalBinder._binderCache = new PSSwitchClauseEvalBinder[32];
        }

        private PSSwitchClauseEvalBinder(SwitchFlags flags)
        {
            this._flags = flags;
        }

        public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            if (!target.HasValue || !args[0].HasValue)
            {
                DynamicMetaObject[] dynamicMetaObjectArray = new DynamicMetaObject[2];
                dynamicMetaObjectArray[0] = args[0];
                dynamicMetaObjectArray[1] = args[1];
                return base.Defer(target, dynamicMetaObjectArray).WriteToDebugLog(this);
            }
            else
            {
                BindingRestrictions bindingRestriction = target.PSGetTypeRestriction();
                if (target.Value as PSObject == null)
                {
                    if (target.Value != null)
                    {
                        if (target.Value as ScriptBlock == null)
                        {
                            Expression expression = args[1].Expression;
                            DynamicExpression dynamicExpression = Expression.Dynamic(PSToStringBinder.Get(), typeof(string), args[0].Expression, expression);
                            if (target.Value as Regex != null || (this._flags & SwitchFlags.Regex) != SwitchFlags.None)
                            {
                                MethodCallExpression methodCallExpression = Expression.Call(CachedReflectionInfo.SwitchOps_ConditionSatisfiedRegex, ExpressionCache.Constant((this._flags & SwitchFlags.CaseSensitive) != SwitchFlags.None), target.Expression.Cast(typeof(object)), ExpressionCache.NullExtent, dynamicExpression, expression);
                                return new DynamicMetaObject(methodCallExpression, bindingRestriction).WriteToDebugLog(this);
                            }
                            else
                            {
                                if (target.Value as WildcardPattern != null || (this._flags & SwitchFlags.Wildcard) != SwitchFlags.None)
                                {
                                    MethodCallExpression methodCallExpression1 = Expression.Call(CachedReflectionInfo.SwitchOps_ConditionSatisfiedWildcard, ExpressionCache.Constant((this._flags & SwitchFlags.CaseSensitive) != SwitchFlags.None), target.Expression.Cast(typeof(object)), dynamicExpression, expression);
                                    return new DynamicMetaObject(methodCallExpression1, bindingRestriction).WriteToDebugLog(this);
                                }
                                else
                                {
                                    DynamicExpression dynamicExpression1 = Expression.Dynamic(PSToStringBinder.Get(), typeof(string), target.Expression, expression);
                                    return new DynamicMetaObject(Compiler.CallStringEquals(dynamicExpression1, dynamicExpression, (this._flags & SwitchFlags.CaseSensitive) == SwitchFlags.None), bindingRestriction).WriteToDebugLog(this);
                                }
                            }
                        }
                        else
                        {
                            Expression[] automationNullConstant = new Expression[6];
                            automationNullConstant[0] = ExpressionCache.Constant(true);
                            automationNullConstant[1] = Expression.Constant(ScriptBlock.ErrorHandlingBehavior.WriteToExternalErrorPipe);
                            automationNullConstant[2] = args[0].CastOrConvert(typeof(object));
                            automationNullConstant[3] = ExpressionCache.AutomationNullConstant;
                            automationNullConstant[4] = ExpressionCache.AutomationNullConstant;
                            automationNullConstant[5] = ExpressionCache.NullObjectArray;
                            MethodCallExpression methodCallExpression2 = Expression.Call(target.Expression.Cast(typeof(ScriptBlock)), CachedReflectionInfo.ScriptBlock_DoInvokeReturnAsIs, automationNullConstant);
                            return new DynamicMetaObject(Expression.Dynamic(PSConvertBinder.Get(typeof(bool)), typeof(bool), methodCallExpression2), bindingRestriction).WriteToDebugLog(this);
                        }
                    }
                    else
                    {
                        return new DynamicMetaObject(Expression.Equal(args[0].Expression.Cast(typeof(object)), ExpressionCache.NullConstant), target.PSGetTypeRestriction()).WriteToDebugLog(this);
                    }
                }
                else
                {
                    return new DynamicMetaObject(Expression.Dynamic(this, this.ReturnType, Expression.Call(CachedReflectionInfo.PSObject_Base, target.Expression.Cast(typeof(object))), args[0].Expression, args[1].Expression), bindingRestriction).WriteToDebugLog(this);
                }
            }
        }

        internal static PSSwitchClauseEvalBinder Get(SwitchFlags flags)
        {
            PSSwitchClauseEvalBinder pSSwitchClauseEvalBinder;
            lock (PSSwitchClauseEvalBinder._binderCache)
            {
                PSSwitchClauseEvalBinder pSSwitchClauseEvalBinder1 = PSSwitchClauseEvalBinder._binderCache[(int)flags];
                PSSwitchClauseEvalBinder pSSwitchClauseEvalBinder2 = pSSwitchClauseEvalBinder1;
                if (pSSwitchClauseEvalBinder1 == null)
                {
                    PSSwitchClauseEvalBinder pSSwitchClauseEvalBinder3 = new PSSwitchClauseEvalBinder(flags);
                    PSSwitchClauseEvalBinder pSSwitchClauseEvalBinder4 = pSSwitchClauseEvalBinder3;
                    PSSwitchClauseEvalBinder._binderCache[(int)flags] = pSSwitchClauseEvalBinder3;
                    pSSwitchClauseEvalBinder2 = pSSwitchClauseEvalBinder4;
                }
                pSSwitchClauseEvalBinder = pSSwitchClauseEvalBinder2;
            }
            return pSSwitchClauseEvalBinder;
        }
    }
}