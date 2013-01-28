namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal static class FormatInfoDataClassFactory
    {
        private static readonly Hashtable constructors = new Hashtable();
        [TraceSource("FormatInfoDataClassFactory", "FormatInfoDataClassFactory")]
        internal static PSTraceSource tracer = PSTraceSource.GetTracer("FormatInfoDataClassFactory", "FormatInfoDataClassFactory");

        static FormatInfoDataClassFactory()
        {
            constructors.Add("033ecb2bc07a4d43b5ef94ed5a35d280", typeof(FormatStartData));
            constructors.Add("cf522b78d86c486691226b40aa69e95c", typeof(FormatEndData));
            constructors.Add("9e210fe47d09416682b841769c78b8a3", typeof(GroupStartData));
            constructors.Add("4ec4f0187cb04f4cb6973460dfe252df", typeof(GroupEndData));
            constructors.Add("27c87ef9bbda4f709f6b4002fa4af63c", typeof(FormatEntryData));
            constructors.Add("b2e2775d33d544c794d0081f27021b5c", typeof(WideViewHeaderInfo));
            constructors.Add("e3b7a39c089845d388b2e84c5d38f5dd", typeof(TableHeaderInfo));
            constructors.Add("7572aa4155ec4558817a615acf7dd92e", typeof(TableColumnInfo));
            constructors.Add("830bdcb24c1642258724e441512233a4", typeof(ListViewHeaderInfo));
            constructors.Add("cf58f450baa848ef8eb3504008be6978", typeof(ListViewEntry));
            constructors.Add("b761477330ce4fb2a665999879324d73", typeof(ListViewField));
            constructors.Add("0e59526e2dd441aa91e7fc952caf4a36", typeof(TableRowEntry));
            constructors.Add("59bf79de63354a7b9e4d1697940ff188", typeof(WideViewEntry));
            constructors.Add("5197dd85ca6f4cce9ae9e6fd6ded9d76", typeof(ComplexViewHeaderInfo));
            constructors.Add("22e7ef3c896449d4a6f2dedea05dd737", typeof(ComplexViewEntry));
            constructors.Add("919820b7eadb48be8e202c5afa5c2716", typeof(GroupingEntry));
            constructors.Add("dd1290a5950b4b27aa76d9f06199c3b3", typeof(PageHeaderEntry));
            constructors.Add("93565e84730645c79d4af091123eecbc", typeof(PageFooterEntry));
            constructors.Add("a27f094f0eec4d64845801a4c06a32ae", typeof(AutosizeInfo));
            constructors.Add("de7e8b96fbd84db5a43aa82eb34580ec", typeof(FormatNewLine));
            constructors.Add("091C9E762E33499eBE318901B6EFB733", typeof(FrameInfo));
            constructors.Add("b8d9e369024a43a580b9e0c9279e3354", typeof(FormatTextField));
            constructors.Add("78b102e894f742aca8c1d6737b6ff86a", typeof(FormatPropertyField));
            constructors.Add("fba029a113a5458d932a2ed4871fadf2", typeof(FormatEntry));
            constructors.Add("29ED81BA914544d4BC430F027EE053E9", typeof(RawTextFormatEntry));
        }

        internal static FormatInfoData CreateInstance(PSObject so, FormatObjectDeserializer deserializer)
        {
            if (so == null)
            {
                throw PSTraceSource.NewArgumentNullException("so");
            }
            string property = FormatObjectDeserializer.GetProperty(so, "ClassId2e4f51ef21dd47e99d3c952918aff9cd") as string;
            if (property == null)
            {
                string message = StringUtil.Format(FormatAndOut_format_xxx.FOD_InvalidClassidProperty, new object[0]);
                ErrorRecord errorRecord = new ErrorRecord(PSTraceSource.NewArgumentException("classid"), "FormatObjectDeserializerInvalidClassidProperty", ErrorCategory.InvalidData, so) {
                    ErrorDetails = new ErrorDetails(message)
                };
                deserializer.TerminatingErrorContext.ThrowTerminatingError(errorRecord);
            }
            return CreateInstance(property, deserializer);
        }

        private static FormatInfoData CreateInstance(string clsid, FormatObjectDeserializer deserializer)
        {
            Type type = GetType(clsid);
            if (null == type)
            {
                CreateInstanceError(PSTraceSource.NewArgumentException("clsid"), clsid, deserializer);
                return null;
            }
            try
            {
                return (FormatInfoData) Activator.CreateInstance(type);
            }
            catch (ArgumentException exception)
            {
                CreateInstanceError(exception, clsid, deserializer);
            }
            catch (NotSupportedException exception2)
            {
                CreateInstanceError(exception2, clsid, deserializer);
            }
            catch (TargetInvocationException exception3)
            {
                CreateInstanceError(exception3, clsid, deserializer);
            }
            catch (MemberAccessException exception4)
            {
                CreateInstanceError(exception4, clsid, deserializer);
            }
            catch (InvalidComObjectException exception5)
            {
                CreateInstanceError(exception5, clsid, deserializer);
            }
            catch (COMException exception6)
            {
                CreateInstanceError(exception6, clsid, deserializer);
            }
            catch (TypeLoadException exception7)
            {
                CreateInstanceError(exception7, clsid, deserializer);
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }

        private static void CreateInstanceError(Exception e, string clsid, FormatObjectDeserializer deserializer)
        {
            string message = StringUtil.Format(FormatAndOut_format_xxx.FOD_InvalidClassid, clsid);
            ErrorRecord errorRecord = new ErrorRecord(e, "FormatObjectDeserializerInvalidClassid", ErrorCategory.InvalidData, null) {
                ErrorDetails = new ErrorDetails(message)
            };
            deserializer.TerminatingErrorContext.ThrowTerminatingError(errorRecord);
        }

        private static Type GetType(string clsid)
        {
            object obj2 = constructors[clsid];
            return (obj2 as Type);
        }
    }
}

