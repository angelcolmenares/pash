namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal static class Verbs
    {
        private static Dictionary<string, string[]> recommendedAlternateVerbs = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, bool> validVerbs = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        static Verbs()
        {
            Type[] typeArray = new Type[] { typeof(VerbsCommon), typeof(VerbsCommunications), typeof(VerbsData), typeof(VerbsDiagnostic), typeof(VerbsLifecycle), typeof(VerbsOther), typeof(VerbsSecurity) };
            foreach (Type type in typeArray)
            {
                foreach (FieldInfo info in type.GetFields())
                {
                    if (info.IsLiteral)
                    {
                        validVerbs.Add((string) info.GetValue(null), true);
                    }
                }
            }
            recommendedAlternateVerbs.Add("accept", new string[] { "Receive" });
            recommendedAlternateVerbs.Add("acquire", new string[] { "Get", "Read" });
            recommendedAlternateVerbs.Add("allocate", new string[] { "New" });
            recommendedAlternateVerbs.Add("allow", new string[] { "Enable", "Grant", "Unblock" });
            recommendedAlternateVerbs.Add("amend", new string[] { "Edit" });
            recommendedAlternateVerbs.Add("analyze", new string[] { "Measure", "Test" });
            recommendedAlternateVerbs.Add("append", new string[] { "Add" });
            recommendedAlternateVerbs.Add("assign", new string[] { "Set" });
            recommendedAlternateVerbs.Add("associate", new string[] { "Join", "Merge" });
            recommendedAlternateVerbs.Add("attach", new string[] { "Add", "Debug" });
            recommendedAlternateVerbs.Add("bc", new string[] { "Compare" });
            recommendedAlternateVerbs.Add("boot", new string[] { "Start" });
            recommendedAlternateVerbs.Add("break", new string[] { "Disconnect" });
            recommendedAlternateVerbs.Add("broadcast", new string[] { "Send" });
            recommendedAlternateVerbs.Add("build", new string[] { "New" });
            recommendedAlternateVerbs.Add("burn", new string[] { "Backup" });
            recommendedAlternateVerbs.Add("calculate", new string[] { "Measure" });
            recommendedAlternateVerbs.Add("cancel", new string[] { "Stop" });
            recommendedAlternateVerbs.Add("cat", new string[] { "Get" });
            recommendedAlternateVerbs.Add("change", new string[] { "Convert", "Edit", "Rename" });
            recommendedAlternateVerbs.Add("clean", new string[] { "Uninstall" });
            recommendedAlternateVerbs.Add("clone", new string[] { "Copy" });
            recommendedAlternateVerbs.Add("combine", new string[] { "Join", "Merge" });
            recommendedAlternateVerbs.Add("compact", new string[] { "Compress" });
            recommendedAlternateVerbs.Add("concatenate", new string[] { "Add" });
            recommendedAlternateVerbs.Add("configure", new string[] { "Set" });
            recommendedAlternateVerbs.Add("create", new string[] { "New" });
            recommendedAlternateVerbs.Add("cut", new string[] { "Remove" });
            recommendedAlternateVerbs.Add("delete", new string[] { "Remove" });
            recommendedAlternateVerbs.Add("deploy", new string[] { "Install", "Publish" });
            recommendedAlternateVerbs.Add("detach", new string[] { "Dismount", "Remove" });
            recommendedAlternateVerbs.Add("determine", new string[] { "Measure", "Resolve" });
            recommendedAlternateVerbs.Add("diagnose", new string[] { "Debug", "Test" });
            recommendedAlternateVerbs.Add("diff", new string[] { "Checkpoint", "Compare" });
            recommendedAlternateVerbs.Add("difference", new string[] { "Checkpoint", "Compare" });
            recommendedAlternateVerbs.Add("dig", new string[] { "Trace" });
            recommendedAlternateVerbs.Add("dir", new string[] { "Get" });
            recommendedAlternateVerbs.Add("discard", new string[] { "Remove" });
            recommendedAlternateVerbs.Add("display", new string[] { "Show", "Write" });
            recommendedAlternateVerbs.Add("dispose", new string[] { "Remove" });
            recommendedAlternateVerbs.Add("divide", new string[] { "Split" });
            recommendedAlternateVerbs.Add("dump", new string[] { "Get" });
            recommendedAlternateVerbs.Add("duplicate", new string[] { "Copy" });
            recommendedAlternateVerbs.Add("empty", new string[] { "Clear" });
            recommendedAlternateVerbs.Add("end", new string[] { "Stop" });
            recommendedAlternateVerbs.Add("erase", new string[] { "Clear", "Remove" });
            recommendedAlternateVerbs.Add("examine", new string[] { "Get" });
            recommendedAlternateVerbs.Add("execute", new string[] { "Invoke" });
            recommendedAlternateVerbs.Add("explode", new string[] { "Expand" });
            recommendedAlternateVerbs.Add("extract", new string[] { "Export" });
            recommendedAlternateVerbs.Add("fix", new string[] { "Repair", "Restore" });
            recommendedAlternateVerbs.Add("flush", new string[] { "Clear" });
            recommendedAlternateVerbs.Add("follow", new string[] { "Trace" });
            recommendedAlternateVerbs.Add("generate", new string[] { "New" });
            recommendedAlternateVerbs.Add("halt", new string[] { "Disable" });
            recommendedAlternateVerbs.Add("in", new string[] { "ConvertTo" });
            recommendedAlternateVerbs.Add("index", new string[] { "Update" });
            recommendedAlternateVerbs.Add("initiate", new string[] { "Start" });
            recommendedAlternateVerbs.Add("input", new string[] { "ConvertTo", "Unregister" });
            recommendedAlternateVerbs.Add("insert", new string[] { "Add", "Unregister" });
            recommendedAlternateVerbs.Add("inspect", new string[] { "Trace" });
            recommendedAlternateVerbs.Add("kill", new string[] { "Stop" });
            recommendedAlternateVerbs.Add("launch", new string[] { "Start" });
            recommendedAlternateVerbs.Add("load", new string[] { "Import" });
            recommendedAlternateVerbs.Add("locate", new string[] { "Search", "Select" });
            recommendedAlternateVerbs.Add("logoff", new string[] { "Disconnect" });
            recommendedAlternateVerbs.Add("mail", new string[] { "Send" });
            recommendedAlternateVerbs.Add("make", new string[] { "New" });
            recommendedAlternateVerbs.Add("match", new string[] { "Select" });
            recommendedAlternateVerbs.Add("migrate", new string[] { "Move" });
            recommendedAlternateVerbs.Add("modify", new string[] { "Edit" });
            recommendedAlternateVerbs.Add("name", new string[] { "Move" });
            recommendedAlternateVerbs.Add("nullify", new string[] { "Clear" });
            recommendedAlternateVerbs.Add("obtain", new string[] { "Get" });
            recommendedAlternateVerbs.Add("output", new string[] { "ConvertFrom" });
            recommendedAlternateVerbs.Add("pause", new string[] { "Suspend", "Wait" });
            recommendedAlternateVerbs.Add("peek", new string[] { "Receive" });
            recommendedAlternateVerbs.Add("permit", new string[] { "Enable" });
            recommendedAlternateVerbs.Add("purge", new string[] { "Clear", "Remove" });
            recommendedAlternateVerbs.Add("pick", new string[] { "Select" });
            recommendedAlternateVerbs.Add("prevent", new string[] { "Block" });
            recommendedAlternateVerbs.Add("print", new string[] { "Write" });
            recommendedAlternateVerbs.Add("prompt", new string[] { "Read" });
            recommendedAlternateVerbs.Add("put", new string[] { "Send", "Write" });
            recommendedAlternateVerbs.Add("puts", new string[] { "Write" });
            recommendedAlternateVerbs.Add("quota", new string[] { "Limit" });
            recommendedAlternateVerbs.Add("quote", new string[] { "Limit" });
            recommendedAlternateVerbs.Add("rebuild", new string[] { "Initialize" });
            recommendedAlternateVerbs.Add("recycle", new string[] { "Restart" });
            recommendedAlternateVerbs.Add("refresh", new string[] { "Update" });
            recommendedAlternateVerbs.Add("reinitialize", new string[] { "Initialize" });
            recommendedAlternateVerbs.Add("release", new string[] { "Clear", "Install", "Publish", "Unlock" });
            recommendedAlternateVerbs.Add("reload", new string[] { "Update" });
            recommendedAlternateVerbs.Add("renew", new string[] { "Initialize", "Update" });
            recommendedAlternateVerbs.Add("replicate", new string[] { "Copy" });
            recommendedAlternateVerbs.Add("resample", new string[] { "Convert" });
            recommendedAlternateVerbs.Add("restrict", new string[] { "Lock" });
            recommendedAlternateVerbs.Add("return", new string[] { "Repair", "Restore" });
            recommendedAlternateVerbs.Add("revert", new string[] { "Unpublish" });
            recommendedAlternateVerbs.Add("revise", new string[] { "Edit" });
            recommendedAlternateVerbs.Add("run", new string[] { "Invoke", "Start" });
            recommendedAlternateVerbs.Add("salvage", new string[] { "Test" });
            recommendedAlternateVerbs.Add("secure", new string[] { "Lock" });
            recommendedAlternateVerbs.Add("separate", new string[] { "Split" });
            recommendedAlternateVerbs.Add("setup", new string[] { "Initialize", "Install" });
            recommendedAlternateVerbs.Add("sleep", new string[] { "Suspend", "Wait" });
            recommendedAlternateVerbs.Add("starttransaction", new string[] { "Checkpoint" });
            recommendedAlternateVerbs.Add("telnet", new string[] { "Connect" });
            recommendedAlternateVerbs.Add("terminate", new string[] { "Stop" });
            recommendedAlternateVerbs.Add("track", new string[] { "Trace" });
            recommendedAlternateVerbs.Add("transfer", new string[] { "Move" });
            recommendedAlternateVerbs.Add("type", new string[] { "Get" });
            recommendedAlternateVerbs.Add("unite", new string[] { "Join", "Merge" });
            recommendedAlternateVerbs.Add("unlink", new string[] { "Dismount" });
            recommendedAlternateVerbs.Add("unmark", new string[] { "Clear" });
            recommendedAlternateVerbs.Add("unrestrict", new string[] { "Unlock" });
            recommendedAlternateVerbs.Add("unsecure", new string[] { "Unlock" });
            recommendedAlternateVerbs.Add("unset", new string[] { "Clear" });
            recommendedAlternateVerbs.Add("verify", new string[] { "Test" });
        }

        internal static bool IsStandard(string verb)
        {
            return validVerbs.ContainsKey(verb);
        }

        internal static string[] SuggestedAlternates(string verb)
        {
            string[] strArray = null;
            recommendedAlternateVerbs.TryGetValue(verb, out strArray);
            return strArray;
        }
    }
}

