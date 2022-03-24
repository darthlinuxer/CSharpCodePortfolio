#Requires -RunAsAdministrator
Clear-Host
#INITIAL SCRIPT
#Storing windows credential in a File to be used as Vault Password
if ($null -eq (Get-Content ~/vaultpassword.xml)) {
    Write-Host "Please Enter Your Credentials ..."
    Get-Credential | Export-CliXml ~/vaultpassword.xml
    Get-Content ~/vaultpassword.xml
}

#GLOBAL VARIABLES
$VaultName = "My_Vault"
$DefaultCertificateName = "OrderAPI.pfx"
$CertificateName = $null
$vaultpasswordSecureStringObject = (Import-CliXml ~/vaultpassword.xml).Password

$host.ui.rawui.BackgroundColor = "Black"
$Host.PrivateData.ErrorForegroundColor = "White"
$Host.PrivateData.ErrorBackgroundColor = "Red"
#Load External Functions
. ".\Scripts\Function_GetPwdFromSecureString.ps1"
. ".\Scripts\Function_CreateSecretVault.ps1"
. ".\Scripts\Function_DeleteSecretVault.ps1"
Write-Host `
    -BackgroundColor yellow `
    -ForegroundColor black `
    "Check if functions were loaded correctly!"
Get-ChildItem -Path Function:\Get-Password*
Get-ChildItem -Path Function:\CreateVault
Get-ChildItem -Path Function:\DeleteVault
PAUSE

Function CREATE_VAULT {    
    Clear-Host
    Write-Host `
        -BackgroundColor yellow `
        -ForegroundColor black `
        "Check if Vault was created correctly!"
    CreateVault -VaultName $VaultName
    PAUSE
    MENU  
}

Function DELETE_VAULT {
    Clear-Host
    DeleteVault -VaultName $VaultName
    PAUSE
    MENU  
}

Function LIST_VAULT_COMMANDS {
    Clear-Host
    Get-Command -Module Microsoft.PowerShell.SecretManagement
    Get-Command -Module Microsoft.PowerShell.SecretStore
    PAUSE
    MENU        
}

Function LIST_VAULT_SECRETS {
    Clear-Host
    Unlock-SecretStore -Password $vaultpasswordSecureStringObject
    Get-SecretInfo -Vault $VaultName     
    PAUSE
    MENU    
}
Function SET_API_PASSWORD {
    Clear-Host
    $SecureStringObject = Read-Host "Type your API secret password:" -AsSecureString
    Set-Secret -Name API_KESTREL_PASSWORD -Secret $SecureStringObject -Vault $VaultName 
    Get-Secret -Name API_KESTREL_PASSWORD 
    PAUSE
    MENU    
}

Function READ_API_PASSWORD {
    Clear-Host
    Unlock-SecretStore -Password $vaultpasswordSecureStringObject
    return Get-Secret -Name API_KESTREL_PASSWORD | Get-PasswordFromSecureString     
}
Function SET_DB_PASSWORD {
    Clear-Host
    $SecureStringObject = Read-Host "Type your DB secret password:" -AsSecureString
    Set-Secret -Name RAVEN_SERVER_PASSWORD -Secret $SecureStringObject -Vault $VaultName 
    Get-Secret -Name RAVEN_SERVER_PASSWORD 
    PAUSE
    MENU    
}
Function READ_DB_PASSWORD {
    Clear-Host
    Unlock-SecretStore -Password $vaultpasswordSecureStringObject
    return Get-Secret -Name RAVEN_SERVER_PASSWORD | Get-PasswordFromSecureString 
}

Function CREATE_DOTNET_CERTIFICATE {
    Clear-Host
    $API = READ_API_PASSWORD;
    if (!$API) {
        Read-Host "Error: Vault does not have an API Password. Please Set one!"
        MENU
    }

    Write-Host "Step 1: Creating SSL Certificates for .NET ..."
    if (!($CertificateName = Read-Host "Certificate name [$DefaultCertificateName]")) {
        $CertificateName = $DefaultCertificateName
    }

    dotnet dev-certs https --clean
    dotnet dev-certs https -ep "$env:USERPROFILE\.aspnet\https\$CertificateName" -p $INSECURE_STRING --trust
    PAUSE
    MENU    
}

Function CREATE_RAVEN_CERTIFICATE {
    Clear-Host
    $INSECURE_STRING = Get-Secret -Name RAVEN_SERVER_PASSWORD | Get-PasswordFromSecureString 
    if (!$INSECURE_STRING) {
        Read-Host "Error: Vault does not have a Db Password. Please Set one!"
        MENU
    }
    PAUSE
    MENU    
}

Function RUN_CONTAINERS {
    Clear-Host
    $API_CERTIFICATE_FILE = $CertificateName
    $API = READ_API_PASSWORD
    $DB = READ_DB_PASSWORD
    if (!$API) {
        Read-Host "Please Set API password first! Press any key to continue..."
        MENU
    }
    if (!$DB) {
        Read-Host "Please Set Db password first! Press any key to continue..."
        MENU
    }
    if (!$API_CERTIFICATE_FILE){
        Read-Host "Please First Create DOTNET Certificates! Press any key to continue..."
        MENU
    }
    [Environment]::SetEnvironmentVariable("API_KESTREL_PASSWORD", $API, "USER")
    [Environment]::SetEnvironmentVariable("API_CERTIFICATE_FILE", $API_CERTIFICATE_FILE, "USER")
    [Environment]::SetEnvironmentVariable("RAVEN_SERVER_PASSWORD", $DB, "USER")

    docker-compose up -d 

    PAUSE
    MENU    
}

Function PAUSE {
    Read-Host "Press any key to return to MENU ..."
}

Function MENU {
    Clear-Host
    #Present Menu
    Write-Host "1. Create Vault"
    Write-Host "2. Delete Vault"
    Write-Host "3. Set/Reset API Password"
    Write-Host "4. Read API Password from Vault"
    Write-Host "5. Set/Reset Db Password"
    Write-Host "6. Read Db Password from Vault"
    Write-Host "7. List Vault Secrets"
    Write-Host "8. List Vault Commands"
    Write-Host "9. Create Dotnet Certificate"
    Write-Host '10. Create Raven Certificate'
    Write-Host "11. Run Containers"
    $option = Read-Host "Choose one option"
    switch ($option) {
        1 { CREATE_VAULT; break }
        2 { DELETE_VAULT; break }
        3 { SET_API_PASSWORD; break }
        4 { $API = READ_API_PASSWORD; $API; PAUSE; MENU break }
        5 { SET_DB_PASSWORD; break }
        6 { $DB = READ_DB_PASSWORD; $DB; PAUSE; MENU; break }
        7 { LIST_VAULT_SECRETS; break }
        8 { LIST_VAULT_COMMANDS; break }
        9 { CREATE_DOTNET_CERTIFICATE; break }
        10 { CREATE_RAVEN_CERTIFICATE; break }
        11 { RUN_CONTAINERS ; break }
        default { MENU }
    }
}

MENU

Exit

