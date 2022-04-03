#Requires -RunAsAdministrator
Clear-Host

#CONFIG HOST
$host.ui.rawui.BackgroundColor = "Black"
$host.ui.rawui.ForegroundColor = "White"

#CONFIG VARIABLES
$VaultName = "My_Vault"
$DefaultAPICertificateName = "ravenconnection.pfx"
$DefaultRavenCertificateName = "ravensecure.pfx"

$env:API_IMAGE_NAME = "mytestapi"
$env:API_TAG_NAME = "dev"
$env:TEST_HOSTNAME = "testhostname"
$env:API_HOSTNAME = "myapihostname"
$env:API_IMAGE_NAME = "mytestapi"
$env:API_TAG_NAME = "dev"
$env:API_CERTIFICATE_PATH = "https"
$env:RAVEN_CERTIFICATE_PATH = "Certs"
$env:RAVEN_HOSTNAME = "raven"
$env:RAVEN_INITIAL_MODE = "initial"

$env:API_CERTIFICATE_FILE = $DefaultAPICertificateName
$env:RAVEN_CERTIFICATE_FILE = $DefaultRavenCertificateName

$CertificateName = $null
$RavenCertificateName = $null

#INITIAL SCRIPT

$host.ui.rawui.BackgroundColor = "Black"
$host.ui.rawui.ForegroundColor = "White"
$Host.PrivateData.ErrorForegroundColor = "White"
$Host.PrivateData.ErrorBackgroundColor = "Red"
#Load External Functions
. ".\Scripts\Function_GetPwdFromSecureString.ps1"
. ".\Scripts\Function_CreateSecretVault.ps1"
. ".\Scripts\Function_DeleteSecretVault.ps1"
. ".\Scripts\Function_Write_Message.ps1"

#TRY TO READ CREDENTIALS FROM FILE
try {
    $vaultpasswordSecureStringObject = (Import-CliXml ~/vaultpassword.xml).Password
}
catch {
    WRITE_MSG -Message "There is no Credential File yet.. let's create one!" -Type "info"
    #Storing windows credential in a File to be used as Vault Password   
    WRITE_MSG -Message "Please Enter Your Credentials ..." -Type "alert"
    Get-Credential | Export-CliXml ~/vaultpassword.xml
    Get-Content ~/vaultpassword.xml   
}

Function READ_API_PASSWORD {
    Clear-Host
    try {
        Unlock-SecretStore -Password $vaultpasswordSecureStringObject
        return Get-Secret -Name API_KESTREL_PASSWORD | Get-PasswordFromSecureString     
    }
    catch {
        PRINT_LAST_ERROR
    }
} 

Function READ_DB_PASSWORD {
    Clear-Host
    try {
        Unlock-SecretStore -Password $vaultpasswordSecureStringObject
        return Get-Secret -Name RAVEN_SERVER_PASSWORD | Get-PasswordFromSecureString 
    }
    catch {
        PRINT_LAST_ERROR
    }
}

#Secrets need docker swarm
docker swarm init

#CHECK IF POWERSHELL VAULT ALREADY EXISTS AND IF SO, RE-CREATE DOCKER SECRET VAULT
#WITH THE VALUES CONTAINED IN POWERSHELL VAULT
try {
    If ($null -ne (Get-SecretVault -Name $VaultName -ErrorAction Stop)) {          
        If ($null -ne (READ_API_PASSWORD)) {
            #Remove previous value if exists
            if (docker secret rm API_KESTREL_PASSWORD ) {
                WRITE_MSG -Message "Removed Previous Value..." -Type "info"
            } 
            #SET value from Vault
            READ_API_PASSWORD | docker secret create API_KESTREL_PASSWORD -
        }
        else {
            WRITE_MSG `
                -Message "Vault has no Key: API_KESTREL_PASSWORD, please create one!"`
                -Type "danger"                 
            PAUSE
        }            
        If ($null -ne (READ_DB_PASSWORD)) {
            #Remove previous value if exists
            if (docker secret rm RAVEN_SERVER_PASSWORD) { 
                WRITE_MSG -Message "Removed Previous Value..." -Type "info"
            } 
            #SET value from Vault
            READ_DB_PASSWORD | docker secret create RAVEN_SERVER_PASSWORD -
        }
        else {
            WRITE_MSG `
                -Message "Vault has no Key: RAVEN_SERVER_PASSWORD, please create one!"`
                -Type "danger"
            PAUSE
        }
    }
}
catch {
    #Vault doesn't exist, let´s create it
    WRITE_MSG -Message "Creating Vault $VaultName ..." -Type "info"
    CreateVault -VaultName $VaultName
}

Function CREATE_VAULT {    
    Clear-Host  
    CreateVault -VaultName $VaultName
    MENU  
}
Function DELETE_VAULT {
    Clear-Host   
    WRITE_MSG -Message "Trying to Delete Vault: $VaultName" -Type "alert"
    DeleteVault -VaultName $VaultName       
    docker secret rm API_KESTREL_PASSWORD
    docker secret rm RAVEN_SERVER_PASSWORD
    PAUSE
    MENU  
}

Function LIST_VAULT_COMMANDS {
    Clear-Host
    WRITE_MSG -Message "SecretManagement COMMANDS" -Type "info"
    Get-Command -Module Microsoft.PowerShell.SecretManagement
    $msg = "-" * 120
    WRITE_MSG -Message $msg -Type "alert"
    WRITE_MSG -Message "SecretStore COMMANDS" -Type "info"
    Get-Command -Module Microsoft.PowerShell.SecretStore
    PAUSE
    MENU        
}

Function LIST_VAULT_SECRETS {
    Clear-Host
    WRITE_MSG -Message "LISTING VAULT SECRETS" -Type "info"
    try {
        Unlock-SecretStore -Password $vaultpasswordSecureStringObject
        WRITE_MSG -Message "Powershell Vault Description ..." -Type "alert"
        Get-SecretVault -Name $VaultName | Select-Object *
        WRITE_MSG -Message "Stored Secrets in Powershell Vault ..." -Type "alert"
        Get-SecretInfo -Vault $VaultName   
        WRITE_MSG -Message "Stored Secrets in DOCKER Vault ..."  -Type "alert"
        docker secret ls
    }
    catch {
        PRINT_LAST_ERROR
    }    
    PAUSE
    MENU    
}
Function SET_API_PASSWORD {
    Clear-Host
    WRITE_MSG -Message "Setting API Password" -Type "info"
    try {
        $SecureStringObject = Read-Host "Type your API secret password:" -AsSecureString
        Set-Secret -Name API_KESTREL_PASSWORD `
            -Secret $SecureStringObject `
            -Vault $VaultName `
            -Metadata @{Purpose = "This is the API Certificate password for https" }
        Get-Secret -Name API_KESTREL_PASSWORD 
        #Remove previous value if exists
        if (docker secret rm API_KESTREL_PASSWORD ) { Write-Host "Removed Previous Value..." } 
        #SET new value
        Write-Host "Writing new value on Docker Vault..."
        $notSoSecretPasswordAnymore = READ_API_PASSWORD
        $notSoSecretPasswordAnymore | docker secret create API_KESTREL_PASSWORD -
       
    }
    catch {
        PRINT_LAST_ERROR
    }
    PAUSE
    MENU    
}


  
Function SET_DB_PASSWORD {
    Clear-Host   
    try {
        $SecureStringObject = Read-Host "Type your DB secret password:" -AsSecureString
        Set-Secret -Name RAVEN_SERVER_PASSWORD `
            -Secret $SecureStringObject `
            -Vault $VaultName `
            -Metadata @{Purpose = "This is the Raven Certificate password for https" }
        Get-Secret -Name RAVEN_SERVER_PASSWORD    
        #Remove previous value if exists
        if (docker secret rm RAVEN_SERVER_PASSWORD) { Write-Host "Removed Previous Value..." } 
        #SET new value
        Write-Host "Writing new value on Docker Vault..."
        $notSoSecretPasswordAnymore = READ_DB_PASSWORD
        $notSoSecretPasswordAnymore | docker secret create RAVEN_SERVER_PASSWORD -
    }
    catch {
        PRINT_LAST_ERROR        
    }
    PAUSE
    MENU    
}

Function CREATE_DOTNET_CERTIFICATE {
    Clear-Host
    $API_INSECURE_PASSWORD = READ_API_PASSWORD;
    if (!$API_INSECURE_PASSWORD) {
        Read-Host "Error: Vault does not have an API Password. Please Set one!"
        MENU
    }
    Write-Host "Step 1: Creating SSL Certificates for .NET ..."
    if (!($CertificateName = Read-Host "Certificate name [$DefaultAPICertificateName]")) {
        $CertificateName = $DefaultAPICertificateName
    }

    $env:API_CERTIFICATE_FILE = $CertificateName
    
    if (Test-Path -Path "~\.aspnet") { Remove-Item -Path ~\.aspnet -Recurse -Force }
    New-Item -Path "~" -Name ".aspnet" -ItemType "directory" 
    New-Item -Path "~\.aspnet" -Name "https" -ItemType "directory" 


    try {
        WRITE_MSG -Message "Creating $CertificateName with Pwd: $API_INSECURE_PASSWORD" -Type "info"
        dotnet dev-certs https --clean
        dotnet dev-certs https -ep "$env:USERPROFILE\.aspnet\https\$CertificateName" -p $API_INSECURE_PASSWORD --trust
    }
    catch {
        PRINT_LAST_ERROR
    }    
    PAUSE
    MENU    
}

Function CREATE_RAVEN_CERTIFICATE {

    Clear-Host
    $RAVENSERVERPASSWORD_INSECURE_STRING = Get-Secret -Name RAVEN_SERVER_PASSWORD | Get-PasswordFromSecureString 
    if (!$RAVENSERVERPASSWORD_INSECURE_STRING) {
        Read-Host "Error: Vault does not have a Db Password. Please Set one!"
        MENU
    }

    Write-Host "Creating SSL Certificates for Raven ..."
    if (!($RavenCertificateName = Read-Host "Certificate name [$DefaultRavenCertificateName]")) {
        $RavenCertificateName = $DefaultRavenCertificateName
    }

    $env:RAVEN_CERTIFICATE_FILE = $RavenCertificateName

    $DefaultCertificateName = "ravensecure"
    $DefaultSignerName = "RavenDB Server CA"

    $CertificatePassword = $RAVENSERVERPASSWORD_INSECURE_STRING 

    if (!($CertificateName = Read-Host "Certificate Name [$DefaultCertificateName]")) { 
        $CertificateName = $DefaultCertificateName 
    }
    if (!($SignerName = Read-Host "SignerName [$DefaultSignerName]")) {
        $SignerName = $DefaultSignerName
    }

    $DefaultSubjectCA = "CN=$DefaultCertificateName Certificate Authority,O=$env:UserDomain,OU=$DefaultSignerName's RavenDB Operations"
    if (!($SubjectCA = Read-Host "SubjectCA [$DefaultSubjectCA]")) {
        $SubjectCA = $DefaultSubjectCA
    }

    $certId = -Join ((65..90) | Get-Random -Count 5 | % { [char]$_ })
    $SuggestedUniqueCertName = "$CertificateName-$certId"; 
    if (!($UniqueCertName = Read-Host "Suggested UniqueCertName [$SuggestedUniqueCertName]")) {
        $UniqueCertName = $SuggestedUniqueCertName
    }

    .\Script_Generate_Server_Certificates.ps1 `
        -CertificatePassword $CertificatePassword `
        -CN $CertificateName `
        -CertName $UniqueCertName `
        -CertFile $CertFileName `
        -SignerName $SignerName `
        -SubjectCA $SubjectCA

    PAUSE
    MENU    
}

Function CHECK_PASSWORD_VARIABLES {
    Clear-Host
    $API_INSECURE_PASSWORD = READ_API_PASSWORD
    $DB_INSECURE_PASSWORD = READ_DB_PASSWORD
    if (!$API_INSECURE_PASSWORD) {
        Read-Host "Please Set API password first! Press any key to continue..."
        MENU
    }
    if (!$DB_INSECURE_PASSWORD) {
        Read-Host "Please Set Db password first! Press any key to continue..."
        MENU
    }    
}

Function RUN_TEST_CONTAINER {
    Clear-Host
    WRITE_MSG `
        -Message "docker stack deploy -c stack-common.yml -c stack-test.yml -c stack-visualizer.yml rdi" `
        -Type "info"
    docker stack deploy -c stack-common.yml -c stack-test.yml -c stack-visualizer.yml rdi
    WRITE_MSG -Message "Waiting 7s until all services are initialized" -Type "info"
    Start-Sleep 7
    WRITE_MSG `
        -Message "docker service logs rdi_test" `
        -Type "info"
    docker service logs rdi_test
    WRITE_MSG `
        -Message "Now check above the Environment Variables, Scripts and Certificates..." `
        -Type "info"
    PAUSE
    docker service rm rdi_test    
    MENU
}

Function RUN_API_CONTAINER {
    Clear-Host
    CHECK_PASSWORD_VARIABLES
    WRITE_MSG `
        -Message "docker stack deploy -c stack-common.yml -c stack-api.yml rdi" `
        -Type "info"
    docker stack deploy `
        -c stack-common.yml `
        -c stack-api.yml `
        rdi
    WRITE_MSG -Message "Waiting 5s until container starts..." -Type "info"
    Start-Sleep 5
    DOCKER_SERVICE_LOGS_API
    WRITE_MSG -Message "Check Log above if services started correctly" -Type "info"
    PAUSE
    MENU
}

Function RUN_VISUALIZER_CONTAINER {
    Clear-Host
    WRITE_MSG `
        -Message "docker stack deploy -c stack-common.yml -c stack-visualizer.yml rdi" `
        -Type "info"
    docker stack deploy `
        -c stack-common.yml `
        -c stack-visualizer.yml `
        rdi
    PAUSE
    MENU
}

Function RUN_LOGGING_IN_CLOUD_CONTAINER {
    Clear-Host
    WRITE_MSG `
        -Message "docker stack deploy -c stack-common.yml -c stack-logging.yml rdi" `
        -Type "info"
    docker stack deploy `
        -c stack-common.yml `
        -c stack-logging.yml `
        rdi
    PAUSE
    MENU
}

Function RUN_RAVEN_UNSECURE_CONTAINER {
    Clear-Host
    CHECK_PASSWORD_VARIABLES
    WRITE_MSG `
        -Message "docker stack deploy -c stack-common.yml -c stack-ravenunsecure.yml -c stack-visualizer.yml rdi" `
        -Type "info"
    docker stack deploy -c stack-common.yml -c stack-ravenunsecure.yml -c stack-visualizer.yml rdi 
    PAUSE
    MENU
}

Function RUN_RAVEN_SECURE_CONTAINER {
    Clear-Host
    CHECK_PASSWORD_VARIABLES
    WRITE_MSG `
        -Message "docker stack deploy -c stack-common.yml -c stack-ravensecure.yml -c stack-visualizer.yml rdi" `
        -Type "info"
    docker stack deploy -c stack-common.yml -c stack-ravensecure.yml -c stack-visualizer.yml rdi
    PAUSE
    MENU
}

Function PAUSE {
    Read-Host "Press any key to return to MENU ..."
}

Function PRINT_LAST_ERROR {
    $theError = $_
    WRITE_MSG -Message "Error message: $theError.Exception.Message" -Type "danger"
}

Function DOCKER_SERVICE_LOGS_API {
    Clear-Host
    WRITE_MSG -Message "docker service logs rdi_api" -Type "info"
    docker service logs rdi_api
    PAUSE
    MENU
}

Function DOCKER_SERVICE_LOGS_RAVEN_SECURE {
    Clear-Host
    WRITE_MSG -Message "docker service logs rdi_ravensecure" -Type "info"
    docker service logs rdi_ravensecure
    PAUSE
    MENU
}

Function DOCKER_SERVICE_LOGS_RAVEN_UNSECURE {
    Clear-Host
    WRITE_MSG -Message "docker service logs rdi_raveunsecure" -Type "info"
    docker service logs rdi_ravenunsecure
    PAUSE
    MENU
}

Function DOCKER_SERVICE_LOGS_TEST {
    Clear-Host
    WRITE_MSG -Message "docker service logs rdi_test" -Type "info"
    docker service logs rdi_test
    PAUSE
    MENU
}

Function DOCKER_SWARM_LEAVE {
    Clear-Host
    WRITE_MSG -Message "docker swarm leave --force" -Type "alert"
    docker swarm leave --force
    PAUSE
    MENU
}

Function DOCKER_BUILD_API_IMAGE {
    Clear-Host
    WRITE_MSG -Message "docker rmi API_IMAGE_NAME:API_TAG_NAME" -Type "info"
    docker rmi ${env:API_IMAGE_NAME}:${env:API_TAG_NAME}
    WRITE_MSG -Message "docker build -f Dockerfile -t API_IMAGE_NAME:API_TAG_NAME ." -Type "info"
    docker build -f Dockerfile -t ${env:API_IMAGE_NAME}:${env:API_TAG_NAME} .
    PAUSE
    MENU
}

Function PRINT_ENV_VARIABLES {
    Clear-Host
    Get-Childitem -Path Env:* | Sort-Object Name
    Pause
    MENU
}

Function MENU {
    Clear-Host
    #Present Menu
    Write-Host "--------------------------------"
    Write-Host "---------Execution Steps--------"
    Write-Host "--------------------------------"
    Write-Host "1. Set API Password"
    Write-Host "2. Set Db Password"
    Write-Host "3. Create Dotnet Certificate"
    Write-Host '4. Create Raven Certificate'
    Write-Host "5. Run TEST Container"
    Write-Host "6. Build Dockerfile to create API image "
    Write-Host "7. Run API Container"
    Write-Host "8. Run Raven Unsecure Container"
    Write-Host "9. Show Running Services in this SWARM node"
    Write-Host "10. Remove all Running Services in this SWARM node"

    Write-Host "--------------------------------"
    Write-Host "---------Extra Commands---------"
    Write-Host "--------------------------------"
    Write-Host "20. Delete Vault"    
    Write-Host "21. Read API Password from Vault"    
    Write-Host "22. Read Db Password from Vault"
    Write-Host "23. List Vault Secrets"
    Write-Host "24. List Vault Commands"
    Write-Host "25. docker service logs api"
    Write-Host "26. docker service logs ravensecure"
    Write-Host "27. docker service logs ravenunsecure"
    Write-Host "28. docker service logs test"
    Write-Host "29. docker swarm leave -force"
    Write-Host "30. Check Environment Variables"
    Write-Host "50. Exit"
    
    $option = Read-Host "Choose one option"
    switch ($option) {
        1 { SET_API_PASSWORD; break }
        2 { SET_DB_PASSWORD; break }
        3 { CREATE_DOTNET_CERTIFICATE; break }
        4 { CREATE_RAVEN_CERTIFICATE; break }
        5 { RUN_TEST_CONTAINER ; break }
        6 { DOCKER_BUILD_API_IMAGE; break }
        7 { RUN_API_CONTAINER ; break }
        8 { RUN_RAVEN_UNSECURE_CONTAINER ; break }
        9 {
            Clear-Host;
            WRITE_MSG -Message "docker service ls" -Type "info"
            docker service ls; 
            PAUSE; 
            MENU;
            break 
        }
        10 {
            Clear-Host;
            WRITE_MSG -Message "docker service ls" -Type "info"
            docker service ls
            WRITE_MSG -Message "docker service rm <services> " -Type "info"
            docker service rm rdi_test
            docker service rm rdi_api
            docker service rm rdi_ravensecure
            docker service rm rdi_ravenunsecure
            docker service rm rdi_visualizer
            docker service rm rdi_logspout
            PAUSE; 
            MENU;
            break 
        }
        20 { DELETE_VAULT; break }
        21 {
            Clear-Host;
            $API = READ_API_PASSWORD;
            WRITE_MSG -Message "docker secret inspect API_KESTREL_PASSWORD" -Type "info"
            docker secret inspect API_KESTREL_PASSWORD; 
            WRITE_MSG -Message "Saved API Password in Powerhshell Vault:" -Type "info"
            $API; 
            PAUSE; 
            MENU; 
            break 
        }        
        22 { 
            Clear-Host;
            $DB = READ_DB_PASSWORD;
            WRITE_MSG -Message "docker secret inspect RAVEN_SERVER_PASSWORD" -Type "info"
            docker secret inspect RAVEN_SERVER_PASSWORD; 
            WRITE_MSG -Message "Saved DB Password in Powerhshell Vault:" -Type "info"
            $DB; 
            PAUSE; 
            MENU; 
            break 
        }
        23 { LIST_VAULT_SECRETS; break }
        24 { LIST_VAULT_COMMANDS; break }
        25 { DOCKER_SERVICE_LOGS_API; break }
        26 { DOCKER_SERVICE_LOGS_RAVEN_SECURE; break }
        27 { DOCKER_SERVICE_LOGS_RAVEN_UNSECURE; break }
        28 { DOCKER_SERVICE_LOGS_TEST; break }
        29 { DOCKER_SWARM_LEAVE; break }
        30 { PRINT_ENV_VARIABLES; break }
        50 { EXIT; break }        
        default { MENU }
    }
}

MENU

Exit

