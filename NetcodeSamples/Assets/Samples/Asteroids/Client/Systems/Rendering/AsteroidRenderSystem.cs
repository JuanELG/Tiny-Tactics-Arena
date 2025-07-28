using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;

[RequireMatchingQueriesForUpdate]
[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
[BurstCompile]
public partial struct AsteroidRenderSystem : ISystem
{
    private float m_Pulse;
    private float m_PulseDelta;
    private const float m_PulseMax = 1.2f;
    private const float m_PulseMin = 0.8f;

    ComponentLookup<URPMaterialPropertyBaseColor> m_URPMaterialPropertyBaseColorFromEntity;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        m_Pulse = 1;
        m_PulseDelta = 1;
        m_URPMaterialPropertyBaseColorFromEntity = state.GetComponentLookup<URPMaterialPropertyBaseColor>();
        state.RequireForUpdate<AsteroidTagComponentData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float astrScale = 30;

        m_Pulse += m_PulseDelta * SystemAPI.Time.DeltaTime;
        if (m_Pulse > m_PulseMax || m_Pulse < m_PulseMin)
        {
            m_PulseDelta = -m_PulseDelta;
            m_Pulse = math.clamp(m_Pulse, m_PulseMin, m_PulseMax);
        }

        float pulse = m_Pulse;

        m_URPMaterialPropertyBaseColorFromEntity.Update(ref state);
        var scaleJob = new ScaleAsteroids
        {
            colorFromEntity = m_URPMaterialPropertyBaseColorFromEntity,
            astrScale = astrScale,
            pulse = pulse
        };
        state.Dependency = scaleJob.ScheduleParallel(state.Dependency);
    }

    [WithAll(typeof(AsteroidTagComponentData))]
    [BurstCompile]
    partial struct ScaleAsteroids : IJobEntity
    {
        [NativeDisableParallelForRestriction]
        public ComponentLookup<URPMaterialPropertyBaseColor> colorFromEntity;

        public float astrScale;
        public float pulse;

        public void Execute(Entity entity, ref LocalTransform localTransform)
        {
            if (colorFromEntity.HasComponent(entity))
                colorFromEntity[entity] = new URPMaterialPropertyBaseColor { Value = new float4(1, 1, 1, 1) };

            localTransform.Scale = astrScale * pulse;
        }
    }
}