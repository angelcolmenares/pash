namespace System.Management.Automation
{
    using System;

    internal static class SerializationUtilities
    {
        internal static object GetPropertyValue(PSObject psObject, string propertyName)
        {
            PSNoteProperty property = (PSNoteProperty) psObject.Properties[propertyName];
            if (property == null)
            {
                return null;
            }
            return property.Value;
        }

        internal static object GetPsObjectPropertyBaseObject(PSObject psObject, string propertyName)
        {
            PSObject propertyValue = (PSObject) GetPropertyValue(psObject, propertyName);
            if (propertyValue == null)
            {
                return null;
            }
            return propertyValue.BaseObject;
        }
    }
}

