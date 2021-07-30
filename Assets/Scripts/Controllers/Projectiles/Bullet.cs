using Controllers.Creatures;
using Controllers.Creatures.Enemies.Base;
using UnityEngine;

namespace Controllers.Projectiles {
    public class Bullet : Projectile {
        [SerializeField] private float damage;

        private float _playerModifier = 1, _enemyModifier = 1;
        private BulletTarget _target = BulletTarget.Everything;

        private GameObject Halo => transform.Find("Halo").gameObject;

        public Bullet SetColor(Color color) {
            GetComponent<Light>().color = color;
            Halo.GetComponent<Light>().color = color;
            return this;
        }

        public Bullet SetTarget(BulletTarget target) {
            _target = target;
            return this;
        }

        public Bullet SetModifiers(float playerModifier = 1F, float enemyModifier = 1F) {
            _playerModifier = playerModifier;
            _enemyModifier = enemyModifier;
            return this;
        }

        public Bullet SetSpeedModifier(float speedModifier) {
            movementSpeed *= speedModifier;
            return this;
        }

        public Bullet SetScale(float scale) {
            transform.localScale = Vector3.one * scale;
            GetComponent<Light>().range *= scale;
            Halo.GetComponent<Light>().range *= scale;
            return this;
        }

        public Bullet SetDamageModifier(float damageModifier) {
            damage *= damageModifier;
            return this;
        }

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
                enemy.ReceiveDamage(damage * _enemyModifier);
            }

            if ((_target == BulletTarget.Player || _target == BulletTarget.Everything) && player != null) {
                player.ReceiveDamage(damage * _playerModifier);
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