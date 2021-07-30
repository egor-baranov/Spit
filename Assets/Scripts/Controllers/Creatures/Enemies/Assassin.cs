﻿using Controllers.Creatures.Enemies.Base;
using Controllers.Projectiles;
using Core;
using Pathfinding;
using UnityEngine;

namespace Controllers.Creatures.Enemies {
    public class Assassin : Enemy {
        public override void SetTarget(Transform target) {
            base.SetTarget(target);
            GetComponent<AIDestinationSetter>().target = target;
        }

        protected override void Shoot() {
            if (!CanShoot) return;
            base.Shoot();

            var bullet = Instantiate(bulletPrefab, ShootPosition, Quaternion.identity)
                .GetComponent<Bullet>()
                .SetTarget(Bullet.BulletTarget.Everything)
                .SetColor(Color.green)
                .SetModifiers(enemyModifier: 0.5F);

            bullet.gameObject.GetComponent<Rigidbody>().velocity =
                _shootDirection.normalized * bullet.GetComponent<Projectile>().MovementSpeed;
        }

        private void Freeze() => GetComponent<AIPath>().enabled = false;
        private void UnFreeze() => GetComponent<AIPath>().enabled = true;

        protected override void Start() {
            base.Start();

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

            if (Target == null) {
                Target = Player.Instance.transform;
            }

            if (Vector3.Distance(transform.position, Target.position) > maxDistanceFromPlayer) {
                Freeze();
                return;
            }

            if (!Physics.SphereCast(ShootPosition, 10, Target.position, out var hit)) {
                UnFreeze();
                return;
            }

            if (Vector3.Distance(transform.position, Target.position) <= maxShootDistance) {
                // we cah shoot
                _shootDirection = Target.position - transform.position;
                _shootDirection = new Vector3(_shootDirection.x, 0, _shootDirection.z);
                Shoot();
                Freeze();
            }
            else {
                UnFreeze();
            }
        }

        protected override void OnSwap() {
            base.OnSwap();
            GlobalScope.ExecuteWithDelay(3, UnFreeze, Freeze);
        }
    }
}