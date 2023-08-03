using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sp2YtNet;
public static class Helpers
{
    private static MemoryStream memoryStream;
    private static StreamWriter streamWriter;
    private static TextWriter originalConsoleOut;

    public static void ActivateOrDeactivateConsoleOutputs(bool isActive)
    {
        if (isActive)
        {
            if (memoryStream != null && streamWriter != null)
            {
                //// Retrieve the captured output from the MemoryStream and display it
                //memoryStream.Position = 0;
                //var output = new StreamReader(memoryStream).ReadToEnd();
                //Console.WriteLine("Captured Output:");
                //Console.WriteLine(output);

                // Revert the standard output back to the original one
                Console.SetOut(originalConsoleOut);

                // Clean up the MemoryStream and StreamWriter
                streamWriter.Dispose();
                memoryStream.Dispose();
                streamWriter = null;
                memoryStream = null;
            }
        }
        else
        {
            // Save the original standard output and redirect to a new MemoryStream
            originalConsoleOut = Console.Out;
            memoryStream = new MemoryStream();
            streamWriter = new StreamWriter(memoryStream);
            Console.SetOut(streamWriter);
        }
    }

    public static string TrimEndWith(this string str, string suffixToRemove)
    {
        if (str.EndsWith(suffixToRemove, StringComparison.InvariantCultureIgnoreCase))
        {
            return str.Substring(0, str.Length - suffixToRemove.Length);
        }
        else
        {
            return str;
        }
    }

    public static int GetPortFromUri(string uriString)
    {
        if (Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out Uri uri))
        {
            return uri.Port;
        }

        // If the provided string is not a valid URI, return 0 to indicate failure.
        return 0;
    }
}
