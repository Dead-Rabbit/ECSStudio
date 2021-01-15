using System;
using Unity.Entities;
using UnityEngine;

public class ChangePlayerClipInput : AnimationInputBase<InputChangeClipSampleData>
{
    private Int32 index;

    protected override bool UpdateComponentData(ref InputChangeClipSampleData data)
    {
        UpdateParameters();
        UpdateText();
        if (data.index != index)
        {
            data.ifModify = true;
            data.index = index;
            return true;
        }

        return false;
    }

    private void UpdateParameters()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            index = (index + 1) % 2;
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
