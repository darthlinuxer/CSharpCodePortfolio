function CreateRandomKeys {
    param(
        [int] $Size = 16
    )

    $Key = New-Object Byte[] $Size   # You can use 16, 24, or 32 for AES
    $rng = New-Object System.Security.Cryptography.RNGCryptoServiceProvider
    $rng.GetBytes($Key)
    #Write-Host "Random Keys Generated:"+($Key -join ',')
    return $Key
}
