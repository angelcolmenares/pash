namespace Microsoft.PowerShell.Commands
{
    using System;

    public sealed class TextMeasureInfo : MeasureInfo
    {
        private int? characters;
        private int? lines;
        private int? words;

        public TextMeasureInfo()
        {
            int? nullable = null;
            this.characters = nullable;
            this.lines = this.words = this.characters = nullable;
        }

        public int? Characters
        {
            get
            {
                return this.characters;
            }
            set
            {
                this.characters = value;
            }
        }

        public int? Lines
        {
            get
            {
                return this.lines;
            }
            set
            {
                this.lines = value;
            }
        }

        public int? Words
        {
            get
            {
                return this.words;
            }
            set
            {
                this.words = value;
            }
        }
    }
}

