#Import function
. ".\Function_GetPwdFromSecureString.ps1"

#Check if Function is Loaded
if ($null -ne (Get-ChildItem -Path Function:\Get-PasswordFromSecureString))
{
    $SecureString = Read-Host "Type a Secret" -AsSecureString
    $SecureString | Get-PasswordFromSecureString 
}

