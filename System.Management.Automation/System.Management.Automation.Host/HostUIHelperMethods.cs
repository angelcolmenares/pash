namespace System.Management.Automation.Host
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class HostUIHelperMethods
    {
        internal static void BuildHotkeysAndPlainLabels(Collection<ChoiceDescription> choices, out string[,] hotkeysAndPlainLabels)
        {
            hotkeysAndPlainLabels = new string[2, choices.Count];
            for (int i = 0; i < choices.Count; i++)
            {
                hotkeysAndPlainLabels[0, i] = string.Empty;
                int index = choices[i].Label.IndexOf('&');
                if (index >= 0)
                {
                    StringBuilder builder = new StringBuilder(choices[i].Label.Substring(0, index), choices[i].Label.Length);
                    if ((index + 1) < choices[i].Label.Length)
                    {
                        builder.Append(choices[i].Label.Substring(index + 1));
                        hotkeysAndPlainLabels[0, i] = choices[i].Label.Substring(index + 1, 1).Trim().ToUpper(CultureInfo.CurrentCulture);
                    }
                    hotkeysAndPlainLabels[1, i] = builder.ToString().Trim();
                }
                else
                {
                    hotkeysAndPlainLabels[1, i] = choices[i].Label;
                }
                if (string.Compare(hotkeysAndPlainLabels[0, i], "?", StringComparison.Ordinal) == 0)
                {
                    throw PSTraceSource.NewArgumentException(string.Format(CultureInfo.InvariantCulture, "choices[{0}].Label", new object[] { i }), "InternalHostUserInterfaceStrings", "InvalidChoiceHotKeyError", new object[0]);
                }
            }
        }

        internal static int DetermineChoicePicked(string response, Collection<ChoiceDescription> choices, string[,] hotkeysAndPlainLabels)
        {
            int num = -1;
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            for (int i = 0; i < choices.Count; i++)
            {
                if (string.Compare(response, hotkeysAndPlainLabels[1, i], true, currentCulture) == 0)
                {
                    num = i;
                    break;
                }
            }
            if (num == -1)
            {
                for (int j = 0; j < choices.Count; j++)
                {
                    if ((hotkeysAndPlainLabels[0, j].Length > 0) && (string.Compare(response, hotkeysAndPlainLabels[0, j], true, currentCulture) == 0))
                    {
                        return j;
                    }
                }
            }
            return num;
        }
    }
}

