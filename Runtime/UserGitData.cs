using System;
using System.IO;
using UnityEngine;

namespace Momos.UnityGitPack.Common {
    [Serializable]
    public class UserGitData {
        public enum GitProvider {
            GitHub = 0,
            GitLab,
            Bitbucket,
            Gitea,
            Customize
        }

        /// <summary>
        /// Git 仓库的 API 基础 URL（对于 GitHub 通常无需修改）。
        /// 例如：https://api.github.com
        /// </summary>
        public string apiBaseUrl = "https://api.github.com";

        /// <summary>
        /// Git 提供商类型（GitHub / GitLab / Gitea / Bitbucket 等）。
        /// </summary>
        public GitProvider provider = GitProvider.GitHub;

        /// <summary>
        /// 用户的 Git Token（PAT / Access Token）。
        /// 用于 API 鉴权。
        /// 建议只在本地加密保存。
        /// </summary>
        [TextArea(1,2)]
        public string accessToken;

        /// <summary>
        /// Git 用户名（例如 GitHub ID）。
        /// 有些 API 需要明确指定 owner。
        /// </summary>
        public string userName;

        /// <summary>
        /// 仓库名称
        /// </summary>
        public string repoName;

        /// <summary>
        /// 仓库描述（可选）。
        /// </summary>
        public string repoDescription;

        /// <summary>
        /// 仓库是否为 Private 的默认选项。
        /// </summary>
        public bool privateRepo = false;

        /// <summary>
        /// 上传 UnityPackage 的目录（本地路径 以包含"Assets/" 从后面写）。
        /// 用于自动打包并上传。
        /// </summary>
        public string localPackagePathOfAssets;

        /// <summary>
        /// 创建仓库后要推送的默认分支名（main / master）。
        /// </summary>
        public string defaultBranch = "main";

        /// <summary>
        /// 提交概述
        /// </summary>
        [TextArea(1, 5)]
        public string submitTitle = string.Empty;

        /// <summary>
        /// 提交描述
        /// </summary>
        [TextArea(1, 10)]
        public string submitBody = string.Empty;

        public string DirPath => Path.Combine(Application.dataPath, localPackagePathOfAssets);
        public string SubmitTitle => string.IsNullOrWhiteSpace(submitTitle) ? "Update" : submitTitle.Replace("\"", "'");
        public string SubmitBody => string.IsNullOrWhiteSpace(submitBody) ? $"Commit at {DateTime.Now:yyyy-MM-dd HH:mm:ss}" : submitBody.Replace("\"", "'");
    }
}
