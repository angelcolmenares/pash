namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;
    using System.Numerics;
    using System.Threading;

    [OutputType(new Type[] { typeof(int), typeof(long), typeof(double) }), Cmdlet("Get", "Random", DefaultParameterSetName="RandomNumberParameterSet", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113446", RemotingCapability=RemotingCapability.None)]
    public class GetRandomCommand : PSCmdlet
    {
        private List<object> chosenListItems;
        private int count;
        private MyParameterSet effectiveParameterSet;
        private Random generator;
        private object[] inputObject;
        private object maximum;
        private object minimum;
        private int numberOfProcessedListItems;
        private const string RandomListItemParameterSet = "RandomListItemParameterSet";
        private const string RandomNumberParameterSet = "RandomNumberParameterSet";
        private static Dictionary<Guid, Random> RunspaceGeneratorMap = new Dictionary<Guid, Random>();
        private static ReaderWriterLock runspaceGeneratorMapLock = new ReaderWriterLock();
        private int? setSeed;

        protected override void BeginProcessing()
        {
            if (this.SetSeed.HasValue)
            {
                this.Generator = new Random(this.SetSeed.Value);
            }
            if (this.EffectiveParameterSet == MyParameterSet.RandomNumber)
            {
                object o = this.ProcessOperand(this.Maximum);
                object obj3 = this.ProcessOperand(this.Minimum);
                if (this.IsInt(o) && this.IsInt(obj3))
                {
                    int min = (obj3 != null) ? ((int) obj3) : 0;
                    int max = (o != null) ? ((int) o) : 0x7fffffff;
                    if (min >= max)
                    {
                        this.ThrowMinGreaterThanOrEqualMax(min, max);
                    }
                    int sendToPipeline = this.Generator.Next(min, max);
                    base.WriteObject(sendToPipeline);
                }
                else if ((this.IsInt64(o) || this.IsInt(o)) && (this.IsInt64(obj3) || this.IsInt(obj3)))
                {
                    long num4 = (obj3 != null) ? ((obj3 is long) ? ((long) obj3) : ((long) ((int) obj3))) : 0L;
                    long num5 = (o != null) ? ((o is long) ? ((long) o) : ((long) ((int) o))) : 0x7fffffffffffffffL;
                    if (num4 >= num5)
                    {
                        this.ThrowMinGreaterThanOrEqualMax(num4, num5);
                    }
                    long num6 = this.GetRandomInt64(num4, num5);
                    base.WriteObject(num6);
                }
                else
                {
                    double num7 = (obj3 is double) ? ((double) obj3) : this.ConvertToDouble(this.Minimum, 0.0);
                    double num8 = (o is double) ? ((double) o) : this.ConvertToDouble(this.Maximum, double.MaxValue);
                    if (num7 >= num8)
                    {
                        this.ThrowMinGreaterThanOrEqualMax(num7, num8);
                    }
                    double randomDouble = this.GetRandomDouble(num7, num8);
                    base.WriteObject(randomDouble);
                }
            }
            else if (this.EffectiveParameterSet == MyParameterSet.RandomListItem)
            {
                this.chosenListItems = new List<object>();
                this.numberOfProcessedListItems = 0;
                if (this.Count == 0)
                {
                    this.Count = 1;
                }
            }
        }

        private double ConvertToDouble(object o, double defaultIfNull)
        {
            if (o == null)
            {
                return defaultIfNull;
            }
            return (double) LanguagePrimitives.ConvertTo(o, typeof(double), CultureInfo.InvariantCulture);
        }

        private static void CurrentRunspace_StateChanged(object sender, RunspaceStateEventArgs e)
        {
            switch (e.RunspaceStateInfo.State)
            {
                case RunspaceState.Closed:
                case RunspaceState.Broken:
                    try
                    {
                        runspaceGeneratorMapLock.AcquireWriterLock(-1);
                        RunspaceGeneratorMap.Remove(((Runspace) sender).InstanceId);
                    }
                    finally
                    {
                        runspaceGeneratorMapLock.ReleaseWriterLock();
                    }
                    break;

                case RunspaceState.Closing:
                    break;

                default:
                    return;
            }
        }

        protected override void EndProcessing()
        {
            if (this.EffectiveParameterSet == MyParameterSet.RandomListItem)
            {
                int count = this.chosenListItems.Count;
                for (int i = 0; i < count; i++)
                {
                    int num3 = this.Generator.Next(i, count);
                    if (i != num3)
                    {
                        object obj2 = this.chosenListItems[i];
                        this.chosenListItems[i] = this.chosenListItems[num3];
                        this.chosenListItems[num3] = obj2;
                    }
                }
                foreach (object obj3 in this.chosenListItems)
                {
                    base.WriteObject(obj3);
                }
            }
        }

        private ErrorDetails GetErrorDetails(string errorId, params object[] args)
        {
            if (string.IsNullOrEmpty(errorId))
            {
                throw PSTraceSource.NewArgumentNullException("errorId");
            }
            return new ErrorDetails(base.GetType().Assembly, "GetRandomCommandStrings", errorId, args);
        }

        private double GetRandomDouble(double min, double max)
        {
            double num;
            double d = max - min;
            if (double.IsInfinity(d))
            {
                do
                {
                    double num3 = this.Generator.NextDouble();
                    num = (min + (num3 * max)) - (num3 * min);
                }
                while (num >= max);
                return num;
            }
            do
            {
                double num4 = this.Generator.NextDouble();
                num = min + (num4 * d);
                d *= num4;
            }
            while (num >= max);
            return num;
        }

        private long GetRandomInt64(long min, long max)
        {
            long num;
            byte[] buffer = new byte[8];
            BigInteger integer = max - min;
            if (integer <= 0x7fffffffL)
            {
                int num2 = this.Generator.Next(0, (int) (max - min));
                return (min + num2);
            }
            long num3 = (long) integer;
            int num4 = 0;
            long num5 = num3;
            while (num5 != 0L)
            {
                num5 = num5 >> 1;
                num4++;
            }
            long num6 = ((long) (-1L)) >> (0x40 - num4);
            do
            {
                this.Generator.NextBytes(buffer);
                num = BitConverter.ToInt64(buffer, 0) & num6;
            }
            while (num3 <= num);
            double num7 = (min * 1.0) + (num * 1.0);
            return (long) num7;
        }

        private bool IsInt(object o)
        {
            if ((o != null) && !(o is int))
            {
                return false;
            }
            return true;
        }

        private bool IsInt64(object o)
        {
            if ((o != null) && !(o is long))
            {
                return false;
            }
            return true;
        }

        private object ProcessOperand(object o)
        {
            if (o == null)
            {
                return o;
            }
            object baseObject = PSObject.AsPSObject(o).BaseObject;
            if (baseObject is string)
            {
                baseObject = Parser.ScanNumber((string) baseObject, typeof(int));
            }
            return baseObject;
        }

        protected override void ProcessRecord()
        {
            if (this.EffectiveParameterSet == MyParameterSet.RandomListItem)
            {
                foreach (object obj2 in this.InputObject)
                {
                    if (this.numberOfProcessedListItems < this.Count)
                    {
                        this.chosenListItems.Add(obj2);
                    }
                    else if (this.Generator.Next(this.numberOfProcessedListItems + 1) < this.Count)
                    {
                        int num = this.Generator.Next(this.chosenListItems.Count);
                        this.chosenListItems[num] = obj2;
                    }
                    this.numberOfProcessedListItems++;
                }
            }
        }

        private void ThrowMinGreaterThanOrEqualMax(object min, object max)
        {
            if (min == null)
            {
                throw PSTraceSource.NewArgumentNullException("min");
            }
            if (max == null)
            {
                throw PSTraceSource.NewArgumentNullException("max");
            }
            string errorId = "MinGreaterThanOrEqualMax";
            ErrorRecord errorRecord = new ErrorRecord(new ArgumentException(this.GetErrorDetails(errorId, new object[] { min, max }).Message), errorId, ErrorCategory.InvalidArgument, null);
            base.ThrowTerminatingError(errorRecord);
        }

        [Parameter(ParameterSetName="RandomListItemParameterSet"), ValidateRange(1, 0x7fffffff)]
        public int Count
        {
            get
            {
                return this.count;
            }
            set
            {
                this.count = value;
            }
        }

        private MyParameterSet EffectiveParameterSet
        {
            get
            {
                if (this.effectiveParameterSet == MyParameterSet.Unknown)
                {
                    if ((base.MyInvocation.ExpectingInput && (this.Maximum == null)) && (this.Minimum == null))
                    {
                        this.effectiveParameterSet = MyParameterSet.RandomListItem;
                    }
                    else if (base.ParameterSetName.Equals("RandomListItemParameterSet", StringComparison.OrdinalIgnoreCase))
                    {
                        this.effectiveParameterSet = MyParameterSet.RandomListItem;
                    }
                    else if (base.ParameterSetName.Equals("RandomNumberParameterSet", StringComparison.OrdinalIgnoreCase))
                    {
                        if ((this.Maximum != null) && this.Maximum.GetType().IsArray)
                        {
                            this.InputObject = (object[]) this.Maximum;
                            this.effectiveParameterSet = MyParameterSet.RandomListItem;
                        }
                        else
                        {
                            this.effectiveParameterSet = MyParameterSet.RandomNumber;
                        }
                    }
                }
                return this.effectiveParameterSet;
            }
        }

        private Random Generator
        {
            get
            {
                if (this.generator == null)
                {
                    Guid instanceId = base.Context.CurrentRunspace.InstanceId;
                    bool flag = false;
                    try
                    {
                        runspaceGeneratorMapLock.AcquireReaderLock(-1);
                        flag = !RunspaceGeneratorMap.TryGetValue(instanceId, out this.generator);
                    }
                    finally
                    {
                        runspaceGeneratorMapLock.ReleaseReaderLock();
                    }
                    if (flag)
                    {
                        this.Generator = new Random();
                    }
                }
                return this.generator;
            }
            set
            {
                this.generator = value;
                Runspace currentRunspace = base.Context.CurrentRunspace;
                try
                {
                    runspaceGeneratorMapLock.AcquireWriterLock(-1);
                    if (!RunspaceGeneratorMap.ContainsKey(currentRunspace.InstanceId))
                    {
                        currentRunspace.StateChanged += new EventHandler<RunspaceStateEventArgs>(GetRandomCommand.CurrentRunspace_StateChanged);
                    }
                    RunspaceGeneratorMap[currentRunspace.InstanceId] = this.generator;
                }
                finally
                {
                    runspaceGeneratorMapLock.ReleaseWriterLock();
                }
            }
        }

        [Parameter(ParameterSetName="RandomListItemParameterSet", ValueFromPipeline=true, Position=0, Mandatory=true), ValidateNotNullOrEmpty]
        public object[] InputObject
        {
            get
            {
                return this.inputObject;
            }
            set
            {
                this.inputObject = value;
            }
        }

        [Parameter(ParameterSetName="RandomNumberParameterSet", Position=0)]
        public object Maximum
        {
            get
            {
                return this.maximum;
            }
            set
            {
                this.maximum = value;
            }
        }

        [Parameter(ParameterSetName="RandomNumberParameterSet")]
        public object Minimum
        {
            get
            {
                return this.minimum;
            }
            set
            {
                this.minimum = value;
            }
        }

        [ValidateNotNull, Parameter]
        public int? SetSeed
        {
            get
            {
                return this.setSeed;
            }
            set
            {
                this.setSeed = value;
            }
        }

        private enum MyParameterSet
        {
            Unknown,
            RandomNumber,
            RandomListItem
        }
    }
}

