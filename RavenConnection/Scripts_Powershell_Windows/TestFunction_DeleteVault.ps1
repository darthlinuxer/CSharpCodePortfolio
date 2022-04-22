param (
    [Parameter(ValueFromPipeline=$true)]
    [string] $Vault
)

#Import function
. ".\Function_DeleteSecretVault.ps1"

#Check if Function is Loaded
if ($null -ne (Get-ChildItem -Path Function:\DeleteVault))
{
    $Vault | DeleteVault 
}

