namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;

    internal sealed class LabelInfo
    {
        private bool _acrossBlockJump;
        private object _definitions;
        private BranchLabel _label;
        private readonly LabelTarget _node;
        private readonly List<LabelScopeInfo> _references = new List<LabelScopeInfo>();

        internal LabelInfo(LabelTarget node)
        {
            this._node = node;
        }

        private void AddDefinition(LabelScopeInfo scope)
        {
            if (this._definitions == null)
            {
                this._definitions = scope;
            }
            else
            {
                HashSet<LabelScopeInfo> set = this._definitions as HashSet<LabelScopeInfo>;
                if (set == null)
                {
                    this._definitions = set = new HashSet<LabelScopeInfo> { (LabelScopeInfo) this._definitions };
                }
                set.Add(scope);
            }
        }

        internal static T CommonNode<T>(T first, T second, Func<T, T> parent) where T: class
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            if (comparer.Equals(first, second))
            {
                return first;
            }
            HashSet<T> set = new HashSet<T>(comparer);
            for (T local = first; local != null; local = parent(local))
            {
                set.Add(local);
            }
            for (T local2 = second; local2 != null; local2 = parent(local2))
            {
                if (set.Contains(local2))
                {
                    return local2;
                }
            }
            return default(T);
        }

        internal void Define(LabelScopeInfo block)
        {
            for (LabelScopeInfo info = block; info != null; info = info.Parent)
            {
                if (info.ContainsTarget(this._node))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Label target already defined: {0}", new object[] { this._node.Name }));
                }
            }
            this.AddDefinition(block);
            block.AddLabelInfo(this._node, this);
            if (this.HasDefinitions && !this.HasMultipleDefinitions)
            {
                foreach (LabelScopeInfo info2 in this._references)
                {
                    this.ValidateJump(info2);
                }
            }
            else
            {
                if (this._acrossBlockJump)
                {
                    throw new InvalidOperationException("Ambiguous jump");
                }
                this._label = null;
            }
        }

        private bool DefinedIn(LabelScopeInfo scope)
        {
            if (this._definitions == scope)
            {
                return true;
            }
            HashSet<LabelScopeInfo> set = this._definitions as HashSet<LabelScopeInfo>;
            return ((set != null) && set.Contains(scope));
        }

        private void EnsureLabel(LightCompiler compiler)
        {
            if (this._label == null)
            {
                this._label = compiler.Instructions.MakeLabel();
            }
        }

        private LabelScopeInfo FirstDefinition()
        {
            LabelScopeInfo info = this._definitions as LabelScopeInfo;
            if (info != null)
            {
                return info;
            }
            return ((HashSet<LabelScopeInfo>) this._definitions).First<LabelScopeInfo>();
        }

        internal BranchLabel GetLabel(LightCompiler compiler)
        {
            this.EnsureLabel(compiler);
            return this._label;
        }

        internal void Reference(LabelScopeInfo block)
        {
            this._references.Add(block);
            if (this.HasDefinitions)
            {
                this.ValidateJump(block);
            }
        }

        internal void ValidateFinish()
        {
            if ((this._references.Count > 0) && !this.HasDefinitions)
            {
                throw new InvalidOperationException("label target undefined");
            }
        }

        private void ValidateJump(LabelScopeInfo reference)
        {
            for (LabelScopeInfo info = reference; info != null; info = info.Parent)
            {
                if (this.DefinedIn(info))
                {
                    return;
                }
                if (info.Kind == LabelScopeKind.Filter)
                {
                    break;
                }
            }
            this._acrossBlockJump = true;
            if (this.HasMultipleDefinitions)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Ambiguous jump {0}", new object[] { this._node.Name }));
            }
            LabelScopeInfo first = this.FirstDefinition();
            LabelScopeInfo info3 = CommonNode<LabelScopeInfo>(first, reference, b => b.Parent);
            for (LabelScopeInfo info4 = reference; info4 != info3; info4 = info4.Parent)
            {
                if (info4.Kind == LabelScopeKind.Filter)
                {
                    throw new InvalidOperationException("Control cannot leave filter test");
                }
            }
            for (LabelScopeInfo info5 = first; info5 != info3; info5 = info5.Parent)
            {
                if (!info5.CanJumpInto)
                {
                    if (info5.Kind == LabelScopeKind.Expression)
                    {
                        throw new InvalidOperationException("Control cannot enter an expression");
                    }
                    throw new InvalidOperationException("Control cannot enter try");
                }
            }
        }

        private bool HasDefinitions
        {
            get
            {
                return (this._definitions != null);
            }
        }

        private bool HasMultipleDefinitions
        {
            get
            {
                return (this._definitions is HashSet<LabelScopeInfo>);
            }
        }
    }
}

