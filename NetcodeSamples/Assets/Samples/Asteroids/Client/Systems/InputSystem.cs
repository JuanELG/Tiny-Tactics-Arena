using Unity.Entities;
using UnityEngine;
using Unity.NetCode.Samples.Common;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class PlayerInputSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float left = (Input.GetKey("left") || TouchInput.GetKey(TouchInput.KeyCode.Left)) ? 1f : 0f;
        float right = (Input.GetKey("right") || TouchInput.GetKey(TouchInput.KeyCode.Right)) ? 1f : 0f;
        float thrust = (Input.GetKey("up") || TouchInput.GetKey(TouchInput.KeyCode.Up)) ? 1f : 0f;
        float shoot = (Input.GetKey("space") || TouchInput.GetKey(TouchInput.KeyCode.Space)) ? 1f : 0f;

        Entities
            .WithName("ApplyPlayerInput")
            .ForEach((ref ShipInputComponent input) =>
            {
                input.left = left;
                input.right = right;
                input.thrust = thrust;
                input.shoot = shoot;
            }).Run();
    }
}



public struct ShipInputComponent : IComponentData
{
    public float left;
    public float right;
    public float thrust;
    public float shoot;
}