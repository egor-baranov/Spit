using UnityEngine;
using Random = UnityEngine.Random;

namespace Controllers {
    public class Enemy : Creature {
        [SerializeField] private float changeTarget;

        private Vector3 _targetPosition;

        private static Vector3 GenerateNextPoint() {
            return new Vector3(Random.Range(-94, 94), 6, Random.Range(-142, 150));
        }

        protected override void Awake() {
            base.Awake();
        }
        
        protected override void Start() {
            transform.position = GenerateNextPoint();
            _targetPosition = GenerateNextPoint();
        }

        protected override void Update() {
            if (Vector3.Distance(transform.position, _targetPosition) < changeTarget) {
                _targetPosition = GenerateNextPoint();
            }

            GetComponent<Rigidbody>().velocity = (_targetPosition - transform.position).normalized * movementSpeed;
        }
    }
}