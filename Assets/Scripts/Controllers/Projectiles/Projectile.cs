using UnityEngine;

namespace Controllers.Projectiles {
    public abstract class Projectile : MonoBehaviour {
        public float MovementSpeed => movementSpeed;

        [SerializeField] protected float maxTimeAlive;
        [SerializeField] protected float movementSpeed;

        protected abstract void Awake();
        protected abstract void Start();
        protected abstract void Update();

        protected abstract void OnDestroy();
    }
}