param($key, $TriggerMetadata)
Write-Host "Deleting recently SET key '$key'"
Push-OutputBinding -Name retVal -Value $key