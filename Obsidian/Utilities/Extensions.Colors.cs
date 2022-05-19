﻿namespace Obsidian.Utilities;

public static partial class Extensions
{
    public static void RenderColoredConsoleMessage(this string message)
    {
        int start = 0;
        int end = message.Length - 1;

        for (int i = 0; i < end; i++)
        {
            if (message[i] != '&' && message[i] != '§')
                continue;

            // Validate color code
            char colorCode = message[i + 1];
            if (!ChatColor.TryParse(colorCode, out var color))
                continue;

            // Print text with previous color
            if (start != i)
            {
                ConsoleHandler.Write(message.Substring(start, i - start));
            }

            // Change color
            if (colorCode == 'r')
            {
                ConsoleHandler.ResetColor();
            }
            else
            {
                if(color.ConsoleColor.HasValue) 
                    ConsoleHandler.SetForegroundColor(color.ConsoleColor.Value);
            }

            // Skip color code
            i++;
            start = i + 1;
        }

        // Print remaining text if any
        if (start != message.Length)
            ConsoleHandler.Write(message.Substring(start));

        ConsoleHandler.ResetColor();
    }
}
