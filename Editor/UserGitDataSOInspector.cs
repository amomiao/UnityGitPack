using Momos.UnityGitPack.Common;
using UnityEditor;
using UnityEngine;

namespace Momos.UnityGitPackEditor.Common {
    using static GitHubApiService.E_CreateRepoState;

    [CustomEditor(typeof(UserGitDataSO))]
    public class UserGitDataSOInspector : Editor {
        UserGitDataSO _this;

        private void OnEnable() {
            _this = (UserGitDataSO)this.target;
        }

        public override void OnInspectorGUI() {
            _this.Data ??= new UserGitData();
            base.OnInspectorGUI();

            if (GUILayout.Button("TokenWeb")) {
                Application.OpenURL("https://github.com/settings/tokens?type=beta");
            }

            if (GUILayout.Button("Check Token")) {
                bool ok = GitHubApiService.CheckToken(_this.Data, out string msg);
                EditorUtility.DisplayDialog("Check Token Result", msg, "OK");
            }

            if (GUILayout.Button("Create Repo")) {
                var result = GitHubApiService.CreateRepo(_this.Data);

                EditorUtility.DisplayDialog("Create Repo Result",
                    $"Result: {result.State}\nMessage: {result.Message}", "OK");
            }

            if (GUILayout.Button("Push")) {
                bool ok = GitLocalService.Push(_this.Data, out string msg);
                EditorUtility.DisplayDialog("Push", msg, "OK");
            }

            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }
    }
}
