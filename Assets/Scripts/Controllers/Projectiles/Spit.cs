using System.Linq;
using Controllers.Creatures;
using Controllers.Creatures.Enemies.Base;
using Controllers.Projectiles.Base;
using Core;
using UnityEngine;

namespace Controllers.Projectiles {
    public class Spit : Projectile {
        private GameObject Halo => transform.Find("Halo").gameObject;
        private GameObject Circle => transform.Find("Circle").gameObject;

        [SerializeField] private float slowTimeScale;
        [SerializeField] private float slowmoTimeout;
        [SerializeField] private float invasionRadius;

        private float _timeAliveLeft;
        private bool _killPlayer = true;

        protected override void Awake() {
            _timeAliveLeft = maxTimeAlive;
            Destroy(gameObject, maxTimeAlive);

            CameraScript.Instance.SetTarget(transform, 14, 0.5F);
            GameManager.Instance.SetTargetForAllEnemies(transform);
        }

        protected override void Start() { }

        protected override void Update() {
            _timeAliveLeft -= Time.deltaTime;
            GetComponent<Light>().range = 200 * Mathf.Sqrt(_timeAliveLeft / maxTimeAlive);
            Halo.GetComponent<Light>().range = 10 * Mathf.Sqrt(_timeAliveLeft / maxTimeAlive);

            var list = GameManager.Instance.EnemyList.ToList();
            list.Sort((x, y) =>
                Vector3.Distance(x.transform.position, transform.position).CompareTo(
                    Vector3.Distance(y.transform.position, transform.position)
                )
            );

            Circle.GetComponent<SpriteRenderer>().color =
                list.Count > 0 && Vector3.Distance(list[0].transform.position,
                    transform.position) <= invasionRadius
                    ? Color.green
                    : Color.white;

            if (_killPlayer) {
                Time.timeScale =
                    Vector3.Distance(list[0].transform.position, transform.position) <= invasionRadius * 1.3F
                        ? slowTimeScale
                        : 1;
            }

            if (Input.GetKeyDown(KeyCode.Mouse1)) {
                if (list.Count > 0 &&
                    Vector3.Distance(list[0].transform.position, transform.position) <= invasionRadius) {
                    _killPlayer = false;
                    GameManager.SoulShotCount += 1;
                    Player.Instance.SwapWith(list[0]);

                    if (list[0].Type != Enemy.EnemyType.Turret) {
                        Player.Instance.GetComponent<Rigidbody>().AddForce(
                            (Player.Instance.transform.position - transform.position).normalized * 400,
                            ForceMode.Impulse
                        );
                    }

                    GlobalScope.ExecuteWithDelay(
                        slowmoTimeout,
                        () => Time.timeScale = 1F,
                        () => Time.timeScale = slowTimeScale
                    );
                    GameManager.Instance.RemoveEnemy(list[0]);
                    Destroy(list[0].gameObject);
                }

                Destroy(gameObject);
            }
        }

        protected override void OnDestroy() {
            if (_killPlayer) {
                Player.Instance.HealthPoints = 0;
                Time.timeScale = 1;
                return;
            }

            CameraScript.Instance.SetTarget(Player.Instance.CameraHolder.transform, 5);
            GameManager.Instance.SetTargetForAllEnemies(Player.Instance.transform);
            Player.Instance.UnFreeze();
        }
    }
}