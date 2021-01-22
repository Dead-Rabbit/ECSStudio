using E_A_DEMO_FISH_ALGORITHM.ecs.component;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace E_A_DEMO_FISH_ALGORITHM.ecs.system
{
    public class EFSFishMoveSystem : SystemBase
    {
        private EntityQuery fishPosQuery;
        private EntityQuery targetPosQuery;

        protected override void OnCreate()
        {
            base.OnCreate();
            fishPosQuery = GetEntityQuery(
                ComponentType.ReadOnly<ECSFishMoveComponentData>(),
                ComponentType.ReadOnly<Translation>()
            );
            targetPosQuery = GetEntityQuery(
                ComponentType.ReadOnly<Translation>());
        }

        // [BurstCompile]
        protected override void OnUpdate()
        {
            var fishPosArray =
                fishPosQuery.ToComponentDataArrayAsync<Translation>(Allocator.TempJob, out var findFishPosJobHandle);
            var fishMovementArray =
                fishPosQuery.ToComponentDataArrayAsync<ECSFishMoveComponentData>(Allocator.TempJob,
                    out var findFishMovementJobHandle);
            var targetMomentArray =
                targetPosQuery.ToComponentDataArrayAsync<Translation>(Allocator.TempJob,
                    out var findTargetPosJobHandle);
            Dependency = JobHandle.CombineDependencies(Dependency, findFishPosJobHandle, findFishMovementJobHandle);
            Dependency = JobHandle.CombineDependencies(Dependency, findTargetPosJobHandle);

            Dependency = Entities
                .WithDisposeOnCompletion(fishPosArray)
                .WithDisposeOnCompletion(fishMovementArray)
                .WithDisposeOnCompletion(targetMomentArray)
                .ForEach((Entity e, int entityInQueryIndex, ref Translation translation, ref Rotation rotation, in ECSFishMoveComponentData movementData) =>
                {
                    if (targetMomentArray.Length == 0)
                    {
                        return;
                    }

                    // 当前点到目标点的距离
                    Vector3 dis = targetMomentArray[0].Value - translation.Value;
                    Vector3 dir = dis.normalized;

                    // Debug.Log($"===================From:{movementData.ID}");
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
                            Vector3 disToOther = (Vector3)(translation.Value - fishPosArray[i].Value);            // 距离
                            v1 += disToOther.normalized * (1 - disToOther.magnitude / movementData.keepDis);    // 查看与周围成员的距离
                            v2 += fishMovementArray[i].myMovement;                                                  // 查看周围成员移动方向
                            // Debug.Log($"Find Fish{fishMovementArray[i].ID} Pos {fishPosArray[i].Value}");
                        }
                    }
                    // 周围成员的信息
                    Vector3 aroundMemberInfo = v1.normalized * movementData.keepWeight + v2.normalized;
                }).Schedule(Dependency);
        }
    }
}
