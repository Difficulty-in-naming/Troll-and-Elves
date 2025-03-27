using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Panthea.Common
{
    public class ProcessUtil
    {
        public static int Execute(string argument, string fileName, string workingDir)
        {
            try
            {
                Debug.Log("============== Start Executing [" + argument + "] ===============");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDir,
                    Arguments = argument
                };
                Process p = new Process { StartInfo = startInfo };
                p.OutputDataReceived += (s, e) => { Debug.Log(e.Data); };
                p.ErrorDataReceived += (s, e) => { Debug.Log(e.Data); };

                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.WaitForExit();
                int exitCode = p.ExitCode;
                return exitCode;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return -1;
            }
        }
    }
}
