namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;

    internal class WildcardPatternMatcher
    {
        private readonly CharacterNormalizer _characterNormalizer;
        private readonly PatternElement[] _patternElements;

        internal WildcardPatternMatcher(WildcardPattern wildcardPattern)
        {
            this._characterNormalizer = new CharacterNormalizer(wildcardPattern.Options);
            this._patternElements = MyWildcardPatternParser.Parse(wildcardPattern, this._characterNormalizer);
        }

        internal bool IsMatch(string str)
        {
            int num3;
            StringBuilder builder = new StringBuilder(str.Length);
            foreach (char ch in str)
            {
                builder.Append(this._characterNormalizer.Normalize(ch));
            }
            str = builder.ToString();
            PatternPositionsVisitor patternPositionsForCurrentStringPosition = new PatternPositionsVisitor(this._patternElements.Length);
            patternPositionsForCurrentStringPosition.Add(0);
            PatternPositionsVisitor patternPositionsForNextStringPosition = new PatternPositionsVisitor(this._patternElements.Length);
            for (int i = 0; i < str.Length; i++)
            {
                int num2;
                char currentStringCharacter = str[i];
                patternPositionsForCurrentStringPosition.StringPosition = i;
                patternPositionsForNextStringPosition.StringPosition = i + 1;
                while (patternPositionsForCurrentStringPosition.MoveNext(out num2))
                {
                    this._patternElements[num2].ProcessStringCharacter(currentStringCharacter, num2, patternPositionsForCurrentStringPosition, patternPositionsForNextStringPosition);
                }
                PatternPositionsVisitor visitor3 = patternPositionsForCurrentStringPosition;
                patternPositionsForCurrentStringPosition = patternPositionsForNextStringPosition;
                patternPositionsForNextStringPosition = visitor3;
            }
            while (patternPositionsForCurrentStringPosition.MoveNext(out num3))
            {
                this._patternElements[num3].ProcessEndOfString(num3, patternPositionsForCurrentStringPosition);
            }
            return patternPositionsForCurrentStringPosition.ReachedEndOfPattern;
        }

        private class AsterixElement : WildcardPatternMatcher.PatternElement
        {
            public override void ProcessEndOfString(int currentPatternPosition, WildcardPatternMatcher.PatternPositionsVisitor patternPositionsForEndOfStringPosition)
            {
                patternPositionsForEndOfStringPosition.Add(currentPatternPosition + 1);
            }

            public override void ProcessStringCharacter(char currentStringCharacter, int currentPatternPosition, WildcardPatternMatcher.PatternPositionsVisitor patternPositionsForCurrentStringPosition, WildcardPatternMatcher.PatternPositionsVisitor patternPositionsForNextStringPosition)
            {
                patternPositionsForCurrentStringPosition.Add(currentPatternPosition + 1);
                patternPositionsForNextStringPosition.Add(currentPatternPosition);
            }
        }

        private class BracketExpressionElement : WildcardPatternMatcher.QuestionMarkElement
        {
            private readonly Regex _regex;

            public BracketExpressionElement(Regex regex)
            {
                this._regex = regex;
            }

            public override void ProcessStringCharacter(char currentStringCharacter, int currentPatternPosition, WildcardPatternMatcher.PatternPositionsVisitor patternPositionsForCurrentStringPosition, WildcardPatternMatcher.PatternPositionsVisitor patternPositionsForNextStringPosition)
            {
                if (this._regex.IsMatch(new string(currentStringCharacter, 1)))
                {
                    base.ProcessStringCharacter(currentStringCharacter, currentPatternPosition, patternPositionsForCurrentStringPosition, patternPositionsForNextStringPosition);
                }
            }
        }

        private class CharacterNormalizer
        {
            private readonly bool _caseInsensitive;
            private readonly CultureInfo _cultureInfo;

            public CharacterNormalizer(WildcardOptions options)
            {
                if (WildcardOptions.CultureInvariant == (options & WildcardOptions.CultureInvariant))
                {
                    this._cultureInfo = CultureInfo.InvariantCulture;
                }
                else
                {
                    this._cultureInfo = CultureInfo.CurrentCulture;
                }
                this._caseInsensitive = WildcardOptions.IgnoreCase == (options & WildcardOptions.IgnoreCase);
            }

            public char Normalize(char x)
            {
                if (this._caseInsensitive)
                {
                    return char.ToLower(x, this._cultureInfo);
                }
                return x;
            }
        }

        private class LiteralCharacterElement : WildcardPatternMatcher.QuestionMarkElement
        {
            private readonly char _literalCharacter;

            public LiteralCharacterElement(char literalCharacter)
            {
                this._literalCharacter = literalCharacter;
            }

            public override void ProcessStringCharacter(char currentStringCharacter, int currentPatternPosition, WildcardPatternMatcher.PatternPositionsVisitor patternPositionsForCurrentStringPosition, WildcardPatternMatcher.PatternPositionsVisitor patternPositionsForNextStringPosition)
            {
                if (this._literalCharacter == currentStringCharacter)
                {
                    base.ProcessStringCharacter(currentStringCharacter, currentPatternPosition, patternPositionsForCurrentStringPosition, patternPositionsForNextStringPosition);
                }
            }
        }

        private class MyWildcardPatternParser : WildcardPatternParser
        {
            private StringBuilder _bracketExpressionBuilder;
            private WildcardPatternMatcher.CharacterNormalizer _characterNormalizer;
            private readonly List<WildcardPatternMatcher.PatternElement> _patternElements = new List<WildcardPatternMatcher.PatternElement>();
            private RegexOptions _regexOptions;

            protected override void AppendAsterix()
            {
                this._patternElements.Add(new WildcardPatternMatcher.AsterixElement());
            }

            protected override void AppendCharacterRangeToBracketExpression(char startOfCharacterRange, char endOfCharacterRange)
            {
                WildcardPatternToRegexParser.AppendCharacterRangeToBracketExpression(this._bracketExpressionBuilder, startOfCharacterRange, endOfCharacterRange);
            }

            protected override void AppendLiteralCharacter(char c)
            {
                c = this._characterNormalizer.Normalize(c);
                this._patternElements.Add(new WildcardPatternMatcher.LiteralCharacterElement(c));
            }

            protected override void AppendLiteralCharacterToBracketExpression(char c)
            {
                WildcardPatternToRegexParser.AppendLiteralCharacterToBracketExpression(this._bracketExpressionBuilder, c);
            }

            protected override void AppendQuestionMark()
            {
                this._patternElements.Add(new WildcardPatternMatcher.QuestionMarkElement());
            }

            protected override void BeginBracketExpression()
            {
                this._bracketExpressionBuilder = new StringBuilder();
                this._bracketExpressionBuilder.Append('[');
            }

            protected override void EndBracketExpression()
            {
                this._bracketExpressionBuilder.Append(']');
                Regex regex = new Regex(this._bracketExpressionBuilder.ToString(), this._regexOptions);
                this._patternElements.Add(new WildcardPatternMatcher.BracketExpressionElement(regex));
            }

            public static WildcardPatternMatcher.PatternElement[] Parse(WildcardPattern pattern, WildcardPatternMatcher.CharacterNormalizer characterNormalizer)
            {
                WildcardPatternMatcher.MyWildcardPatternParser parser = new WildcardPatternMatcher.MyWildcardPatternParser {
                    _characterNormalizer = characterNormalizer,
                    _regexOptions = WildcardPatternToRegexParser.TranslateWildcardOptionsIntoRegexOptions(pattern.Options)
                };
                WildcardPatternParser.Parse(pattern, parser);
                return parser._patternElements.ToArray();
            }
        }

        private abstract class PatternElement
        {
            protected PatternElement()
            {
            }

            public abstract void ProcessEndOfString(int currentPatternPosition, WildcardPatternMatcher.PatternPositionsVisitor patternPositionsForEndOfStringPosition);
            public abstract void ProcessStringCharacter(char currentStringCharacter, int currentPatternPosition, WildcardPatternMatcher.PatternPositionsVisitor patternPositionsForCurrentStringPosition, WildcardPatternMatcher.PatternPositionsVisitor patternPositionsForNextStringPosition);
        }

        private class PatternPositionsVisitor
        {
            private readonly int[] _isPatternPositionVisitedMarker;
            private readonly int _lengthOfPattern;
            private readonly int[] _patternPositionsForFurtherProcessing;
            private int _patternPositionsForFurtherProcessingCount;

            public PatternPositionsVisitor(int lengthOfPattern)
            {
                this._lengthOfPattern = lengthOfPattern;
                this._isPatternPositionVisitedMarker = new int[lengthOfPattern + 1];
                for (int i = 0; i < this._isPatternPositionVisitedMarker.Length; i++)
                {
                    this._isPatternPositionVisitedMarker[i] = -1;
                }
                this._patternPositionsForFurtherProcessing = new int[lengthOfPattern];
                this._patternPositionsForFurtherProcessingCount = 0;
            }

            public void Add(int patternPosition)
            {
                if (this._isPatternPositionVisitedMarker[patternPosition] != this.StringPosition)
                {
                    this._isPatternPositionVisitedMarker[patternPosition] = this.StringPosition;
                    if (patternPosition < this._lengthOfPattern)
                    {
                        this._patternPositionsForFurtherProcessing[this._patternPositionsForFurtherProcessingCount] = patternPosition;
                        this._patternPositionsForFurtherProcessingCount++;
                    }
                }
            }

            public bool MoveNext(out int patternPosition)
            {
                if (this._patternPositionsForFurtherProcessingCount == 0)
                {
                    patternPosition = -1;
                    return false;
                }
                this._patternPositionsForFurtherProcessingCount--;
                patternPosition = this._patternPositionsForFurtherProcessing[this._patternPositionsForFurtherProcessingCount];
                return true;
            }

            public bool ReachedEndOfPattern
            {
                get
                {
                    return (this._isPatternPositionVisitedMarker[this._lengthOfPattern] >= this.StringPosition);
                }
            }

            public int StringPosition { private get; set; }
        }

        private class QuestionMarkElement : WildcardPatternMatcher.PatternElement
        {
            public override void ProcessEndOfString(int currentPatternPosition, WildcardPatternMatcher.PatternPositionsVisitor patternPositionsForEndOfStringPosition)
            {
            }

            public override void ProcessStringCharacter(char currentStringCharacter, int currentPatternPosition, WildcardPatternMatcher.PatternPositionsVisitor patternPositionsForCurrentStringPosition, WildcardPatternMatcher.PatternPositionsVisitor patternPositionsForNextStringPosition)
            {
                patternPositionsForNextStringPosition.Add(currentPatternPosition + 1);
            }
        }
    }
}

