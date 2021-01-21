using System.ComponentModel;
using Unity.Entities;

namespace component.tags
{
    public enum UnitTags
    {
        SOLIDER,
        ZOMBIE
    }

    public struct SoliderUnitType : IComponentData
    {
    }

    public struct ZombieUnitType : IComponentData
    {
    }

    public struct Moving : IComponentData
    {
    }

    public struct Attacking : IComponentData
    {
    }
}
