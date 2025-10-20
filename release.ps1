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
git tag -a $tag -m "Release rust-bridge $tag" -f

Write-Host "Pushing tag $tag to origin (force)"
git push origin refs/tags/$tag:refs/tags/$tag --force

Write-Host "Release script completed for $tag"