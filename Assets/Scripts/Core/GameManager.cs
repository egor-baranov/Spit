using Core.Managers;
using UnityEngine;

namespace Core {
    public class GameManager : MonoBehaviour {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private int enemyCount;

        public void OnDeath() => UiManager.Instance.OnDeath();

        private void Awake() {
            if (Instance != null) {
                Destroy(gameObject);
            }

            Instance = this;

            for (var i = 0; i < enemyCount; i++) {
                Instantiate(enemyPrefab);
            }
        }
    }
}