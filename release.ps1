param(
	[string]$Version = "0.0.1"
)

if ($Version -notmatch '^[0-9]+\.[0-9]+\.[0-9]+$') {
	Write-Host "Invalid version format: $Version" -ForegroundColor Red
	exit 1
}

$tag = "rust-v$Version"
Write-Host "Preparing release with tag: $tag"

Write-Host "Creating tag $tag (force)"
# Create (or update) annotated tag. Use -f to replace existing tag if present.
git tag -a $tag -m "Release rust-bridge $tag" -f

Write-Host "Pushing tag $tag to origin (force)"

# Verify 'origin' remote exists to avoid constructing an invalid refspec
$remoteExists = git remote | Select-String -Pattern '^origin$' -Quiet
if (-not $remoteExists) {
	Write-Host "Remote 'origin' not found. Skipping push." -ForegroundColor Yellow
} else {
	# Push by tag name -- simpler and avoids manually assembling refspecs that can duplicate segments.
	$pushCmd = "git push origin $tag --force"
	Write-Host "Running: $pushCmd"
	iex $pushCmd
}

Write-Host "Release script completed for $tag"