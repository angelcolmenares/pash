namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;

    internal class FormatMessagesContextManager
    {
        internal FormatContextCreationCallback contextCreation;
        internal FormatEndCallback fe;
        internal FormatStartCallback fs;
        internal GroupEndCallback ge;
        internal GroupStartCallback gs;
        internal PayloadCallback payload;
        private Stack stack = new Stack();

        internal void Process(object o)
        {
            PacketInfoData formatData = o as PacketInfoData;
            FormatEntryData formatEntryData = formatData as FormatEntryData;
            if (formatEntryData != null)
            {
                OutputContext c = null;
                if (!formatEntryData.outOfBand)
                {
                    c = (OutputContext) this.stack.Peek();
                }
                this.payload(formatEntryData, c);
            }
            else
            {
                bool flag = formatData is FormatStartData;
                bool flag2 = formatData is GroupStartData;
                if (flag || flag2)
                {
                    OutputContext context2 = this.contextCreation(this.ActiveOutputContext, formatData);
                    this.stack.Push(context2);
                    if (flag)
                    {
                        this.fs(context2);
                    }
                    else if (flag2)
                    {
                        this.gs(context2);
                    }
                }
                else
                {
                    GroupEndData fe = formatData as GroupEndData;
                    FormatEndData data4 = formatData as FormatEndData;
                    if ((fe != null) || (data4 != null))
                    {
                        OutputContext context3 = (OutputContext) this.stack.Peek();
                        if (data4 != null)
                        {
                            this.fe(data4, context3);
                        }
                        else if (fe != null)
                        {
                            this.ge(fe, context3);
                        }
                        this.stack.Pop();
                    }
                }
            }
        }

        internal OutputContext ActiveOutputContext
        {
            get
            {
                if (this.stack.Count <= 0)
                {
                    return null;
                }
                return (OutputContext) this.stack.Peek();
            }
        }

        internal delegate FormatMessagesContextManager.OutputContext FormatContextCreationCallback(FormatMessagesContextManager.OutputContext parentContext, FormatInfoData formatData);

        internal delegate void FormatEndCallback(FormatEndData fe, FormatMessagesContextManager.OutputContext c);

        internal delegate void FormatStartCallback(FormatMessagesContextManager.OutputContext c);

        internal delegate void GroupEndCallback(GroupEndData fe, FormatMessagesContextManager.OutputContext c);

        internal delegate void GroupStartCallback(FormatMessagesContextManager.OutputContext c);

        internal abstract class OutputContext
        {
            private FormatMessagesContextManager.OutputContext parentContext;

            internal OutputContext(FormatMessagesContextManager.OutputContext parentContextInStack)
            {
                this.parentContext = parentContextInStack;
            }

            internal FormatMessagesContextManager.OutputContext ParentContext
            {
                get
                {
                    return this.parentContext;
                }
            }
        }

        internal delegate void PayloadCallback(FormatEntryData formatEntryData, FormatMessagesContextManager.OutputContext c);
    }
}

