using System;
using Controllers.Projectiles;
using Core;
using Pathfinding;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Controllers.Creatures {
    public class Enemy : Creature {
        [SerializeField] private float changeTarget;
        [SerializeField] private float maxDistanceFromPlayer, maxShootDistance;
        [SerializeField] private float rechargeTime;

        [SerializeField] private GameObject bulletPrefab;

        private bool _canShoot = false;
        private Vector3 _shootDirection;
        private Transform _target;

        private Vector3 ShootPosition => transform.position + _shootDirection.normalized * 15;

        public void SetTarget(Transform target) {
            _target = target;
            GetComponent<AIDestinationSetter>().target = target;
        }

        private void Recharge() => _canShoot = true;

        private void Shoot() {
            if (!_canShoot) return;

            _canShoot = false;
            var bullet = Instantiate(bulletPrefab, ShootPosition, Quaternion.identity)
                .GetComponent<Bullet>()
                .SetTarget(Bullet.BulletTarget.Everything)
                .SetColor(Color.green)
                .SetModifiers(enemyModifier: 0.5F);

            bullet.gameObject.GetComponent<Rigidbody>().velocity =
                _shootDirection.normalized * bulletPrefab.GetComponent<Projectile>().MovementSpeed;

            GlobalScope.ExecuteWithDelay(
                rechargeTime * Random.Range(0.7F, 1.3F),
                Recharge
            );
        }

        private void Freeze() => GetComponent<AIPath>().enabled = false;
        private void UnFreeze() => GetComponent<AIPath>().enabled = true;

        protected override void Start() {
            SetTarget(Player.Instance.transform);

            GameManager.Instance.RegisterEnemy(this);
            GlobalScope.ExecuteWithDelay(
                rechargeTime * Random.Range(0.7F, 1.3F),
                Recharge
            );

            var prevGuo = new GraphUpdateObject(GetComponent<Collider>().bounds) {updatePhysics = true};
            GlobalScope.ExecuteEveryInterval(
                0.1F, () => {
                    var guo = new GraphUpdateObject(GetComponent<Collider>().bounds) {updatePhysics = true};

                    AstarPath.active.UpdateGraphs(guo);
                    AstarPath.active.UpdateGraphs(prevGuo);

                    prevGuo = guo;
                }, () => false);
        }

        protected override void Update() {
            base.Update();

            if (_target == null) {
                _target = Player.Instance.transform;
            }

            if (Vector3.Distance(
                transform.position,
                _target.position
            ) > maxDistanceFromPlayer) {
                Freeze();
                return;
            }


            if (!Physics.SphereCast(ShootPosition, 10, _target.position, out var hit)) {
                UnFreeze();
                return;
            }


            if (Vector3.Distance(
                transform.position,
                _target.position
            ) <= maxShootDistance) {
                // we cah shoot
                _shootDirection = _target.position - transform.position;
                _shootDirection = new Vector3(_shootDirection.x, 0, _shootDirection.z);
                Shoot();
                Freeze();
            }
            else {
                UnFreeze();
            }
        }

        protected override void OnDeath() {
            base.OnDeath();
            GameManager.Instance.RemoveEnemy(this);
            GameManager.Instance.SpawnEnemies(2);
            GameManager.killedEnemiesCount += 1;
        }

        protected override void OnSwap() {
            base.OnSwap();
            GlobalScope.ExecuteWithDelay(3, UnFreeze, Freeze);
        }
    }
}