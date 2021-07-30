using Controllers.Creatures.Base;
using Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Controllers.Creatures.Enemies.Base {
    public class Enemy : Creature {
        [SerializeField] protected float changeTarget;
        [SerializeField] protected float maxDistanceFromPlayer, maxShootDistance;
        [SerializeField] protected float rechargeTime;

        [SerializeField] protected GameObject bulletPrefab;

        protected bool CanShoot = false;
        protected Vector3 _shootDirection;
        protected Transform Target;

        protected virtual Vector3 ShootPosition => transform.position + _shootDirection.normalized * 18;

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
            base.Start();

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
            GameManager.killedEnemiesCount += 1;
        }

        protected void ApplyAutoShooting() {
            if (Target == null) {
                Target = Player.Instance.transform;
            }

            _shootDirection = Target.position - transform.position;
            _shootDirection = new Vector3(_shootDirection.x, 0, _shootDirection.z);


            if (Vector3.Distance(transform.position, Target.position) > maxDistanceFromPlayer) {
                return;
            }

            if (!Physics.SphereCast(ShootPosition, 10, Target.position, out var hit)) {
                return;
            }

            if (Vector3.Distance(transform.position, Target.position) <= maxShootDistance) {
                _shootDirection = Target.position - transform.position;
                _shootDirection = new Vector3(_shootDirection.x, 0, _shootDirection.z);
                Shoot();
            }
        }

        public enum EnemyType {
            Assassin, 
            Turret
        }
    }
}