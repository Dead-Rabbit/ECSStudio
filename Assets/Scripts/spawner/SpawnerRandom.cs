using System;
using System.Collections.Generic;
using component.tags;
using Unity.Animation.Hybrid;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace mono
{
    public class SpawnerRandom : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {
        public GameObject         RigPrefab;
        public AnimationGraphBase GraphPrefab;

        public int GenerateNumberPerTime = 1;           // 每次生成数量
        public float GenerateRange = 10f;               // 生成位置（范围外）
        public int GenerateDeltaTime = 2;            // 生成时间间隔
        public UnitTags spawnerTag;                     // 生成Obj的Tag

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(RigPrefab);

            if (GraphPrefab != null)
            {
                GraphPrefab.DeclareReferencedPrefabs(referencedPrefabs);
            }
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var rigPrefab = conversionSystem.TryGetPrimaryEntity(RigPrefab);

            if (rigPrefab == Entity.Null)
                throw new Exception($"Something went wrong while creating an Entity for the rig prefab: {RigPrefab.name}");

            if (GraphPrefab != null)
            {
                var rigComponent = RigPrefab.GetComponent<RigComponent>();
                GraphPrefab.PreProcessData(rigComponent);
                GraphPrefab.AddGraphSetupComponent(rigPrefab, dstManager, conversionSystem);
            }

            dstManager.AddComponentData(entity, new RigSpawnerRandom
            {
                RigPrefab = rigPrefab,
                GeneratePerTime = GenerateNumberPerTime,
                GenerateDeltaTime = GenerateDeltaTime,
                GenerateRange = GenerateRange,
                typeTag = spawnerTag,
            });
        }
    }

    public struct RigSpawnerRandom : IComponentData
    {
        public Entity RigPrefab;
        public int GeneratePerTime;
        public int GenerateDeltaTime;
        public float GenerateRange;
        public UnitTags typeTag;
    }

    public class RigSpawnerRandomSystem : SystemBase
    {
        public float curExecuteTime = 0;

        protected override void OnUpdate()
        {
            CompleteDependency();
            Entities
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach((Entity e, ref RigSpawnerRandom spawner) =>
                {
                    if (curExecuteTime <= 0)
                    {
                        curExecuteTime = spawner.GenerateDeltaTime;

                        var rigInstance = EntityManager.Instantiate(spawner.RigPrefab);
                        var translation = new float3(0, 0, 0);

                        EntityManager.SetComponentData(rigInstance, new Translation { Value = translation });
                        switch (spawner.typeTag)
                        {
                            case UnitTags.ZOMBIE:
                                EntityManager.AddComponent<ZombieUnitType>(rigInstance);
                                break;
                            case UnitTags.SOLIDER:
                                EntityManager.AddComponent<SoliderUnitType>(rigInstance);
                                break;
                        }
                    }
                    curExecuteTime -= Time.DeltaTime;
                }).Run();
        }
    }
}
