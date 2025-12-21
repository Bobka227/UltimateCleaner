


param(
  [Parameter(Mandatory=$true)]
  [ValidateSet("User","Windows")]
  [string]$Mode
)

$ErrorActionPreference = "SilentlyContinue"
$ProgressPreference = "SilentlyContinue"

function RemoveSafe($path) {
  if (Test-Path $path) {
    Remove-Item "$path\*" -Recurse -Force -ErrorAction SilentlyContinue
  }
}

if ($Mode -eq "User") {
  RemoveSafe $env:TEMP
[PSCustomObject]@{ ok=$true; mode="User"; message="USER_TEMP_CLEANED" } | ConvertTo-Json
}
elseif ($Mode -eq "Windows") {
  RemoveSafe "C:\Windows\Temp"
  [PSCustomObject]@{ ok=$true; mode="Windows"; message="Windows TEMP cleared" } | ConvertTo-Json
}
