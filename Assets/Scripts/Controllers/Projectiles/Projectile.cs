using UnityEngine;

namespace Controllers.Projectiles {
    public class Projectile : MonoBehaviour {
        public float MovementSpeed => movementSpeed;

        [SerializeField] protected float maxTimeAlive;
        [SerializeField] protected float movementSpeed;

        protected virtual void Awake() { }
        protected virtual void Start() { }
        protected virtual void Update() { }

        protected virtual void OnDestroy() { }
    }
}