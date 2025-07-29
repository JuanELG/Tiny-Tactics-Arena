using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class PositioningTimerUI : MonoBehaviour
{
    private Text timerText;
    private EntityManager entityManager;
    private EntityQuery query;

    void Start()
    {
        timerText = GetComponent<Text>();
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        query = entityManager.CreateEntityQuery(typeof(GamePhaseComponent));
    }

    void Update()
    {
        if (entityManager == null || query == null || query.IsEmptyIgnoreFilter)
        {
            timerText.text = "";
            return;
        }

        var gamePhase = query.GetSingleton<GamePhaseComponent>();

        if (gamePhase.Value == GamePhase.ShipPositioning || gamePhase.Value == GamePhase.AsteroidsPositioning)
            timerText.text = $"Deploy Units: {Mathf.CeilToInt(gamePhase.Timer)}s";
        else
            timerText.text = "";
    }
}
