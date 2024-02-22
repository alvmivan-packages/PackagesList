using System;
using System.Diagnostics;
//using System.Diagnostics;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace PackagesList.SSH
{
    public static class SSHManagement
    {
        public static async Task<bool> HasSSHSetup()
        {
            try
            {
                using var process = new Process();
                
                process.StartInfo.FileName = "ssh-keygen";
                process.StartInfo.Arguments = "-l"; // List fingerprints of specified public key file
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                    
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                    
                Debug.Log(output);
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError(error);
                }
                    
                process.WaitForExit();
                
                // If the process exit code is 0, it means keys are set up
                return process.ExitCode == 0;
            }
            catch (Exception)
            {
                // Debug.LogException(e);
                throw;
                return false;
            }
        }
    }
}