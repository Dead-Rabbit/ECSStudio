using E_A_DEMO_FISH_ALGORITHM.ecs.component;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace E_A_DEMO_FISH_ALGORITHM.ecs.system
{
    public class ECSFishCalculateMovementSystem : SystemBase
    {
        private EntityQuery fishPosQuery;
        private EntityQuery targetPosQuery;

        protected override void OnCreate()
        {
            base.OnCreate();
            fishPosQuery = GetEntityQuery(
                ComponentType.ReadOnly<ECSFishMovementData>(),
                ComponentType.ReadOnly<Translation>()
            );
            targetPosQuery = GetEntityQuery(
                ComponentType.ReadOnly<ECSTargetComponent>(),
                ComponentType.ReadOnly<Translation>());
        }

        // [BurstCompile]
        protected override void OnUpdate()
        {
            NativeArray<Translation> fishPosArray =
                fishPosQuery.ToComponentDataArrayAsync<Translation>(Allocator.TempJob, out var findFishPosJobHandle);
            NativeArray<ECSFishMovementData> fishMovementArray =
                fishPosQuery.ToComponentDataArrayAsync<ECSFishMovementData>(Allocator.TempJob,
                    out var findFishMovementJobHandle);
            NativeArray<Translation> targetMomentArray =
                targetPosQuery.ToComponentDataArrayAsync<Translation>(Allocator.TempJob,
                    out var findTargetPosJobHandle);
            Dependency = JobHandle.CombineDependencies(Dependency, findFishPosJobHandle, findFishMovementJobHandle);
            Dependency = JobHandle.CombineDependencies(Dependency, findTargetPosJobHandle);

            var DeltaTime = Time.DeltaTime;

            Dependency = Entities
                .WithDisposeOnCompletion(fishPosArray)
                .WithDisposeOnCompletion(fishMovementArray)
                .WithDisposeOnCompletion(targetMomentArray)
                .ForEach((Entity e, int entityInQueryIndex, ref Translation translation,
                    ref Rotation rotation, ref ECSFishMoveComponentData moveAct, in ECSFishMovementData movementData) =>
                    {
                        if (targetMomentArray.Length == 0)
                        {
                            return;
                        }

                        // 当前点到目标点的距离
                        var targetPos = targetMomentArray[0].Value;

                        Vector3 dis =  targetPos - translation.Value;
                        Vector3 dir = dis.normalized;

                        //重新计算目的地距离权重
                        if (dis.magnitude < movementData.targetCloseDistance)
                        {
                            dir *= dis.magnitude / movementData.targetCloseDistance;
                        }

                        // Debug.Log($"Run Calculate In:{movementData.ID}");
                        Vector3 v1 = Vector3.zero;
                        Vector3 v2 = Vector3.zero;
                        for (int i = 0; i < fishPosArray.Length; i++)
                        {
                            if (movementData.ID == fishMovementArray[i].ID)
                            {
                                // 此处Movement组件的Index来自fishPos，如果证明每次都能扫到自己，说明Movement的Index和Pos的Index是一回事
                                // Debug.Log($"Ignore self:{fishMovement[i].ID}?");
                                continue;
                            }

                            if (Vector3.SqrMagnitude(translation.Value - fishPosArray[i].Value) < movementData.distanceSquare)
                            {
                                Vector3 disToOther = (Vector3)(translation.Value - fishPosArray[i].Value);        // 距离
                                v1 += disToOther.normalized * (1 - disToOther.magnitude / movementData.keepDis); // 查看与周围成员的距离
                                v2 += fishMovementArray[i].myMovement;                                              // 查看周围成员移动方向
                                // Debug.Log($"Find Fish{fishMovementArray[i].ID} Pos {fishPosArray[i].Value}");
                            }
                        }
                        // 周围成员的信息
                        dir += v1.normalized * movementData.keepWeight + v2.normalized;

                        //计算移动速度
                        if (dis.sqrMagnitude < movementData.stopDistanceSquare)
                        {
                            moveAct.targetSpeed = 0;
                        }
                        else
                        {
                            moveAct.targetSpeed = movementData.moveSpeed;
                        }

                        moveAct.speed = Mathf.Lerp(moveAct.speed, moveAct.targetSpeed, 2 * DeltaTime);

                        rotation.Value = Quaternion.FromToRotation(Vector3.right, -dir);
                        moveAct.dir = dir;
                    }).Schedule(Dependency);
        }
    }
}
