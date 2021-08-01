using Controllers.Animation;
using Controllers.Creatures.Base;
using Controllers.Projectiles;
using Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Controllers.Creatures.Enemies.Base {
    public abstract class Enemy : Creature {
        public abstract EnemyType Type { get; }
        public abstract Bullet.Builder BulletBuilder(Vector3 bulletPosition);

        [SerializeField] protected float maxDistanceFromPlayer, maxShootDistance;
        [SerializeField] protected GameObject bulletPrefab;

        protected bool CanShoot;
        protected Vector3 ShootDirection;
        protected Transform Target;

        protected virtual Vector3 ShootPosition => transform.position + ShootDirection.normalized * 18;

        public virtual void SetTarget(Transform target) {
            Target = target;
        }

        protected virtual void Recharge() => CanShoot = true;

        protected virtual void Shoot() {
            if (!CanShoot) return;

            CanShoot = false;

            GlobalScope.ExecuteWithDelay(
                rechargeTime * Random.Range(0.7F, 1.3F),
                Recharge
            );
        }

        protected override void Start() {
            SetTarget(Player.Instance.transform);

            GameManager.Instance.RegisterEnemy(this);
            GlobalScope.ExecuteWithDelay(
                rechargeTime * Random.Range(0.7F, 1.3F),
                Recharge
            );
        }

        protected override void OnDeath() {
            base.OnDeath();
            Player.Instance.RechargeSoulBlast();
            GameManager.Instance.RemoveEnemy(this);
            GameManager.Instance.SpawnEnemies(2);
            GameManager.KilledEnemiesCount += 1;
        }

        protected override void OnReceiveDamage() {
            Body.GetComponent<CreatureAnimationController>()?
                .SetColor(new Color(0.4F, 1F, 0.4F, 1F), 2);
        }

        protected void ApplyAutoShooting() {
            if (Target == null) {
                Target = Player.Instance.transform;
            }

            ShootDirection = Target.position - transform.position;
            ShootDirection = new Vector3(ShootDirection.x, 0, ShootDirection.z);


            if (Vector3.Distance(transform.position, Target.position) > maxDistanceFromPlayer) {
                return;
            }

            if (!Physics.SphereCast(ShootPosition, 1, Target.position, out var hit) ||
                hit.collider.CompareTag("Player")) {
                return;
            }

            if (Vector3.Distance(transform.position, Target.position) <= maxShootDistance) {
                Shoot();
            }
        }

        public enum EnemyType {
            Assassin,
            Turret
        }
    }
}