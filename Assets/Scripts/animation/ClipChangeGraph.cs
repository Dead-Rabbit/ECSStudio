using Unity.Entities;
using Unity.Animation;
using Unity.DataFlowGraph;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using Unity.Animation.Hybrid;
using UnityEngine;

public class ClipChangeGraph : AnimationGraphBase
{
    public AnimationClip Clip;

    public AnimationClip[] Clips;

    public float ClipTimeInit;

    private StringHash m_MotionId;

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

        dstManager.AddComponentData(entity, graphSetup);
        dstManager.AddComponent<DeltaTime>(entity);
    }
}
#endif

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

        var deltaTimeNode = graphSystem.CreateNode<ConvertDeltaTimeToFloatNode>(data.Graph);
        var entityNode = graphSystem.CreateNode(data.Graph, entity);

        var set = graphSystem.Set;

        // Connect kernel ports
        set.Connect(entityNode, deltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Input);
        set.Connect(deltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Output, data.ClipNode, ClipPlayerNode.KernelPorts.DeltaTime);
        set.Connect(data.ClipNode, ClipPlayerNode.KernelPorts.Output, entityNode, NodeSetAPI.ConnectionType.Feedback);

        // Send messages to set parameters on the ClipPlayerNode
        set.SetData(data.ClipNode, ClipPlayerNode.KernelPorts.Speed, 1.0f);
        set.SendMessage(data.ClipNode, ClipPlayerNode.SimulationPorts.Configuration, new ClipConfiguration { Mask = ClipConfigurationMask.LoopTime });
        set.SendMessage(data.ClipNode, ClipPlayerNode.SimulationPorts.Rig, rig);
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
        Entities
            .WithAll<InputChangeClipSampleData>()
            .ForEach((Entity e, ref ChangeClipPlayerData data, ref InputChangeClipSampleData input) =>
            {
                if (input.ifModify)
                {
                    Debug.Log("Update Entity Entity " + e);
                    Debug.Log("Update Entity input " + input.ifModify);
                    DynamicBuffer<StoreClipBuffer> animationBuff = m_AnimationSystem.GetBuffer<StoreClipBuffer>(e);
                    m_AnimationSystem.Set.SendMessage(data.ClipNode, ClipPlayerNode.SimulationPorts.Clip, animationBuff[input.index].Clip);
                }
                input.ifModify = false;
            });
    }
}