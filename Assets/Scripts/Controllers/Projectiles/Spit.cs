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
        private bool _killPlayer = true;
        public bool _wasEnemyDestroyed = false;

        protected override void Awake() {
            base.Awake();
            _timeAliveLeft = maxTimeAlive;
            Destroy(gameObject, maxTimeAlive);

            CameraScript.Instance.SetTarget(transform);
        }

        protected override void Update() {
            base.Awake();
            _timeAliveLeft -= Time.deltaTime;
            GetComponent<Light>().intensity = 30 * Mathf.Sqrt(_timeAliveLeft / maxTimeAlive);
            GetComponent<Light>().range = 30 * Mathf.Sqrt(_timeAliveLeft / maxTimeAlive);
        }

        private void OnCollisionEnter(Collision other) {
            try {
                if (!other.collider.GetComponent<Enemy>() || _timeAliveLeft > maxTimeAlive - 0.02F ||
                    _wasEnemyDestroyed) {
                    return;
                }

                _killPlayer = false;
                _wasEnemyDestroyed = true;
                GameManager.soulShotCount += 1;
                Player.Instance.SwapWith(other.collider.GetComponent<Enemy>());
                GlobalScope.ExecuteWithDelay(
                    slowmoTimeout,
                    () => Time.timeScale = 1F,
                    () => Time.timeScale = slowTimeScale
                );
                GameManager.Instance.RemoveEnemy(other.gameObject.GetComponent<Enemy>());
                Destroy(other.gameObject);
                Destroy(gameObject);
            }
            catch (NullReferenceException) { }
        }

        protected override void OnDestroy() {
            base.OnDestroy();

            if (_killPlayer) {
                Player.Instance.HealthPoints = 0;
                return;
            }

            Player.Instance.RechargeSoulBlast();
            CameraScript.Instance.SetTarget(Player.Instance.CameraHolder.transform);
            Player.Instance.UnFreeze();
        }
    }
}