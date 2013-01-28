namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Runtime.InteropServices;

    internal sealed class LocalVariables
    {
        private Dictionary<ParameterExpression, LocalVariable> _closureVariables;
        private int _localCount;
        private int _maxLocalCount;
        private readonly HybridReferenceDictionary<ParameterExpression, VariableScope> _variables = new HybridReferenceDictionary<ParameterExpression, VariableScope>();

        internal LocalVariables()
        {
        }

        internal LocalVariable AddClosureVariable(ParameterExpression variable)
        {
            if (this._closureVariables == null)
            {
                this._closureVariables = new Dictionary<ParameterExpression, LocalVariable>();
            }
            LocalVariable variable2 = new LocalVariable(this._closureVariables.Count, true, false);
            this._closureVariables.Add(variable, variable2);
            return variable2;
        }

        internal void Box(ParameterExpression variable, InstructionList instructions)
        {
            VariableScope scope = this._variables[variable];
            LocalVariable variable2 = scope.Variable;
            this._variables[variable].Variable.IsBoxed = true;
            int num = 0;
            for (int i = scope.Start; (i < scope.Stop) && (i < instructions.Count); i++)
            {
                if ((scope.ChildScopes != null) && (scope.ChildScopes[num].Start == i))
                {
                    VariableScope scope2 = scope.ChildScopes[num];
                    i = scope2.Stop;
                    num++;
                }
                else
                {
                    instructions.SwitchToBoxed(variable2.Index, i);
                }
            }
        }

        internal bool ContainsVariable(ParameterExpression variable)
        {
            return this._variables.ContainsKey(variable);
        }

        internal Dictionary<ParameterExpression, LocalVariable> CopyLocals()
        {
            Dictionary<ParameterExpression, LocalVariable> dictionary = new Dictionary<ParameterExpression, LocalVariable>(this._variables.Count);
            foreach (KeyValuePair<ParameterExpression, VariableScope> pair in this._variables)
            {
                dictionary[pair.Key] = pair.Value.Variable;
            }
            return dictionary;
        }

        public LocalDefinition DefineLocal(ParameterExpression variable, int start)
        {
            VariableScope scope;
            VariableScope scope2;
            LocalVariable variable2 = new LocalVariable(this._localCount++, false, false);
            this._maxLocalCount = Math.Max(this._localCount, this._maxLocalCount);
            if (this._variables.TryGetValue(variable, out scope))
            {
                scope2 = new VariableScope(variable2, start, scope);
                if (scope.ChildScopes == null)
                {
                    scope.ChildScopes = new List<VariableScope>();
                }
                scope.ChildScopes.Add(scope2);
            }
            else
            {
                scope2 = new VariableScope(variable2, start, null);
            }
            this._variables[variable] = scope2;
            return new LocalDefinition(variable2.Index, variable);
        }

        public int GetLocalIndex(ParameterExpression var)
        {
            VariableScope scope;
            if (!this._variables.TryGetValue(var, out scope))
            {
                return -1;
            }
            return scope.Variable.Index;
        }

        public int GetOrDefineLocal(ParameterExpression var)
        {
            int localIndex = this.GetLocalIndex(var);
            if (localIndex == -1)
            {
                return this.DefineLocal(var, 0).Index;
            }
            return localIndex;
        }

        public bool TryGetLocalOrClosure(ParameterExpression var, out LocalVariable local)
        {
            VariableScope scope;
            if (this._variables.TryGetValue(var, out scope))
            {
                local = scope.Variable;
                return true;
            }
            if ((this._closureVariables != null) && this._closureVariables.TryGetValue(var, out local))
            {
                return true;
            }
            local = null;
            return false;
        }

        public void UndefineLocal(LocalDefinition definition, int end)
        {
            VariableScope scope = this._variables[definition.Parameter];
            scope.Stop = end;
            if (scope.Parent != null)
            {
                this._variables[definition.Parameter] = scope.Parent;
            }
            else
            {
                this._variables.Remove(definition.Parameter);
            }
            this._localCount--;
        }

        internal Dictionary<ParameterExpression, LocalVariable> ClosureVariables
        {
            get
            {
                return this._closureVariables;
            }
        }

        public int LocalCount
        {
            get
            {
                return this._maxLocalCount;
            }
        }

        private sealed class VariableScope
        {
            public List<LocalVariables.VariableScope> ChildScopes;
            public readonly LocalVariables.VariableScope Parent;
            public readonly int Start;
            public int Stop = 0x7fffffff;
            public readonly LocalVariable Variable;

            public VariableScope(LocalVariable variable, int start, LocalVariables.VariableScope parent)
            {
                this.Variable = variable;
                this.Start = start;
                this.Parent = parent;
            }
        }
    }
}

