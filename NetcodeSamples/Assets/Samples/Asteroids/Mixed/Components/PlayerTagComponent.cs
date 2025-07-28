using Unity.Entities;

public struct ShipTagComponentData : IComponentData
{
}

public struct ShipStateComponentData : IComponentData
{
    public int State;
    public float WeaponCooldownTimer;
    public float WeaponCooldownDuration;
}
