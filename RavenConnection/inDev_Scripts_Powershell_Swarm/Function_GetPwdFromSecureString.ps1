Function Get-PasswordFromSecureString {

    Param (
        [Parameter(Mandatory, ValueFromPipeline)]
        [SecureString] $SecureString
    )

    try{
        Write-Verbose -Message "Attempting to Decode the SecureString"
        $output = (New-Object System.Net.NetworkCredential("",$SecureString)).Password 
        Write-Output $output
    }
    catch {
        Write-Warning -Message "Oops.. Are you sure you entered a SecureString ?"
    }   
    
}