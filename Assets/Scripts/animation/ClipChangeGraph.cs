using Unity.Entities;
using Unity.Animation;
using Unity.DataFlowGraph;
using Debug = UnityEngine.Debug;
using Unity.Animation.Hybrid;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ClipChangeGraph : AnimationGraphBase
{
    public AnimationClip Clip;

    public AnimationClip[] Clips;

    public string MotionName;

    public float ClipTimeInit;

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
        var graphSetup = new ChangeClipSetup
        {
            Clip = Clip.ToDenseClip(),
            ClipTime = ClipTimeInit,
            MotionID = m_MotionId
        };

        var clipBuffer = dstManager.AddBuffer<StoreClipBuffer>(entity);
        for (int i = 0; i < Clips.Length; ++i)
            clipBuffer.Add(new StoreClipBuffer { Clip = Clips[i].ToDenseClip() });

        dstManager.AddComponent<ProcessDefaultAnimationGraph.AnimatedRootMotion>(entity);
        dstManager.AddComponentData(entity, graphSetup);
        dstManager.AddComponent<DeltaTime>(entity);
    }
}

/**
 * 记录当前Spawner中的所有动画Clip
 */
public struct StoreClipBuffer : IBufferElementData
{
    public BlobAssetReference<Clip> Clip;
}

public struct InputChangeClipSampleData : ISampleData
{
    public bool ifModify;
    public int index;
}

public struct ChangeClipSetup : ISampleSetup
{
    public BlobAssetReference<Clip> Clip;
    public float ClipTime;
    public StringHash MotionID;
}

public struct ChangeClipPlayerData : ISampleData
{
    public GraphHandle Graph;
    public NodeHandle<ClipPlayerNode> ClipNode;
    public StringHash MotionID;
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

        var deltaTimeNode = graphSystem.CreateNode<ConvertDeltaTimeToFloatNode>(data.Graph);
        var entityNode = graphSystem.CreateNode(data.Graph, entity);

        var set = graphSystem.Set;

        // Connect kernel ports
        set.Connect(entityNode, deltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Input);
        set.Connect(deltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Output, data.ClipNode, ClipPlayerNode.KernelPorts.DeltaTime);
        set.Connect(data.ClipNode, ClipPlayerNode.KernelPorts.Output, entityNode, NodeSetAPI.ConnectionType.Feedback);

        // Send messages to set parameters on the ClipPlayerNode
        set.SetData(data.ClipNode, ClipPlayerNode.KernelPorts.Speed, 1.0f);

        set.SendMessage(data.ClipNode, ClipPlayerNode.SimulationPorts.Rig, rig);
        set.SendMessage(data.ClipNode, ClipPlayerNode.SimulationPorts.Configuration, new ClipConfiguration
        {
            Mask = ClipConfigurationMask.LoopTime | ClipConfigurationMask.CycleRootMotion | ClipConfigurationMask.DeltaRootMotion,
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
        var DeltaTime = Time.DeltaTime;

        base.OnUpdate();
        Entities
            .WithAll<InputChangeClipSampleData>()
            .ForEach((Entity e, ref ChangeClipPlayerData data, ref InputChangeClipSampleData input, ref Rotation rotation) =>
            {
                // rotation.Value = math.mul(math.normalize(rotation.Value), quaternion.AxisAngle(math.up(), 1f * DeltaTime));

                if (input.ifModify)
                {
                    // DynamicBuffer<StoreClipBuffer> animationBuff = m_AnimationSystem.GetBuffer<StoreClipBuffer>(e);
                    // m_AnimationSystem.Set.SendMessage(data.ClipNode, ClipPlayerNode.SimulationPorts.Clip, animationBuff[input.index].Clip);
                    // Debug.Log("rotation.Value = " + rotation.Value);
                    // rotation.Value = new float3(0, rotation.Value.y + 10, 0);
                    // rotation.Value = math.mul(math.normalize(rotation.Value), quaternion.AxisAngle(math.up(), 1f * DeltaTime));
                }
                input.ifModify = false;
            });
    }
}
