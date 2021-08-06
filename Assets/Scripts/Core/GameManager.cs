using System;
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

        public static int SoulShotCount = 0, KilledEnemiesCount = 0;

        public IEnumerable<Enemy> EnemyList => _enemyList;

        public LayerMask WallLayerMask => wallLayerMask;

        [SerializeField] private GameObject assassinPrefab, turretPrefab;
        [SerializeField] private int spawnEnemyCount;
        [SerializeField] private LayerMask wallLayerMask;

        private readonly List<Enemy> _enemyList = new List<Enemy>();

        public static void OnDeath() =>
            UiManager.Instance.OnDeath(
                (int) Time.timeSinceLevelLoad * (SoulShotCount + 1) * (KilledEnemiesCount + 1)
            );

        public static void OnRestart() => SceneManager.LoadScene("Prototype");

        public Enemy SpawnEnemyAt(Vector3 position) =>
            Instantiate(GetRandomEnemyPrefab(), position, Quaternion.identity).GetComponent<Enemy>();

        public Enemy SpawnEnemyAt(Vector3 position, Enemy.EnemyType enemyType) =>
            Instantiate(GetEnemyWithType(enemyType), position, Quaternion.identity).GetComponent<Enemy>();

        public void SpawnEnemies(int count) {
            var list = new List<Enemy>();
            _enemyList.ToList().ForEach(it => list.Add(it));
            foreach (var i in 0.Until(count)) {
                list.Add(Instantiate(
                    GetRandomEnemyPrefab(),
                    FindPointWithCondition(point =>
                        !Physics.OverlapSphere(point, 50, wallLayerMask).Any() &&
                        list.All(it => Vector3.Distance(it.transform.position, point) > 70)
                    ),
                    Quaternion.identity
                ).GetComponent<Enemy>());
            }
        }

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
                    (int) Time.timeSinceLevelLoad * (SoulShotCount + 1) * (KilledEnemiesCount + 1),
                    (int) Time.timeSinceLevelLoad, KilledEnemiesCount, SoulShotCount
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

        private static Vector3 FindPointWithCondition(Func<Vector3, bool> condition) {
            var point = new Vector3(Random.Range(-400, 400), 6, Random.Range(-400, 400));
            foreach (var _ in 0.Until(30000)) {
                if (condition(point)) {
                    return point;
                }

                point = new Vector3(Random.Range(-400, 400), 6, Random.Range(-400, 400));
            }

            return point;
        }
    }
}