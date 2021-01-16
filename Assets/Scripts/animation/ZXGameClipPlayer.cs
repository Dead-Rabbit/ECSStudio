using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;
using UnityEngine;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using Unity.Animation.Hybrid;

[ConverterVersion("ZXGameClipPlayer", 1)]
public class ZXGameClipPlayer : MonoBehaviour, IConvertGameObjectToEntity
{
    public AnimationClip Clip1;
    public AnimationClip Clip2;

    // 此处使用Unity中的
    // public InputChangeClip input;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if (Clip1 == null || Clip2 == null)
            return;

        // if (null != input)
        // {
        //     input.RegisterEntity(entity);
        // }

        conversionSystem.DeclareAssetDependency(gameObject, Clip1);
        conversionSystem.DeclareAssetDependency(gameObject, Clip2);

        DynamicBuffer<SampleClip> buffer = dstManager.AddBuffer<SampleClip>(entity);
        buffer.Add(new SampleClip { Clip = conversionSystem.BlobAssetStore.GetClip(Clip1) });
        buffer.Add(new SampleClip { Clip = conversionSystem.BlobAssetStore.GetClip(Clip2) });

        dstManager.AddComponentData(entity, new PlayClipComponent
        {
            Clip = conversionSystem.BlobAssetStore.GetClip(Clip1),
        });
        // dstManager.AddComponentData(entity, new ChangeClipSampleData
        // {
        //     ifModify = false,
        //     index = 0
        // });

        dstManager.AddComponent<DeltaTime>(entity);
    }
}
#endif
//
// public struct ChangeClipSampleData : ISampleData
// {
//     public bool ifModify;
//     public int index;
// }

public struct PlayClipComponent : IComponentData
{
    public BlobAssetReference<Clip> Clip;
}

public struct PlayClipStateComponent : ISystemStateComponentData
{
    public GraphHandle Graph;
    public NodeHandle<ClipPlayerNode> ClipPlayerNode;
}

[UpdateBefore(typeof(DefaultAnimationSystemGroup))]
public class ZXGamePlayClipSystem : SystemBase
{
    ProcessDefaultAnimationGraph m_GraphSystem;
    EndSimulationEntityCommandBufferSystem m_ECBSystem;
    EntityQuery m_AnimationDataQuery;

    protected override void OnCreate()
    {
        base.OnCreate();
        m_GraphSystem = World.GetOrCreateSystem<ProcessDefaultAnimationGraph>();
        // Increase the reference count on the graph system so it knows
        // that we want to use it.
        m_GraphSystem.AddRef();
        m_ECBSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        m_AnimationDataQuery = GetEntityQuery(new EntityQueryDesc()
        {
            None = new ComponentType[] { typeof(PlayClipComponent) },
            All = new ComponentType[] { typeof(PlayClipStateComponent) }
        });

        m_GraphSystem.Set.RendererModel = NodeSet.RenderExecutionModel.Islands;
    }

    protected override void OnDestroy()
    {
        if (m_GraphSystem == null)
            return;

        m_GraphSystem.RemoveRef();
        base.OnDestroy();
    }

    protected override void OnUpdate()
    {
        CompleteDependency();

        var ecb = m_ECBSystem.CreateCommandBuffer();

        // Create graph for entities that have a PlayClipComponent but no graph (PlayClipStateComponent)
        Entities
            .WithName("CreateGraph")
            .WithNone<PlayClipStateComponent>()
            .WithoutBurst()
            .WithStructuralChanges()
            .ForEach((Entity e, ref Rig rig, ref PlayClipComponent animation) =>
            {
                var state = CreateGraph(e, m_GraphSystem, ref rig, ref animation);
                ecb.AddComponent(e, state);
            }).Run();

        // Update graph if the animation component changed(Use ref in System on Entities.forEach)
        Entities
            .WithName("UpdateGraph")
            .WithChangeFilter<PlayClipComponent>()
            .WithoutBurst()
            .ForEach((Entity e, ref PlayClipComponent animation, ref PlayClipStateComponent state) =>
            {
                m_GraphSystem.Set.SendMessage(state.ClipPlayerNode, ClipPlayerNode.SimulationPorts.Clip, animation.Clip);
            }).Run();

        // Destroy graph for which the entity is missing the PlayClipComponent
        Entities
            .WithName("DestroyGraph")
            .WithNone<PlayClipComponent>()
            .WithoutBurst()
            .WithStructuralChanges()
            .ForEach((Entity e, ref PlayClipStateComponent state) =>
            {
                m_GraphSystem.Dispose(state.Graph);
            }).Run();

        if (m_AnimationDataQuery.CalculateEntityCount() > 0)
            ecb.RemoveComponent(m_AnimationDataQuery, typeof(PlayClipStateComponent));
    }

    static PlayClipStateComponent CreateGraph(
        Entity entity,
        ProcessDefaultAnimationGraph graphSystem,
        ref Rig rig,
        ref PlayClipComponent playClip
    )
    {
        GraphHandle graph = graphSystem.CreateGraph();
        var data = new PlayClipStateComponent
        {
            Graph = graph,
            ClipPlayerNode = graphSystem.CreateNode<ClipPlayerNode>(graph)
        };

        var deltaTimeNode = graphSystem.CreateNode<ConvertDeltaTimeToFloatNode>(graph);
        var entityNode = graphSystem.CreateNode(graph, entity);

        var set = graphSystem.Set;

        // Connect kernel ports
        set.Connect(entityNode, deltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Input);
        set.Connect(deltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Output, data.ClipPlayerNode, ClipPlayerNode.KernelPorts.DeltaTime);
        set.Connect(data.ClipPlayerNode, ClipPlayerNode.KernelPorts.Output, entityNode, NodeSetAPI.ConnectionType.Feedback);

        // Send messages to set parameters on the ClipPlayerNode
        set.SetData(data.ClipPlayerNode, ClipPlayerNode.KernelPorts.Speed, 1.0f);
        set.SendMessage(data.ClipPlayerNode, ClipPlayerNode.SimulationPorts.Configuration, new ClipConfiguration
        {
            Mask = ClipConfigurationMask.LoopTime | ClipConfigurationMask.CycleRootMotion
        });
        set.SendMessage(data.ClipPlayerNode, ClipPlayerNode.SimulationPorts.Rig, rig);
        set.SendMessage(data.ClipPlayerNode, ClipPlayerNode.SimulationPorts.Clip, playClip.Clip);

        return data;
    }
}

public struct SampleClip : IBufferElementData
{
    public BlobAssetReference<Clip> Clip;
}

public class InputAndChangeClipComponentSystem : SystemBase
{
    private EntityManager entityManager;

    protected override void OnCreate()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    protected override void OnUpdate()
    {
        // Entities
        //     .WithChangeFilter<ChangeClipSampleData>()
        //     .ForEach((Entity e, ref ChangeClipSampleData input, ref PlayClipComponent clipComponent, in DynamicBuffer<SampleClip> buffer) =>
        //     {
        //         Debug.Log("Modify And Update");
        //         if (input.ifModify)
        //         {
        //             clipComponent.Clip = buffer[input.index].Clip;
        //             input.ifModify = false;
        //         }
        //     }).Run();
    }
}
