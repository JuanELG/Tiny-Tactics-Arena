using Unity.Burst;
using Unity.Entities;
using Unity.NetCode.Samples.Common;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct PlayerInputSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float left = Input.GetKey("left") || TouchInput.GetKey(TouchInput.KeyCode.Left) ? 1f : 0f;
        float right = Input.GetKey("right") || TouchInput.GetKey(TouchInput.KeyCode.Right) ? 1f : 0f;
        float thrust = Input.GetKey("up") || TouchInput.GetKey(TouchInput.KeyCode.Up) ? 1f : 0f;
        float shoot = Input.GetKey("space") || TouchInput.GetKey(TouchInput.KeyCode.Space) ? 1f : 0f;

        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (transform, input, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<ShipInputComponent>>().WithEntityAccess())
        {
            input.ValueRW.left = left;
            input.ValueRW.right = right;
            input.ValueRW.thrust = thrust;
            input.ValueRW.shoot = shoot;
        }
    }
}


public struct ShipInputComponent : IComponentData
{
    public float left;
    public float right;
    public float thrust;
    public float shoot;
}