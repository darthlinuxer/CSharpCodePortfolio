param (
    [Parameter(ValueFromPipeline=$true)]
    [string] $Vault
)

$Vault | .\Script_DeleteVault.ps1 


