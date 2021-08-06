using System.Collections.Generic;
using System.Linq;
using Controllers.Creatures.Base;
using Controllers.Creatures.Enemies.Base;
using Controllers.Projectiles;
using Controllers.Projectiles.Base;
using Core;
using Pathfinding;
using UnityEngine;
using Util.ExtensionMethods;

namespace Controllers.Creatures.Enemies {
    public class Assassin : Enemy {
        public override EnemyType Type => EnemyType.Assassin;

        [SerializeField] private float runAwayDistance;
        private Vector3 _lastPoint;

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
                .SetSpeedModifier(0.5F)
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
                },
                () => false
            );

            PickNextPoint();
        }

        protected override void Update() {
            if (Target == null) {
                Target = Player.Instance.transform;
            }

            ShootDirection = Target.position - transform.position;
            ShootDirection = new Vector3(ShootDirection.x, 0, ShootDirection.z);

            if (Vector3.Distance(transform.position, Target.position) > maxDistanceFromPlayer) {
                return;
            }


            if (Vector3.Distance(_lastPoint, Target.position) > maxShootDistance ||
                Vector3.Distance(_lastPoint, Target.position) < runAwayDistance || Physics.Linecast(
                    _lastPoint,
                    Target.transform.position,
                    GameManager.Instance.WallLayerMask)) {
                PickNextPoint();
            }
            else {
                Shoot();
            }
        }

        protected override void OnSwap(Creature other) {
            GlobalScope.ExecuteWithDelay(3, UnFreeze, Freeze);
        }

        private void PickNextPoint() {
            var positionList = 0.Until(300)
                .Select(_ => Random.insideUnitCircle * maxShootDistance)
                .Select(circle => Target.transform.position + new Vector3(circle.x, 0, circle.y))
                .Where(point =>
                    !Physics.OverlapSphere(point, 20, GameManager.Instance.WallLayerMask).Any() &&
                    !Physics.Linecast(point, Target.transform.position, GameManager.Instance.WallLayerMask)
                )
                .ToList();

            if (positionList.IsEmpty()) {
                return;
            }

            _lastPoint = positionList
                .OrderBy(it => -Vector3.Distance(it, Target.transform.position))
                .First();

            GetComponent<Seeker>().StartPath(
                transform.position,
                _lastPoint,
                OnPathComplete
            );
        }

        private void OnPathComplete(Path p) {
            //We got our path back
            if (p.error) {
                // a valid path couldn't be found
            }
            else {
                // now we can get a Vector3 representation of the path from p.vectorPath
            }
        }
    }
}