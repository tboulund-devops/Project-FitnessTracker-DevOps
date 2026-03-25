using System.IO.Pipes;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Xunit;

/// <summary>
/// Skips integration tests when Docker is not reachable on the current machine.
/// </summary>
public sealed class DockerFactAttribute : FactAttribute
{
    public DockerFactAttribute()
    {
        if (!DockerAvailability.IsDockerAvailable())
        {
            Skip = "Docker is not available on this machine.";
        }
    }
}

internal static class DockerAvailability
{
    private static bool? _isAvailable;
    private static readonly object Sync = new();

    public static bool IsDockerAvailable()
    {
        if (_isAvailable.HasValue)
        {
            return _isAvailable.Value;
        }

        lock (Sync)
        {
            if (_isAvailable.HasValue)
            {
                return _isAvailable.Value;
            }

            _isAvailable = TryConnect();
            return _isAvailable.Value;
        }
    }

    private static bool TryConnect()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var pipe = new NamedPipeClientStream(".", "docker_engine", PipeDirection.InOut);
                pipe.Connect(150);
                return pipe.IsConnected;
            }

            using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            socket.ReceiveTimeout = 150;
            socket.SendTimeout = 150;
            socket.Connect(new UnixDomainSocketEndPoint("/var/run/docker.sock"));
            return socket.Connected;
        }
        catch
        {
            return false;
        }
    }
}

