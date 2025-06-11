using System.Net.Sockets;
using System.Net;

class tcp_server
{
    static async Task Main()
    {
        while (true)
        {
            await StartServerAsync();
        }
    }
    static async Task StartServerAsync()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, 8080);
        listener.Start();
        Console.WriteLine("Server is listening...");

        using TcpClient client = await listener.AcceptTcpClientAsync();
        await using NetworkStream stream = client.GetStream();

        // Read 4 bytes to get the length
        byte[] lengthBytes = new byte[4];
        await stream.ReadAsync(lengthBytes, 0, 4);
        int fileSize = BitConverter.ToInt32(lengthBytes, 0);

        // Receive the file
        byte[] fileBuffer = new byte[fileSize];
        int totalRead = 0;
        while (totalRead < fileSize)
        {
            int read = await stream.ReadAsync(fileBuffer, totalRead, fileSize - totalRead);
            if (read == 0) break;
            totalRead += read;
        }

        // Save to disk
        string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "received_file.txt");
        await File.WriteAllBytesAsync(outputPath, fileBuffer);

        Console.WriteLine($"File received and saved to: {outputPath}");
        listener.Stop();
    }
}


