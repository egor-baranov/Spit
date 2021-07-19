using System.Collections.Generic;
using System.Linq;
using Controllers.Creatures;
using Core.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util.ExtensionMethods;
using Random = UnityEngine.Random;

namespace Core {
    public class GameManager : MonoBehaviour {
        public static GameManager Instance { get; private set; }

        public static int soulShotCount = 0, killedEnemiesCount = 0;

        public IEnumerable<Enemy> EnemyList => _enemyList;

        [SerializeField] private GameObject enemyPrefab;

        private readonly List<Enemy> _enemyList = new List<Enemy>();

        public static void OnDeath() =>
            UiManager.Instance.OnDeath(
                (int) Time.timeSinceLevelLoad * (soulShotCount + 1) * (killedEnemiesCount + 1)
            );

        public static void OnRestart() => SceneManager.LoadScene("Prototype");

        public Enemy SpawnEnemyAt(Vector3 position) =>
            Instantiate(enemyPrefab, position, Quaternion.identity).GetComponent<Enemy>();

        public void SpawnEnemies(int count) => 0.Until(count).ToList().ForEach(it =>
            Instantiate(
                enemyPrefab,
                new Vector3(Random.Range(-94, 94), 6, Random.Range(-142, 150)),
                Quaternion.identity
            )
        );

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