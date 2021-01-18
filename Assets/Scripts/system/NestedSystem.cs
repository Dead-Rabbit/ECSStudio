// using component.tags;
// using Unity.Transforms;
// using Unity.Mathematics;
// using Unity.Entities;
// using Unity.Collections;
// using Unity.Jobs;
// using UnityEngine;
//
/**
 * 嵌套遍历学习
 */
// namespace system
// {
//     public class DestructionSystem : SystemBase
//     {
//         EndSimulationEntityCommandBufferSystem endSimulationEcbSystem;
//         private EntityQuery soliderQuery;
//         float thresholdDistance = 2f;
//
//         protected override void OnCreate()
//         {
//             base.OnCreate();
//             endSimulationEcbSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
//             soliderQuery = GetEntityQuery(
//                 ComponentType.ReadOnly<SoliderUnitType>(),
//                 ComponentType.ReadOnly<Translation>());
//         }
//
//         protected override void OnUpdate()
//         {
//             var ecb = endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
//             var soliderPos = soliderQuery.ToComponentDataArrayAsync<Translation>(Allocator.TempJob, out var jobHandle1);
//             Dependency = JobHandle.CombineDependencies(Dependency, jobHandle1);
//
//             // float3 playerPosition = (float3)GameManager.GetPlayerPosition();
//             float dstSq = thresholdDistance * thresholdDistance;
//
//             Dependency = Entities
//                 .WithAll<ZombieUnitType>()
//                 .WithDisposeOnCompletion(soliderPos)
//                 // .WithDeallocateOnJobCompletion(soliderPos)
//                 .ForEach((Entity zombie, ref Rotation Rotation, in Translation zombiePos) =>
//                 {
//                     // playerPosition.y = enemyPos.Value.y;
//                     //
//                     // if (math.distancesq(enemyPos.Value, playerPosition) <= dstSq)
//                     // {
//                     //     ecb.DestroyEntity(entityInQueryIndex, enemy);
//                     // }
//                     // else
//                     // {
//                     for (int i = 0; i < soliderPos.Length; i++)
//                     {
//                         var distance = math.distancesq(soliderPos[i].Value, zombiePos.Value);
//                         Debug.Log(distance);
//                     }
//                     // }
//                 }).ScheduleParallel(Dependency);
//
//             endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
//         }
//     }
// }
