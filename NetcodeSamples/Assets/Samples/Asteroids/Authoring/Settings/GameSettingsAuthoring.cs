using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class GameSettingsAuthoring : MonoBehaviour
{
    [RegisterBinding(typeof(GameSettings), "levelData")]
    public LevelComponent levelData;

    class Baker : Baker<GameSettingsAuthoring>
    {
        public override void Bake(GameSettingsAuthoring authoring)
        {
            GameSettings component = default(GameSettings);
            component.levelData = authoring.levelData;
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, component);
        }
    }
}
