using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Animation;
using Debug = UnityEngine.Debug;

public class InputChangeClip : AnimationInputBase<ChangeClipSampleData>
{
    private Int32 index;

    protected override void UpdateComponentData(ref ChangeClipSampleData data)
    {
        UpdateParameters();
        UpdateText();
        if (data.index != index)
        {
            Debug.Log("Change Value to " + data.index);
            data.ifModify = true;
            data.index = index;
        }
    }

    private void UpdateParameters()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Push C");
            index = index + 1;
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
