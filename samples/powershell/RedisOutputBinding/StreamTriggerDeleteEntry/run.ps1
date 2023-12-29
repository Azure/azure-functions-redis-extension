param($entry, $TriggerMetadata)
Write-Host "Stream entry from key 'streamTest2' with Id '$($entry.Id)' and values '$($entry.Values)'"
Push-OutputBinding -Name retVal -Value "streamTest2 $($entry.Id)"