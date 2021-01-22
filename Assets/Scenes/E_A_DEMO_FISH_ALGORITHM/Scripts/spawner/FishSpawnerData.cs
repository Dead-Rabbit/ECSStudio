using UnityEngine;

namespace E_A_DEMO_FISH_ALGORITHM
{
    public class FishSpawnerData : MonoBehaviour
    {
        [Header("随机移动速度")]
        public float FishMinSpeed = 40;
        public float FishMaxSpeed = 55;

        [Header("组中成员始终保持的距离")] public float keepDis = 8;
        [Header("组中成员始终保持的距离的权重")] public float keepWeight = 0.6f;
        [Header("多少距离算离得太近")] public float targetCloseDistance = 0.5f;
        [Header("停止移动的距离")] public float stopDis = 0.001f;
        [Header("旋转速度")] public float rotateSpeed;

        public int GenerateFishNums = 0;
    }
}
