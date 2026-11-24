using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Telnet_File_Manager
{
    public static class StringBuilderExt
    {
        public static StringBuilder RemoveLastLine(this StringBuilder sb)
        {
            for (int i = sb.Length - 1; i >= 0; i--)
            {
                if (Environment.NewLine.Contains(sb[i]))
                {
                    sb.Remove(i, sb.Length - i);
                    return sb;
                }
            }
            return sb;
        }

        /// Removes the first line from a StringBuilder.
        /// Handles empty content and different newline formats.
        /// </summary>
        public static void RemoveFirstLine(this StringBuilder sb)
        {
            if (sb == null || sb.Length == 0)
                return; // Nothing to remove

            // Find the first newline position
            int newlineIndex = sb.ToString().IndexOfAny(new char[] { '\n', '\r' });

            if (newlineIndex == -1)
            {
                // No newline found, remove everything
                sb.Clear();
                return;
            }

            // Handle Windows-style newline (\r\n)
            if (newlineIndex < sb.Length - 1 && sb[newlineIndex] == '\r' && sb[newlineIndex + 1] == '\n')
            {
                newlineIndex += 2;
            }
            else
            {
                // Handle Unix-style newline (\n) or old Mac (\r)
                newlineIndex += 1;
            }

            // Remove from start to the newline index
            sb.Remove(0, newlineIndex);
        }
    }
}
