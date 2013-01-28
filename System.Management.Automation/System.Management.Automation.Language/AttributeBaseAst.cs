namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public abstract class AttributeBaseAst : Ast
    {
        protected AttributeBaseAst(IScriptExtent extent, ITypeName typeName) : base(extent)
        {
            if (typeName == null)
            {
                throw PSTraceSource.NewArgumentNullException("typeName");
            }
            this.TypeName = typeName;
        }

        internal abstract Attribute GetAttribute();
        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            return Ast.EmptyPSTypeNameArray;
        }

        public ITypeName TypeName { get; private set; }
    }
}

