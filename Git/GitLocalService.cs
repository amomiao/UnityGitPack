using Momos.UnityGitPack.Common;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace Momos.UnityGitPackEditor.Common {
    public static class GitLocalService {
        private static bool Execute(string workingDir, string command, out string output) {
            try {
                ProcessStartInfo psi = new ProcessStartInfo("git", command) {
                    WorkingDirectory = workingDir,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process p = Process.Start(psi);
                p.WaitForExit();

                output = p.StandardOutput.ReadToEnd() + "\n" + p.StandardError.ReadToEnd();

                return p.ExitCode == 0;
            }
            catch (System.Exception ex) {
                output = "Exception: " + ex.Message;
                return false;
            }
        }

        public static bool Push(UserGitData data, out string message) {
            string dir = Path.Combine(Application.dataPath, data.localPackagePathOfAssets);

            if (string.IsNullOrEmpty(data.localPackagePathOfAssets)) {
                message = $"{nameof(data.localPackagePathOfAssets)} is Empty!";
                return false;
            }
            if (!Directory.Exists(dir)) {
                message = $"Directory is Not Exist!\n Path:{dir}";
                return false;
            }

            string remoteUrl = $"https://github.com/{data.userName}/{data.repoName}.git";

            // Step 1: init
            if (!Execute(dir, "init", out message))
                return false;

            // Step 2: add remote
            Execute(dir, "remote remove origin", out _); // prevent duplicates
            if (!Execute(dir, $"remote add origin {remoteUrl}", out message))
                return false;

            // Step 3: add & commit
            if (!Execute(dir, "add .", out message))
                return false;
            if (!Execute(dir, "commit -m \"Initial commit from UnityGitPack\"", out message))
                return false;

            // Step 4: 拉取远端，避免 "src refspec main does not match any" 和 "unrelated histories"
            //
            // **关键新增**
            Execute(dir, $"pull origin {data.defaultBranch} --allow-unrelated-histories", out _);

            // Step 5: push
            if (!Execute(dir, $"push -u origin {data.defaultBranch}", out message))
                return false;

            message = "Push success!\n\n" + message;
            return true;
        }
    }
}
