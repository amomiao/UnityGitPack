using Momos.UnityGitPack.Common;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Momos.UnityGitPackEditor.Common {
    [CustomEditor(typeof(UnityGitPackDataSO))]
    public class UserGitDataSOInspector : Editor {
        UnityGitPackDataSO _this;

        private void OnEnable() {
            _this = (UnityGitPackDataSO)this.target;
        }

        public override void OnInspectorGUI() {
            _this.gitData ??= new UserGitData();
            base.OnInspectorGUI();

            if (GUILayout.Button("TokenWeb")) {
                Application.OpenURL("https://github.com/settings/tokens?type=beta");
            }

            if (GUILayout.Button("Check Token")) {
                bool ok = GitHubApiService.CheckToken(_this.gitData, out string msg);
                EditorUtility.DisplayDialog("Check Token Result", msg, "OK");
            }

            if (GUILayout.Button("Create Repo")) {
                var result = GitHubApiService.CreateRepo(_this.gitData);

                EditorUtility.DisplayDialog("Create Repo Result",
                    $"Result: {result.State}\nMessage: {result.Message}", "OK");
            }

            if (GUILayout.Button("Push")) {
                File.WriteAllText(Path.Combine(_this.gitData.DirPath, "package.json"), JsonUtility.ToJson(_this.upmData));
                bool ok = GitLocalService.Push(_this.gitData, out string msg);
                EditorUtility.DisplayDialog("Push", msg, "OK");
            }

            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }
    }
}
