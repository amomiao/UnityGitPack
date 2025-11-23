using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace Momos.UnityGitPack.Common {
    public static class GitLocalService {
        /// <summary>
        /// 执行 Git 命令
        /// </summary>
        private static bool Execute(string workingDir, string command, out string output) {
            try {
                // 配置 Git 命令执行环境
                ProcessStartInfo psi = new ProcessStartInfo("git", command) {
                    WorkingDirectory = workingDir,        // Git 工作目录
                    RedirectStandardOutput = true,        // 捕获标准输出
                    RedirectStandardError = true,         // 捕获错误输出
                    UseShellExecute = false,              // 必须为 false 才能重定向
                    CreateNoWindow = true                 // 不弹出黑窗
                };

                // 创建 Git 进程
                using Process p = new Process();
                p.StartInfo = psi;

                // 缓存异步读取的数据，避免输出太多卡死
                var stdout = new System.Text.StringBuilder();
                var stderr = new System.Text.StringBuilder();

                // 当 Git 标准输出有新内容时回调
                p.OutputDataReceived += (s, e) => {
                    if (e.Data != null)
                        stdout.AppendLine(e.Data);
                };

                // 当 Git 错误输出有新内容时回调
                p.ErrorDataReceived += (s, e) => {
                    if (e.Data != null)
                        stderr.AppendLine(e.Data);
                };

                // 启动进程
                p.Start();

                // 开始异步读取输出（关键点：避免死锁）
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                // 等待命令执行结束
                p.WaitForExit();

                // 组合输出内容
                output = stdout.ToString() + stderr.ToString();

                // 返回是否成功
                return p.ExitCode == 0;
            }
            catch (Exception ex) {
                // 捕获异常并输出
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
