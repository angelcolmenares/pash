namespace System.Management.Automation.Host
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public interface IHostUISupportsMultipleChoiceSelection
    {
        Collection<int> PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, IEnumerable<int> defaultChoices);
    }
}

