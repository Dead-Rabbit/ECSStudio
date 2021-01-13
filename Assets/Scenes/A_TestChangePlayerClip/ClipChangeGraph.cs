using Unity.Entities;
using Unity.Animation;
using Unity.DataFlowGraph;

#if UNITY_EDITOR
using Unity.Animation.Hybrid;
using UnityEngine;

public class ClipChangeGraph : AnimationGraphBase
{
    public AnimationClip Clip;

    public AnimationClip[] Clips;

    public string MotionName;
    public float ClipTimeInit;

    private StringHash m_MotionId;

    public override void PreProcessData<T>(T data)
    {
        // if (data is RigComponent)
        // {
        //     var rig = data as RigComponent;
        //
        //     for (var boneIter = 0; boneIter < rig.Bones.Length; boneIter++)
        //     {
        //         if (MotionName == rig.Bones[boneIter].name)
        //         {
        //             m_MotionId = RigGenerator.ComputeRelativePath(rig.Bones[boneIter], rig.transform);
        //         }
        //     }
        // }
    }

    public override void AddGraphSetupComponent(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var graphSetup = new ConfigurableClipSetup
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

public struct ConfigurableClipSetup : ISampleSetup
{
    public BlobAssetReference<Clip> Clip;
    public float ClipTime;
    public StringHash MotionID;
}

public struct ConfigurableClipData : ISampleData
{
    public GraphHandle Graph;
    public NodeHandle<ClipPlayerNode> ClipNode;

    // public float ClipTime;
    //
    // public bool UpdateConfiguration;
    // public bool InPlace;
    //
    // public ClipConfigurationMask ClipOptions;
    // public StringHash MotionID;
}

[UpdateBefore(typeof(DefaultAnimationSystemGroup))]
public class ClipChangeGraphSystem : SampleSystemBase<
    ConfigurableClipSetup,
    ConfigurableClipData,
    ProcessDefaultAnimationGraph
>
{
    protected override ConfigurableClipData CreateGraph(
        Entity entity,
        ref Rig rig,
        ProcessDefaultAnimationGraph graphSystem,
        ref ConfigurableClipSetup setup)
    {
        var data = new ConfigurableClipData();

        data.Graph = graphSystem.CreateGraph();
        // data.MotionID = setup.MotionID;
        // data.UpdateConfiguration = true;
        data.ClipNode = graphSystem.CreateNode<ClipPlayerNode>(data.Graph);

        var deltaTimeNode = graphSystem.CreateNode<ConvertDeltaTimeToFloatNode>(data.Graph);
        var entityNode = graphSystem.CreateNode(data.Graph, entity);

        var set = graphSystem.Set;
        // set.Connect(data.ClipNode, ClipPlayerNode.KernelPorts.Output, entityNode);
        //
        // set.SendMessage(data.ClipNode, ClipPlayerNode.SimulationPorts.Rig, rig);
        // set.SendMessage(data.ClipNode, ClipPlayerNode.SimulationPorts.Clip, setup.Clip);
        // set.SetData(data.ClipNode, ClipPlayerNode.KernelPorts.Speed, 1.0f);
        // set.SetData(data.ClipNode, ClipPlayerNode.KernelPorts.Time, setup.ClipTime);

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

    protected override void DestroyGraph(Entity entity, ProcessDefaultAnimationGraph graphSystem, ref ConfigurableClipData data)
    {
        graphSystem.Dispose(data.Graph);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        // Performed on main thread since sending messages from NodeSet and ClipConfiguration changes incur a structural graph change.
        // It's not recommended to do this at runtime, it's mostly shown here to showcase clip configuration features.
        Entities.WithAll<ConfigurableClipSetup, ConfigurableClipData>()
            .ForEach((Entity e, ref ConfigurableClipData data) =>
            {
                // m_AnimationSystem.Set.SetData(data.ConfigurableClipNode, ConfigurableClipNode.KernelPorts.Time, data.ClipTime);
                // if (data.UpdateConfiguration)
                // {
                //     var config = new ClipConfiguration { Mask = data.ClipOptions, MotionID = data.InPlace ? data.MotionID : 0 };
                //     m_AnimationSystem.Set.SendMessage(data.ConfigurableClipNode, ConfigurableClipNode.SimulationPorts.Configuration, config);
                //     data.UpdateConfiguration = false;
                // }
            });

        Entities
        // .WithName("ModifyConfigurableClipSetup")
        // .WithoutBurst()
            .ForEach((Entity e, ref ConfigurableClipData data, ref ConfigurableClipSetup setup) =>
        {
            m_AnimationSystem.Set.SendMessage(data.ClipNode, ClipPlayerNode.SimulationPorts.Clip, setup.Clip);
        });
    }
}
