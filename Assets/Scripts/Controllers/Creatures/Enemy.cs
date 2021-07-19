using Core;
using Pathfinding;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Controllers.Creatures {
    public class Enemy : Creature {
        [SerializeField] private float changeTarget;
        [SerializeField] private float maxDistanceFromPlayer;

        private Vector3 _targetPosition;

        private static Vector3 GenerateNextPoint() {
            return new Vector3(Random.Range(-94, 94), 6, Random.Range(-142, 150));
        }

        private void Freeze() => GetComponent<AIDestinationSetter>().enabled = false;
        private void UnFreeze() => GetComponent<AIDestinationSetter>().enabled = true;

        protected override void Start() {
            GetComponent<AIDestinationSetter>().target = Player.Instance.transform;

            GameManager.Instance.RegisterEnemy(this);
        }

        protected override void Update() {
            base.Update();

            if ((Vector3.Distance(
                transform.position,
                Player.Instance.transform.position
            ) > maxDistanceFromPlayer) || Player.Instance.IsFreezed) {
                Freeze();
            }
            else {
                UnFreeze();
            }
        }

        protected override void OnDeath() {
            base.OnDeath();
            GameManager.Instance.RemoveEnemy(this);
            GameManager.Instance.SpawnEnemies(2);
            GameManager.killedEnemiesCount += 1;
        }

        protected override void OnSwap() {
            base.OnSwap();
            GlobalScope.ExecuteWithDelay(3, UnFreeze, Freeze);
        }
    }
}