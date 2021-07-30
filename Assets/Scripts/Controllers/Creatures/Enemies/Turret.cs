﻿using Controllers.Creatures.Base;
using Controllers.Creatures.Enemies.Base;
using Controllers.Projectiles;
using Core;
using UnityEngine;

namespace Controllers.Creatures.Enemies {
    public class Turret : Enemy {
        private Transform Center => body.transform.Find("Center");

        protected override Vector3 ShootPosition => transform.position + ShootDirection.normalized * 18;

        protected override void Shoot() {
            if (!CanShoot) return;
            base.Shoot();

            var bullet = Instantiate(bulletPrefab, ShootPosition, Quaternion.identity)
                .GetComponent<Bullet>()
                .SetTarget(Bullet.BulletTarget.Everything)
                .SetColor(Color.green)
                .SetDamageModifier(0.5F)
                .SetSpeedModifier(1.2F)
                .SetModifiers(enemyModifier: 0.5F)
                .SetScale(0.6F);

            bullet.gameObject.GetComponent<Rigidbody>().velocity =
                Quaternion.AngleAxis(Random.Range(-10F, 10F), Vector3.up) * ShootDirection.normalized *
                bullet.GetComponent<Projectile>().MovementSpeed;
        }

        protected override void Update() {
            base.Update();
            Center.transform.LookAt(Target);
            ApplyAutoShooting();
        }
    }
}