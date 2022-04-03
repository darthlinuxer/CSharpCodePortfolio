$DefaultCertFileName = "ravensecure.pfx"
$DefaultCertificateName = "ravensecure"
$DefaultSignerName = "RavenDB Server CA"

$CertificatePassword = Read-Host "Choose Certificate Password" -AsSecureString

if (!($CertFileName = Read-Host "Certificate FileName [$DefaultCertFileName]")) { 
    $CertFileName = $DefaultCertFileName 
}
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




