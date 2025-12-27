param(
  [switch]$ResetBase
)

$ErrorActionPreference = "Continue"
$ProgressPreference = "SilentlyContinue"

$args = @("/Online", "/Cleanup-Image", "/StartComponentCleanup")
if ($ResetBase) {
  $args += "/ResetBase"
}

$principal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
$isAdmin = $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
  [PSCustomObject]@{
    ok = $false
    needsAdmin = $true
    exitCode = 740
    message = "DISM требует запуск от администратора."
    args = $args -join " "
  } | ConvertTo-Json -Depth 4
  exit 0
}

$tmpOut = [System.IO.Path]::GetTempFileName()
$tmpErr = [System.IO.Path]::GetTempFileName()

try {
  $p = Start-Process -FilePath "dism.exe" `
                     -ArgumentList $args `
                     -NoNewWindow `
                     -Wait `
                     -PassThru `
                     -RedirectStandardOutput $tmpOut `
                     -RedirectStandardError $tmpErr

  $stdout = Get-Content -LiteralPath $tmpOut -Raw -ErrorAction SilentlyContinue
  $stderr = Get-Content -LiteralPath $tmpErr -Raw -ErrorAction SilentlyContinue

  $ok = ($p.ExitCode -eq 0)

  [PSCustomObject]@{
    ok = $ok
    needsAdmin = $false
    exitCode = $p.ExitCode
    message = if ($ok) { "DISM StartComponentCleanup выполнен успешно." } else { "DISM завершился с ошибкой." }
    args = $args -join " "
    stdout = $stdout
    stderr = $stderr
  } | ConvertTo-Json -Depth 6
}
finally {
  Remove-Item -LiteralPath $tmpOut -Force -ErrorAction SilentlyContinue
  Remove-Item -LiteralPath $tmpErr -Force -ErrorAction SilentlyContinue
}
