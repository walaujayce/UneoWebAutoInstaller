using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UneoWebApplicationAutoInstaller.Utilities;
using ProcessOrigin = System.Diagnostics.Process;

namespace UneoWebApplicationAutoInstaller.Command
{
    public class CommandExecutor
    {
        private const string TAG = "CMD";
        private static readonly Lazy<CommandExecutor> _instance = new Lazy<CommandExecutor>(() => new CommandExecutor());

        private bool success = false;
        private static readonly object _lock = new object();
        public static CommandExecutor Instance => _instance.Value;

        private CommandExecutor()
        {
            // Private constructor prevents external instantiation
        }

        public async Task<bool> RunCommandAsAdminReturnBoolAsync(string command, string msg)
        {
            lock (_lock)
            {
                success = false; // Reset success before executing
            }

            try
            {
                Log.I(TAG, msg);
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    Verb = "runas", // Run as administrator
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (var process = ProcessOrigin.Start(processStartInfo))
                {
                    if (process == null)
                    {
                        Debug.WriteLine("Failed to start the process.");
                        return false;
                    }

                    await process.WaitForExitAsync();

                    lock (_lock)
                    {
                        success = process.ExitCode == 0;
                    }
                    return success;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<string> RunCommandAsAdminReturnStringAsync(string command, string msg)
        {
            try
            {
                Log.I(TAG, msg);
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    Verb = "runas",
                    UseShellExecute = false, // Must be false to capture output
                    RedirectStandardOutput = true, // Capture command output
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (var process = new ProcessOrigin { StartInfo = processStartInfo })
                {
                    process.Start();
                    string output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    return output.Trim(); // Trim to clean up whitespace/newlines
                }
            }
            catch (Exception ex)
            {
                return string.Empty; // Return empty if there was an error
            }
        }
    }
}
