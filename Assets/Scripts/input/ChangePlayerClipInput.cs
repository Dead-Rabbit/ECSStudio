using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ChangePlayerClipInput : AnimationInputBase<InputChangeClipSampleData>
{
    private List<int> animationIndex = new List<int>();

    public override void RegisterEntity(Entity entity)
    {
        base.RegisterEntity(entity);
        animationIndex.Add(0);
    }

    protected override bool UpdateComponentData(ref InputChangeClipSampleData data, int index, Entity entity)
    {
        UpdateParameters(index);
        UpdateText();
        if (data.index != animationIndex[index])
        {
            Debug.Log("Modify Entity: " + entity);
            data.ifModify = true;
            data.index = animationIndex[index];
            return true;
        }

        return false;
    }

    private void UpdateParameters(int index)
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            // 此处的2为定制的动画的总数，目前为2个，考虑如何将2设置到对应的Buff中
            animationIndex[index] = (animationIndex[index] + 1) % 2;
        }
    }

    private void UpdateText()
    {
        // MenuText.text =
        //     $"Down-Up : Speed (current = {Speed})\n" +
        //     $"Left-Right : ClipTime ({ClipTime})\n" +
        //     $"P : Toggle Play ({Play})\n" +
        //     $"N : Toggle NormalizedTime ({NormalizedTime})\n" +
        //     $"T : Toggle LoopTime ({LoopTime})\n" +
        //     $"V : Toggle LoopValues ({LoopValues})\n" +
        //     $"C : Toggle CycleRootMotion ({CycleRootMotion})\n" +
        //     $"I : Toggle InPlace ({InPlace})\n" +
        //     $"B : Toggle BankPivot ({BankPivot})\n" +
        //     $"R : Reset";
    }
}
