
using Unity.DataFlowGraph;
using UnityEngine;

public class DFG_API : MonoBehaviour
{
    private NodeSet m_Set;

    // NodeDefinition 用来定义单个节点, 以下是一个完整的但是并非强制的定义, 你可以根据需要添加接口:
    public class MyNode : NodeDefinition<
        MyNode.InstanceData,
        MyNode.SimPorts,
        MyNode.KernelData,
        MyNode.KernelDefs,
        MyNode.GraphKernel>
        , IMsgHandler<int>
    {
        // 在 DFG 中, 执行流通常被分为 simulation 和 rendering, simulation 的目的主要是为 rendering 做准备的, 以确保最终执行时数据的不可变性(方便后续做并行)
        // 节点可处理两类数据, 第一类是 INodeData, 可以理解为该 Node 在运行前所需要的配置数据 (当然配置也可以在运行中被更改), 该数据即是 simulation 阶段所需要的数据.
        public struct InstanceData : INodeData { public string Name; }

        // ISimulationPortDefinition 则定义了 simulation 阶段该节点如何接受什么样的输入和输出:
        public struct SimPorts : ISimulationPortDefinition
        {
            public MessageInput<MyNode, int> MyInput;
            public MessageOutput<MyNode, int> MyOutput;
        }
        
        // Init 用以在初始化一个节点时, 设置 INodeData
        protected override void Init(InitContext ctx)
        {
            // Handle 指向了该 node 的实例
            ref var myData = ref GetNodeData(ctx.Handle);
            myData.Name = "Node Name";
        }

        // SimPorts 定义的数据结构可由 SendMessage 传入到节点中做处理, 然后输出
        // msg 的流向具体将根据由 SimulationPorts 组成的有向图来决定
        // 一般来说 HandleMessage 可以用来配置节点的功能性数据
        public void HandleMessage(in MessageContext ctx, in int msg)
        {
            Debug.Log($"'{GetNodeData(ctx.Handle).Name}' received an int message of value: {msg}");

            // 使用 EmitMessage 将结果写入到输出 port
            ctx.EmitMessage(
                SimulationPorts.MyOutput,
                msg + 1
            );
        }
        
        //  OnUpdate 会在 NodeSet.Update() 时调用, 这个过程和 rendering 没有关系, 但是方便你和游戏的 Update 做嫁接.
        protected override void OnUpdate(in UpdateContext ctx)
        {
            Debug.Log("Updating MyNode");
        }

        // 第二类数据是 IKernelData, 处理该数据的阶段被称为 Rendering
        //  IKernelData 非常类似于 shader 中的 uniform
        public struct KernelData : IKernelData { }
        
        // 和 simulation 阶段类似, 定义了 rendering 阶段该节点如何接受什么样的输入和输出
        public struct KernelDefs : IKernelPortDefinition
        {
            public DataInput<MyNode, float> InputA, InputB;
            public DataOutput<MyNode, float> Output;
        }
        
        // IGraphKernel 是节点执行具体功能的核心, Execute 方法仅在上游 port 被 resolve 之后调用, 这样确保了 graph 的执行顺序
        // 该方法最好使用 Burst 编译来提高性能
        public struct GraphKernel : IGraphKernel<KernelData, KernelDefs>
        {
            public void Execute(RenderContext ctx, KernelData data, ref KernelDefs ports)
            {
                var inputA = ctx.Resolve(ports.InputA);
                var inputB = ctx.Resolve(ports.InputB);

                ref var output = ref ctx.Resolve(ref ports.Output);

                output = inputA + inputB + 10;

                Debug.Log($"My output was {output}");
            }
        }
    }

    void Start()
    {
        using (var set = new NodeSet())
        {
            m_Set = set;
            
            NodeHandle<MyNode>
                a = set.Create<MyNode>(),
                b = set.Create<MyNode>(),
                c = set.Create<MyNode>(),
                d = set.Create<MyNode>(),
                e = set.Create<MyNode>(),
                f = set.Create<MyNode>(),
                g = set.Create<MyNode>();

            /*
             * 以下代码构建了如下的图结构, 该结构的数据流动只针对 SimulationPorts
             * 
             *   b
             *  / \
             * a   d - e
             *  \ /
             *   c
             */

            set.Connect(a, MyNode.SimulationPorts.MyOutput, b, MyNode.SimulationPorts.MyInput);
            set.Connect(a, MyNode.SimulationPorts.MyOutput, c, MyNode.SimulationPorts.MyInput);

            set.Connect(b, MyNode.SimulationPorts.MyOutput, d, MyNode.SimulationPorts.MyInput);
            set.Connect(c, MyNode.SimulationPorts.MyOutput, d, MyNode.SimulationPorts.MyInput);

            set.Connect(d, MyNode.SimulationPorts.MyOutput, e, MyNode.SimulationPorts.MyInput);

            /*
             * 以下代码构建了如下的图结构, 该结构的数据流动只针对 KernelPorts
             * 
             * a
             *  \
             *   e
             *  / \
             * b   \
             *      g
             * c   /
             *  \ /
             *   f
             *  /
             * d
             * 
             */

            set.Connect(a, MyNode.KernelPorts.Output, e, MyNode.KernelPorts.InputA);
            set.Connect(b, MyNode.KernelPorts.Output, e, MyNode.KernelPorts.InputB);

            set.Connect(c, MyNode.KernelPorts.Output, f, MyNode.KernelPorts.InputA);
            set.Connect(d, MyNode.KernelPorts.Output, f, MyNode.KernelPorts.InputB);

            set.Connect(e, MyNode.KernelPorts.Output, g, MyNode.KernelPorts.InputA);
            set.Connect(f, MyNode.KernelPorts.Output, g, MyNode.KernelPorts.InputB);


            set.SendMessage(a, MyNode.SimulationPorts.MyInput, 0);

            set.Update();

            // GraphValue 用来获取当前 graph 里任意节点的执行结果
            var valueAtE = set.CreateGraphValue(e, MyNode.KernelPorts.Output);
            var val = set.GetValueBlocking(valueAtE);

            set.Destroy(a, b, c, d, e, f, g);

        }
    }

    // void Update()
    // {
    //     // 你可以在 update 时尝试修改 graph 运行过程中所用到的参数
    //     m_Set.Update();
    // }
}