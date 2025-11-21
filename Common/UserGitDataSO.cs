using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Momos.UnityGitPack.Common {
    [CreateAssetMenu(menuName = "Tools/MomosUnityGitPack/Create User Git Settings")]
    public class UserGitDataSO : ScriptableObject {
        public UserGitData Data = new UserGitData();
    }
}