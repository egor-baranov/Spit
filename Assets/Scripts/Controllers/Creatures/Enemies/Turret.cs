using System.Linq;
using Controllers.Creatures.Base;
using Controllers.Creatures.Enemies.Base;
using Controllers.Projectiles;
using Controllers.Projectiles.Base;
using Core;
using UnityEngine;

namespace Controllers.Creatures.Enemies {
    public class Turret : Enemy {
        public override EnemyType Type => EnemyType.Turret;
        [SerializeField] private float targetAppearReactionTimeout;

        private bool _doesSeeTarget;

        private bool SeeTarget {
            get => _doesSeeTarget;
            set {
                if (_doesSeeTarget == value) {
                    return;
                }

                if (value) {
                    OnTargetAppear();
                }
                else {
                    OnTargetDisappear();
                }

                _doesSeeTarget = value;
            }
        }

        public override Bullet.Builder BulletBuilder(Vector3 bulletPosition) =>
            new Bullet.Builder(
                    Instantiate(bulletPrefab, bulletPosition, Quaternion.identity).GetComponent<Bullet>()
                )
                .SetTarget(Bullet.BulletTarget.Everything)
                .SetColor(Color.green)
                .SetDamageModifier(0.5F)
                .SetSpeedModifier(1.2F)
                .SetModifiers(enemyModifier: 0.5F)
                .SetScale(0.2F);

        protected override Vector3 ShootPosition => transform.position + ShootDirection.normalized * 18;
        private Transform Center => body.transform.Find("Center");

        protected override void Shoot() {
            if (!CanShoot) return;
            base.Shoot();

            var bullet = BulletBuilder(ShootPosition)
                .SetTarget(Bullet.BulletTarget.Everything)
                .SetColor(Color.green)
                .SetModifiers(enemyModifier: 0.5F)
                .Result();

            bullet.gameObject.GetComponent<Rigidbody>().velocity =
                Quaternion.AngleAxis(Random.Range(-10F, 10F), Vector3.up) * ShootDirection.normalized *
                bullet.GetComponent<Projectile>().MovementSpeed;
        }

        protected override void Start() {
            base.Start();
            GlobalScope.ExecuteEveryInterval(
                0.5F,
                () => SeeTarget = DoesSeeTarget()
            );
        }

        protected override void Update() {
            if (Target == null) {
                Target = Player.Instance.transform;
            }
            
            if (SeeTarget) {
                Center.transform.LookAt(Target);
                ShootDirection = Target.position - transform.position;
                ShootDirection = new Vector3(ShootDirection.x, 0, ShootDirection.z);
                if (CanShoot) Shoot();
            }
        }

        protected override void OnSwap(Creature other) { }

        private bool DoesSeeTarget() {
            var hitList = Physics.RaycastAll(
                transform.position,
                Target.transform.position - transform.position,
                Mathf.Infinity
            ).OrderBy(h=>h.distance);
            
            Debug.Log("[" + string.Join(", ", hitList.Select(it => it.collider.tag)) + "]");

            foreach (var hit in hitList) {
                if (hit.collider.CompareTag("Wall")) return false;
                if (hit.collider.CompareTag("Player")) {
                    return Vector3.Distance(transform.position, Target.position) <= maxShootDistance;
                }
            }
            
            return false;
        }

        private void OnTargetAppear() {
            GlobalScope.ExecuteWithDelay(
                targetAppearReactionTimeout,
                () => CanShoot = SeeTarget || CanShoot
            );
        }

        private void OnTargetDisappear() {
            GlobalScope.ExecuteWithDelay(
                3F, () => CanShoot = SeeTarget && CanShoot
            );
        }
    }
}