using component;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace system
{
    public class MoveSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref Translation translation, ref MoveSpeedComponent moveSpeedComponent) =>
            {
                translation.Value.y += moveSpeedComponent.moveSpeed * Time.DeltaTime;
                if (translation.Value.y > 5f)
                {
                    moveSpeedComponent.moveSpeed = -math.abs(moveSpeedComponent.moveSpeed);
                }

                if (translation.Value.y < -5f)
                {
                    moveSpeedComponent.moveSpeed = math.abs(moveSpeedComponent.moveSpeed);
                }
            });
        }
    }
}
