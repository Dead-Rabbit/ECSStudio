using component;
using UnityEngine;
using Unity.Entities;

namespace test
{
    public class Testing : MonoBehaviour
    {
        private void Start()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity entity = entityManager.CreateEntity(typeof(LevelComponent));
            
            entityManager.SetComponentData(entity, new LevelComponent{level = 10});
        }
    }
}