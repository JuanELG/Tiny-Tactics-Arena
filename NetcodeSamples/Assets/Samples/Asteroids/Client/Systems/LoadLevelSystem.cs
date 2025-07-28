using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct LevelVisualSetupSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameSettings>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var level = SystemAPI.GetSingleton<GameSettings>().levelData;

        foreach (var (trans, scaleMatrix, border) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<PostTransformMatrix>, RefRO<LevelBorder>>())
        {
            float3 scale = new float3(1);

            switch (border.ValueRO.Side)
            {
                case 0:
                    trans.ValueRW.Position = new float3(level.levelWidth / 2f, 1, 0);
                    scale = new float3(level.levelWidth, 2, 1);
                    break;
                case 1:
                    trans.ValueRW.Position = new float3(level.levelWidth / 2f, level.levelHeight - 1, 0);
                    scale = new float3(level.levelWidth, 2, 1);
                    break;
                case 2:
                    trans.ValueRW.Position = new float3(1, level.levelHeight / 2f, 0);
                    scale = new float3(2, level.levelHeight, 1);
                    break;
                case 3:
                    trans.ValueRW.Position = new float3(level.levelWidth - 1, level.levelHeight / 2f, 0);
                    scale = new float3(2, level.levelHeight, 1);
                    break;
            }

            scaleMatrix.ValueRW.Value = float4x4.Scale(scale);
        }
    }
}
