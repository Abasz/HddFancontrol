using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace HddFancontrol.ConsoleApp.Libs
{
    public static class HelperMethods
    {
        /// <summary>
        /// Executes a bash script in Async mode and waits for it to finish
        /// </summary>
        /// <param name="cmd">The bash command as string</param>
        /// <returns>Returns the command output string wrapped as a Task</returns>
        public static Task<string> BashAsync(this string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo
                {
                FileName = Path.Combine("/", "bin", "bash"),
                Arguments = $"-c \"{escapedArgs} 2>&1\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                }
            };

            process.Start();

            return process.StandardOutput.ReadToEndAsync();
        }

        /// <summary>
        /// Executes a bash script in Async mode and reports the command output in real time via the DataReceivedEventHandler
        /// </summary>
        /// <param name="cmd">The bash command as string</param>
        /// <param name="cmdOutputEventHandler">The event handler typed as DataReceivedEventHandler</param>
        /// <returns>Returns process exit code as wrapped as a Task</returns>
        public static Task<int> BashStreamOutput(this string cmd, DataReceivedEventHandler cmdOutputEventHandler)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");
            var tcs = new TaskCompletionSource<int>();

            var process = new Process()
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo
                {
                FileName = Path.Combine("/", "bin", "bash"),
                Arguments = $"-c \"{escapedArgs} 2>&1\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                }
            };
            process.OutputDataReceived += cmdOutputEventHandler;
            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };
            process.Start();
            process.BeginOutputReadLine();

            return tcs.Task;
        }

        /// <summary>
        /// Converts string to camelCase
        /// </summary>
        /// <param name="str">The string to convert</param>
        /// <returns>Returns the camelCased <see cref="string"/></returns>
        public static string ToCamelCase(this string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                return Char.ToLowerInvariant(str[0]) + str[1..];
            }
            return str;
        }

    }
}