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
        [SerializeField] private Mesh _mesh;
        [SerializeField] private Material _material;

        private void Start()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            EntityArchetype entityArchetype = entityManager.CreateArchetype(
                typeof(LevelComponent),
                typeof(Translation),
                typeof(RenderMesh),
                typeof(RenderBounds),
                typeof(LocalToWorld),
                typeof(MoveSpeedComponent)
            );
            // 此处的NativeArray仅用于给Entities赋值，所以使用Allocator的Temp存储类型
            NativeArray<Entity> entitiesArray = new NativeArray<Entity>(10000, Allocator.Temp);
            entityManager.CreateEntity(entityArchetype, entitiesArray);

            for (var i = 0; i < entitiesArray.Length; i++)
            {
                Entity entity = entitiesArray[i];
                entityManager.SetComponentData(entity, new LevelComponent {level = Random.Range(10, 20)});
                entityManager.SetComponentData(entity, new MoveSpeedComponent { moveSpeed = Random.Range(1f, 2f)});
                entityManager.SetComponentData(entity, new Translation
                {
                    Value = new Vector3(Random.Range(-8, 8), Random.Range(-5, 5))
                });
                entityManager.SetSharedComponentData(entity, new RenderMesh
                {
                    mesh = _mesh,
                    material = _material
                });
            }

            entitiesArray.Dispose();
        }
    }
}
