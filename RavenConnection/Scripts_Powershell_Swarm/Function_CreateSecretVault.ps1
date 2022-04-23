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
            Microsoft.PowerShell.SecretStore `
            -Force -AllowClobber -Verbose 
        Get-Command -Module Microsoft.PowerShell.SecretManagement
        Get-Command -Module Microsoft.PowerShell.SecretStore
    } else {
        Write-Host "Secure Modules are installed! Ok..."
    }

    #Create a Secret Vault
    try {
        If ($null -ne (Get-SecretVault -Name $VaultName -ErrorAction Stop)) {   
            Write-Host `
                -BackgroundColor yellow `
                -ForegroundColor black `
                "Vault $VaultName already exists!!!"
        }
    }
    catch {
        Write-Host `
            -BackgroundColor red `
            -ForegroundColor white `
            "Vault $VaultName does not exist!"
        Write-Host `
            -BackgroundColor yellow `
            -ForegroundColor black `
            "Creating Vault $VaultName"
        Register-SecretVault -Name $VaultName -ModuleName Microsoft.PowerShell.SecretStore -Description "My Secret vault"
        #Show created Vault
        Get-SecretVault -Name $VaultName | Select-Object *
        Write-Host `
            -BackgroundColor yellow `
            -ForegroundColor black `
            "Check if Vault was created correctly!"
        PAUSE
    }
    
    #This will Disable passwords in the Vault. 
    #Set-SecretStoreConfiguration -Authentication None -Interaction None
}