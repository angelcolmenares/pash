namespace System.Management.Automation.Language
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;

    internal class BinderUtils
    {
        internal static BindingRestrictions GetLanguageModeCheckIfHasEverUsedConstrainedLanguage()
        {
            BindingRestrictions empty = BindingRestrictions.Empty;
            if (!ExecutionContext.HasEverUsedConstrainedLanguage)
            {
                return empty;
            }
            if (LocalPipeline.GetExecutionContextFromTLS().LanguageMode == PSLanguageMode.ConstrainedLanguage)
            {
                return BindingRestrictions.GetExpressionRestriction(Expression.Equal(Expression.Property(ExpressionCache.GetExecutionContextFromTLS, CachedReflectionInfo.ExecutionContext_LanguageMode), Expression.Constant(PSLanguageMode.ConstrainedLanguage)));
            }
            return BindingRestrictions.GetExpressionRestriction(Expression.NotEqual(Expression.Property(ExpressionCache.GetExecutionContextFromTLS, CachedReflectionInfo.ExecutionContext_LanguageMode), Expression.Constant(PSLanguageMode.ConstrainedLanguage)));
        }

        internal static BindingRestrictions GetOptionalVersionAndLanguageCheckForType(DynamicMetaObjectBinder binder, Type targetType, int expectedVersionNumber)
        {
            BindingRestrictions empty = BindingRestrictions.Empty;
            if (CoreTypes.Contains(targetType))
            {
                return empty;
            }
            if (expectedVersionNumber != -1)
            {
                empty = empty.Merge(GetVersionCheck(binder, expectedVersionNumber));
            }
            return empty.Merge(GetLanguageModeCheckIfHasEverUsedConstrainedLanguage());
        }

        internal static BindingRestrictions GetVersionCheck(DynamicMetaObjectBinder binder, int expectedVersionNumber)
        {
            return BindingRestrictions.GetExpressionRestriction(Expression.Equal(Expression.Field(Expression.Constant(binder), "_version"), ExpressionCache.Constant(expectedVersionNumber)));
        }
    }
}

