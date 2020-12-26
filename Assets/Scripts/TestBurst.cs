using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;

public class TestBurst : MonoBehaviour
{
    void Update()
    {
        RunTestBurst();
    }

    [BurstCompile]
    void RunTestBurst()
    {
        NativeArray<Vector3> velocity = new NativeArray<Vector3>(1000, Allocator.Persistent);
        Profiler.BeginSample("NativeArray");
        for (var i = 0; i < velocity.Length; i++) {
            velocity[i] = Vector3.zero;
        }
        Profiler.EndSample();
        velocity.Dispose();

        Vector3[] velocity2 = new Vector3[1000];
        Profiler.BeginSample("Vector3");
        for (var i = 0; i < velocity2.Length; i++) {
            velocity2[i] = Vector3.zero;
        }
        Profiler.EndSample();
    }
}
