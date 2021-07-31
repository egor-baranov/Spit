using Controllers.Creatures.Base;
using Controllers.Creatures.Enemies.Base;
using Controllers.Projectiles;
using Controllers.Projectiles.Base;
using UnityEngine;

namespace Controllers.Creatures.Enemies {
    public class Turret : Enemy {
        public override EnemyType Type => EnemyType.Turret;

        public override Bullet.Builder BulletBuilder(Vector3 bulletPosition) =>
            new Bullet.Builder(
                    Instantiate(bulletPrefab, bulletPosition, Quaternion.identity).GetComponent<Bullet>()
                )
                .SetTarget(Bullet.BulletTarget.Everything)
                .SetColor(Color.green)
                .SetDamageModifier(0.5F)
                .SetSpeedModifier(1.2F)
                .SetModifiers(enemyModifier: 0.5F)
                .SetScale(0.6F);

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

        protected override void Update() {
            Center.transform.LookAt(Target);
            ApplyAutoShooting();
        }

        protected override void OnSwap(Creature other) { }
    }
}