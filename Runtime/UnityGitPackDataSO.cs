using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Momos.UnityGitPack.Common {
    [CreateAssetMenu(fileName = "UnityGitPack Data", menuName = "Tools/MomosUnityGitPack/Create UnityGitPack Data")]
    public class UnityGitPackDataSO : ScriptableObject {
        public UserGitData gitData = new UserGitData();
        public UPMData upmData = new UPMData();
        public short proxyPort = 7890;
    }
}