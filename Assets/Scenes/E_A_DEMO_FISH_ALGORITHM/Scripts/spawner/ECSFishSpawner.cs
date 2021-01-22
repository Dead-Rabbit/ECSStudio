using System;
using System.Collections.Generic;
using E_A_DEMO_FISH_ALGORITHM.ecs.component;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace E_A_DEMO_FISH_ALGORITHM.ecs
{
    public class ECSFishSpawner : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        [Header("鱼类制体")]
        public GameObject spawnerFish;
        [Header("生成范围")]
        public float GenerateRange = 10;

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
                GenerateRange = GenerateRange,
                GenerateCount = GenerateCount,
                FishMinSpeed = SpawnerData.FishMinSpeed,
                FishMaxSpeed = SpawnerData.FishMaxSpeed,
                rotateSpeed = SpawnerData.rotateSpeed,
                keepDis = SpawnerData.keepDis,
                keepWeight = SpawnerData.keepWeight,
                targetCloseDistance = SpawnerData.targetCloseDistance,
                stopDistance = SpawnerData.stopDis
            });
        }
    }

    public struct ECSFishSpawnerData : IComponentData
    {
        public Entity entity;             // 记录Prefab对应的主要Entity
        public float GenerateRange;       // 生成Entity的范围
        public float GenerateCount;       // 生成数量
        public float FishMinSpeed;        // 最小速度
        public float FishMaxSpeed;        // 最大速度
        public float rotateSpeed;         // 旋转速度
        public float keepDis;             // 鱼之间距离
        public float keepWeight;          // 保持距离的权重
        public float targetCloseDistance; // 多少距离算离得太近
        public float stopDistance;        // 停止移动距离
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
                    float keepDistanceSquare = spawner.keepDis * spawner.keepDis;
                    float stopDistanceSquare = spawner.stopDistance * spawner.stopDistance;
                    for (int i = 0; i < spawner.GenerateCount; i++)
                    {
                        var fishInstance = EntityManager.Instantiate(spawner.entity);

                        // 随机位置
                        var generateRange = spawner.GenerateRange;
                        var randomPosition = new float3(Random.Range(-generateRange, generateRange),
                            Random.Range(-generateRange, generateRange),
                            Random.Range(-generateRange, generateRange));
                        var randomRotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), 0);
                        if (i == 0)
                        {
                            randomPosition = new float3(0, 0, 0);
                            randomRotation = Quaternion.Euler(0, 0, 1);
                        }
                        EntityManager.AddComponentData(fishInstance, new Translation
                        {
                            Value = randomPosition
                        });

                        // 随机方向
                        EntityManager.AddComponentData(fishInstance, new Rotation
                        {
                            Value = randomRotation
                        });

                        EntityManager.AddComponentData(fishInstance, new ECSFishMovementData
                        {
                            ID = i,
                            moveSpeed = Random.Range(spawner.FishMinSpeed, spawner.FishMaxSpeed),
                            rotateSpeed = spawner.rotateSpeed,
                            keepWeight = spawner.keepWeight,
                            keepDis = spawner.keepDis,
                            distanceSquare = keepDistanceSquare,
                            targetCloseDistance = spawner.targetCloseDistance,
                            stopDistance = spawner.stopDistance,
                            stopDistanceSquare = stopDistanceSquare
                        });

                        EntityManager.AddComponentData(fishInstance, new ECSFishMoveComponentData
                        {
                            speed = 0,
                            dir = Vector3.zero,
                            targetSpeed = 0,
                            currentSpeed = 0
                        });
                    }
                    EntityManager.DestroyEntity(entity);
                }).Run();
        }
    }
}
