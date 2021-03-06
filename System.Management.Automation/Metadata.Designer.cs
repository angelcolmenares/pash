﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace System.Management.Automation {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Metadata {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Metadata() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("System.Management.Automation.Metadata", typeof(Metadata).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Parameter alias cannot be specified because an alias with the name &apos;{0}&apos; was defined multiple times for the command..
        /// </summary>
        public static string AliasParameterNameAlreadyExistsForCommand {
            get {
                return ResourceManager.GetString("AliasParameterNameAlreadyExistsForCommand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot process argument because the argument value is a non-string. Arguments to parameters with the ArgumentTransformationAttribute specified should be strings..
        /// </summary>
        public static string ArgumentTransformationArgumentsShouldBeStrings {
            get {
                return ResourceManager.GetString("ArgumentTransformationArgumentsShouldBeStrings", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Attribute cannot be added because it would cause the variable {0} with value {1} to become invalid..
        /// </summary>
        public static string InvalidMetadataForCurrentValue {
            get {
                return ResourceManager.GetString("InvalidMetadataForCurrentValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The variable cannot be validated because the value {1} is not a valid value for the {0} variable..
        /// </summary>
        public static string InvalidValueFailure {
            get {
                return ResourceManager.GetString("InvalidValueFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Job conversion type must derive from IAstToScriptBlockConverter..
        /// </summary>
        public static string JobDefinitionMustDeriveFromIJobConverter {
            get {
                return ResourceManager.GetString("JobDefinitionMustDeriveFromIJobConverter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot initialize attributes for &quot;{0}&quot;: &quot;{1}&quot;.
        /// </summary>
        public static string MetadataMemberInitialization {
            get {
                return ResourceManager.GetString("MetadataMemberInitialization", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A parameter with the name &apos;{0}&apos; was defined multiple times for the command..
        /// </summary>
        public static string ParameterNameAlreadyExistsForCommand {
            get {
                return ResourceManager.GetString("ParameterNameAlreadyExistsForCommand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Parameter &apos;{0}&apos; cannot be specified because it conflicts with the parameter alias of the same name for parameter &apos;{1}&apos;..
        /// </summary>
        public static string ParameterNameConflictsWithAlias {
            get {
                return ResourceManager.GetString("ParameterNameConflictsWithAlias", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot get or execute command. The maximum number of parameter sets for this command is exceeded..
        /// </summary>
        public static string ParsingTooManyParameterSets {
            get {
                return ResourceManager.GetString("ParsingTooManyParameterSets", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The number of supplied arguments ({1}) exceeds the maximum number of allowed arguments ({0}). Specify less than {0} arguments and then try the command again..
        /// </summary>
        public static string ValidateCountMaxLengthFailure {
            get {
                return ResourceManager.GetString("ValidateCountMaxLengthFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified maximum number of arguments for a parameter is smaller than the specified minimum number of arguments. Update the ValidateCount attribute for the parameter..
        /// </summary>
        public static string ValidateCountMaxLengthSmallerThanMinLength {
            get {
                return ResourceManager.GetString("ValidateCountMaxLengthSmallerThanMinLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The number of supplied arguments ({1}) is less than the minimum number of allowed arguments ({0}). Specify more than {0} arguments and then try the command again..
        /// </summary>
        public static string ValidateCountMinLengthFailure {
            get {
                return ResourceManager.GetString("ValidateCountMinLengthFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The ValidateCount attribute cannot be applied to a non-array parameter. Either remove the attribute from the parameter or make the parameter an array parameter..
        /// </summary>
        public static string ValidateCountNotInArray {
            get {
                return ResourceManager.GetString("ValidateCountNotInArray", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &quot;{0}&quot; failed on property &quot;{1}&quot; {2}.
        /// </summary>
        public static string ValidateFailureResult {
            get {
                return ResourceManager.GetString("ValidateFailureResult", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The argument length of {1} is too long. Shorten the length of the argument to less than or equal to &quot;{0}&quot; and then try the command again..
        /// </summary>
        public static string ValidateLengthMaxLengthFailure {
            get {
                return ResourceManager.GetString("ValidateLengthMaxLengthFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified maximum argument length is smaller than the specified minimum argument length. Update the ValidateLength attribute for the parameter..
        /// </summary>
        public static string ValidateLengthMaxLengthSmallerThanMinLength {
            get {
                return ResourceManager.GetString("ValidateLengthMaxLengthSmallerThanMinLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The number of characters ({1}) in the argument is too small. Specify an argument whose length is greater than or equal to &quot;{0}&quot; and then try the command again..
        /// </summary>
        public static string ValidateLengthMinLengthFailure {
            get {
                return ResourceManager.GetString("ValidateLengthMinLengthFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The ValidateLength attribute cannot be applied to a parameter that is not a string or string[] parameter. Make the parameter a string or string[] parameter..
        /// </summary>
        public static string ValidateLengthNotString {
            get {
                return ResourceManager.GetString("ValidateLengthNotString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The argument is null, or an element of the argument collection contains a null value. Supply a collection that does not contain any null values and then try the command again..
        /// </summary>
        public static string ValidateNotNullCollectionFailure {
            get {
                return ResourceManager.GetString("ValidateNotNullCollectionFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The argument is null. Supply a non-null argument and try the command again..
        /// </summary>
        public static string ValidateNotNullFailure {
            get {
                return ResourceManager.GetString("ValidateNotNullFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The argument is null, empty, or an element of the argument collection contains a null value. Supply a collection that does not contain any null values and then try the command again..
        /// </summary>
        public static string ValidateNotNullOrEmptyCollectionFailure {
            get {
                return ResourceManager.GetString("ValidateNotNullOrEmptyCollectionFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The argument is null or empty. Supply an argument that is not null or empty and then try the command again..
        /// </summary>
        public static string ValidateNotNullOrEmptyFailure {
            get {
                return ResourceManager.GetString("ValidateNotNullOrEmptyFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The argument &quot;{0}&quot; does not match the &quot;{1}&quot; pattern. Supply an argument that matches &quot;{1}&quot; and try the command again..
        /// </summary>
        public static string ValidatePatternFailure {
            get {
                return ResourceManager.GetString("ValidatePatternFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The argument cannot be validated because its type &quot;{0}&quot; is not the same type ({1}) as the maximum and minimum limits of the parameter. Make sure the argument is of type {1} and then try the command again..
        /// </summary>
        public static string ValidateRangeElementType {
            get {
                return ResourceManager.GetString("ValidateRangeElementType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The {0} argument is greater than the maximum allowed range of {1}. Supply an argument that is less than or equal to {1} and then try the command again..
        /// </summary>
        public static string ValidateRangeGreaterThanMaxRangeFailure {
            get {
                return ResourceManager.GetString("ValidateRangeGreaterThanMaxRangeFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified maximum range cannot be accepted because it is less than the specified minimum range. Update the ValidateRange attribute for the parameter..
        /// </summary>
        public static string ValidateRangeMaxRangeSmallerThanMinRange {
            get {
                return ResourceManager.GetString("ValidateRangeMaxRangeSmallerThanMinRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified minimum range ({0}) cannot be accepted because it is not the same type as the specified maximum range ({1}). Update the ValidateRange attribute for the parameter..
        /// </summary>
        public static string ValidateRangeMinRangeMaxRangeType {
            get {
                return ResourceManager.GetString("ValidateRangeMinRangeMaxRangeType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot accept the MaxRange and MinRange parameter type. Both parameters must be objects that implement IComparable interface..
        /// </summary>
        public static string ValidateRangeNotIComparable {
            get {
                return ResourceManager.GetString("ValidateRangeNotIComparable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The {0} argument is less than the minimum allowed range of {1}. Supply an argument that is greater than or equal to {1} and then try the command again..
        /// </summary>
        public static string ValidateRangeSmallerThanMinRangeFailure {
            get {
                return ResourceManager.GetString("ValidateRangeSmallerThanMinRangeFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The &quot;{1}&quot; validation script for the argument with value &quot;{0}&quot; did not return true. Determine why the validation script failed and then try the command again..
        /// </summary>
        public static string ValidateScriptFailure {
            get {
                return ResourceManager.GetString("ValidateScriptFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The argument &quot;{0}&quot; does not belong to the set &quot;{1}&quot; specified by the ValidateSet attribute. Supply an argument that is in the set and then try the command again..
        /// </summary>
        public static string ValidateSetFailure {
            get {
                return ResourceManager.GetString("ValidateSetFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ,.
        /// </summary>
        public static string ValidateSetSeparator {
            get {
                return ResourceManager.GetString("ValidateSetSeparator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot validate argument &apos;{0}&apos; because it is not a valid variable name..
        /// </summary>
        public static string ValidateVariableName {
            get {
                return ResourceManager.GetString("ValidateVariableName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The &quot;{0}&quot; argument does not contain a valid Windows PowerShell version. Supply a valid version number and then try the command again..
        /// </summary>
        public static string ValidateVersionFailure {
            get {
                return ResourceManager.GetString("ValidateVersionFailure", resourceCulture);
            }
        }
    }
}
