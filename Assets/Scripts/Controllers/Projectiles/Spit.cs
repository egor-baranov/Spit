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
        public bool wasEnemyDestroyed = false;

        private Enemy _selectedEnemy = null;

        protected override void Awake() {
            base.Awake();
            _timeAliveLeft = maxTimeAlive;
            Destroy(gameObject, maxTimeAlive);

            CameraScript.Instance.SetTarget(transform, 8);
            GameManager.Instance.SetTargetForAllEnemies(transform);
        }

        protected override void Update() {
            base.Awake();
            _timeAliveLeft -= Time.deltaTime;
            GetComponent<Light>().intensity = 30 * Mathf.Sqrt(_timeAliveLeft / maxTimeAlive);
            GetComponent<Light>().range = 30 * Mathf.Sqrt(_timeAliveLeft / maxTimeAlive);
            
            if (Input.GetKeyDown(KeyCode.Mouse1) && _selectedEnemy != null) {
                _killPlayer = false;
                wasEnemyDestroyed = true;
                GameManager.soulShotCount += 1;
                Player.Instance.SwapWith(_selectedEnemy);
                Player.Instance.GetComponent<Rigidbody>()
                    .AddForce(GetComponent<Rigidbody>().velocity * 4, ForceMode.Impulse);

                GlobalScope.ExecuteWithDelay(
                    slowmoTimeout,
                    () => Time.timeScale = 1F,
                    () => Time.timeScale = slowTimeScale
                );
                GameManager.Instance.RemoveEnemy(_selectedEnemy);
                Destroy(_selectedEnemy.gameObject);
                Destroy(gameObject);
            }
        }

        private void OnTriggerStay(Collider other) {
            try {
                if (!other.GetComponent<Enemy>() || _timeAliveLeft > maxTimeAlive - 0.1F ||
                    wasEnemyDestroyed) {
                    return;
                }

                _selectedEnemy = other.transform.GetComponent<Enemy>();
            }
            catch (NullReferenceException) { }
        }

        protected override void OnDestroy() {
            base.OnDestroy();

            if (_killPlayer) {
                Player.Instance.HealthPoints = 0;
                return;
            }

            // GlobalScope.ExecuteWithDelay(0.5F, () => Player.Instance.RechargeSoulBlast());
            CameraScript.Instance.SetTarget(Player.Instance.CameraHolder.transform, 2);
            GameManager.Instance.SetTargetForAllEnemies(Player.Instance.transform);
            Player.Instance.UnFreeze();
        }
    }
}