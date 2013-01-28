namespace System.Spatial
{
    using System;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class TextRes
    {
        internal const string GeoJsonReader_ExpectedArray = "GeoJsonReader_ExpectedArray";
        internal const string GeoJsonReader_ExpectedNumeric = "GeoJsonReader_ExpectedNumeric";
        internal const string GeoJsonReader_InvalidCrsName = "GeoJsonReader_InvalidCrsName";
        internal const string GeoJsonReader_InvalidCrsType = "GeoJsonReader_InvalidCrsType";
        internal const string GeoJsonReader_InvalidNullElement = "GeoJsonReader_InvalidNullElement";
        internal const string GeoJsonReader_InvalidPosition = "GeoJsonReader_InvalidPosition";
        internal const string GeoJsonReader_InvalidTypeName = "GeoJsonReader_InvalidTypeName";
        internal const string GeoJsonReader_MissingRequiredMember = "GeoJsonReader_MissingRequiredMember";
        internal const string GmlReader_EmptyRingsNotAllowed = "GmlReader_EmptyRingsNotAllowed";
        internal const string GmlReader_ExpectReaderAtElement = "GmlReader_ExpectReaderAtElement";
        internal const string GmlReader_InvalidAttribute = "GmlReader_InvalidAttribute";
        internal const string GmlReader_InvalidSpatialType = "GmlReader_InvalidSpatialType";
        internal const string GmlReader_InvalidSrsName = "GmlReader_InvalidSrsName";
        internal const string GmlReader_PosListNeedsEvenCount = "GmlReader_PosListNeedsEvenCount";
        internal const string GmlReader_PosNeedTwoNumbers = "GmlReader_PosNeedTwoNumbers";
        internal const string GmlReader_UnexpectedElement = "GmlReader_UnexpectedElement";
        internal const string InvalidPointCoordinate = "InvalidPointCoordinate";
        internal const string JsonReaderExtensions_CannotReadPropertyValueAsString = "JsonReaderExtensions_CannotReadPropertyValueAsString";
        internal const string JsonReaderExtensions_CannotReadValueAsJsonObject = "JsonReaderExtensions_CannotReadValueAsJsonObject";
        private static TextRes loader;
        internal const string Point_AccessCoordinateWhenEmpty = "Point_AccessCoordinateWhenEmpty";
        internal const string PriorityQueueDoesNotContainItem = "PriorityQueueDoesNotContainItem";
        internal const string PriorityQueueEnqueueExistingPriority = "PriorityQueueEnqueueExistingPriority";
        internal const string PriorityQueueOperationNotValidOnEmptyQueue = "PriorityQueueOperationNotValidOnEmptyQueue";
        private ResourceManager resources;
        internal const string SpatialBuilder_CannotCreateBeforeDrawn = "SpatialBuilder_CannotCreateBeforeDrawn";
        internal const string SpatialImplementation_NoRegisteredOperations = "SpatialImplementation_NoRegisteredOperations";
        internal const string Validator_FullGlobeCannotHaveElements = "Validator_FullGlobeCannotHaveElements";
        internal const string Validator_FullGlobeInCollection = "Validator_FullGlobeInCollection";
        internal const string Validator_InvalidLatitudeCoordinate = "Validator_InvalidLatitudeCoordinate";
        internal const string Validator_InvalidLongitudeCoordinate = "Validator_InvalidLongitudeCoordinate";
        internal const string Validator_InvalidPointCoordinate = "Validator_InvalidPointCoordinate";
        internal const string Validator_InvalidPolygonPoints = "Validator_InvalidPolygonPoints";
        internal const string Validator_InvalidType = "Validator_InvalidType";
        internal const string Validator_LineStringNeedsTwoPoints = "Validator_LineStringNeedsTwoPoints";
        internal const string Validator_NestingOverflow = "Validator_NestingOverflow";
        internal const string Validator_SridMismatch = "Validator_SridMismatch";
        internal const string Validator_UnexpectedCall = "Validator_UnexpectedCall";
        internal const string Validator_UnexpectedCall2 = "Validator_UnexpectedCall2";
        internal const string Validator_UnexpectedGeography = "Validator_UnexpectedGeography";
        internal const string Validator_UnexpectedGeometry = "Validator_UnexpectedGeometry";
        internal const string WellKnownText_TooManyDimensions = "WellKnownText_TooManyDimensions";
        internal const string WellKnownText_UnexpectedCharacter = "WellKnownText_UnexpectedCharacter";
        internal const string WellKnownText_UnexpectedToken = "WellKnownText_UnexpectedToken";
        internal const string WellKnownText_UnknownTaggedText = "WellKnownText_UnknownTaggedText";

        internal TextRes()
        {
            this.resources = new ResourceManager("System.Spatial", base.GetType().Assembly);
        }

        private static TextRes GetLoader()
        {
            if (loader == null)
            {
                TextRes res = new TextRes();
                Interlocked.CompareExchange<TextRes>(ref loader, res, null);
            }
            return loader;
        }

        public static object GetObject(string name)
        {
            TextRes loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetObject(name, Culture);
        }

        public static string GetString(string name)
        {
            TextRes loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetString(name, Culture);
        }

        public static string GetString(string name, out bool usedFallback)
        {
            usedFallback = false;
            return GetString(name);
        }

        public static string GetString(string name, params object[] args)
        {
            TextRes loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            string format = loader.resources.GetString(name, Culture);
            if ((args == null) || (args.Length <= 0))
            {
                return format;
            }
            for (int i = 0; i < args.Length; i++)
            {
                string str2 = args[i] as string;
                if ((str2 != null) && (str2.Length > 0x400))
                {
                    args[i] = str2.Substring(0, 0x3fd) + "...";
                }
            }
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        private static CultureInfo Culture
        {
            get
            {
                return null;
            }
        }

        public static ResourceManager Resources
        {
            get
            {
                return GetLoader().resources;
            }
        }
    }
}

