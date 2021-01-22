using System;
using System.Collections.Generic;
using E_A_DEMO_FISH_ALGORITHM.mono.component;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace E_A_DEMO_FISH_ALGORITHM.ecs
{
    public class ECSFishSpawner : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        [Header("鱼类制体")]
        public GameObject spawnerFish;

        public int GenerateCount = 100;
        public FishSpawnerData SpawnerData;

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(spawnerFish);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var fishPrefabEntity = conversionSystem.TryGetPrimaryEntity(spawnerFish);
            if (fishPrefabEntity == Entity.Null)
                throw new Exception($"Something went wrong while creating an Entity for the rig prefab: {fishPrefabEntity}");

            dstManager.AddComponentData(entity, new ECSFishSpawnerData
            {
                entity = fishPrefabEntity,
                GenerateRange = SpawnerData.GenerateRange,
                GenerateCount = GenerateCount,
                FishMinSpeed = SpawnerData.FishMinSpeed,
                FishMaxSpeed = SpawnerData.FishMaxSpeed
            });
        }
    }

    public struct ECSFishSpawnerData : IComponentData
    {
        public Entity entity;
        public float GenerateRange;
        public float GenerateCount;
        public float FishMinSpeed;
        public float FishMaxSpeed;
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class ECSFishSpawnerSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            CompleteDependency();
            Entities
                .WithAll<ECSFishSpawnerData>()
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach((Entity entity, in ECSFishSpawnerData spawner) =>
                {
                    for (int i = 0; i < spawner.GenerateCount; i++)
                    {
                        var fishInstance = EntityManager.Instantiate(spawner.entity);

                        // 随机位置
                        var generateRange = spawner.GenerateRange;
                        var randomPosition = new float3(Random.Range(-generateRange, generateRange),
                            Random.Range(-generateRange, generateRange),
                            Random.Range(-generateRange, generateRange));
                        EntityManager.AddComponentData(fishInstance, new Translation
                        {
                            Value = randomPosition
                        });

                        // 随机方向
                        EntityManager.AddComponentData(fishInstance, new Rotation
                        {
                            Value = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), 0)
                        });

                        // 随机速度
                        EntityManager.AddComponentData(fishInstance, new ECSFishMoveComponent
                        {
                            moveSpeed = Random.Range(spawner.FishMinSpeed, spawner.FishMaxSpeed)
                        });
                    }
                    EntityManager.DestroyEntity(entity);
                }).Run();
        }
    }
}
