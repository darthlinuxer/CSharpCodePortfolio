#.\Script_DeleteVault -VaultName "Name"
# "Name" | .\Script_DeleteVault
param (
    [Parameter( ValueFromPipeline = $true, `
            ValueFromPipelineByPropertyName = $true, `
            ParameterSetName = "VaultName", `
            HelpMessage = "Name of the Vault.")]
    [string] $VaultName = "My_Vault"
)
    
$Policy = "RemoteSigned"
If ((Get-ExecutionPolicy) -ne $Policy) { Set-ExecutionPolicy $Policy -Force }   

#Check if SecretVault exists and if exists, delete it!
$vault = Get-SecretVault -Name $VaultName
If ($null -ne $vault) {   
    Write-Host "Deleting Vault " + $VaultName
    Unlock-SecretStore      
    Get-SecretInfo -Vault $VaultName  | ForEach-Object { Remove-Secret -Vault $VaultName $_.Name }
    UnRegister-SecretVault -Name $VaultName 
    return $true
}
else {
    Write-Host "There is no Vault named $VaultName"
    return $false
}

