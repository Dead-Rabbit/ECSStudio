using E_A_DEMO_FISH_ALGORITHM.ecs.component;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace E_A_DEMO_FISH_ALGORITHM.ecs.system
{
    [UpdateAfter(typeof(ECSFishCalculateMovementSystem))]
    public class ECSFishMoveSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Dependency = Entities
                .ForEach((Entity e, int entityInQueryIndex, ref Translation translation, ref Rotation rotation,
                    ref ECSFishMovementData movementData, in ECSFishMoveComponentData moveAct) =>
                {
                    var _dir = moveAct.dir;
                    var _speed = moveAct.speed;

                    Vector3 finialDirection = _dir.normalized;
                    float finialSpeed = _speed, finialRotate = 0;
                    // float rotateDir = Vector3.Dot(finialDirection, transform.right);
                    // float forwardDir = Vector3.Dot(finialDirection, transform.forward);
                    //
                    // if (forwardDir < 0)
                    // {
                    //     rotateDir = Mathf.Sign(rotateDir);
                    // }
                    // if (forwardDir < -0.2f)
                    // {
                    //     finialSpeed = Mathf.Lerp(currentSpeed, -_speed * 8, 4 * Time.deltaTime);
                    // }
                    //
                    // //——————————防抖
                    // if (forwardDir < 0.98f)
                    // {
                    //     finialRotate = Mathf.Clamp(rotateDir * 180, -rotateSpeed, rotateSpeed);
                    // }
                    //
                    // finialSpeed *= Mathf.Clamp01(_dir.magnitude);
                    // finialSpeed *= Mathf.Clamp01(1 - Mathf.Abs(rotateDir) * 0.8f);
                    //
                    // transform.Translate(Vector3.left * finialSpeed * Time.deltaTime);
                    // transform.Rotate(Vector3.forward * finialRotate * Time.deltaTime);
                    //
                    // currentSpeed = finialSpeed;
                    // myMovement = _dir * finialSpeed;
                }).Schedule(Dependency);
        }
    }
}
