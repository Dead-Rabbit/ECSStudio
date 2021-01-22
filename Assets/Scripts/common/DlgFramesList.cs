using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class DlgFramesList : MonoBehaviour
{
    public Text fpsText;
    public Text heapSizeText;
    public Text usedSizeText;
    public Text allocatedMemoryText;
    public Text reservedMemoryText;
    public Text unusedReservedMemoryText;
    public Text cpuText;

    private int _index = 1;
    private int _indexCount = 100;
    private int _framesIndex;

    private float _fps;
    private float _curTime;
    private float _lastTime;
    private float _fpsDelay;

    private TimeSpan _preCpuTime;
    private TimeSpan _curCpuTime;

    private const long Kb = 1024;
    private const long Mb = 1024 * 1024;

    private void Start()
    {
        _fpsDelay = 0.5F;
        _curCpuTime = Process.GetCurrentProcess().TotalProcessorTime;
        _preCpuTime = _curCpuTime;
        _curTime = Time.realtimeSinceStartup;
        _lastTime = _curTime;
    }

    private void Update()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        _index++;
        if (_index == _indexCount)
        {
            ShowProfilerMsg();
        }

        _curCpuTime = Process.GetCurrentProcess().TotalProcessorTime;
        if ((_curCpuTime - _preCpuTime).TotalMilliseconds >= 1000)
        {
            ShowCpuMsg();
        }

        _framesIndex++;
        _curTime = Time.realtimeSinceStartup;
        if (_curTime - _lastTime > _fpsDelay)
        {
            ShowFpsMsg();
        }
    }

    private void ShowProfilerMsg()
    {
        _index = 0;
        //堆内存
        if (heapSizeText)
        {
            heapSizeText.text = "堆内存 : " + Profiler.GetMonoHeapSizeLong() / Mb + " Mb";
        }

        //使用的
        if (usedSizeText)
        {
            usedSizeText.text = "使用大小 : " + Profiler.GetMonoUsedSizeLong() / Mb + " Mb";
        }

        // unity分配
        if (allocatedMemoryText)
        {
            allocatedMemoryText.text = "Unity分配 : " + Profiler.GetTotalAllocatedMemoryLong() / Mb + " Mb";
        }

        // 总内存
        if (reservedMemoryText)
        {
            reservedMemoryText.text = "总内存 : " + Profiler.GetTotalReservedMemoryLong() / Mb + " Mb";
        }

        // 未使用内存
        if (unusedReservedMemoryText)
        {
            unusedReservedMemoryText.text = "未使用内存 : " + Profiler.GetTotalUnusedReservedMemoryLong() / Mb + " Mb";
        }
    }

    private void ShowCpuMsg()
    {
        // 间隔时间（毫秒）
        int interval = 1000;
        var value = (_curCpuTime - _preCpuTime).TotalMilliseconds / interval / Environment.ProcessorCount * 100;
        _preCpuTime = _curCpuTime;
        if (cpuText)
        {
            cpuText.text = "CpuValue : " + value.ToString("f2");
        }
    }

    private void ShowFpsMsg()
    {
        _fps = _framesIndex / (_curTime - _lastTime);
        _framesIndex = 0;
        _lastTime = _curTime;
        if (fpsText)
        {
            fpsText.text = "FPS : " + _fps.ToString("f2");
        }
    }
}
