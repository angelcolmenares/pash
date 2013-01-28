namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal class TokenizerState
    {
        internal int CurrentIndex;
        internal int NestedTokensAdjustment;
        internal string Script;
        internal BitArray SkippedCharOffsets;
        internal List<Token> TokenList;
        internal int TokenStart;
    }
}

