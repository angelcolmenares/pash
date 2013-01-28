namespace Microsoft.Data.Spatial
{
    using System;
    using System.Runtime.CompilerServices;

    internal class LexerToken
    {
        public bool MatchToken(int targetType, string targetText, StringComparison comparison)
        {
            if (this.Type != targetType)
            {
                return false;
            }
            if (!string.IsNullOrEmpty(targetText))
            {
                return this.Text.Equals(targetText, comparison);
            }
            return true;
        }

        public override string ToString()
        {
            return string.Concat(new object[] { "Type:[", this.Type, "] Text:[", this.Text, "]" });
        }

        public string Text { get; set; }

        public int Type { get; set; }
    }
}

