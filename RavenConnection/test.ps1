$rootStore = new-object System.Security.Cryptography.X509Certificates.X509Store(
    [System.Security.Cryptography.X509Certificates.StoreName]::AuthRoot,
    "localmachine"
)

$rootStore.Open("MaxAllowed")

$existingCert = $($rootStore.Certificates | Where-Object { $_.FriendlyName -eq "RavenDB Server CA" }) | Select-Object -First 1
$existingCert 
$rootStore.Certificates 