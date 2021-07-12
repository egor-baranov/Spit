using System;
using Controllers.Creatures;
using UnityEngine;

namespace Controllers.Projectiles {
    public class Spit : Projectile {
        private float _timeAliveLeft;
        
        protected override void Awake() {
            base.Awake();
            _timeAliveLeft = maxTimeAlive;
            Destroy(gameObject, maxTimeAlive);
        }

        protected override void Update() {
            base.Awake();
            _timeAliveLeft -= Time.deltaTime;
            GetComponent<Light>().intensity = 30 * _timeAliveLeft / maxTimeAlive;
            GetComponent<Light>().range = 30 * _timeAliveLeft / maxTimeAlive;
        }

        private void OnTriggerEnter(Collider other) {
            try {
                if (other.transform.parent.GetComponent<Enemy>()) {
                    Player.Instance.MoveTo(other.transform.parent.GetComponent<Enemy>());
                    Destroy(gameObject);
                }
            }
            catch (NullReferenceException) {
                GetComponent<Rigidbody>().velocity = -GetComponent<Rigidbody>().velocity * 0.75F;
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            Player.Instance.RechargeSoulBlast();
        }
    }
}