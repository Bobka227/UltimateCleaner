param(
  [Parameter(Mandatory=$true)]
  [string]$DriveRoot,

  [int]$Top = 20
)

$ErrorActionPreference = "SilentlyContinue"
$ProgressPreference = "SilentlyContinue"

$driveLetter = $DriveRoot.Substring(0,1).ToUpper()
$root = "$driveLetter`:\"

$disk = Get-CimInstance -ClassName Win32_LogicalDisk -Filter "DeviceID='$driveLetter`:'"

$sizeBytes = [int64]$disk.Size
$freeBytes = [int64]$disk.FreeSpace
$usedBytes = $sizeBytes - $freeBytes

$folders =
  Get-ChildItem -LiteralPath $root -Directory -Force |
  ForEach-Object {
    $p = $_.FullName
    $sum = (Get-ChildItem -LiteralPath $p -Recurse -File -Force | Measure-Object -Property Length -Sum).Sum
    if ($null -eq $sum) { $sum = 0 }
    [PSCustomObject]@{
      path = $p
      sizeBytes = [int64]$sum
    }
  } |
  Sort-Object sizeBytes -Descending |
  Select-Object -First $Top

$result = [PSCustomObject]@{
  disk = [PSCustomObject]@{
    drive = "$driveLetter`:"
    root = $root
    sizeBytes = $sizeBytes
    freeBytes = $freeBytes
    usedBytes = $usedBytes
  }
  folders = $folders
  generatedAt = (Get-Date).ToString("o")
}

$result | ConvertTo-Json -Depth 6
