namespace System.Management.Automation.Language
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;

    internal class PSInvokeBinder : InvokeBinder
    {
        internal PSInvokeBinder(CallInfo callInfo) : base(callInfo)
        {
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            return (errorSuggestion ?? target.ThrowRuntimeError(args, BindingRestrictions.Empty, "CannotInvoke", ParserStrings.CannotInvoke, new Expression[0]));
        }
    }
}

