using System;
using System.Linq;
using Core.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util.ExtensionMethods;

namespace Core {
    public class GameManager : MonoBehaviour {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private int enemyCount;

        public void OnDeath() => UiManager.Instance.OnDeath();
        public void OnRestart() => SceneManager.LoadScene("Prototype");
        public void SpawnEnemies(int count) => 0.Until(count).ToList().ForEach(it => Instantiate(enemyPrefab));

        private void Awake() {
            if (Instance != null) {
                Destroy(gameObject);
            }

            Instance = this;
        }

        private void Start() {
            SpawnEnemies(2);
        }
    }
}