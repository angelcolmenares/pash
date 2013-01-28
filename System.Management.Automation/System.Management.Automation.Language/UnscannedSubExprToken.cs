namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;

    internal class UnscannedSubExprToken : StringLiteralToken
    {
        internal UnscannedSubExprToken(InternalScriptExtent scriptExtent, TokenFlags tokenFlags, string value, BitArray skippedCharOffsets) : base(scriptExtent, tokenFlags, TokenKind.StringLiteral, value)
        {
            this.SkippedCharOffsets = skippedCharOffsets;
        }

        internal BitArray SkippedCharOffsets { get; private set; }
    }
}

