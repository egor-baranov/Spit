using System;
using System.Collections.Generic;
using System.Linq;
using Controllers.Creatures;
using Core.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util.ExtensionMethods;

namespace Core {
    public class GameManager : MonoBehaviour {
        public static GameManager Instance { get; private set; }

        public IEnumerable<Enemy> EnemyList => _enemyList;

        [SerializeField] private GameObject enemyPrefab;

        private readonly List<Enemy> _enemyList = new List<Enemy>();

        public static void OnDeath() => UiManager.Instance.OnDeath();
        public static void OnRestart() => SceneManager.LoadScene("Prototype");
        
        public void SpawnEnemies(int count) => 0.Until(count).ToList().ForEach(it => Instantiate(enemyPrefab));
        public void RegisterEnemy(Enemy enemy) => _enemyList.Add(enemy);
        public void RemoveEnemy(Enemy enemy) => _enemyList.Remove(enemy);

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