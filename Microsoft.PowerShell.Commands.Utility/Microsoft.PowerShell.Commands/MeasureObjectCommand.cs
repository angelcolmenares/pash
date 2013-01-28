namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Threading;

    [OutputType(new Type[] { typeof(GenericMeasureInfo), typeof(TextMeasureInfo), typeof(GenericObjectMeasureInfo) }), Cmdlet("Measure", "Object", DefaultParameterSetName="GenericMeasure", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113349", RemotingCapability=RemotingCapability.None)]
    public sealed class MeasureObjectCommand : PSCmdlet
    {
        private const string GenericParameterSet = "GenericMeasure";
        private bool ignoreWhiteSpace;
        private PSObject inputObject = AutomationNull.Value;
        private bool measureAverage;
        private bool measureCharacters;
        private bool measureLines;
        private bool measureMax;
        private bool measureMin;
        private bool measureSum;
        private bool measureWords;
        private bool nonNumericError;
        private string[] property;
        private const string ResourcesBaseName = "MeasureObjectStrings";
        private MeasureObjectDictionary<Statistics> statistics = new MeasureObjectDictionary<Statistics>();
        private const string TextParameterSet = "TextMeasure";
        private const string thisObject = "$_";
        private int totalRecordCount;

        private void AnalyzeNumber(double numValue, Statistics stat)
        {
            if (this.measureSum || this.measureAverage)
            {
                stat.sum += numValue;
            }
        }

        private void AnalyzeObjectProperties(PSObject inObj)
        {
            MeasureObjectDictionary<object> dictionary = new MeasureObjectDictionary<object>();
            foreach (string str in this.Property)
            {
                MshExpression expression = new MshExpression(str);
                List<MshExpression> list = expression.ResolveNames(inObj);
                if ((list == null) || (list.Count == 0))
                {
                    if (!expression.HasWildCardCharacters)
                    {
                        string key = expression.ToString();
                        this.statistics.EnsureEntry(key);
                    }
                }
                else
                {
                    foreach (MshExpression expression2 in list)
                    {
                        string str3 = expression2.ToString();
                        if (!dictionary.ContainsKey(str3))
                        {
                            List<MshExpressionResult> values = expression2.GetValues(inObj);
                            if ((values != null) && (values.Count != 0))
                            {
                                this.AnalyzeValue(str3, values[0].Result);
                                dictionary[str3] = null;
                            }
                        }
                    }
                }
            }
        }

        private void AnalyzeString(string strValue, Statistics stat)
        {
            if (this.measureCharacters)
            {
                stat.characters += TextCountUtilities.CountChar(strValue, this.ignoreWhiteSpace);
            }
            if (this.measureWords)
            {
                stat.words += TextCountUtilities.CountWord(strValue);
            }
            if (this.measureLines)
            {
                stat.lines += TextCountUtilities.CountLine(strValue);
            }
        }

        private void AnalyzeValue(string propertyName, object objValue)
        {
            if (propertyName == null)
            {
                propertyName = "$_";
            }
            Statistics stat = this.statistics.EnsureEntry(propertyName);
            stat.count++;
            if ((this.measureCharacters || this.measureWords) || this.measureLines)
            {
                string strValue = (objValue == null) ? "" : objValue.ToString();
                this.AnalyzeString(strValue, stat);
            }
            if (this.measureAverage || this.measureSum)
            {
                double result = 0.0;
                if (!LanguagePrimitives.TryConvertTo<double>(objValue, out result))
                {
                    this.nonNumericError = true;
                    ErrorRecord errorRecord = new ErrorRecord(PSTraceSource.NewInvalidOperationException("MeasureObjectStrings", "NonNumericInputObject", new object[] { objValue }), "NonNumericInputObject", ErrorCategory.InvalidType, objValue);
                    base.WriteError(errorRecord);
                    return;
                }
                this.AnalyzeNumber(result, stat);
            }
            if (this.measureMin)
            {
                stat.min = this.Compare(objValue, stat.min, true);
            }
            if (this.measureMax)
            {
                stat.max = this.Compare(objValue, stat.max, false);
            }
        }

        private object Compare(object objValue, object statMinOrMaxValue, bool isMin)
        {
            double num2;
            object obj2 = objValue;
            object valueToConvert = statMinOrMaxValue;
            int num = isMin ? 1 : -1;
            obj2 = ((objValue != null) && LanguagePrimitives.TryConvertTo<double>(objValue, out num2)) ? num2 : obj2;
            valueToConvert = ((valueToConvert != null) && LanguagePrimitives.TryConvertTo<double>(valueToConvert, out num2)) ? num2 : valueToConvert;
            if (((obj2 != null) && (valueToConvert != null)) && !obj2.GetType().Equals(valueToConvert.GetType()))
            {
                obj2 = PSObject.AsPSObject(obj2).ToString();
                valueToConvert = PSObject.AsPSObject(valueToConvert).ToString();
            }
            if ((valueToConvert != null) && ((LanguagePrimitives.Compare(valueToConvert, obj2, false, Thread.CurrentThread.CurrentCulture) * num) <= 0))
            {
                return statMinOrMaxValue;
            }
            return objValue;
        }

        private MeasureInfo CreateGenericMeasureInfo(Statistics stat, bool shouldUseGenericMeasureInfo)
        {
            double? nullable = null;
            double? nullable2 = null;
            object max = null;
            object min = null;
            if (!this.nonNumericError)
            {
                if (this.measureSum)
                {
                    nullable = new double?(stat.sum);
                }
                if (this.measureAverage && (stat.count > 0))
                {
                    nullable2 = new double?(stat.sum / ((double) stat.count));
                }
            }
            if (this.measureMax)
            {
                if (shouldUseGenericMeasureInfo && (stat.max != null))
                {
                    double num;
                    LanguagePrimitives.TryConvertTo<double>(stat.max, out num);
                    max = num;
                }
                else
                {
                    max = stat.max;
                }
            }
            if (this.measureMin)
            {
                if (shouldUseGenericMeasureInfo && (stat.min != null))
                {
                    double num2;
                    LanguagePrimitives.TryConvertTo<double>(stat.min, out num2);
                    min = num2;
                }
                else
                {
                    min = stat.min;
                }
            }
            if (shouldUseGenericMeasureInfo)
            {
                GenericMeasureInfo info = new GenericMeasureInfo {
                    Count = stat.count,
                    Sum = nullable,
                    Average = nullable2
                };
                if (max != null)
                {
                    info.Maximum = new double?((double) max);
                }
                if (min != null)
                {
                    info.Minimum = new double?((double) min);
                }
                return info;
            }
            return new GenericObjectMeasureInfo { Count = stat.count, Sum = nullable, Average = nullable2, Maximum = max, Minimum = min };
        }

        private TextMeasureInfo CreateTextMeasureInfo(Statistics stat)
        {
            TextMeasureInfo info = new TextMeasureInfo();
            if (this.measureCharacters)
            {
                info.Characters = new int?(stat.characters);
            }
            if (this.measureWords)
            {
                info.Words = new int?(stat.words);
            }
            if (this.measureLines)
            {
                info.Lines = new int?(stat.lines);
            }
            return info;
        }

        protected override void EndProcessing()
        {
            if ((this.totalRecordCount == 0) && (this.Property == null))
            {
                this.statistics.EnsureEntry("$_");
            }
            foreach (string str in this.statistics.Keys)
            {
                Statistics stat = this.statistics[str];
                if ((stat.count == 0) && (this.Property != null))
                {
                    string errorId = this.IsMeasuringGeneric ? "GenericMeasurePropertyNotFound" : "TextMeasurePropertyNotFound";
                    this.WritePropertyNotFoundError(str, errorId);
                }
                else
                {
                    MeasureInfo sendToPipeline = null;
                    if (this.IsMeasuringGeneric)
                    {
                        double num;
                        if (((stat.min == null) || LanguagePrimitives.TryConvertTo<double>(stat.min, out num)) && ((stat.max == null) || LanguagePrimitives.TryConvertTo<double>(stat.max, out num)))
                        {
                            sendToPipeline = this.CreateGenericMeasureInfo(stat, true);
                        }
                        else
                        {
                            sendToPipeline = this.CreateGenericMeasureInfo(stat, false);
                        }
                    }
                    else
                    {
                        sendToPipeline = this.CreateTextMeasureInfo(stat);
                    }
                    if (this.Property != null)
                    {
                        sendToPipeline.Property = str;
                    }
                    base.WriteObject(sendToPipeline);
                }
            }
        }

        protected override void ProcessRecord()
        {
            if ((this.inputObject != null) && (this.inputObject != AutomationNull.Value))
            {
                this.totalRecordCount++;
                if (this.Property == null)
                {
                    this.AnalyzeValue(null, this.inputObject.BaseObject);
                }
                else
                {
                    this.AnalyzeObjectProperties(this.inputObject);
                }
            }
        }

        private void WritePropertyNotFoundError(string propertyName, string errorId)
        {
            ErrorRecord errorRecord = new ErrorRecord(PSTraceSource.NewArgumentException("Property"), errorId, ErrorCategory.InvalidArgument, null) {
                ErrorDetails = new ErrorDetails(this, "MeasureObjectStrings", "PropertyNotFound", new object[] { propertyName })
            };
            base.WriteError(errorRecord);
        }

        [Parameter(ParameterSetName="GenericMeasure")]
        public SwitchParameter Average
        {
            get
            {
                return this.measureAverage;
            }
            set
            {
                this.measureAverage = (bool) value;
            }
        }

        [Parameter(ParameterSetName="TextMeasure")]
        public SwitchParameter Character
        {
            get
            {
                return this.measureCharacters;
            }
            set
            {
                this.measureCharacters = (bool) value;
            }
        }

        [Parameter(ParameterSetName="TextMeasure")]
        public SwitchParameter IgnoreWhiteSpace
        {
            get
            {
                return this.ignoreWhiteSpace;
            }
            set
            {
                this.ignoreWhiteSpace = (bool) value;
            }
        }

        [Parameter(ValueFromPipeline=true)]
        public PSObject InputObject
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

        private bool IsMeasuringGeneric
        {
            get
            {
                return (string.Compare(base.ParameterSetName, "GenericMeasure", StringComparison.Ordinal) == 0);
            }
        }

        [Parameter(ParameterSetName="TextMeasure")]
        public SwitchParameter Line
        {
            get
            {
                return this.measureLines;
            }
            set
            {
                this.measureLines = (bool) value;
            }
        }

        [Parameter(ParameterSetName="GenericMeasure")]
        public SwitchParameter Maximum
        {
            get
            {
                return this.measureMax;
            }
            set
            {
                this.measureMax = (bool) value;
            }
        }

        [Parameter(ParameterSetName="GenericMeasure")]
        public SwitchParameter Minimum
        {
            get
            {
                return this.measureMin;
            }
            set
            {
                this.measureMin = (bool) value;
            }
        }

        [Parameter(Position=0), ValidateNotNullOrEmpty]
        public string[] Property
        {
            get
            {
                return this.property;
            }
            set
            {
                this.property = value;
            }
        }

        [Parameter(ParameterSetName="GenericMeasure")]
        public SwitchParameter Sum
        {
            get
            {
                return this.measureSum;
            }
            set
            {
                this.measureSum = (bool) value;
            }
        }

        [Parameter(ParameterSetName="TextMeasure")]
        public SwitchParameter Word
        {
            get
            {
                return this.measureWords;
            }
            set
            {
                this.measureWords = (bool) value;
            }
        }

        private class MeasureObjectDictionary<V> : Dictionary<string, V> where V: new()
        {
            internal MeasureObjectDictionary() : base(StringComparer.OrdinalIgnoreCase)
            {
            }

            public V EnsureEntry(string key)
            {
                V local;
                if (!base.TryGetValue(key, out local))
                {
                    local = (default(V) == null) ? Activator.CreateInstance<V>() : default(V);
                    base[key] = local;
                }
                return local;
            }
        }

        private class Statistics
        {
            internal int characters;
            internal int count;
            internal int lines;
            internal object max;
            internal object min;
            internal double sum;
            internal int words;
        }

        private static class TextCountUtilities
        {
            internal static int CountChar(string inStr, bool ignoreWhiteSpace)
            {
                if (string.IsNullOrEmpty(inStr))
                {
                    return 0;
                }
                if (!ignoreWhiteSpace)
                {
                    return inStr.Length;
                }
                int num = 0;
                foreach (char ch in inStr)
                {
                    if (!char.IsWhiteSpace(ch))
                    {
                        num++;
                    }
                }
                return num;
            }

            internal static int CountLine(string inStr)
            {
                if (string.IsNullOrEmpty(inStr))
                {
                    return 0;
                }
                int num = 0;
                foreach (char ch in inStr)
                {
                    if (ch == '\n')
                    {
                        num++;
                    }
                }
                if (inStr[inStr.Length - 1] != '\n')
                {
                    num++;
                }
                return num;
            }

            internal static int CountWord(string inStr)
            {
                if (string.IsNullOrEmpty(inStr))
                {
                    return 0;
                }
                int num = 0;
                bool flag = true;
                foreach (char ch in inStr)
                {
                    if (char.IsWhiteSpace(ch))
                    {
                        flag = true;
                    }
                    else
                    {
                        if (flag)
                        {
                            num++;
                        }
                        flag = false;
                    }
                }
                return num;
            }
        }
    }
}

