param (
    [int]$Port = 8000
)

$rootDir = $PSScriptRoot
if (-not $rootDir) { $rootDir = Get-Location }

$listener = New-Object System.Net.HttpListener
$prefix = "http://127.0.0.1:$Port/"

try {
    $listener.Prefixes.Add($prefix)
    $listener.Start()
} catch {
    $Port = 8080
    $listener = New-Object System.Net.HttpListener
    $prefix = "http://127.0.0.1:$Port/"
    $listener.Prefixes.Add($prefix)
    $listener.Start()
}

Write-Host "==========================================================" -ForegroundColor Green
Write-Host "   PAMANA WebGL Local Game Server" -ForegroundColor Cyan
Write-Host "   Running at: $prefix" -ForegroundColor Yellow
Write-Host "   Opening your default web browser..." -ForegroundColor Gray
Write-Host "   (Keep this window open while playing. Close when done.)" -ForegroundColor DarkGray
Write-Host "==========================================================" -ForegroundColor Green

Start-Process "$prefix"

$mimeMap = @{
    ".html" = "text/html; charset=utf-8"
    ".htm"  = "text/html; charset=utf-8"
    ".css"  = "text/css"
    ".js"   = "application/javascript"
    ".wasm" = "application/wasm"
    ".png"  = "image/png"
    ".jpg"  = "image/jpeg"
    ".jpeg" = "image/jpeg"
    ".ico"  = "image/x-icon"
    ".json" = "application/json"
    ".txt"  = "text/plain"
}

try {
    while ($listener.IsListening) {
        $context = $listener.GetContext()
        $request = $context.Request
        $response = $context.Response

        $urlPath = $request.Url.LocalPath.TrimStart('/')
        if ([string]::IsNullOrEmpty($urlPath)) { $urlPath = "index.html" }
        $urlPath = [System.Uri]::UnescapeDataString($urlPath)

        $filePath = Join-Path $rootDir $urlPath

        if (Test-Path $filePath -PathType Leaf) {
            $ext = [System.IO.Path]::GetExtension($filePath).ToLower()
            $fileName = [System.IO.Path]::GetFileName($filePath).ToLower()

            if ($fileName.EndsWith(".wasm.unityweb")) {
                $response.ContentType = "application/wasm"
                $response.AddHeader("Content-Encoding", "br")
            } elseif ($fileName.EndsWith(".data.unityweb")) {
                $response.ContentType = "application/octet-stream"
                $response.AddHeader("Content-Encoding", "br")
            } elseif ($fileName.EndsWith(".framework.js.unityweb") -or $fileName.EndsWith(".js.unityweb")) {
                $response.ContentType = "application/javascript"
                $response.AddHeader("Content-Encoding", "br")
            } elseif ($mimeMap.ContainsKey($ext)) {
                $response.ContentType = $mimeMap[$ext]
            } else {
                $response.ContentType = "application/octet-stream"
            }

            $response.AddHeader("Access-Control-Allow-Origin", "*")
            $response.AddHeader("Cross-Origin-Opener-Policy", "same-origin-allow-popups")
            $response.AddHeader("Cross-Origin-Embedder-Policy", "require-corp")

            $bytes = [System.IO.File]::ReadAllBytes($filePath)
            $response.ContentLength64 = $bytes.Length
            $response.OutputStream.Write($bytes, 0, $bytes.Length)
            $response.StatusCode = 200
        } else {
            $response.StatusCode = 404
            $errBytes = [System.Text.Encoding]::UTF8.GetBytes("404 Not Found: $urlPath")
            $response.OutputStream.Write($errBytes, 0, $errBytes.Length)
        }
        $response.Close()
    }
} finally {
    $listener.Stop()
    $listener.Close()
}
