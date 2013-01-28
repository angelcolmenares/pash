namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal class TraversalInfo
    {
        private int _level;
        private int _maxDepth;

        internal TraversalInfo(int level, int maxDepth)
        {
            this._level = level;
            this._maxDepth = maxDepth;
        }

        internal int Level
        {
            get
            {
                return this._level;
            }
        }

        internal int MaxDepth
        {
            get
            {
                return this._maxDepth;
            }
        }

        internal TraversalInfo NextLevel
        {
            get
            {
                return new TraversalInfo(this._level + 1, this._maxDepth);
            }
        }
    }
}

