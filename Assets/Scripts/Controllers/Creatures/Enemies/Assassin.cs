using Controllers.Creatures.Base;
using Controllers.Creatures.Enemies.Base;
using Controllers.Projectiles;
using Controllers.Projectiles.Base;
using Core;
using Pathfinding;
using UnityEngine;

namespace Controllers.Creatures.Enemies {
    public class Assassin : Enemy {
        public override EnemyType Type => EnemyType.Assassin;

        public override Bullet.Builder BulletBuilder(Vector3 bulletPosition) =>
            new Bullet.Builder(
                Instantiate(bulletPrefab, bulletPosition, Quaternion.identity).GetComponent<Bullet>()
            );

        public override void SetTarget(Transform target) {
            base.SetTarget(target);
            GetComponent<AIDestinationSetter>().target = target;
        }

        protected override void Shoot() {
            if (!CanShoot) return;
            base.Shoot();

            var bullet = BulletBuilder(ShootPosition)
                .SetTarget(Bullet.BulletTarget.Everything)
                .SetColor(Color.green)
                .SetModifiers(enemyModifier: 0.5F)
                .Result();

            bullet.gameObject.GetComponent<Rigidbody>().velocity =
                ShootDirection.normalized * bullet.GetComponent<Projectile>().MovementSpeed;
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
            if (Target == null) {
                Target = Player.Instance.transform;
            }

            if (Vector3.Distance(transform.position, Target.position) > maxDistanceFromPlayer) {
                Freeze();
                return;
            }

            if (!Physics.SphereCast(ShootPosition, 10, Target.position, out var hit) ||
                !hit.collider.gameObject.GetComponent<Player>()) {
                UnFreeze();
            }

            if (Vector3.Distance(transform.position, Target.position) <= maxShootDistance) {
                ShootDirection = Target.position - transform.position;
                ShootDirection = new Vector3(ShootDirection.x, 0, ShootDirection.z);
                Shoot();
                Freeze();
            }
            else {
                UnFreeze();
            }
        }

        protected override void OnSwap(Creature other) {
            GlobalScope.ExecuteWithDelay(3, UnFreeze, Freeze);
        }
    }
}