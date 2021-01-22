using E_A_DEMO_FISH_ALGORITHM.ecs.component;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace E_A_DEMO_FISH_ALGORITHM.ecs.system
{
    [UpdateAfter(typeof(ECSFishCalculateMovementSystem))]
    public class ECSFishMoveSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var DeltaTime = Time.DeltaTime;

            Dependency = Entities
                .ForEach((Entity e, int entityInQueryIndex, ref Translation translation, ref Rotation rotation,
                    ref ECSFishMovementData movementData, ref ECSFishMoveComponentData moveAct, in LocalToWorld localToWorld) =>
                {
                    var _dir = moveAct.dir;
                    var _speed = moveAct.speed;

                    Vector3 finialDirection = _dir.normalized;
                    float finialSpeed = _speed, finialRotate = 0;
                    float rotateDir = Vector3.Dot(finialDirection, localToWorld.Right);
                    float forwardDir = Vector3.Dot(finialDirection, localToWorld.Forward);

                    if (forwardDir < 0)
                    {
                        rotateDir = Mathf.Sign(rotateDir);
                    }
                    if (forwardDir < -0.2f)
                    {
                        finialSpeed = Mathf.Lerp(moveAct.currentSpeed, -_speed * 8, 4 * DeltaTime);
                    }

                    //——————————防抖
                    if (forwardDir < 0.98f)
                    {
                        finialRotate = Mathf.Clamp(rotateDir * 180, -movementData.rotateSpeed, movementData.rotateSpeed);
                    }

                    finialSpeed *= Mathf.Clamp01(_dir.magnitude);
                    finialSpeed *= Mathf.Clamp01(1 - Mathf.Abs(rotateDir) * 0.8f);

                    // transform.Translate(Vector3.left * finialSpeed * DeltaTime);
                    float3 trans = translation.Value + math.left() * finialSpeed * DeltaTime;
                    translation.Value = trans;
                    // transform.Rotate(Vector3.forward * finialRotate * DeltaTime);
                    rotation.Value *= Quaternion.Euler(Vector3.forward * finialRotate * DeltaTime);

                    moveAct.currentSpeed = finialSpeed;
                    movementData.myMovement = _dir * finialSpeed;
                }).Schedule(Dependency);
        }
    }
}
