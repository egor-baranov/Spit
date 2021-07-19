using Controllers.Creatures;
using UnityEngine;

namespace Controllers.Projectiles {
    public class Bullet : Projectile {
        [SerializeField] private float damage;

        private BulletTarget _target;
        public void SetColor(Color color) => GetComponent<Light>().color = color;
        public void SetTarget(BulletTarget target) => _target = target;

        private void OnTriggerEnter(Collider other) {
            if (other.GetComponent<Bullet>()) {
                return;
            }

            var enemy = other.GetComponent<Enemy>();
            var player = other.GetComponent<Player>();

            if (_target == BulletTarget.Enemy && player != null || _target == BulletTarget.Player && enemy != null) {
                return;
            }

            if ((_target == BulletTarget.Enemy || _target == BulletTarget.Everything) && enemy != null) {
                enemy.ReceiveDamage(damage);
            }

            if ((_target == BulletTarget.Player || _target == BulletTarget.Everything) && player != null) {
                player.ReceiveDamage(damage);
            }

            Destroy(gameObject);
        }

        public enum BulletTarget {
            Enemy,
            Player,
            Everything
        }
    }
}