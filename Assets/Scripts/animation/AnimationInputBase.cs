using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public abstract class AnimationInputBase : MonoBehaviour
{
    protected List<Entity> m_RigEntities;

    public abstract Entity ActiveEntity { get; }

    public virtual void RegisterEntity(Entity entity)
    {
        if (m_RigEntities == null)
        {
            m_RigEntities = new List<Entity>();
        }

        m_RigEntities.Add(entity);
    }
}

public abstract class AnimationInputBase<T> : AnimationInputBase
    where T : struct, ISampleData
{
    [HideInInspector] public int m_ActiveEntityIndex;

    private RigSpawnerByCountSystem spawnerSystem;

    void Awake()
    {
        spawnerSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<RigSpawnerByCountSystem>();
        spawnerSystem.RegisterInput(this);
    }

    // 自定义追加控制Data
    public override void RegisterEntity(Entity entity)
    {
        base.RegisterEntity(entity);
        spawnerSystem.EntityManager.AddComponent<T>(entity);
    }

    void Update()
    {
        if (m_RigEntities?.Count > 0)
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // Select next rig entity
            if (Input.GetKeyDown(KeyCode.N))
            {
                m_ActiveEntityIndex = (m_ActiveEntityIndex + 1) % m_RigEntities.Count;
            }

            for (var i = 0; i < m_RigEntities.Count; i++)
            {
                var currentEntity = m_RigEntities[i];
                if (!entityManager.HasComponent<T>(currentEntity))
                    continue;

                var data = entityManager.GetComponentData<T>(currentEntity);
                if (UpdateComponentData(ref data, i, currentEntity))
                {
                    entityManager.SetComponentData(currentEntity, data);
                }
            }
        }
    }

    public override Entity ActiveEntity =>
        (m_RigEntities?.Count > 0) ? m_RigEntities[m_ActiveEntityIndex] : Entity.Null;

    protected abstract bool UpdateComponentData(ref T data, int index, Entity entity);
}
