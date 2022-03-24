$CertificatePassword = Read-Host "Choose Certificate Password" -AsSecureString

$DefaultCertFileName = "ravensecure.pfx"
if (!($CertFileName = Read-Host "Certificate FileName [$DefaultCertFileName]")) { 
    $CertFileName = $DefaultCertFileName 
}

$DefaultCertificateName = "ravensecure"
if (!($CertificateName = Read-Host "Certificate Name [$DefaultCertificateName]")) { 
    $CertificateName = $DefaultCertificateName 
}

$DefaultSignerName = "RavenDB Server CA"
if(!($SignerName = Read-Host "SignerName [$DefaultSignerName]")){
    $SignerName = $DefaultSignerName
}

$DefaultSubjectCA = "CN=$env:UserDomain Certificate Authority,O=$env:UserDomain,OU=$env:UserDomain's RavenDB Operations"
if(!($SubjectCA = Read-Host "SubjectCA [$DefaultSubjectCA]")){
    $SubjectCA  = $DefaultSubjectCA
}

$certId = -Join ((65..90) | Get-Random -Count 5 | % { [char]$_ })
$SuggestedUniqueCertName = "$CertificateName-$certId"; 
if(!($UniqueCertName = Read-Host "Suggested UniqueCertName [$SuggestedUniqueCertName]")){
    $UniqueCertName  = $SuggestedUniqueCertName
}

.\Script_Generate_Server_Certificates.ps1 `
    -CertificatePassword $CertificatePassword `
    -CN $CertName `
    -CertName $UniqueCertName `
    -CertFile $CertFileName `
    -SignerName $SignerName `
    -SubjectCA $SubjectCA




