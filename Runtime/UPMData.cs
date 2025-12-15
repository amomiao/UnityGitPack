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

        [Serializable]
        public class UPMDependency {
            /// <summary> 包名 如com.unity.textmeshpro </summary>
            public string name;

            /// <summary> 版本 / 地址： 
            /// 1.0.0 #如果是官方插件
            /// https://github.com/user/repo.git#v1.2.0 #github上的插件
            /// file:../SomePackage #本地文件
            /// </summary>
            public string reference;
        }

        public string name;
        public string version;
        public string displayName;
        public string description;
        public string unity;
        public List<AuthorData> author = new List<AuthorData>() { new AuthorData() };

        public List<UPMDependency> dependencies = new List<UPMDependency>();
    }
}