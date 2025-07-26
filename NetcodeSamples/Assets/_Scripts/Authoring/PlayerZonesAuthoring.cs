using Unity.Entities;
using UnityEngine;

public class PlayerZonesAuthoring : MonoBehaviour
{
    [Header("World width extent (left to right, centered on X = 0)")]
    public float horizontalExtent;

    [Header("Vertical height of each player zone")]
    public float zoneHeight;
}

public class PlayerZonesBaker : Baker<PlayerZonesAuthoring>
{
    public override void Bake(PlayerZonesAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);

        AddComponent(entity, new PlayerZonesComponent
        {
            HorizontalExtent = authoring.horizontalExtent,
            ZoneHeight = authoring.zoneHeight
        });
    }
}