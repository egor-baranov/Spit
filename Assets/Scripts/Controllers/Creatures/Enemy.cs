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

        protected override void Awake() {
            base.Awake();
        }

        protected override void Start() {
            GetComponent<AIDestinationSetter>().target = Player.Instance.transform;
            transform.position = GenerateNextPoint();
            _targetPosition = GenerateNextPoint();
        }

        protected override void Update() {
            base.Update();
            GetComponent<AIDestinationSetter>().enabled = Vector3.Distance(
                transform.position,
                Player.Instance.transform.position
            ) <= maxDistanceFromPlayer;
        }

        protected override void OnDeath() {
            base.OnDeath();
            GameManager.Instance.SpawnEnemies(2);
        }
    }
}