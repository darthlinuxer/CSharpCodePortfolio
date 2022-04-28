Function WRITE_MSG {
    param (
        [Parameter( Mandatory, ValueFromPipeline, ValueFromPipelineByPropertyName)]
        [string] $Message,
        [Parameter( Mandatory, ValueFromPipeline, ValueFromPipelineByPropertyName)]
        [string] $Type
    )

    if ($Type.ToLower() -eq "alert") {
        Write-Host -ForegroundColor Black -BackgroundColor Yellow $Message
    }
    elseif ($Type.ToLower() -eq "info") {
        Write-Host -ForegroundColor White -BackgroundColor Blue $Message
    }
    elseif ($Type.ToLower() -eq "danger") {
        Write-Host -ForegroundColor White -BackgroundColor Red $Message
    }
    else {
        Write-Host $Message
    }
}