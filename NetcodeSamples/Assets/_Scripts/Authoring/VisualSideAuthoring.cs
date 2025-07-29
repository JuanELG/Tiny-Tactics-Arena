using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class VisualSideAuthoring : MonoBehaviour
{
    public PlayerSide Side;
}

public class VisualSideBaker : Baker<VisualSideAuthoring>
{
    public override void Bake(VisualSideAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new VisualSideComponent { Side = authoring.Side });
        AddComponent(entity, new CollisionBoxComponent { halfExtents = float2.zero });
    }
}