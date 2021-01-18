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
}
