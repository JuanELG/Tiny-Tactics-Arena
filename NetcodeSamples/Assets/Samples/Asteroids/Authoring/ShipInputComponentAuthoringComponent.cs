using Unity.Entities;
using UnityEngine;

public class ShipInputComponentAuthoringComponent : MonoBehaviour
{
    public class ShipInputBaker : Baker<ShipInputComponentAuthoringComponent>
    {
        public override void Bake(ShipInputComponentAuthoringComponent authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<ShipInputComponent>(entity);
        }
    }
}