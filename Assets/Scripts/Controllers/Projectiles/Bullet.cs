using System;
using Controllers.Creatures;
using UnityEngine;

namespace Controllers.Projectiles {
    public class Bullet : Projectile {
        [SerializeField] private float damage;

        private void OnTriggerEnter(Collider other) {
            try {
                var enemy = other.transform.parent.GetComponent<Enemy>();
                if (enemy.gameObject != Player.Instance.gameObject) {
                    enemy.ReceiveDamage(damage);
                    Destroy(gameObject);
                }
            }
            catch (NullReferenceException) { }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
        }
    }
}