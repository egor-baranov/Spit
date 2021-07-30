using System.Collections.Generic;
using System.Linq;
using Controllers.Creatures;
using Controllers.Creatures.Enemies.Base;
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

        [SerializeField] private GameObject assassinPrefab, turretPrefab;
        [SerializeField] private int spawnEnemyCount;

        private readonly List<Enemy> _enemyList = new List<Enemy>();

        public static void OnDeath() =>
            UiManager.Instance.OnDeath(
                (int) Time.timeSinceLevelLoad * (soulShotCount + 1) * (killedEnemiesCount + 1)
            );

        public static void OnRestart() => SceneManager.LoadScene("Prototype");

        public Enemy SpawnEnemyAt(Vector3 position) =>
            Instantiate(GetRandomEnemyPrefab(), position, Quaternion.identity).GetComponent<Enemy>();

        public Enemy SpawnEnemyAt(Vector3 position, Enemy.EnemyType enemyType) =>
            Instantiate(GetEnemyWithType(enemyType), position, Quaternion.identity).GetComponent<Enemy>();

        public void SpawnEnemies(int count) => 0.Until(count).ToList().ForEach(it =>
            Instantiate(
                GetRandomEnemyPrefab(),
                new Vector3(Random.Range(-200, 200), 6, Random.Range(-200, 200)),
                Quaternion.identity
            )
        );

        public void RegisterEnemy(Enemy enemy) => _enemyList.Add(enemy);
        public void RemoveEnemy(Enemy enemy) => _enemyList.Remove(enemy);
        public void SetTargetForAllEnemies(Transform target) => _enemyList.ForEach(it => it.SetTarget(target));

        private void Awake() {
            if (Instance != null) {
                Destroy(gameObject);
            }

            Instance = this;
        }

        private void Start() {
            SpawnEnemies(spawnEnemyCount);
        }

        private void Update() {
            if (Player.Instance.HealthPoints > 0) {
                UiManager.Instance.DisplayScore(
                    (int) Time.timeSinceLevelLoad * (soulShotCount + 1) * (killedEnemiesCount + 1),
                    (int) Time.timeSinceLevelLoad, killedEnemiesCount, soulShotCount
                );
            }
        }

        private GameObject GetRandomEnemyPrefab() =>
            new List<GameObject> {assassinPrefab, turretPrefab}.RandomElement();

        private GameObject GetEnemyWithType(Enemy.EnemyType enemyType) {
            switch (enemyType) {
                case Enemy.EnemyType.Assassin:
                    return assassinPrefab;
                case Enemy.EnemyType.Turret:
                    return turretPrefab;
                default:
                    return turretPrefab;
            }
        }
    }
}