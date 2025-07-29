using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class RetryButtonUI : MonoBehaviour
{
    [SerializeField] private Button retryButton;

    private EntityManager entityManager;
    private EntityQuery phaseQuery;
    private EntityQuery playerDestroyedQuery;

    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        phaseQuery = entityManager.CreateEntityQuery(typeof(GamePhaseComponent));
        playerDestroyedQuery = entityManager.CreateEntityQuery(typeof(PlayerDestroyedTag));

        retryButton.gameObject.SetActive(false);
        retryButton.onClick.AddListener(OnRetryClicked);
    }

    void Update()
    {
        if (!phaseQuery.IsEmptyIgnoreFilter &&
            !playerDestroyedQuery.IsEmptyIgnoreFilter)
        {
            var phase = phaseQuery.GetSingleton<GamePhaseComponent>();
            if (phase.Value == GamePhase.Battle)
            {
                retryButton.gameObject.SetActive(true);
                return;
            }
        }

        retryButton.gameObject.SetActive(false);
    }

    private void OnRetryClicked()
    {
        retryButton.gameObject.SetActive(false);
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
