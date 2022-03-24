function CreateVault {

    # Need to run this as administrator
    #Requires -RunAsAdministrator

    param (
        [Parameter( ValueFromPipeline = $true, `
                ValueFromPipelineByPropertyName = $true, `
                ParameterSetName = "VaultName", `
                HelpMessage = "Name of the Vault.")]
        [string] $VaultName = "My_Vault"
    )
    
    $Policy = "RemoteSigned"
    If ((Get-ExecutionPolicy) -ne $Policy) { Set-ExecutionPolicy $Policy -Force }

    $haveSecureModuleInstalled = $null -ne ( `
            Get-Module -ListAvailable `
            Microsoft.PowerShell.SecretManagement, `
            Microsoft.PowerShell.SecretStore `
    ) 

    If (-not $haveSecureModuleInstalled) {
        Write-Host "Secure Module not installed! Installing..."
        Install-Module -Name `
            Microsoft.PowerShell.SecretManagement, `
            Microsoft.PowerShell.SecretStore
        Get-Command -Module Microsoft.PowerShell.SecretManagement
        Get-Command -Module Microsoft.PowerShell.SecretStore
    }

    #Create a Secret Vault
    If ($null -eq (Get-SecretVault -Name $VaultName)) {   
        Write-Host "Vault $VaultName does not exist!"
        Write-Host "Creating Vault $VaultName"
        Register-SecretVault -Name $VaultName -ModuleName Microsoft.PowerShell.SecretStore -Description "My Secret vault"
    }

    #This will Disable passwords in the Vault. 
    #Set-SecretStoreConfiguration -Authentication None -Interaction None

    #Show created Vault
    Get-SecretVault -Name $VaultName | Select-Object *

}