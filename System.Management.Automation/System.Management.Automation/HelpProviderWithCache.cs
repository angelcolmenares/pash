namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal abstract class HelpProviderWithCache : HelpProvider
    {
        private bool _cacheFullyLoaded;
        private bool _hasCustomMatch;
        private Hashtable _helpCache;

        internal HelpProviderWithCache(HelpSystem helpSystem) : base(helpSystem)
        {
            this._helpCache = new Hashtable(StringComparer.OrdinalIgnoreCase);
        }

        internal void AddCache(string target, HelpInfo helpInfo)
        {
            this._helpCache[target] = helpInfo;
        }

        protected virtual bool CustomMatch(string target, string key)
        {
            return (target == key);
        }

        internal virtual void DoExactMatchHelp(HelpRequest helpRequest)
        {
        }

        internal virtual IEnumerable<HelpInfo> DoSearchHelp(HelpRequest helpRequest)
        {
            return new HelpInfo[0]; //TODO: <DoSearchHelp>d__18(-2) { <>4__this = this };
        }

        internal override IEnumerable<HelpInfo> ExactMatchHelp(HelpRequest helpRequest)
        {
            string target = helpRequest.Target;
            if (!this.HasCustomMatch)
            {
                if (this._helpCache.Contains(target))
                {
                    yield return (HelpInfo) this._helpCache[target];
                }
            }
            else
            {
                IEnumerator enumerator = this._helpCache.Keys.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string current = (string) enumerator.Current;
                    if (this.CustomMatch(target, current))
                    {
                        yield return (HelpInfo) this._helpCache[current];
                    }
                }
            }
            if (!this.CacheFullyLoaded)
            {
                this.DoExactMatchHelp(helpRequest);
                if (this._helpCache.Contains(target))
                {
                    yield return (HelpInfo) this._helpCache[target];
                }
            }
        }

        internal HelpInfo GetCache(string target)
        {
            return (HelpInfo) this._helpCache[target];
        }

        internal virtual string GetWildCardPattern(string target)
        {
            if (WildcardPattern.ContainsWildcardCharacters(target))
            {
                return target;
            }
            return ("*" + target + "*");
        }

        internal override void Reset()
        {
            base.Reset();
            this._helpCache.Clear();
            this._cacheFullyLoaded = false;
        }

        internal override IEnumerable<HelpInfo> SearchHelp(HelpRequest helpRequest, bool searchOnlyContent)
        {
            string target = helpRequest.Target;
            string wildCardPattern = this.GetWildCardPattern(target);
            HelpRequest iteratorVariable2 = helpRequest.Clone();
            iteratorVariable2.Target = wildCardPattern;
            if (!this.CacheFullyLoaded)
            {
                IEnumerable<HelpInfo> iteratorVariable3 = this.DoSearchHelp(iteratorVariable2);
                if (iteratorVariable3 != null)
                {
                    foreach (HelpInfo iteratorVariable4 in iteratorVariable3)
                    {
                        yield return iteratorVariable4;
                    }
                }
            }
            else
            {
                int iteratorVariable5 = 0;
                WildcardPattern pattern = new WildcardPattern(wildCardPattern, WildcardOptions.IgnoreCase);
                IEnumerator enumerator = this._helpCache.Keys.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string current = (string) enumerator.Current;
                    if ((!searchOnlyContent && pattern.IsMatch(current)) || (searchOnlyContent && ((HelpInfo) this._helpCache[current]).MatchPatternInContent(pattern)))
                    {
                        iteratorVariable5++;
                        yield return (HelpInfo) this._helpCache[current];
                        if ((helpRequest.MaxResults > 0) && (iteratorVariable5 >= helpRequest.MaxResults))
                        {
                            break;
                        }
                    }
                }
            }
        }

        protected internal bool CacheFullyLoaded
        {
            get
            {
                return this._cacheFullyLoaded;
            }
            set
            {
                this._cacheFullyLoaded = value;
            }
        }

        protected bool HasCustomMatch
        {
            get
            {
                return this._hasCustomMatch;
            }
            set
            {
                this._hasCustomMatch = value;
            }
        }

        
    }
}

