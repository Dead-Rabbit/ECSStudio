using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;
using UnityEngine;

using Unity.Animation.Hybrid;

namespace animation
{
    [ConverterVersion("SingleClipPlayer", 1)]
    public class SingleClipPlayer : MonoBehaviour, IConvertGameObjectToEntity
    {
        public AnimationClip Clip;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (Clip == null)
                return;

            conversionSystem.DeclareAssetDependency(gameObject, Clip);

            dstManager.AddComponentData(entity, new SinglePlayClipComponent
            {
                Clip = conversionSystem.BlobAssetStore.GetClip(Clip)
            });

            dstManager.AddComponent<DeltaTime>(entity);
        }
    }

    public struct SinglePlayClipComponent : IComponentData
    {
        public BlobAssetReference<Clip> Clip;
    }

    public struct SinglePlayClipStateComponent : ISystemStateComponentData
    {
        public GraphHandle Graph;
        public NodeHandle<ClipPlayerNode> ClipPlayerNode;
    }

    [UpdateBefore(typeof(DefaultAnimationSystemGroup))]
    public class SinglePlayClipSystem : SystemBase
    {
        ProcessDefaultAnimationGraph m_GraphSystem;
        EndSimulationEntityCommandBufferSystem m_ECBSystem;
        EntityQuery m_AnimationDataQuery;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_GraphSystem = World.GetOrCreateSystem<ProcessDefaultAnimationGraph>();
            m_GraphSystem.AddRef();
            m_ECBSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            m_AnimationDataQuery = GetEntityQuery(new EntityQueryDesc()
            {
                None = new ComponentType[] { typeof(SinglePlayClipComponent) },
                All = new ComponentType[] { typeof(SinglePlayClipStateComponent) }
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

            Entities
                .WithName("CreateGraph")
                .WithNone<SinglePlayClipStateComponent>()
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach((Entity e, ref Rig rig, ref SinglePlayClipComponent animation) =>
                {
                    var state = CreateGraph(e, m_GraphSystem, ref rig, ref animation);
                    ecb.AddComponent(e, state);
                }).Run();

            // Update graph if the animation component changed
            Entities
                .WithName("UpdateGraph")
                .WithChangeFilter<SinglePlayClipComponent>()
                .WithoutBurst()
                .ForEach((Entity e, ref SinglePlayClipComponent animation, ref SinglePlayClipStateComponent state) =>
                {
                    m_GraphSystem.Set.SendMessage(state.ClipPlayerNode, ClipPlayerNode.SimulationPorts.Clip, animation.Clip);
                }).Run();

            // Destroy graph for which the entity is missing the PlayClipComponent
            Entities
                .WithName("DestroyGraph")
                .WithNone<SinglePlayClipComponent>()
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach((Entity e, ref SinglePlayClipStateComponent state) =>
                {
                    m_GraphSystem.Dispose(state.Graph);
                }).Run();

            if (m_AnimationDataQuery.CalculateEntityCount() > 0)
                ecb.RemoveComponent(m_AnimationDataQuery, typeof(SinglePlayClipStateComponent));
        }

        /// <summary>
        /// The graph executes in the ProcessDefaultAnimationGraph system, but because we connect an EntityNode to the output of the ClipPlayerNode,
        /// the AnimatedData buffer gets updated on the entity and can be used in other systems, such as the ProcessLateAnimationGraph system.
        /// </summary>
        /// <param name="entity">An entity that has a PlayClipComponent and a Rig.</param>
        /// <param name="graphSystem">The ProcessDefaultAnimationGraph.</param>
        /// <param name="rig">The rig that will get animated.</param>
        /// <param name="playClip">The clip to play.</param>
        /// <returns>Returns a StateComponent containing the NodeHandles of the graph.</returns>
        static SinglePlayClipStateComponent CreateGraph(
            Entity entity,
            ProcessDefaultAnimationGraph graphSystem,
            ref Rig rig,
            ref SinglePlayClipComponent playClip
        )
        {
            GraphHandle graph = graphSystem.CreateGraph();
            var data = new SinglePlayClipStateComponent
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
            set.SetData(data.ClipPlayerNode, ClipPlayerNode.KernelPorts.Speed, 2.0f);
            set.SendMessage(data.ClipPlayerNode, ClipPlayerNode.SimulationPorts.Configuration, new ClipConfiguration { Mask = ClipConfigurationMask.LoopTime });
            set.SendMessage(data.ClipPlayerNode, ClipPlayerNode.SimulationPorts.Rig, rig);
            set.SendMessage(data.ClipPlayerNode, ClipPlayerNode.SimulationPorts.Clip, playClip.Clip);

            return data;
        }
    }
}
