using component.tags;
using Unity.Entities;
using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Animation.Hybrid;
using Unity.Transforms;
using UnityEngine;

public class ClipChangeGraph : AnimationGraphBase
{
    public AnimationClip[] Clips;
    public Vector3[] clipDefaultRotateOffset;

    public float clipPlaySpeed = 1f;

    public string MotionName;

    private StringHash m_MotionId;

    // 此处的PreProcessData仅执行一次，给Spawner Entity 增加了一次 Component
    public override void PreProcessData<T>(T data)
    {
        if (data is RigComponent)
        {
            var rig = data as RigComponent;

            for (var boneIter = 0; boneIter < rig.Bones.Length; boneIter++)
            {
                if (MotionName == rig.Bones[boneIter].name)
                {
                    m_MotionId = RigGenerator.ComputeRelativePath(rig.Bones[boneIter], rig.transform);
                }
            }
        }
    }

    public override void AddGraphSetupComponent(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var defaultRotate = clipDefaultRotateOffset[0];
        var graphSetup = new ChangeClipSetup
        {
            Clip = Clips[0].ToDenseClip(),
            MotionID = m_MotionId,
            playSpeed = clipPlaySpeed,
            defaultRotationOnSetup = defaultRotate
        };
        dstManager.AddComponentData(entity, new Rotation()
        {
            Value = Quaternion.Euler(defaultRotate)
        });

        var clipBuffer = dstManager.AddBuffer<StoreClipBuffer>(entity);

        for (int i = 0; i < Clips.Length; ++i)
        {
            clipBuffer.Add(new StoreClipBuffer
            {
                Clip = Clips[i].ToDenseClip(),
                clipRotateOffset = clipDefaultRotateOffset[i]
            });
        }

        dstManager.AddComponent<ProcessDefaultAnimationGraph.AnimatedRootMotion>(entity);
        dstManager.AddComponentData(entity, graphSetup);
        dstManager.AddComponent<DeltaTime>(entity);

        dstManager.AddComponentData(entity, new AiChangeClipSampleData
        {
            ifModify = false,
            index = 0
        });

        // 默认首先打上Moving的Tag
        dstManager.AddComponent<Moving>(entity);
    }
}

/**
 * 记录当前Spawner中的所有动画Clip
 */
public struct StoreClipBuffer : IBufferElementData
{
    public BlobAssetReference<Clip> Clip;
    public Vector3 clipRotateOffset;
}

public struct InputChangeClipSampleData : ISampleData
{
    public bool ifModify;
    public int index;                   // 播放Clip的Index
}

public struct AiChangeClipSampleData : IComponentData
{
    public bool ifModify;
    public int index; // 播放Clip的Index
}

public struct ChangeClipSetup : ISampleSetup
{
    public BlobAssetReference<Clip> Clip;
    public StringHash MotionID;
    public float playSpeed;
    public Vector3 defaultRotationOnSetup;
}

public struct ChangeClipPlayerData : ISampleData
{
    public GraphHandle Graph;
    public NodeHandle<ClipPlayerNode> ClipNode;
    public StringHash MotionID;
    public Vector3 clipRotation;
    public Vector3 currentRotation;
}

[UpdateBefore(typeof(DefaultAnimationSystemGroup))]
public class ClipChangeGraphSystem : SampleSystemBase<
    ChangeClipSetup,
    ChangeClipPlayerData,
    ProcessDefaultAnimationGraph
>
{
    protected override ChangeClipPlayerData CreateGraph(
        Entity entity,
        ref Rig rig,
        ProcessDefaultAnimationGraph graphSystem,
        ref ChangeClipSetup setup)
    {
        var data = new ChangeClipPlayerData();

        data.Graph = graphSystem.CreateGraph();
        data.ClipNode = graphSystem.CreateNode<ClipPlayerNode>(data.Graph);
        data.MotionID = setup.MotionID;
        data.clipRotation = setup.defaultRotationOnSetup;
        data.currentRotation = setup.defaultRotationOnSetup;

        var deltaTimeNode = graphSystem.CreateNode<ConvertDeltaTimeToFloatNode>(data.Graph);
        var entityNode = graphSystem.CreateNode(data.Graph, entity);

        var set = graphSystem.Set;

        // Connect kernel ports
        set.Connect(entityNode, deltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Input);
        set.Connect(deltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Output, data.ClipNode, ClipPlayerNode.KernelPorts.DeltaTime);
        set.Connect(data.ClipNode, ClipPlayerNode.KernelPorts.Output, entityNode, NodeSetAPI.ConnectionType.Feedback);

        // Send messages to set parameters on the ClipPlayerNode
        set.SetData(data.ClipNode, ClipPlayerNode.KernelPorts.Speed, setup.playSpeed);

        set.SendMessage(data.ClipNode, ClipPlayerNode.SimulationPorts.Rig, rig);
        set.SendMessage(data.ClipNode, ClipPlayerNode.SimulationPorts.Configuration, new ClipConfiguration
        {
            // Mask = ClipConfigurationMask.LoopTime | ClipConfigurationMask.CycleRootMotion | ClipConfigurationMask.DeltaRootMotion,       // 可移动
            Mask = ClipConfigurationMask.LoopTime | ClipConfigurationMask.CycleRootMotion | ClipConfigurationMask.DeltaRootMotion | ClipConfigurationMask.BankPivot,       // 可移动
            MotionID = data.MotionID
        });

        set.SendMessage(data.ClipNode, ClipPlayerNode.SimulationPorts.Clip, setup.Clip);

        return data;
    }

    protected override void DestroyGraph(Entity entity, ProcessDefaultAnimationGraph graphSystem, ref ChangeClipPlayerData data)
    {
        graphSystem.Dispose(data.Graph);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        // var DeltaTime = Time.DeltaTime;
        Entities
            .WithAll<AiChangeClipSampleData>()
            .ForEach((Entity e, ref ChangeClipPlayerData data, ref AiChangeClipSampleData ai, ref Rotation rotation) =>
            {
                // 用于旋转（此处保留）
                // rotation.Value = math.mul(math.normalize(rotation.Value), quaternion.AxisAngle(math.up(), 1f * DeltaTime));

                if (ai.ifModify)
                {
                    DynamicBuffer<StoreClipBuffer> animationBuff = m_AnimationSystem.GetBuffer<StoreClipBuffer>(e);
                    m_AnimationSystem.Set.SendMessage(data.ClipNode, ClipPlayerNode.SimulationPorts.Clip, animationBuff[ai.index].Clip);
                }
                ai.ifModify = false;
            });
    }
}
