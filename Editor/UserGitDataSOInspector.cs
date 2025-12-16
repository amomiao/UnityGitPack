using Momos.UnityGitPack.Common;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using static Momos.UnityGitPack.Common.UPMData;
using static Momos.UnityGitPack.Common.UserGitData;

namespace Momos.UnityGitPackEditor.Common {
    [CustomEditor(typeof(UnityGitPackDataSO))]
    public class UserGitDataSOInspector : Editor {
        UnityGitPackDataSO _this;
        int index;
        string[] tabNames = new string[] { "GitData", "UpmData" };

        SerializedProperty leadAuthor;

        SerializedProperty provider;
        SerializedProperty apiBaseUrl;
        SerializedProperty accessToken;
        SerializedProperty userName;
        SerializedProperty repoName;
        SerializedProperty repoDescription;
        SerializedProperty privateRepo;
        SerializedProperty localPackagePathOfAssets;
        SerializedProperty defaultBranch;
        SerializedProperty submitTitle;
        SerializedProperty submitBody;

        new SerializedProperty name;
        SerializedProperty version;
        SerializedProperty displayName;
        SerializedProperty description;
        SerializedProperty unity;
        SerializedProperty author;
        SerializedProperty dependencies;

        SerializedProperty proxyPort;

        private void OnEnable() {
            _this = (UnityGitPackDataSO)this.target;
            index = 0;
            _this.gitData ??= new UserGitData();
            InitProperty();
        }

        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();
            EditorGUILayout.PropertyField(leadAuthor);

            index = GUILayout.Toolbar(index, tabNames);
            if (index == 0) {
                DrawGitDataTab();
            }
            else {
                DrawUPMDataTab();
            }

            GUILayout.Space(10);
            EditorGUILayout.PropertyField(proxyPort);
            DrawButton();
            serializedObject.ApplyModifiedProperties();
            if (GUI.changed) {
                EditorUtility.SetDirty(target);
            }
        }

        void InitProperty() {
            SerializedProperty GetPropertyOfGitData(string memberName) {
                return serializedObject.FindProperty($"{nameof(_this.gitData)}.{memberName}");
            }
            SerializedProperty GetPropertyOfUPMData(string memberName) {
                return serializedObject.FindProperty($"{nameof(_this.upmData)}.{memberName}");
            }
            leadAuthor = serializedObject.FindProperty(nameof(_this.leadAuthor));

            provider = GetPropertyOfGitData(nameof(_this.gitData.provider));
            apiBaseUrl = GetPropertyOfGitData(nameof(_this.gitData.apiBaseUrl));
            accessToken = GetPropertyOfGitData(nameof(_this.gitData.accessToken));
            userName = GetPropertyOfGitData(nameof(_this.gitData.userName));
            repoName = GetPropertyOfGitData(nameof(_this.gitData.repoName));
            repoDescription = GetPropertyOfGitData(nameof(_this.gitData.repoDescription));
            privateRepo = GetPropertyOfGitData(nameof(_this.gitData.privateRepo));
            localPackagePathOfAssets = GetPropertyOfGitData(nameof(_this.gitData.localPackagePathOfAssets));
            defaultBranch = GetPropertyOfGitData(nameof(_this.gitData.defaultBranch));
            submitTitle = GetPropertyOfGitData(nameof(_this.gitData.submitTitle));
            submitBody = GetPropertyOfGitData(nameof(_this.gitData.submitBody));

            name = GetPropertyOfUPMData(nameof(_this.upmData.name));
            version = GetPropertyOfUPMData(nameof(_this.upmData.version));
            displayName = GetPropertyOfUPMData(nameof(_this.upmData.displayName));
            description = GetPropertyOfUPMData(nameof(_this.upmData.description));
            unity = GetPropertyOfUPMData(nameof(_this.upmData.unity));
            author = GetPropertyOfUPMData(nameof(_this.upmData.author));
            dependencies = GetPropertyOfUPMData(nameof(_this.upmData.dependencies));

            proxyPort = serializedObject.FindProperty(nameof(_this.proxyPort));
        }

        void DrawGitDataTab() {
            // provider
            EditorGUILayout.PropertyField(provider);
            // apiBaseUrl
            apiBaseUrl.stringValue = provider.enumValueIndex switch {
                (int)GitProvider.GitHub => "https://api.github.com",
                (int)GitProvider.GitLab => "https://gitlab.com/api/v4",
                (int)GitProvider.Bitbucket => "https://api.bitbucket.org/2.0",
                (int)GitProvider.Gitea => "https://gitea.com/api/v1",
                (int)GitProvider.Customize => apiBaseUrl.stringValue,
                _ => apiBaseUrl.stringValue
            };
            EditorGUILayout.PropertyField(apiBaseUrl);
            // accessToken
            EditorGUILayout.PropertyField(accessToken);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("TokenWeb")) {
                Application.OpenURL("https://github.com/settings/tokens?type=beta");
            }
            if (GUILayout.Button("Check Token")) {
                bool ok = GitHubApiService.CheckToken(_this.gitData, out string msg);
                EditorUtility.DisplayDialog("Check Token Result", msg, "OK");
            }
            EditorGUILayout.EndHorizontal();
            // userName
            EditorGUILayout.PropertyField(userName);
            // repoName
            EditorGUILayout.PropertyField(repoName);
            EditorGUILayout.BeginHorizontal(); 
            if (GUILayout.Button("Create Repo")) {
                var result = GitHubApiService.CreateRepo(_this.gitData);
                EditorUtility.DisplayDialog("Create Repo Result",
                    $"Result: {result.State}\nMessage: {result.Message}", "OK");
            }
            EditorGUILayout.EndHorizontal();
            // repoDescription
            EditorGUILayout.PropertyField(repoDescription);
            // privateRepo
            EditorGUILayout.PropertyField(privateRepo);
            // localPackagePathOfAssets
            EditorGUILayout.PropertyField(localPackagePathOfAssets);
            // defaultBranch
            EditorGUILayout.PropertyField(defaultBranch);
            // submitTitle
            EditorGUILayout.PropertyField(submitTitle);
            // submitBody
            EditorGUILayout.PropertyField(submitBody);

            EditorGUILayout.HelpBox($"{nameof(repoName)}:仓库名不应加空格\n" +
                $"{nameof(localPackagePathOfAssets)}:自Assets/后续写",
                MessageType.None);
        }

        void DrawUPMDataTab() {
            EditorGUILayout.PropertyField(name);
            EditorGUILayout.PropertyField(version);
            EditorGUILayout.PropertyField(displayName);
            EditorGUILayout.PropertyField(description);
            EditorGUILayout.PropertyField(unity);
            EditorGUILayout.PropertyField(author);
            EditorGUILayout.PropertyField(dependencies);
            if (GUILayout.Button("Auto")) {
                // name: com.author.repoName(lower)
                name.stringValue = $"com.{leadAuthor.stringValue}.{repoName.stringValue.ToLower()}";
                // version: 默认1.0.0
                if (string.IsNullOrEmpty(version.stringValue) || version.stringValue.Split('.').Length != 3) {
                    version.stringValue = "1.0.0";
                }
                // displayName: AbCdEf => Ab Cd Ef
                string dn = repoName.stringValue;
                StringBuilder sb = new StringBuilder(dn.Length);
                if (!string.IsNullOrEmpty(dn) && dn.Length > 0) {
                    sb.Append(char.ToUpper(dn[0]));
                    for (int i = 1; i < dn.Length; i++) {
                        if (char.IsLower(dn[i])) {
                            sb.Append(dn[i]);
                        }
                        else {
                            sb.Append($" {dn[i]}");
                        }
                    }
                }
                displayName.stringValue = sb.ToString();
                // unity: 版本号20xx.xx
                unity.stringValue = Application.unityVersion[..Application.unityVersion.LastIndexOf('.')];
                // author: 补作者
                SerializedProperty sp;
                if (author.arraySize == 0) {
                    author.InsertArrayElementAtIndex(0);
                    sp = author.GetArrayElementAtIndex(0);
                    sp.FindPropertyRelative(nameof(AuthorData.name)).stringValue = leadAuthor.stringValue;
                }
                // dependencies: 修正依赖版本格式
                if (dependencies.arraySize > 0) {
                    for (int i = 0; i < dependencies.arraySize; i++) {
                        sp = dependencies.GetArrayElementAtIndex(i);
                        if (sp.FindPropertyRelative(nameof(DependencyData.reference)).stringValue.Split('.').Length != 3) {
                            sp.FindPropertyRelative(nameof(DependencyData.reference)).stringValue = "1.0.0";
                        }
                    }
                }
            }
        }

        void DrawButton() {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Pull")) {
                bool ok = GitLocalService.Pull(_this.gitData, out string msg);
                EditorUtility.DisplayDialog("Pull", msg, "OK");
            }
            if (GUILayout.Button("Pull(Proxy)")) {
                bool ok = GitLocalService.PullProxy(_this.gitData, out string msg, true, _this.proxyPort);
                EditorUtility.DisplayDialog("Pull(Proxy)", msg, "OK");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Push")) {
                File.WriteAllText(Path.Combine(_this.gitData.DirPath, "package.json"), _this.upmData.ToJson());
                bool ok = GitLocalService.Push(_this.gitData, out string msg);
                EditorUtility.DisplayDialog("Push", msg, "OK");
            }
            if (GUILayout.Button("Push(Proxy)")) {
                File.WriteAllText(Path.Combine(_this.gitData.DirPath, "package.json"), _this.upmData.ToJson());
                bool ok = GitLocalService.PushProxy(_this.gitData, out string msg, _this.proxyPort);
                EditorUtility.DisplayDialog("Push(Proxy)", msg, "OK");
            }
            GUILayout.EndHorizontal();
        }
    }
}
