@{
    GUID = "5696d5ef-fa2d-4997-94f1-0bc13daa2ac5"
    Author = "Microsoft Corporation"
    CompanyName = "Microsoft Corporation"
    Copyright = "© Microsoft Corporation. All rights reserved."
    ModuleVersion = "1.0.0.0"
    PowerShellVersion = "3.0"
    ClrVersion = "4.0"
    NestedModules = @(
        "dnslookup",
        "MSFT_DnsClient.cdxml",
        "MSFT_DnsClientCache.cdxml",
        "MSFT_DnsClientGlobalSetting.cdxml",
        "MSFT_DnsClientServerAddress.cdxml",
        "PS_DnsClientNrptPolicy_v1.0.0.cdxml",
        "PS_DnsClientNRPTGlobal_v1.0.0.cdxml",
        "PS_DnsClientNRPTRule_v1.0.0.cdxml"
        )
    TypesToProcess = @(
        "DnsCmdlets.Types.ps1xml",
        "DnsConfig.Types.ps1xml",
        "DnsClientPSProvider.Types.ps1xml"
        )
    FormatsToProcess = @(
        "DnsCmdlets.Format.ps1xml",
        "DnsConfig.Format.ps1xml",
        "DnsClientPSProvider.Format.ps1xml"
        )
    CmdletsToExport = @(
        "Resolve-DnsName"
        )
    FunctionsToExport = @(
        "Clear-DnsClientCache",
        "Get-DnsClient",
        "Get-DnsClientCache",
        "Get-DnsClientGlobalSetting",
        "Get-DnsClientServerAddress",
        "Register-DnsClient",
        "Set-DnsClient",
        "Set-DnsClientGlobalSetting",
        "Set-DnsClientServerAddress",
        "Add-DnsClientNrptRule",
        "Get-DnsClientNrptPolicy",
        "Get-DnsClientNrptGlobal",
        "Get-DnsClientNrptRule",
        "Remove-DnsClientNrptRule",
        "Set-DnsClientNrptGlobal",
        "Set-DnsClientNrptRule"
        )
    HelpInfoUri = "http://go.microsoft.com/fwlink/?LinkId=216157"
}
