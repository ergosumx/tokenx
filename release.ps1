param(
	[string]$Prefix = "rust",
	[string]$Version = "0.0.1"
)

# Support POSIX-style arguments --prefix and --version when passed in $args
for ($i = 0; $i -lt $args.Count; $i++) {
	switch ($args[$i]) {
		'--prefix' { if ($i + 1 -lt $args.Count) { $Prefix = $args[$i + 1]; $i++ } }
		'-p' { if ($i + 1 -lt $args.Count) { $Prefix = $args[$i + 1]; $i++ } }
		'--version' { if ($i + 1 -lt $args.Count) { $Version = $args[$i + 1]; $i++ } }
		'-v' { if ($i + 1 -lt $args.Count) { $Version = $args[$i + 1]; $i++ } }
		default { }
	}
}

if ($Version -notmatch '^[0-9]+\.[0-9]+\.[0-9]+$') {
	Write-Host "Invalid version format: $Version" -ForegroundColor Red
	exit 1
}

$tag = "${Prefix}-v${Version}"
Write-Host "Preparing release with tag: $tag"

Write-Host "Creating tag $tag (force)"
# Create (or update) annotated tag. Use -f to replace existing tag if present.
git tag -a $tag -m "Release $Prefix bridge $tag" -f

Write-Host "Pushing tag $tag to origin (force)"

# Verify 'origin' remote exists to avoid constructing an invalid refspec
$remoteExists = git remote | Select-String -Pattern '^origin$' -Quiet
if (-not $remoteExists) {
	Write-Host "Remote 'origin' not found. Skipping push." -ForegroundColor Yellow
} else {
	# Push by tag name -- simpler and avoids manually assembling refspecs that can duplicate segments.
	$pushCmd = "git push origin $tag --force"
	Write-Host "Running: $pushCmd"
	Invoke-Expression $pushCmd
}

Write-Host "Release script completed for $tag"