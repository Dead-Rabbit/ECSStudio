using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using System;
using System.Collections.Generic;
// #if UNITY_EDITOR
using component.tags;
using Unity.Animation.Hybrid;

public class SpawnerByCount : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    public GameObject         RigPrefab;
    public AnimationGraphBase GraphPrefab;

    public int CountX = 100;
    public int CountY = 100;

    public Vector2 startPosition;
    public UnitTags spawnerTag;

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

        dstManager.AddComponentData(entity, new RigSpawnerByCount
        {
            RigPrefab = rigPrefab,
            CountX = CountX,
            CountY = CountY,
            startPosition = new float3(startPosition.x, 0, startPosition.y),
            typeTag = spawnerTag,
        });
    }
}
// #endif

public struct RigSpawnerByCount : IComponentData
{
    public Entity RigPrefab;
    public int CountX;
    public int CountY;
    public float3 startPosition;
    public UnitTags typeTag;
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class RigSpawnerByCountSystem : SystemBase
{
    AnimationInputBase m_Input;

    public void RegisterInput(AnimationInputBase input)
    {
        m_Input = input;
    }

    protected override void OnUpdate()
    {
        CompleteDependency();

        Entities
            .WithoutBurst()
            .WithStructuralChanges()
            .ForEach((Entity e, ref RigSpawnerByCount spawner) =>
            {
                for (var x = 0; x < spawner.CountX; x++)
                {
                    for (var y = 0; y < spawner.CountY; ++y)
                    {
                        var rigInstance = EntityManager.Instantiate(spawner.RigPrefab);
                        var translation = new float3(spawner.startPosition.x + x * 1.3F, spawner.startPosition.y, spawner.startPosition.z + y * 1.3F);

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

                        if (m_Input != null)
                            m_Input.RegisterEntity(rigInstance);
                    }
                }

                EntityManager.DestroyEntity(e);
            }).Run();
    }
}
