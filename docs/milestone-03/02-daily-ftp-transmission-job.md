---
status: draft
milestone: 3
github_issue:
task_issues: []
---

# Daily FTP Transmission Job

## User Story

> As a **customer**,
> I need my submitted orders to be automatically transmitted to the ERP system each day,
> in order to have my orders fulfilled without any manual intervention.

## Background

When an order is submitted, a fixed-width text file is written to a local pickup directory (format defined in `docs/standards/erp-order-format.md`). This job reads those files and transmits them to the ERP system via FTP once per day. Transmission status is tracked per order (see Story 1) so customers can monitor progress.

## Scope

**In scope:**
- Scheduled daily job that runs at 5:00 PM local time and processes all orders with `Pending` or `Missing` transmission status
- The job run time is configurable via an application setting (defaults to 5:00 PM)
- For each order: locate the file in the pickup directory, transmit via FTP, update status
- If the file is not found: set transmission status to `Missing` (retried on every subsequent run)
- If transmission succeeds: set transmission status to `Transmitted` with current timestamp
- If transmission fails: set transmission status to `Failed` with current timestamp
- Aspire AppHost FTP server resource for local development

**Out of scope:**
- Retrying `Failed` orders (on-demand retransmission, Story 3)
- UI changes (covered in Story 1)
- Modifying the ERP file format or FTP configuration

## Developer Notes

- The job run time defaults to 5:00 PM UTC and is controlled by `TransmissionJob:ScheduleHourUtc`, like `ExpireDraftOrdersJob` (which uses `DateTime.UtcNow` + `DraftOrderExpiry:ScheduleHourUtc`)
- FTP configuration (host, credentials, target directory) comes from app config; the Aspire AppHost overrides these for local development using a `delfer/alpine-ftp-server` container
- The job processes `Pending` and `Missing` orders each run; `Failed` orders are left untouched
- `Missing` orders re-enter the queue automatically on every run — no manual intervention needed
- ERP file format spec: `docs/standards/erp-order-format.md` — filename `EXT-{OrderNum zero-padded to 10}.TXT`
- The job should run immediately on startup.

## NuGet Package

Use **`FluentFTP`** for FTP transmission.

- Full async API (`AsyncFtpClient`) — consistent with the project's async/await style
- Zero native dependencies — safe for containerized deployments
- Supports .NET 10
- Passive mode required by the `delfer/alpine-ftp-server` dev container is built-in

```csharp
using FluentFTP;

public class FtpOrderTransmitter
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly string _remoteDirectory;
    private readonly ILogger<FtpOrderTransmitter> _logger;

    public FtpOrderTransmitter(IConfiguration configuration, ILogger<FtpOrderTransmitter> logger)
    {
        _host            = configuration["FtpTransmission:Host"] ?? throw new InvalidOperationException("FtpTransmission:Host is required.");
        _port            = configuration.GetValue("FtpTransmission:Port", 21);
        _username        = configuration["FtpTransmission:Username"] ?? throw new InvalidOperationException("FtpTransmission:Username is required.");
        _password        = configuration["FtpTransmission:Password"] ?? throw new InvalidOperationException("FtpTransmission:Password is required.");
        _remoteDirectory = configuration.GetValue("FtpTransmission:RemoteDirectory", "/")!;
        _logger          = logger;
    }

    public async Task<bool> TransmitAsync(string localFilePath, string fileName, CancellationToken cancellationToken)
    {
        using var client = new AsyncFtpClient(_host, _username, _password, _port);

        // PASV required for containerized deployments and the local dev FTP container.
        client.Config.DataConnectionType = FtpDataConnectionType.PASV;

        await client.Connect(cancellationToken);

        var remotePath = $"{_remoteDirectory.TrimEnd('/')}/{fileName}";

        var status = await client.UploadFile(
            localPath: localFilePath,
            remotePath: remotePath,
            existsMode: FtpRemoteExists.Overwrite,
            createRemoteDir: true,
            token: cancellationToken);

        _logger.LogInformation("FTP upload {FileName} → {RemotePath}: {Status}", fileName, remotePath, status);

        return status == FtpStatus.Success;
    }
}
```

Use `AsyncFtpClient` (FluentFTP v50+); the older `FtpClient` async wrappers are deprecated. `FtpRemoteExists.Overwrite` is safe for retries.

## Application Settings

All settings live in `appsettings.json` under `WidgetDepot.ApiService`. The Aspire AppHost overrides FTP settings for local development.

| Setting Key | Type | Default | Description |
|---|---|---|---|
| `TransmissionJob:ScheduleHourUtc` | `int` | `17` | Hour of day (0–23) in UTC time at which the job fires. Default `17` = 5:00 PM. |
| `FtpTransmission:Host` | `string` | _(required)_ | FTP server hostname or IP. AppHost overrides to `127.0.0.1` for local dev. |
| `FtpTransmission:Port` | `int` | `21` | FTP control port. Local dev container maps host `2121` → container `21`, so AppHost overrides to `2121`. |
| `FtpTransmission:Username` | `string` | _(required)_ | FTP account username. |
| `FtpTransmission:Password` | `string` | _(required)_ | FTP account password. Supply via environment variable or secrets — do not commit to `appsettings.json`. |
| `FtpTransmission:RemoteDirectory` | `string` | `/` | Remote directory path on the FTP server where files are uploaded. |

## Acceptance Criteria

- [ ] A scheduled job runs once per day at 5:00 PM UTC time and processes all orders with `Pending` or `Missing` transmission status
- [ ] The job run time is configurable via an application setting and defaults to 5:00 PM
- [ ] If the order file is not found in the pickup directory, the order transmission status is set to `Missing` with the current timestamp
- [ ] If the order file is found, the job transmits it to the configured FTP server
- [ ] On successful transmission, the order transmission status is updated to `Transmitted` with the current timestamp
- [ ] On FTP transmission failure, the order transmission status is updated to `Failed` with the current timestamp
- [ ] `Failed` orders are not retried by the job
- [ ] `Missing` orders are retried on every subsequent job run
- [ ] The Aspire AppHost includes a `delfer/alpine-ftp-server` container that the app connects to in the Development environment

## Refinement Notes

Four transmission statuses: `Pending` (set on submission), `Transmitted`, `Failed`, `Missing`. The job retries `Pending` and `Missing` each run; `Failed` is retried only on customer demand (Story 3). `Missing` was introduced to distinguish "file not found in pickup directory" from an FTP-level failure, as both are recoverable but through different paths.

FTP credentials and configuration are not changed from the existing system (hard constraint from PRD §7.1).

## Template for adding FTP Container to AppHost

```csharp
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Passive FTP needs a predictable passive port range.
// The delfer/alpine-ftp-server image defaults to 21000-21010, but defining
// the values here keeps the container config and Aspire endpoints aligned.
const int ftpHostPort = 2121;
const int passivePortStart = 21000;
const int passivePortEnd = 21010;

var ftp = builder
    .AddContainer("fake-ftp", "delfer/alpine-ftp-server")
    .WithEnvironment("USERS", "devftp|devftp|/home/devftp")
    .WithEnvironment("ADDRESS", "127.0.0.1")
    .WithEnvironment("MIN_PORT", passivePortStart.ToString())
    .WithEnvironment("MAX_PORT", passivePortEnd.ToString())

    // FTP control connection.
    // Host: localhost:2121
    // Container: 21
    .WithEndpoint(
        port: ftpHostPort,
        targetPort: 21,
        scheme: "ftp",
        name: "ftp",
        isProxied: false);

// Expose the passive FTP data ports.
// These must be real TCP port mappings, not just service-discovery endpoints.
for (var port = passivePortStart; port <= passivePortEnd; port++)
{
    ftp = ftp.WithEndpoint(
        port: port,
        targetPort: port,
        scheme: "tcp",
        name: $"ftp-passive-{port}",
        isProxied: false);
}

builder.Build().Run();
```