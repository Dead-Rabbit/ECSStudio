using Unity.Entities;
using UnityEngine;

namespace E_A_DEMO_FISH_ALGORITHM.ecs.component
{
    public struct ECSFishMovementData : IComponentData
    {
        public int ID;
        public float moveSpeed;           // 移动速度
        public float rotateSpeed;         // 旋转速度
        public float keepDis;             // 保持的距离
        public float distanceSquare;      // 距离判断
        public float keepWeight;          // 保持距离的权重
        public float targetCloseDistance; // 多少距离算离得太近
        public float stopDistance;        // 停止移动距离
        public float stopDistanceSquare;  // 判断停止移动距离
        public Vector3 myMovement;
    }

    public struct ECSFishMoveComponentData : IComponentData
    {
        public Vector3 dir;
        public float speed;
        public float currentSpeed;
    }
}
