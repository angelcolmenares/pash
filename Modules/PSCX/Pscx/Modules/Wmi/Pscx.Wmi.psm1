Set-StrictMode -Version Latest

if ($PSVersionTable.PSVersion.Major -eq 3)
{
    $acceleratorsType = [psobject].Assembly.GetType('System.Management.Automation.TypeAccelerators')
}
else
{
    $acceleratorsType = [Type]::GetType('System.Management.Automation.TypeAccelerators')
}

# If these accelerators have already been defined, don't override (and don't error)
function AddAccelerator($name, $type)
{
    if (!$acceleratorsType::Get.ContainsKey($name))
    {
        $acceleratorsType::Add($name, $type)
    }
}

AddAccelerator "accelerators" $acceleratorsType
AddAccelerator "wmidatetime"  ([Pscx.TypeAccelerators.WmiDateTime])
AddAccelerator "wmitimespan"  ([Pscx.TypeAccelerators.WmiTimeSpan])

# Export nothing
Export-ModuleMember
