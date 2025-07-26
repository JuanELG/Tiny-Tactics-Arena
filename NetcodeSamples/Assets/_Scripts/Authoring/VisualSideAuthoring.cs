using Unity.Entities;
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

        AddComponent(entity, new VisualSideComponent
        {
            Side = authoring.Side
        });
    }
}