using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct PlayerZonesGeneratorSystem : ISystem
{

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LevelComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var level = SystemAPI.GetSingleton<LevelComponent>();

        foreach (var (side, trans, scaleMatrix) in SystemAPI.Query<
                     RefRO<VisualSideComponent>,
                     RefRW<LocalTransform>,
                     RefRW<PostTransformMatrix>>())
        {
            float3 scale = new float3(1, 1, 1);
            float3 pos = float3.zero;

            switch (side.ValueRO.Side)
            {
                case PlayerSide.Top:
                    pos = new float3(level.levelWidth / 2f, level.levelHeight - 1f, 0);
                    scale = new float3(level.levelWidth, 2f, 1f);
                    break;

                case PlayerSide.Bottom:
                    pos = new float3(level.levelWidth / 2f, 1f, 0);
                    scale = new float3(level.levelWidth, 2f, 1f);
                    break;
            }

            trans.ValueRW.Position = pos;
            scaleMatrix.ValueRW.Value = float4x4.Scale(scale);
        }
    }
}
