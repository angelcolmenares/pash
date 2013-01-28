namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    [Cmdlet("Sort", "Object", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113403", RemotingCapability=RemotingCapability.None)]
    public sealed class SortObjectCommand : OrderObjectBase
    {
        private bool unique;

        protected override void EndProcessing()
        {
            OrderByProperty orderByProperty = new OrderByProperty(this, base.InputObjects, base.Property, this.Descending == 0, base.ConvertedCulture, (bool) base.CaseSensitive);
            if (((orderByProperty.Comparer != null) && (orderByProperty.OrderMatrix != null)) && (orderByProperty.OrderMatrix.Count != 0))
            {
                orderByProperty.OrderMatrix.Sort(orderByProperty.Comparer);
                if (this.unique)
                {
                    RemoveDuplicates(orderByProperty);
                }
                foreach (OrderByPropertyEntry entry in orderByProperty.OrderMatrix)
                {
                    base.WriteObject(entry.inputObject);
                }
            }
        }

        private static void RemoveDuplicates(OrderByProperty orderByProperty)
        {
            int num = 0;
            OrderByPropertyEntry firstEntry = orderByProperty.OrderMatrix[num];
            while ((num + 1) < orderByProperty.OrderMatrix.Count)
            {
                int index = num + 1;
                OrderByPropertyEntry secondEntry = orderByProperty.OrderMatrix[index];
                if (orderByProperty.Comparer.Compare(firstEntry, secondEntry) == 0)
                {
                    orderByProperty.OrderMatrix.RemoveAt(index);
                }
                else
                {
                    num = index;
                    firstEntry = secondEntry;
                }
            }
        }

        [Parameter]
        public SwitchParameter Descending
        {
            get
            {
                return base.DescendingOrder;
            }
            set
            {
                base.DescendingOrder = value;
            }
        }

        [Parameter]
        public SwitchParameter Unique
        {
            get
            {
                return this.unique;
            }
            set
            {
                this.unique = (bool) value;
            }
        }
    }
}

