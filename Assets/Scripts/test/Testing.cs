using component;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Random = UnityEngine.Random;
using Unity.Rendering;

namespace test
{
    public class Testing : MonoBehaviour
    {
        private void Start()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            EntityArchetype entityArchetype = entityManager.CreateArchetype(
                typeof(LevelComponent),
                typeof(Translation),
                typeof(RenderMesh)
                );

            // 此处的NativeArray仅用于给Entities赋值，所以使用Allocator的Temp存储类型
            NativeArray<Entity> entitiesArray = new NativeArray<Entity>(1, Allocator.Temp);
            entityManager.CreateEntity(entityArchetype, entitiesArray);
            for (var i = 0; i < entitiesArray.Length; i++) {
                Entity entity = entitiesArray[i];
                entityManager.SetComponentData(entity, new LevelComponent{level = Random.Range(10, 20)});
            }

            entitiesArray.Dispose();
        }
    }
}