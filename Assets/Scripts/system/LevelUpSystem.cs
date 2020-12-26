using component;
using Unity.Entities;

namespace system
{
    public class LevelUpSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref LevelComponent levelComponent) =>
            {
                levelComponent.level += 1f * Time.DeltaTime;
            });
        }
    }
}