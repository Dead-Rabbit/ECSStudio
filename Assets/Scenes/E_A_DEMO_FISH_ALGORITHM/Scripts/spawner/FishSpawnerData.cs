using UnityEngine;

namespace E_A_DEMO_FISH_ALGORITHM
{
    public class FishSpawnerData : MonoBehaviour
    {
        [Header("生成范围")]
        public float GenerateRange = 10;

        [Header("随机移动速度")]
        public float FishMinSpeed = 40;
        public float FishMaxSpeed = 55;
    }
}
