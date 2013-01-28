namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;
    using System.Threading;

    [DataContract]
    public class ProgressRecord
    {
        [DataMember]
        private string activity;
        [DataMember]
        private string currentOperation;
        [DataMember]
        private int id;
        [DataMember]
        private int parentId;
        [DataMember]
        private int percent;
        private const string resTableName = "ProgressRecordStrings";
        [DataMember]
        private int secondsRemaining;
        [DataMember]
        private string status;
        [DataMember]
        private ProgressRecordType type;

        internal ProgressRecord(ProgressRecord other)
        {
            this.parentId = -1;
            this.percent = -1;
            this.secondsRemaining = -1;
            this.activity = other.activity;
            this.currentOperation = other.currentOperation;
            this.id = other.id;
            this.parentId = other.parentId;
            this.percent = other.percent;
            this.secondsRemaining = other.secondsRemaining;
            this.status = other.status;
            this.type = other.type;
        }

        public ProgressRecord(int activityId, string activity, string statusDescription)
        {
            this.parentId = -1;
            this.percent = -1;
            this.secondsRemaining = -1;
            if (activityId < 0)
            {
                throw PSTraceSource.NewArgumentOutOfRangeException("activityId", activityId, "ProgressRecordStrings", "ArgMayNotBeNegative", new object[] { "activityId" });
            }
            if (string.IsNullOrEmpty(activity))
            {
                throw PSTraceSource.NewArgumentException("activity", "ProgressRecordStrings", "ArgMayNotBeNullOrEmpty", new object[] { "activity" });
            }
            if (string.IsNullOrEmpty(statusDescription))
            {
                throw PSTraceSource.NewArgumentException("activity", "ProgressRecordStrings", "ArgMayNotBeNullOrEmpty", new object[] { "statusDescription" });
            }
            this.id = activityId;
            this.activity = activity;
            this.status = statusDescription;
        }

        internal static ProgressRecord FromPSObjectForRemoting(PSObject progressAsPSObject)
        {
            if (progressAsPSObject == null)
            {
                throw PSTraceSource.NewArgumentNullException("progressAsPSObject");
            }
            string propertyValue = RemotingDecoder.GetPropertyValue<string>(progressAsPSObject, "Activity");
            int activityId = RemotingDecoder.GetPropertyValue<int>(progressAsPSObject, "ActivityId");
            return new ProgressRecord(activityId, propertyValue, RemotingDecoder.GetPropertyValue<string>(progressAsPSObject, "StatusDescription")) { CurrentOperation = RemotingDecoder.GetPropertyValue<string>(progressAsPSObject, "CurrentOperation"), ParentActivityId = RemotingDecoder.GetPropertyValue<int>(progressAsPSObject, "ParentActivityId"), PercentComplete = RemotingDecoder.GetPropertyValue<int>(progressAsPSObject, "PercentComplete"), RecordType = RemotingDecoder.GetPropertyValue<ProgressRecordType>(progressAsPSObject, "Type"), SecondsRemaining = RemotingDecoder.GetPropertyValue<int>(progressAsPSObject, "SecondsRemaining") };
        }

        internal static int GetPercentageComplete(DateTime startTime, TimeSpan expectedDuration)
        {
            DateTime utcNow = DateTime.UtcNow;
            if (startTime > utcNow)
            {
                throw new ArgumentOutOfRangeException("startTime");
            }
            if (expectedDuration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("expectedDuration");
            }
            TimeSpan span = (TimeSpan) (utcNow - startTime);
            double num = expectedDuration.TotalSeconds / 9.0;
            double num2 = 100.0 * num;
            double num3 = num2 / (span.TotalSeconds + num);
            double d = 100.0 - num3;
            return (int) Math.Floor(d);
        }

        internal static int? GetSecondsRemaining(DateTime startTime, double percentageComplete)
        {
            TimeSpan span2;
            if ((percentageComplete < 1E-05) || double.IsNaN(percentageComplete))
            {
                return null;
            }
            TimeSpan span = (TimeSpan) (DateTime.UtcNow - startTime);
            try
            {
                span2 = TimeSpan.FromMilliseconds(span.TotalMilliseconds / percentageComplete);
            }
            catch (OverflowException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
            TimeSpan span3 = span2 - span;
            return new int?((int) span3.TotalSeconds);
        }

        internal PSObject ToPSObjectForRemoting()
        {
            PSObject obj2 = RemotingEncoder.CreateEmptyPSObject();
            obj2.Properties.Add(new PSNoteProperty("Activity", this.Activity));
            obj2.Properties.Add(new PSNoteProperty("ActivityId", this.ActivityId));
            obj2.Properties.Add(new PSNoteProperty("StatusDescription", this.StatusDescription));
            obj2.Properties.Add(new PSNoteProperty("CurrentOperation", this.CurrentOperation));
            obj2.Properties.Add(new PSNoteProperty("ParentActivityId", this.ParentActivityId));
            obj2.Properties.Add(new PSNoteProperty("PercentComplete", this.PercentComplete));
            obj2.Properties.Add(new PSNoteProperty("Type", this.RecordType));
            obj2.Properties.Add(new PSNoteProperty("SecondsRemaining", this.SecondsRemaining));
            return obj2;
        }

        public override string ToString()
        {
            return string.Format(Thread.CurrentThread.CurrentCulture, "parent = {0} id = {1} act = {2} stat = {3} cur = {4} pct = {5} sec = {6} type = {7}", new object[] { this.parentId, this.id, this.activity, this.status, this.currentOperation, this.percent, this.secondsRemaining, this.type });
        }

        public string Activity
        {
            get
            {
                return this.activity;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw PSTraceSource.NewArgumentException("value", "ProgressRecordStrings", "ArgMayNotBeNullOrEmpty", new object[] { "value" });
                }
                this.activity = value;
            }
        }

        public int ActivityId
        {
            get
            {
                return this.id;
            }
        }

        public string CurrentOperation
        {
            get
            {
                return this.currentOperation;
            }
            set
            {
                this.currentOperation = value;
            }
        }

        public int ParentActivityId
        {
            get
            {
                return this.parentId;
            }
            set
            {
                if (value == this.ActivityId)
                {
                    throw PSTraceSource.NewArgumentException("value", "ProgressRecordStrings", "ParentActivityIdCantBeActivityId", new object[0]);
                }
                this.parentId = value;
            }
        }

        public int PercentComplete
        {
            get
            {
                return this.percent;
            }
            set
            {
                if (value > 100)
                {
                    throw PSTraceSource.NewArgumentOutOfRangeException("value", value, "ProgressRecordStrings", "PercentMayNotBeMoreThan100", new object[] { "PercentComplete" });
                }
                this.percent = value;
            }
        }

        public ProgressRecordType RecordType
        {
            get
            {
                return this.type;
            }
            set
            {
                if ((value != ProgressRecordType.Completed) && (value != ProgressRecordType.Processing))
                {
                    throw PSTraceSource.NewArgumentException("value");
                }
                this.type = value;
            }
        }

        public int SecondsRemaining
        {
            get
            {
                return this.secondsRemaining;
            }
            set
            {
                this.secondsRemaining = value;
            }
        }

        public string StatusDescription
        {
            get
            {
                return this.status;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw PSTraceSource.NewArgumentException("value", "ProgressRecordStrings", "ArgMayNotBeNullOrEmpty", new object[] { "value" });
                }
                this.status = value;
            }
        }
    }
}

