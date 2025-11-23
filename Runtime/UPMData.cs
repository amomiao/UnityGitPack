using System;
using System.Collections.Generic;
namespace Momos.UnityGitPack.Common {
    [Serializable]
    public class UPMData {
        [Serializable]
        public class AuthorData {
            public string name;
            public string email;
        }

        public string name;
        public string version;
        public string displayName;
        public string description;
        public string unity;
        public List<AuthorData> author = new List<AuthorData>() { new AuthorData() };
    }
}