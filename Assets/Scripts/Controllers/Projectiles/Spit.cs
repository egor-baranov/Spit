using System;
using Controllers.Creatures;
using Core;
using UnityEngine;

namespace Controllers.Projectiles {
    public class Spit : Projectile {
        [SerializeField] private float slowTimeDistance;
        [SerializeField] private float slowTimeScale;
        [SerializeField] private float slowmoTimeout;

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

        private void OnCollisionEnter(Collision other) {
            try {
                if (!other.collider.GetComponent<Enemy>()) {
                    return;
                }

                Player.Instance.SwapWith(other.collider.GetComponent<Enemy>());
                GlobalScope.ExecuteWithDelay(
                    slowmoTimeout,
                    () => Time.timeScale = 1F,
                    () => Time.timeScale = slowTimeScale
                );
                Destroy(gameObject);
            }
            catch (NullReferenceException) { }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            Player.Instance.RechargeSoulBlast();
        }
    }
}