using Unity.Entities;
using UnityEngine;

public class ShipStateAuthoringComponent : MonoBehaviour
{
    public float cooldown = 0.2f;

    public class Baker : Baker<ShipStateAuthoringComponent>
    {
        public override void Bake(ShipStateAuthoringComponent authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ShipStateComponentData
            {
                State = 0,
                WeaponCooldownTimer = 0f,
                WeaponCooldownDuration = authoring.cooldown
            });
        }
    }

}
