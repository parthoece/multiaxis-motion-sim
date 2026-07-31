param(
    [string]$HostName = "127.0.0.1",
    [int]$Port = 23000
)

$client = [System.Net.Sockets.TcpClient]::new()
$stream = $null

function Read-AvailableText {
    param(
        [System.Net.Sockets.NetworkStream]$NetworkStream,
        [int]$WaitMilliseconds = 750
    )

    $encoding = [System.Text.Encoding]::ASCII
    $buffer = [byte[]]::new(4096)
    $result = [System.Text.StringBuilder]::new()
    $deadline = [DateTime]::UtcNow.AddMilliseconds($WaitMilliseconds)

    do {
        while ($NetworkStream.DataAvailable) {
            $bytesRead = $NetworkStream.Read(
                $buffer,
                0,
                $buffer.Length)

            if ($bytesRead -le 0) {
                break
            }

            [void]$result.Append(
                $encoding.GetString(
                    $buffer,
                    0,
                    $bytesRead))
        }

        Start-Sleep -Milliseconds 25
    }
    while ([DateTime]::UtcNow -lt $deadline)

    return $result.ToString().Trim()
}

try {
    Write-Host "Connecting to ${HostName}:${Port}..."

    $client.Connect($HostName, $Port)
    $stream = $client.GetStream()

    Write-Host "Connected."

    # Read any startup banner.
    $startup = Read-AvailableText `
        -NetworkStream $stream `
        -WaitMilliseconds 500

    if (-not [string]::IsNullOrWhiteSpace($startup)) {
        Write-Host "`nStartup response:"
        Write-Host $startup
    }

    $encoding = [System.Text.Encoding]::ASCII

    # $I is a normal line-based command.
    $buildInfoCommand = $encoding.GetBytes(
        '$I' + "`n")

    $stream.Write(
        $buildInfoCommand,
        0,
        $buildInfoCommand.Length)

    $stream.Flush()

    $buildInfo = Read-AvailableText `
        -NetworkStream $stream `
        -WaitMilliseconds 1000

    Write-Host "`nBuild information:"
    Write-Host $buildInfo

    # ? is a real-time command and must not include a newline.
    $stream.WriteByte([byte][char]'?')
    $stream.Flush()

    $status = Read-AvailableText `
        -NetworkStream $stream `
        -WaitMilliseconds 1000

    Write-Host "`nMachine status:"
    Write-Host $status

    if ([string]::IsNullOrWhiteSpace($buildInfo)) {
        throw "The simulator did not respond to the `$I command."
    }

    if ($status -notmatch "<.+>") {
        throw "The simulator did not return a status report."
    }

    Write-Host "`nSmoke test passed."
}
finally {
    if ($null -ne $stream) {
        $stream.Dispose()
    }

    $client.Dispose()
}