param (
    [Parameter(ValueFromPipeline=$true)]
    [int] $Size
)

#Import function
. ".\Function_CreateRandomKeys.ps1"
#Check if Function is Loaded
if ($null -ne (Get-ChildItem -Path Function:\CreateRandomKeys))
{
    $keys = CreateRandomKeys -Size $Size
    $keys -join ","
}

