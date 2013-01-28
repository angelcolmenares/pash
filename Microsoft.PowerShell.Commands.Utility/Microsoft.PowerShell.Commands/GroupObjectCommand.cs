namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    [OutputType(new Type[] { typeof(Hashtable), typeof(GroupInfo) }), Cmdlet("Group", "Object", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113338", RemotingCapability=RemotingCapability.None)]
    public class GroupObjectCommand : OrderObjectBase
    {
        private SwitchParameter ashashtable;
        private SwitchParameter asstring;
        private bool noElement;
        [TraceSource("GroupObjectCommand", "Class that has group base implementation")]
        private static PSTraceSource tracer = PSTraceSource.GetTracer("GroupObjectCommand", "Class that has group base implementation");

        internal static List<GroupInfo> DoGrouping(List<OrderByPropertyEntry> orderMatrix, OrderByPropertyComparer comparer, bool noElement)
        {
            if (((orderMatrix == null) || (orderMatrix.Count == 0)) || (comparer == null))
            {
                return null;
            }
            List<GroupInfo> groups = new List<GroupInfo>();
            foreach (OrderByPropertyEntry entry in orderMatrix)
            {
                int num = FindInObjectGroups(groups, entry, comparer);
                if (num == -1)
                {
                    tracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Create a new group: {0}", new object[] { entry.orderValues }), new object[0]);
                    GroupInfo item = noElement ? new GroupInfoNoElement(entry) : new GroupInfo(entry);
                    groups.Add(item);
                }
                else
                {
                    tracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Add to group {0}: {1}", new object[] { num, entry.orderValues }), new object[0]);
                    PSObject inputObject = entry.inputObject;
                    groups[num].Add(inputObject);
                }
            }
            return groups;
        }

        protected override void EndProcessing()
        {
            OrderByProperty property = new OrderByProperty(this, base.InputObjects, base.Property, true, base.ConvertedCulture, (bool) base.CaseSensitive);
            if (((property.Comparer != null) && (property.OrderMatrix != null)) && (property.OrderMatrix.Count != 0))
            {
                List<GroupInfo> sendToPipeline = DoGrouping(property.OrderMatrix, property.Comparer, (bool) this.NoElement);
                tracer.WriteLine(sendToPipeline.Count);
                if (sendToPipeline != null)
                {
                    if (this.ashashtable != 0)
                    {
                        Hashtable hashtable = CollectionsUtil.CreateCaseInsensitiveHashtable();
                        try
                        {
                            foreach (GroupInfo info in sendToPipeline)
                            {
                                if (this.asstring != 0)
                                {
                                    hashtable.Add(info.Name, info.Group);
                                }
                                else if (info.Values.Count == 1)
                                {
                                    hashtable.Add(info.Values[0], info.Group);
                                }
                                else
                                {
                                    ArgumentException exception = new ArgumentException(UtilityCommonStrings.GroupObjectSingleProperty);
                                    ErrorRecord errorRecord = new ErrorRecord(exception, "ArgumentException", ErrorCategory.InvalidArgument, base.Property);
                                    base.ThrowTerminatingError(errorRecord);
                                }
                            }
                        }
                        catch (ArgumentException exception2)
                        {
                            this.WriteNonTerminatingError(exception2, UtilityCommonStrings.InvalidOperation, ErrorCategory.InvalidArgument);
                            return;
                        }
                        base.WriteObject(hashtable);
                    }
                    else if (this.asstring != 0)
                    {
                        ArgumentException exception3 = new ArgumentException(UtilityCommonStrings.GroupObjectWithHashTable);
                        ErrorRecord record2 = new ErrorRecord(exception3, "ArgumentException", ErrorCategory.InvalidArgument, this.asstring);
                        base.ThrowTerminatingError(record2);
                    }
                    else
                    {
                        base.WriteObject(sendToPipeline, true);
                    }
                }
            }
        }

        private static int FindInObjectGroups(List<GroupInfo> groups, OrderByPropertyEntry target, OrderByPropertyComparer comparer)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                if (comparer.Compare(groups[i].GroupValue, target) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        private void WriteNonTerminatingError(Exception exception, string resourceIdAndErrorId, ErrorCategory category)
        {
            Exception exception2 = new Exception(StringUtil.Format(resourceIdAndErrorId, new object[0]), exception);
            base.WriteError(new ErrorRecord(exception2, resourceIdAndErrorId, category, null));
        }

        [Alias(new string[] { "AHT" }), Parameter(ParameterSetName="HashTable")]
        public SwitchParameter AsHashTable
        {
            get
            {
                return this.ashashtable;
            }
            set
            {
                this.ashashtable = value;
            }
        }

        [Parameter(ParameterSetName="HashTable")]
        public SwitchParameter AsString
        {
            get
            {
                return this.asstring;
            }
            set
            {
                this.asstring = value;
            }
        }

        [Parameter]
        public SwitchParameter NoElement
        {
            get
            {
                return this.noElement;
            }
            set
            {
                this.noElement = (bool) value;
            }
        }
    }
}

