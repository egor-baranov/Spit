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

        private Vector3 ShootPosition => transform.position + _shootDirection.normalized * 15;

        private void Recharge() => _canShoot = true;

        private void Shoot() {
            if (!_canShoot) return;

            _canShoot = false;
            var bullet = Instantiate(bulletPrefab, ShootPosition, Quaternion.identity).GetComponent<Bullet>();
            bullet.gameObject.GetComponent<Rigidbody>().velocity =
                _shootDirection.normalized * bulletPrefab.GetComponent<Projectile>().MovementSpeed;

            bullet.SetTarget(Bullet.BulletTarget.Player);
            bullet.SetColor(Color.green);

            GlobalScope.ExecuteWithDelay(
                rechargeTime * Random.Range(0.7F, 1.3F),
                Recharge
            );
        }

        private void Freeze() => GetComponent<AIPath>().enabled = false;
        private void UnFreeze() => GetComponent<AIPath>().enabled = true;

        protected override void Start() {
            GetComponent<AIDestinationSetter>().target = Player.Instance.transform;

            GameManager.Instance.RegisterEnemy(this);
            GlobalScope.ExecuteWithDelay(
                rechargeTime * Random.Range(0.7F, 1.3F),
                Recharge
            );
        }

        protected override void Update() {
            base.Update();

            if (Vector3.Distance(
                transform.position,
                Player.Instance.transform.position
            ) > maxDistanceFromPlayer || Player.Instance.IsFreezed) {
                Freeze();
                return;
            }


            if (!Physics.Linecast(ShootPosition, Player.Instance.transform.position, out var hit)) {
                return;
            }
            
            if (hit.transform.GetComponent<Player>() && Vector3.Distance(
                transform.position,
                Player.Instance.transform.position
            ) <= maxShootDistance) {
                // we cah shoot
                _shootDirection = Player.Instance.transform.position - transform.position;
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