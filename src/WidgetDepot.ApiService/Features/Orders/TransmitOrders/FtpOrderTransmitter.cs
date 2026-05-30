using FluentFTP;

namespace WidgetDepot.ApiService.Features.Orders.TransmitOrders;

public class FtpOrderTransmitter : IOrderTransmitter
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly string _remoteDirectory;
    private readonly ILogger<FtpOrderTransmitter> _logger;

    public FtpOrderTransmitter(IConfiguration configuration, ILogger<FtpOrderTransmitter> logger)
    {
        _host = configuration["FtpTransmission:Host"] ?? throw new InvalidOperationException("FtpTransmission:Host is required.");
        _port = configuration.GetValue("FtpTransmission:Port", 21);
        _username = configuration["FtpTransmission:Username"] ?? throw new InvalidOperationException("FtpTransmission:Username is required.");
        _password = configuration["FtpTransmission:Password"] ?? throw new InvalidOperationException("FtpTransmission:Password is required.");
        _remoteDirectory = configuration.GetValue("FtpTransmission:RemoteDirectory", "/")!;
        _logger = logger;
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