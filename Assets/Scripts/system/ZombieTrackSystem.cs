using component.tags;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace system
{
    public class ZombieTrackSystem : SystemBase
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
            float DeltaTime = Time.DeltaTime;
            var soliderPos = soliderQuery.ToComponentDataArrayAsync<Translation>(Allocator.TempJob, out var jobHandle1);
            Dependency = JobHandle.CombineDependencies(Dependency, jobHandle1);

            Dependency = Entities
                .WithAll<ZombieUnitType, Moving>()
                .WithDisposeOnCompletion(soliderPos)
                .ForEach((Entity entity, int entityInQueryIndex, ref ChangeClipPlayerData clipData, ref AiChangeClipSampleData aiData, ref Rotation rotation, in Translation zombiePos) =>
                {
                    // 遍历寻找距离最近的Solider Index
                    var minDis = float.MaxValue;
                    var minPos = float3.zero;
                    for (int i = 0; i < soliderPos.Length; i++)
                    {
                        var distance = math.distancesq(soliderPos[i].Value, zombiePos.Value);
                        if (distance < minDis)
                        {
                            minPos = soliderPos[i].Value;
                        }
                    }

                    var minDistance = math.distancesq(minPos, zombiePos.Value);
                    if (minDistance < 1f)
                    {
                        // 切换攻击动作
                        ecb.RemoveComponent<Moving>(entityInQueryIndex, entity);
                        aiData.ifModify = true;
                        aiData.index = 1;
                        return;
                    }

                    float3 towardVector = minPos - zombiePos.Value;

                    Quaternion finalRotation = Quaternion.LookRotation((Vector3)towardVector, math.up());
                    finalRotation = Quaternion.Euler(finalRotation.eulerAngles + clipData.clipRotation);
                    rotation.Value = finalRotation;
                }).Schedule(Dependency);

            endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
