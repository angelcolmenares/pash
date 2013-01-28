namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class LineBreakpoint : Breakpoint
    {
        internal LineBreakpoint(string script, int line, System.Management.Automation.ScriptBlock action) : base(script, action)
        {
            this.Line = line;
            this.Column = 0;
            this.SequencePointIndex = -1;
        }

        internal LineBreakpoint(string script, int line, int column, System.Management.Automation.ScriptBlock action) : base(script, action)
        {
            this.Line = line;
            this.Column = column;
            this.SequencePointIndex = -1;
        }

        private static IScriptExtent FindSequencePoint(FunctionContext functionContext, int line, int column, out int sequencePointIndex)
        {
            IScriptExtent[] sequencePoints = functionContext._scriptBlock.SequencePoints;
            for (int i = 0; i < sequencePoints.Length; i++)
            {
                IScriptExtent extent = sequencePoints[i];
                if (extent.ContainsLineAndColumn(line, column))
                {
                    sequencePointIndex = i;
                    return extent;
                }
            }
            sequencePointIndex = -1;
            return null;
        }

        internal override void RemoveSelf(Debugger debugger)
        {
            Func<LineBreakpoint, bool> predicate = null;
            if (this.ScriptBlock != null)
            {
                List<LineBreakpoint> boundBreakpoints = debugger.GetBoundBreakpoints(this.ScriptBlock);
                if (boundBreakpoints != null)
                {
                    boundBreakpoints.Remove(this);
                    if (predicate == null)
                    {
                        predicate = breakpoint => breakpoint.SequencePointIndex != this.SequencePointIndex;
                    }
                    if (boundBreakpoints.All<LineBreakpoint>(predicate))
                    {
                        this.BreakpointBitArray.Set(this.SequencePointIndex, false);
                    }
                }
            }
            debugger.RemoveLineBreakpoint(this);
        }

        private void SetBreakpoint(FunctionContext functionContext, int sequencePointIndex)
        {
            this.BreakpointBitArray = functionContext._breakPoints;
            this.ScriptBlock = functionContext._scriptBlock;
            this.SequencePointIndex = sequencePointIndex;
            base.SetEnabled(true);
            this.BreakpointBitArray.Set(this.SequencePointIndex, true);
        }

        public override string ToString()
        {
            if (this.Column != 0)
            {
                return StringUtil.Format(DebuggerStrings.StatementBreakpointString, new object[] { base.Script, this.Line, this.Column });
            }
            return StringUtil.Format(DebuggerStrings.LineBreakpointString, base.Script, this.Line);
        }

        internal bool TrySetBreakpoint(string scriptFile, FunctionContext functionContext)
        {
            if (scriptFile.Equals(base.Script, StringComparison.OrdinalIgnoreCase))
            {
                int num;
                System.Management.Automation.ScriptBlock block = functionContext._scriptBlock;
                Ast ast = block.Ast;
                if (!ast.Extent.ContainsLineAndColumn(this.Line, this.Column))
                {
                    return false;
                }
                IScriptExtent[] sequencePoints = block.SequencePoints;
                if ((sequencePoints.Length == 1) && (sequencePoints[0] == block.Ast.Extent))
                {
                    return false;
                }
                bool flag = CheckBreakpointInScript.IsInNestedScriptBlock(((IParameterMetadataProvider) ast).Body, this);
                IScriptExtent extent = FindSequencePoint(functionContext, this.Line, this.Column, out num);
                if ((extent != null) && (!flag || ((extent.StartLineNumber == this.Line) && (this.Column == 0))))
                {
                    this.SetBreakpoint(functionContext, num);
                    return true;
                }
                if (flag)
                {
                    return false;
                }
                ScriptBlockAst body = ((IParameterMetadataProvider) ast).Body;
                if ((((body.DynamicParamBlock == null) || body.DynamicParamBlock.Extent.IsAfter(this.Line, this.Column)) && ((body.BeginBlock == null) || body.BeginBlock.Extent.IsAfter(this.Line, this.Column))) && (((body.ProcessBlock == null) || body.ProcessBlock.Extent.IsAfter(this.Line, this.Column)) && ((body.EndBlock == null) || body.EndBlock.Extent.IsAfter(this.Line, this.Column))))
                {
                    this.SetBreakpoint(functionContext, 0);
                    return true;
                }
                if ((this.Column == 0) && (FindSequencePoint(functionContext, this.Line + 1, 0, out num) != null))
                {
                    this.SetBreakpoint(functionContext, num);
                    return true;
                }
            }
            return false;
        }

        internal BitArray BreakpointBitArray { get; set; }

        public int Column { get; private set; }

        public int Line { get; private set; }

        internal System.Management.Automation.ScriptBlock ScriptBlock { get; set; }

        internal int SequencePointIndex { get; set; }

        private class CheckBreakpointInScript : AstVisitor
        {
            private LineBreakpoint _breakpoint;
            private bool _result;

            public static bool IsInNestedScriptBlock(Ast ast, LineBreakpoint breakpoint)
            {
                LineBreakpoint.CheckBreakpointInScript visitor = new LineBreakpoint.CheckBreakpointInScript {
                    _breakpoint = breakpoint
                };
                ast.InternalVisit(visitor);
                return visitor._result;
            }

            public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
            {
                if (functionDefinitionAst.Extent.ContainsLineAndColumn(this._breakpoint.Line, this._breakpoint.Column))
                {
                    this._result = true;
                    return AstVisitAction.StopVisit;
                }
                return AstVisitAction.SkipChildren;
            }

            public override AstVisitAction VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockExpressionAst)
            {
                if (scriptBlockExpressionAst.Extent.ContainsLineAndColumn(this._breakpoint.Line, this._breakpoint.Column))
                {
                    this._result = true;
                    return AstVisitAction.StopVisit;
                }
                return AstVisitAction.SkipChildren;
            }
        }
    }
}

