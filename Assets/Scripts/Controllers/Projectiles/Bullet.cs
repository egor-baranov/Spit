using Controllers.Creatures;
using Controllers.Creatures.Enemies.Base;
using Controllers.Projectiles.Base;
using UnityEngine;

namespace Controllers.Projectiles {
    public class Bullet : Projectile {
        [SerializeField] private float damage;

        private float _playerModifier = 1, _enemyModifier = 1;
        private BulletTarget _target = BulletTarget.Everything;

        private GameObject Halo => transform.Find("Halo").gameObject;

        private void OnTriggerEnter(Collider other) {
            if (other.GetComponent<Bullet>()) {
                return;
            }

            if (_target == BulletTarget.Enemy || _target == BulletTarget.Everything) {
                other.GetComponent<Enemy>()?.ReceiveDamage(damage * _enemyModifier);
            }

            if (_target == BulletTarget.Player || _target == BulletTarget.Everything) {
                other.GetComponent<Player>()?.ReceiveDamage(damage * _playerModifier);
            }

            Destroy(gameObject);
        }

        public enum BulletTarget {
            Enemy,
            Player,
            Everything
        }

        protected override void Awake() { }

        protected override void Start() { }

        protected override void Update() { }

        protected override void OnDestroy() { }

        public class Builder {
            private readonly Bullet _bullet;
            public Builder(Bullet bullet) => _bullet = bullet;

            public Builder SetColor(Color color) {
                _bullet.GetComponent<Light>().color = color;
                _bullet.Halo.GetComponent<Light>().color = color;
                return this;
            }

            public Builder SetTarget(BulletTarget bulletTarget) {
                _bullet._target = bulletTarget;
                return this;
            }

            public Builder SetModifiers(float playerModifier = 1F, float enemyModifier = 1F) {
                _bullet._playerModifier = playerModifier;
                _bullet._enemyModifier = enemyModifier;
                return this;
            }
            
            public Builder SetSpeedModifier(float speedModifier) {
                _bullet.movementSpeed *= speedModifier;
                return this;
            }

            public Builder SetScale(float scale) {
                _bullet.transform.localScale = Vector3.one * scale;
                _bullet.GetComponent<Light>().range *= scale;
                _bullet.Halo.GetComponent<Light>().range *= scale;
                return this;
            }

            public Builder SetDamageModifier(float damageModifier) {
                _bullet.damage *= damageModifier;
                return this;
            }

            public Bullet Result() => _bullet;
        }
    }
}