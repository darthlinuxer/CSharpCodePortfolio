function DeleteVault {

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
    If ($null -ne (Get-SecretVault -Name $VaultName)) {   
        Write-Host "Deleting Vault " + $VaultName
        if($null -ne $vaultpasswordSecureStringObject){
            Unlock-SecretStore -Password $vaultpasswordSecureStringObject
        } else{
            Unlock-SecretStore 
        }        
        Get-SecretInfo -Vault $VaultName  | ForEach-Object { Remove-Secret -Vault $VaultName $_.Name }
        UnRegister-SecretVault -Name $VaultName 
        return $true
    }
    else {
        Write-Host "There is no Vault named $VaultName"
        return $false
    }

}