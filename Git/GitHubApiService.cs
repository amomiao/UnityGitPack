using Momos.UnityGitPack.Common;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Unity.VisualScripting;

namespace Momos.UnityGitPackEditor.Common {
    public static class GitHubApiService {
        public enum E_CreateRepoState {
            Success,
            Default,
            AlreadyExists,
            Error
        }
        public class Payload {
            public string name;
            public string description;
            public bool @private;
            public bool auto_init;
            public Payload(UserGitData data) {
                name = data.repoName;
                description = data.repoDescription;
                @private = data.privateRepo;
                auto_init = true;
            }
        }

        private static HttpClient CreateClient(UserGitData data) {
            var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("UnityGitPack", "1.0"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", data.accessToken);
            return client;
        }

        #region Token Check
        public static bool CheckToken(UserGitData data, out string message) {
            try {
                var client = CreateClient(data);
                var res = client.GetAsync($"{data.apiBaseUrl}/user").Result;

                string json = res.Content.ReadAsStringAsync().Result;

                if (res.IsSuccessStatusCode) {
                    message = "Token is valid.";
                    return true;
                }
                else {
                    message = "Token invalid:\n" + json;
                    return false;
                }
            }
            catch (System.Exception ex) {
                message = "Exception: " + ex.Message;
                return false;
            }
        }
        #endregion

        #region Create Repo
        public struct CreateRepoResult {
            public string Message { get; set; }
            public E_CreateRepoState State { get; set; }
        }

        public static CreateRepoResult CreateRepo(UserGitData data) {
            try {
                if (string.IsNullOrEmpty(data.repoName)) {
                    return new CreateRepoResult() {
                        State = E_CreateRepoState.Error,
                        Message = "Name is Empty!"
                    };
                }

                var client = CreateClient(data);

                var payload = new Payload(data);
                string jsonPayload = UnityEngine.JsonUtility.ToJson(payload);

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var res = client.PostAsync($"{data.apiBaseUrl}/user/repos", content).Result;
                string json = res.Content.ReadAsStringAsync().Result;

                if (res.IsSuccessStatusCode) {
                    return new CreateRepoResult {
                        State = E_CreateRepoState.Success,
                        Message = json
                    };
                }

                if (json.Contains("name already exists")) {
                    return new CreateRepoResult {
                        State = E_CreateRepoState.AlreadyExists,
                        Message = "Repository already exists.\n\n" + json
                    };
                }

                return new CreateRepoResult {
                    State = E_CreateRepoState.Error,
                    Message = "Error:\n" + json
                };
            }
            catch (System.Exception ex) {
                return new CreateRepoResult {
                    State = E_CreateRepoState.Error,
                    Message = "Exception:\n" + ex.Message
                };
            }
        }
        #endregion
    }
}
