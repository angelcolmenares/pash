namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Collections.Generic;

    internal sealed class BranchLabel
    {
        internal int _continuationStackDepth = -2147483648;
        private List<int> _forwardBranchFixups;
        internal int _labelIndex = -2147483648;
        internal int _stackDepth = -2147483648;
        internal int _targetIndex = -2147483648;
        internal const int UnknownDepth = -2147483648;
        internal const int UnknownIndex = -2147483648;

        internal void AddBranch(InstructionList instructions, int branchIndex)
        {
            if (this._targetIndex == -2147483648)
            {
                if (this._forwardBranchFixups == null)
                {
                    this._forwardBranchFixups = new List<int>();
                }
                this._forwardBranchFixups.Add(branchIndex);
            }
            else
            {
                this.FixupBranch(instructions, branchIndex);
            }
        }

        internal void FixupBranch(InstructionList instructions, int branchIndex)
        {
            instructions.FixupBranch(branchIndex, this._targetIndex - branchIndex);
        }

        internal void Mark(InstructionList instructions)
        {
            this._stackDepth = instructions.CurrentStackDepth;
            this._continuationStackDepth = instructions.CurrentContinuationsDepth;
            this._targetIndex = instructions.Count;
            if (this._forwardBranchFixups != null)
            {
                foreach (int num in this._forwardBranchFixups)
                {
                    this.FixupBranch(instructions, num);
                }
                this._forwardBranchFixups = null;
            }
        }

        internal RuntimeLabel ToRuntimeLabel()
        {
            return new RuntimeLabel(this._targetIndex, this._continuationStackDepth, this._stackDepth);
        }

        internal bool HasRuntimeLabel
        {
            get
            {
                return (this._labelIndex != -2147483648);
            }
        }

        internal int LabelIndex
        {
            get
            {
                return this._labelIndex;
            }
            set
            {
                this._labelIndex = value;
            }
        }

        internal int StackDepth
        {
            get
            {
                return this._stackDepth;
            }
        }

        internal int TargetIndex
        {
            get
            {
                return this._targetIndex;
            }
        }
    }
}

