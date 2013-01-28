namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    internal class MshExpression
    {
        private bool _isResolved;
        private ScriptBlock _script;
        private string _stringValue;

        internal MshExpression(ScriptBlock scriptBlock)
        {
            if (scriptBlock == null)
            {
                throw PSTraceSource.NewArgumentNullException("scriptBlock");
            }
            this._script = scriptBlock;
        }

        internal MshExpression(string s) : this(s, false)
        {
        }

        internal MshExpression(string s, bool isResolved)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw PSTraceSource.NewArgumentNullException("s");
            }
            this._stringValue = s;
            this._isResolved = isResolved;
        }

        private MshExpressionResult GetValue(PSObject target, bool eatExceptions)
        {
            try
            {
                object obj2;
                if (this._script != null)
                {
                    obj2 = this._script.DoInvokeReturnAsIs(true, ScriptBlock.ErrorHandlingBehavior.WriteToExternalErrorPipe, target, AutomationNull.Value, AutomationNull.Value, new object[0]);
                }
                else
                {
                    PSMemberInfo info = target.Properties[this._stringValue];
                    if (info == null)
                    {
                        return new MshExpressionResult(null, this, null);
                    }
                    obj2 = info.Value;
                }
                return new MshExpressionResult(obj2, this, null);
            }
            catch (RuntimeException exception)
            {
                if (!eatExceptions)
                {
                    throw;
                }
                return new MshExpressionResult(null, this, exception);
            }
        }

        internal List<MshExpressionResult> GetValues(PSObject target)
        {
            return this.GetValues(target, true, true);
        }

        internal List<MshExpressionResult> GetValues(PSObject target, bool expand, bool eatExceptions)
        {
            List<MshExpressionResult> list = new List<MshExpressionResult>();
            if (this._script != null)
            {
                MshExpressionResult item = new MshExpression(this._script).GetValue(target, eatExceptions);
                list.Add(item);
                return list;
            }
            foreach (MshExpression expression2 in this.ResolveNames(target, expand))
            {
                MshExpressionResult result2 = expression2.GetValue(target, eatExceptions);
                list.Add(result2);
            }
            return list;
        }

        internal List<MshExpression> ResolveNames(PSObject target)
        {
            return this.ResolveNames(target, true);
        }

        internal List<MshExpression> ResolveNames(PSObject target, bool expand)
        {
            List<MshExpression> list = new List<MshExpression>();
            if (this._isResolved)
            {
                list.Add(this);
                return list;
            }
            if (this._script != null)
            {
                MshExpression item = new MshExpression(this._script) {
                    _isResolved = true
                };
                list.Add(item);
                return list;
            }
            IEnumerable<PSMemberInfo> enumerable = null;
            if (this.HasWildCardCharacters)
            {
                enumerable = target.Members.Match(this._stringValue, PSMemberTypes.PropertySet | PSMemberTypes.Properties);
            }
            else
            {
                PSMemberInfo info = target.Members[this._stringValue];
                List<PSMemberInfo> list2 = new List<PSMemberInfo>();
                if (info != null)
                {
                    list2.Add(info);
                }
                enumerable = list2;
            }
            List<PSMemberInfo> list3 = new List<PSMemberInfo>();
            foreach (PSMemberInfo info2 in enumerable)
            {
                PSPropertySet set = info2 as PSPropertySet;
                if (set != null)
                {
                    if (expand)
                    {
                        Collection<string> referencedPropertyNames = set.ReferencedPropertyNames;
                        for (int i = 0; i < referencedPropertyNames.Count; i++)
                        {
                            ReadOnlyPSMemberInfoCollection<PSPropertyInfo> infos = target.Properties.Match(referencedPropertyNames[i]);
                            for (int j = 0; j < infos.Count; j++)
                            {
                                list3.Add(infos[j]);
                            }
                        }
                    }
                }
                else if (info2 is PSPropertyInfo)
                {
                    list3.Add(info2);
                }
            }
            Hashtable hashtable = new Hashtable();
            foreach (PSMemberInfo info3 in list3)
            {
                if (!hashtable.ContainsKey(info3.Name))
                {
                    MshExpression expression2 = new MshExpression(info3.Name) {
                        _isResolved = true
                    };
                    list.Add(expression2);
                    hashtable.Add(info3.Name, null);
                }
            }
            return list;
        }

        public override string ToString()
        {
            if (this._script != null)
            {
                return this._script.ToString();
            }
            return this._stringValue;
        }

        internal bool HasWildCardCharacters
        {
            get
            {
                if (this._script != null)
                {
                    return false;
                }
                return WildcardPattern.ContainsWildcardCharacters(this._stringValue);
            }
        }

        public ScriptBlock Script
        {
            get
            {
                return this._script;
            }
        }
    }
}

