using System.Linq;
using Controllers.Creatures;
using Controllers.Creatures.Enemies.Base;
using Controllers.Projectiles.Base;
using Core;
using UnityEngine;
using Util.Classes;
using Util.ExtensionMethods;

namespace Controllers.Projectiles {
    public class Spit : Projectile {
        private GameObject Halo => transform.Find("Halo").gameObject;
        private GameObject Circle => transform.Find("Circle").gameObject;

        private float InvasionRadius =>
            invasionRadius * (1.25F * (maxTimeAlive - _timeAliveLeft) + 1F * _timeAliveLeft) / maxTimeAlive;

        [Tooltip("Start slowmo scale coefficient (1 = normal, < 1 = slowed).")] [SerializeField]
        private float startSlowmoScale;

        [Tooltip("How long time will be slowed after spit launch.")] [SerializeField]
        private float startSlowmoTimeout;

        [Tooltip("Before invasion (when enemy is near) slowmo scale coefficient (1 = normal, < 1 = slowed).")]
        [SerializeField]
        private float beforeInvasionSlowmoScale;

        [Tooltip("How long time will be slowed when spit is near to enemy.")] [SerializeField]
        private float beforeInvasionSlowmoTimeout;

        [Tooltip("After invasion slowmo scale coefficient (1 = normal, < 1 = slowed).")] [SerializeField]
        private float afterInvasionSlowmoScale;

        [Tooltip("How long time will be slowed after invasion.")] [SerializeField]
        private float afterInvasionSlowmoTimeout;

        [Tooltip("Radius of invasion to body.")] [SerializeField]
        private float invasionRadius;

        private float _timeAliveLeft, _circleDefaultLocalScale;
        private bool _killPlayer = true;

        private GameThread _timeManagementThread;

        protected override void Awake() {
            _timeAliveLeft = maxTimeAlive;
            Destroy(gameObject, maxTimeAlive);

            CameraScript.Instance.SetTarget(transform, 50, 0.5F);
            GameManager.Instance.SetTargetForAllEnemies(transform);
        }

        protected override void Start() {
            _circleDefaultLocalScale = Circle.transform.localScale.x;

            Time.timeScale = startSlowmoScale;
            _timeManagementThread = GameThread.Create().Subscribe(
                0.1F,
                () => Time.timeScale = Mathf.Min(Time.timeScale + 0.1F / startSlowmoTimeout, 1)
            );
        }

        protected override void Update() {
            _timeAliveLeft -= Time.deltaTime;
            GetComponent<Light>().range = 200 * Mathf.Sqrt(_timeAliveLeft / maxTimeAlive);
            Halo.GetComponent<Light>().range = 10 * Mathf.Sqrt(_timeAliveLeft / maxTimeAlive);

            Circle.transform.localScale =
                Vector3.one *
                (_circleDefaultLocalScale *
                 (1.25F * (maxTimeAlive - _timeAliveLeft) + 1F * _timeAliveLeft)) /
                maxTimeAlive;

            var list = GameManager.Instance.EnemyList.ToList();
            list.Sort((x, y) =>
                Vector3.Distance(x.transform.position, transform.position).CompareTo(
                    Vector3.Distance(y.transform.position, transform.position)
                )
            );

            Circle.GetComponent<SpriteRenderer>().color =
                list.Count > 0 && Vector3.Distance(list[0].transform.position,
                    transform.position) <= InvasionRadius
                    ? Color.green
                    : Color.white;

            if (list.IsNotEmpty() && maxTimeAlive - _timeAliveLeft > 0.2F) {
                Time.timeScale = beforeInvasionSlowmoScale;
                _timeManagementThread.Unsubscribe().Subscribe(
                    0.1F,
                    () => Time.timeScale = Mathf.Min(Time.timeScale + 0.1F / beforeInvasionSlowmoTimeout, 1)
                    );
            }

            if (Input.GetKeyDown(KeyCode.Mouse1)) {
                if (list.Count > 0 &&
                    Vector3.Distance(list[0].transform.position, transform.position) <= InvasionRadius) {
                    _killPlayer = false;
                    GameManager.SoulShotCount += 1;
                    Player.Instance.SwapWith(list[0]);

                    if (list[0].Type != Enemy.EnemyType.Turret) {
                        Player.Instance.GetComponent<Rigidbody>().AddForce(
                            (Player.Instance.transform.position - transform.position).normalized * 400,
                            ForceMode.Impulse
                        );
                    }

                    Time.timeScale = afterInvasionSlowmoScale;
                    _timeManagementThread.Unsubscribe().Subscribe(
                        0.1F,
                        () => Time.timeScale = Mathf.Min(Time.timeScale + 0.1F / afterInvasionSlowmoTimeout, 1));
                    GameManager.Instance.RemoveEnemy(list[0]);
                    Destroy(list[0].gameObject);
                }

                Destroy(gameObject);
            }
        }

        protected override void OnDestroy() {
            if (_killPlayer) {
                Time.timeScale = 1F;
                Player.Instance.HealthPoints = 0;
                return;
            }

            CameraScript.Instance.SetTarget(Player.Instance.CameraHolder.transform, 3);
            GameManager.Instance.SetTargetForAllEnemies(Player.Instance.transform);
            Player.Instance.UnFreeze();
        }
    }
}