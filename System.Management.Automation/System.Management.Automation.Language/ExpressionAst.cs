namespace System.Management.Automation.Language
{
    using System;

    public abstract class ExpressionAst : CommandElementAst
    {
        protected ExpressionAst(IScriptExtent extent) : base(extent)
        {
        }

        public virtual Type StaticType
        {
            get
            {
                return typeof(object);
            }
        }
    }
}

