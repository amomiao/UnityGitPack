using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace Momos.UnityGitPack.Common {
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

        /// <summary>
        /// 拉取远程更新
        /// </summary>
        public static bool Pull(UserGitData data, out string message, bool rebase = true) {
            string dir = data.DirPath;
            if (!Directory.Exists(dir)) {
                message = $"Directory does not exist: {dir}";
                return false;
            }

            string branch = data.defaultBranch;

            string pullCmd = rebase ? $"pull --rebase origin {branch}" : $"pull origin {branch}";
            bool result = Execute(dir, pullCmd, out message);

            if (!result) {
                message = "Pull failed:\n" + message;
            }
            else {
                message = "Pull success:\n" + message;
            }
            return result;
        }

        public static bool Push(UserGitData data, out string message) {
            string dir = data.DirPath;

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

            // Step 1.5 重命名本地分支为 Default Branch
            Execute(dir, $"branch -M {data.defaultBranch}", out _);

            // Step 2: add remote
            Execute(dir, "remote remove origin", out _); // prevent duplicates
            if (!Execute(dir, $"remote add origin {remoteUrl}", out message))
                return false;

            // Step 3: add & commit
            // Push失败时commit是成功的,再次提交需要绕过逻辑
            Execute(dir, "status --porcelain", out var status);
            bool hasChanges = !string.IsNullOrWhiteSpace(status);
            if (hasChanges) {
                if (!Execute(dir, "add .", out message))
                    return false;
                if (!Execute(dir, "commit -m \"Initial commit from UnityGitPack\"", out message))
                    return false;
            }

            // Step 4: 拉取远端，避免 "src refspec main does not match any" 和 "unrelated histories"
            Execute(dir, $"pull origin {data.defaultBranch} --allow-unrelated-histories", out _);

            // Step 5: push
            if (!Execute(dir, $"push -u origin {data.defaultBranch}", out message))
                return false;

            message = "Push success!\n\n" + message;
            return true;
        }

        public static bool PushProxy(UserGitData data, out string message, short port) {
            string dir = data.DirPath;
            Execute(dir, $"config http.proxy http://127.0.0.1:{port}", out _);
            Execute(dir, $"config https.proxy http://127.0.0.1:{port}", out _);
            bool result = Push(data, out message);
            Execute(dir, "config --unset http.proxy", out _);
            Execute(dir, "config --unset https.proxy", out _);
            if (!result) {
                message = "Proxy Push Failed\n" + message;
            }
            return result;
        }
    }
}
