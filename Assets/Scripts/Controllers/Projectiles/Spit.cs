using System;
using System.Linq;
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

            // Time.timeScale = GameManager.Instance.EnemyList.Any(
            //     it =>
            //         Vector3.Distance(
            //             transform.position,
            //             it.transform.position
            //         ) <= slowTimeDistance
            // )
            //     ? slowTimeScale
            //     : 1F;
            // Debug.Log(GameManager.Instance.EnemyList.Min(
            //     it =>
            //         Vector3.Distance(
            //             transform.position,
            //             it.transform.position
            //         )
            // ));
        }

        private void OnTriggerEnter(Collider other) {
            try {
                if (!other.transform.parent.GetComponent<Enemy>()) {
                    return;
                }

                Player.Instance.MoveTo(other.transform.parent.GetComponent<Enemy>());
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