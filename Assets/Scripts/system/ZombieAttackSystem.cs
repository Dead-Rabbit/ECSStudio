using component.tags;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

namespace system
{
    public class ZombieAttackSystem : SystemBase
    {
        // EndSimulationEntityCommandBufferSystem endSimulationEcbSystem;
        private EntityQuery soliderQuery;
        float thresholdDistance = 2f;

        protected override void OnCreate()
        {
            base.OnCreate();
            // endSimulationEcbSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            soliderQuery = GetEntityQuery(
                ComponentType.ReadOnly<SoliderUnitType>(),
                ComponentType.ReadOnly<Translation>());
        }

        protected override void OnUpdate()
        {
            // var soliderPos = soliderQuery.ToComponentDataArrayAsync<Translation>(Allocator.TempJob, out var jobHandle1);
            // Dependency = JobHandle.CombineDependencies(Dependency, jobHandle1);
            //
            // Dependency = Entities
            //     .WithAll<ZombieUnitType>()
            //     .WithDisposeOnCompletion(soliderPos)
            //     .ForEach((ref Rotation rotation, in Translation zombiePos) =>
            //     {
            //         // 遍历寻找距离最近的Solider Index
            //         var minDis = float.MaxValue;
            //         var minPos = float3.zero;
            //         for (int i = 0; i < soliderPos.Length; i++)
            //         {
            //             var distance = math.distancesq(soliderPos[i].Value, zombiePos.Value);
            //             if (distance < minDis)
            //             {
            //                 minPos = soliderPos[i].Value;
            //             }
            //         }
            //
            //     }).Schedule(Dependency);

        }
    }
}
