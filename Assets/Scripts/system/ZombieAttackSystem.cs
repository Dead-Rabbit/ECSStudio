using component.tags;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace system
{
    public class ZombieAttackSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem endSimulationEcbSystem;
        private EntityQuery soliderQuery;
        float thresholdDistance = 2f;

        protected override void OnCreate()
        {
            base.OnCreate();
            endSimulationEcbSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            soliderQuery = GetEntityQuery(
                ComponentType.ReadOnly<SoliderUnitType>(),
                ComponentType.ReadOnly<Translation>());
        }

        protected override void OnUpdate()
        {
            var ecb = endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
            var soliderPos = soliderQuery.ToComponentDataArrayAsync<Translation>(Allocator.TempJob, out var jobHandle1);
            Dependency = JobHandle.CombineDependencies(Dependency, jobHandle1);

            // float3 playerPosition = (float3)GameManager.GetPlayerPosition();
            float dstSq = thresholdDistance * thresholdDistance;

            Dependency = Entities
                .WithAll<ZombieUnitType>()
                .WithDisposeOnCompletion(soliderPos)
                // .WithDeallocateOnJobCompletion(soliderPos)
                .ForEach((Entity zombie, ref Rotation rotation, in Translation zombiePos) =>
                {
                    // playerPosition.y = enemyPos.Value.y;
                    //
                    // if (math.distancesq(enemyPos.Value, playerPosition) <= dstSq)
                    // {
                    //     ecb.DestroyEntity(entityInQueryIndex, enemy);
                    // }
                    // else
                    // {
                    Debug.Log(rotation);
                    // 遍历寻找距离最近的Solider Index
                    for (int i = 0; i < soliderPos.Length; i++)
                    {
                        var distance = math.distancesq(soliderPos[i].Value, zombiePos.Value);
                        // Debug.Log(distance);
                    }

                    Quaternion.RotateTowards(rotation.Value, Quaternion.identity, 10);

                    // }
                }).ScheduleParallel(Dependency);

            endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
