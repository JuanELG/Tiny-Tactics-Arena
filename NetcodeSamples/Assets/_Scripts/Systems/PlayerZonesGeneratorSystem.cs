using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct PlayerZonesGeneratorSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameSettings>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var level = SystemAPI.GetSingleton<GameSettings>().levelData;

        float third = level.levelHeight / 3f;

        foreach (var (side, trans, scaleMatrix) in SystemAPI.Query<
                     RefRO<VisualSideComponent>,
                     RefRW<LocalTransform>,
                     RefRW<PostTransformMatrix>>())
        {
            float3 scale = new float3(level.levelWidth, 5f, 1f);
            float3 pos = float3.zero;

            switch (side.ValueRO.Side)
            {
                case PlayerSide.Bottom:
                    pos = new float3(level.levelWidth / 2f, third, 0f);
                    break;

                case PlayerSide.Top:
                    pos = new float3(level.levelWidth / 2f, third * 2f, 0f);
                    break;
            }

            trans.ValueRW.Position = pos;
            scaleMatrix.ValueRW.Value = float4x4.Scale(scale);
        }
    }
}
