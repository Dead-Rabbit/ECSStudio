using UnityEngine;

using Unity.Mathematics;

public class AnimationControllerInput : AnimationInputBase<AnimationControllerData>
{
    public float DirectionStep = 0.1f;
    public float SpeedStep = 0.1f;

    protected override void UpdateComponentData(ref AnimationControllerData data)
    {
        // 此处的修改Player为1代表了当前对Entity的Controller进行了操作
        // 当在AnimationController中的Update中进行更新时，会将其修改为0
        data.Player = 1;

        var deltaDir = Input.GetAxisRaw("Horizontal");
        var deltaSpeed = Input.GetAxisRaw("Vertical");

        data.Direction = math.clamp(data.Direction + deltaDir * DirectionStep, 0.0f, 4.0f);
        data.Speed = math.clamp(data.Speed + deltaSpeed * SpeedStep, 0.0f, 1.0f);
    }
}
