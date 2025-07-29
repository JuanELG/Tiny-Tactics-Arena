using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class GameSettingsAuthoring : MonoBehaviour
{
    [RegisterBinding(typeof(GameSettings), "levelData")]
    public LevelComponent levelData;
    [RegisterBinding(typeof(GameSettings), "positioningDuration")]
    public float positioningDuration = 10f;

    class Baker : Baker<GameSettingsAuthoring>
    {
        public override void Bake(GameSettingsAuthoring authoring)
        {
            GameSettings component = default(GameSettings);
            component.levelData = authoring.levelData;
            component.positioningDuration = authoring.positioningDuration;
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, component);
        }
    }
}
