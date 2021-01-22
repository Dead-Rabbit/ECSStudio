using E_A_DEMO_FISH_ALGORITHM.ecs.component;
using Unity.Entities;
using UnityEngine;

namespace E_A_DEMO_FISH_ALGORITHM.ecs.system
{
    public class ECSTargetConvert : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new ECSTargetComponent());
        }
    }
}
