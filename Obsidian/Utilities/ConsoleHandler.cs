using System.Text;
using System.Threading;

namespace Obsidian.Utilities;

public static class ConsoleHandler 
{
    public struct ConsoleText
    {
        public string Text { get; }
        public ConsoleTextData ExtraData { get; }

        public ConsoleText(string text, ConsoleTextData data = ConsoleTextData.None) 
        {
            this.Text = text;
            this.ExtraData = data;
        }
    }

    public enum ConsoleTextData : sbyte
    {
        None = -1,
        ResetColor = -2,
        UpdateStatus = -3,

        BlackColor = ConsoleColor.Black,
        DarkBlueColor = ConsoleColor.DarkBlue,
        DarkGreenColor = ConsoleColor.DarkGreen,
        DarkCyanColor = ConsoleColor.DarkCyan,
        DarkRedColor = ConsoleColor.DarkRed,
        DarkMagentaColor = ConsoleColor.DarkMagenta,
        DarkYellowColor = ConsoleColor.DarkYellow,
        GrayColor = ConsoleColor.Gray,
        DarkGrayColor = ConsoleColor.DarkGray,
        BlueColor = ConsoleColor.Blue,
        GreenColor = ConsoleColor.Green,
        CyanColor = ConsoleColor.Cyan,
        RedColor = ConsoleColor.Red,
        MagentaColor = ConsoleColor.Magenta,
        YellowColor = ConsoleColor.Yellow,
        WhiteColor = ConsoleColor.White
    }

    /// <summary>
    /// Buffer where all the Console Output is written to.
    ///
    /// This Buffer is handled (written to the console) and cleared if ConsoleHandler.HandleConsoleIO() is running.
    /// </summary>
    public static ConcurrentQueue<ConsoleText> ConsoleOutput = new();
    /// <summary>
    /// Buffer where all the Console Input is written to. Each string is a line/command
    ///
    /// This Buffer only is updated if ConsoleHandler.HandleConsoleIO() is running
    /// </summary>
    public static ConcurrentQueue<string> ConsoleInput = new();

    public static void ResetColor() => ConsoleOutput.Enqueue(new ConsoleHandler.ConsoleText(string.Empty, ConsoleHandler.ConsoleTextData.ResetColor));
    public static void SetForegroundColor(ConsoleColor color) => ConsoleOutput.Enqueue(new ConsoleHandler.ConsoleText(string.Empty, (ConsoleHandler.ConsoleTextData)color));
    public static void Write(string text) => ConsoleOutput.Enqueue(new ConsoleHandler.ConsoleText(text));
    public static void SpecialWrite(string text, ConsoleTextData data) => ConsoleOutput.Enqueue(new ConsoleHandler.ConsoleText(text, data));
    public static void UpdateStatus(string text) => ConsoleOutput.Enqueue(new ConsoleHandler.ConsoleText(text, ConsoleTextData.UpdateStatus));

    /// <summary>
    /// Basic Console IO Method.
    ///
    /// This method should run in a separate thread.
    /// </summary>
    /// <param name="inputDelay">How long to wait if there isn't any key or console output</param>
    public static async Task HandleConsoleIO(double inputDelay = 50)
    {
        var readBuffer = new StringBuilder();
        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(inputDelay));
        bool waitingCharacter = false;
        
        while(true) 
        {
            while(!Console.KeyAvailable && ConsoleOutput.IsEmpty) 
            {
                await timer.WaitForNextTickAsync();
            }

            while(ConsoleOutput.TryDequeue(out var text)) 
            {
                waitingCharacter = HandleConsoleText(text, readBuffer, waitingCharacter);
            }

            if(!Console.KeyAvailable)
                continue;

            var key = Console.ReadKey(true);

            switch(key.Key) 
            {
                case ConsoleKey.LeftArrow:
                    if(Console.CursorLeft > (waitingCharacter ? 1 : 0)) 
                    {
                        Console.CursorLeft--;
                    }
                    break;
                case ConsoleKey.RightArrow:
                    if(Console.CursorLeft < readBuffer.Length + (waitingCharacter ? 1 : 0)) 
                    {
                        Console.CursorLeft++;
                    }
                    break;
                case ConsoleKey.Backspace:
                    HandleBackspaceKey(readBuffer, waitingCharacter);
                    break;
                case ConsoleKey.Enter:
                    waitingCharacter = HandleEnterKey(readBuffer);
                    break;
                default:
                    HandleDefaultKey(key.KeyChar, readBuffer, waitingCharacter);
                    break;
            }
        }
    }

    private static bool HandleConsoleText(in ConsoleText text, StringBuilder buffer, bool waitingCharacter) 
    {
        switch(text.ExtraData) 
        {
            case ConsoleTextData.ResetColor:
                Console.ResetColor();
                break;
            case ConsoleTextData.BlackColor:
            case ConsoleTextData.DarkBlueColor:
            case ConsoleTextData.DarkGreenColor:
            case ConsoleTextData.DarkCyanColor:
            case ConsoleTextData.DarkRedColor:
            case ConsoleTextData.DarkMagentaColor:
            case ConsoleTextData.DarkYellowColor:
            case ConsoleTextData.GrayColor:
            case ConsoleTextData.BlueColor:
            case ConsoleTextData.GreenColor:
            case ConsoleTextData.CyanColor:
            case ConsoleTextData.RedColor:
            case ConsoleTextData.MagentaColor:
            case ConsoleTextData.YellowColor:
            case ConsoleTextData.WhiteColor:
                Console.ForegroundColor = (ConsoleColor)(text.ExtraData);
                break;
            case ConsoleTextData.UpdateStatus:
                ConsoleUpdateStatus(text.Text);
                return waitingCharacter;
        }

    
        var length = buffer.Length + (waitingCharacter ? 1 : 0);
        Console.CursorLeft -= length;
        
        for(int i = 0; i < length; i++)
            Console.Write(' ');

        Console.CursorLeft -= length;

        Console.Write(text.Text);

        Console.Write('>');
        return true;
    }

    private static void HandleDefaultKey(char c, StringBuilder buffer, bool waitingCharacter) 
    {
        buffer.Insert(Console.CursorLeft - (waitingCharacter ? 1 : 0), c);
        var lastLeft = Console.CursorLeft;

        Console.CursorLeft = 0; // Print new buffer
        Console.Write('>');
        Console.Write(buffer.ToString());

        Console.CursorLeft = lastLeft + 1;// Set the character to the last position
    }

    private static void HandleBackspaceKey(StringBuilder buffer, bool waitingCharacter) 
    {
        var cursorLeft = Console.CursorLeft;
        var length = buffer.Length + (waitingCharacter ? 1 : 0);

        if(buffer.Length > 0 && cursorLeft > (waitingCharacter ? 1 : 0)) // There are characters in the buffer
        {
            if(cursorLeft == length) // Easier path
            {
                buffer.Length -= 1;
                
                Console.CursorLeft--; // Just remove 1 character and return
                Console.Write(' ');
                Console.CursorLeft--;
                return;
            }

            buffer.Remove((cursorLeft - (waitingCharacter ? 1 : 0)) - 1, 1); // Remove the character next to the cursor

            Console.Write(' '); // Remove 1 character

            Console.CursorLeft = 0; // Print new buffer
            Console.Write('>');
            Console.Write(buffer.ToString());

            Console.CursorLeft = cursorLeft - 1; // Set the character to the last position
        }
    }

    private static bool HandleEnterKey(StringBuilder buffer)
    {
        if(buffer.Length > 0) // There are characters in the buffer
        {
            ConsoleInput.Enqueue(buffer.ToString()); // Enqueue the input so we can handle the command later
            buffer.Clear();
            Console.Write('\n');
            return false;
        }

        Console.Write('\n');
        Console.Write('>');
        return true;
    }

    private static void ConsoleUpdateStatus(string text) 
    {
        try 
        {
            var consolePos = Console.GetCursorPosition(); // Get current position

            Console.CursorTop = 0;
            Console.CursorLeft = Console.BufferWidth - text.Length - 1;

            Console.Write(' '); // Write the status
            Console.Write(text);

            Console.SetCursorPosition(consolePos.Left, consolePos.Top); // Set the last position we were in
        } 
        catch(Exception) {} // Not Supported?
    }
}