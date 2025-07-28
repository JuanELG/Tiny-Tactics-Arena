using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Samples.Asteroids.Client.UI
{
    public class ScoreUI : MonoBehaviour
    {
        private Text m_ScoreTextToUpdate;
        private EntityQuery m_ScoreQuery;
        private EntityManager m_EntityManager;

        void Start()
        {
            m_ScoreTextToUpdate = GetComponent<Text>();
            m_EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            m_ScoreQuery = m_EntityManager.CreateEntityQuery(ComponentType.ReadOnly<AsteroidScore>());
        }

        void Update()
        {
            if (m_EntityManager == null || m_ScoreQuery == default || m_ScoreQuery.IsEmpty)
                return;

            var score = m_ScoreQuery.GetSingleton<AsteroidScore>().Value;
            m_ScoreTextToUpdate.text = "Score: " + score;
        }
    }
}
