using System.Collections.Generic;
using System.IO;
using System.Linq;
using Controllers.Creatures.Base;
using Controllers.Creatures.Enemies.Base;
using Controllers.Projectiles;
using Core;
using Unity.Mathematics;
using UnityEngine;
using Util.Classes;
using Util.ExtensionMethods;
using Random = UnityEngine.Random;

namespace Controllers.Creatures {
    public class Player : Creature {
        public static Player Instance { get; private set; }

        public bool CanPerformSoulBlast => _canPerformSoulBlast;

        public Quaternion DefaultRotation => _defaultRotation;

        public override void ReceiveDamage(float damage) {
            if (_isInvincible) return;
            GetComponent<Animator>().Play("Receive Damage");

            base.ReceiveDamage(damage);
            _isInvincible = true;

            GlobalScope.ExecuteWithDelay(3, () => {
                _isInvincible = false;
                GetComponent<Animator>().Play("Idle");
            });
        }

        public GameObject CameraHolder => transform.Find("Camera Holder").gameObject;
        private bool IsFreezed { get; set; }


        [SerializeField] private float shotCost;

        [SerializeField] private LayerMask layerMask, wallLayerMask;

        [SerializeField] private GameObject spitPrefab;
        [SerializeField] private GameObject bulletPrefab;

        private float _timeInBody = 2F;
        private Enemy.EnemyType _appliedEnemyType = Enemy.EnemyType.Assassin;

        private bool _canPerformSoulBlast = true;
        private bool _canShoot = true, _isInvincible;
        private Quaternion _defaultRotation;

        private Vector3 _shootDirection;

        public void RechargeSoulBlast() => _canPerformSoulBlast = true;
        private void Recharge() => _canShoot = true;

        private void Freeze() {
            IsFreezed = true;
            GetComponent<CapsuleCollider>().enabled = false;
        }

        public void UnFreeze() {
            IsFreezed = false;
            GetComponent<CapsuleCollider>().enabled = true;
        }

        private void Shoot() {
            if (!_canShoot) return;

            _canShoot = false;
            var bullet = Instantiate(
                    bulletPrefab,
                    transform.position + _shootDirection.normalized * 25,
                    Quaternion.identity
                )
                .GetComponent<Bullet>()
                .SetTarget(Bullet.BulletTarget.Enemy)
                .SetColor(Color.red).SetSpeedModifier(2F);

            bullet.gameObject.GetComponent<Rigidbody>().velocity =
                _shootDirection.normalized * bullet.GetComponent<Bullet>().MovementSpeed;
            HealthPoints -= shotCost;

            GlobalScope.ExecuteWithDelay(
                rechargeTime * Random.Range(0.8F, 1.2F),
                Recharge
            );
        }

        private void SoulBlast() {
            if (!_canPerformSoulBlast || _timeInBody < 2F) return;

            _canPerformSoulBlast = false;
            var spit = Instantiate(spitPrefab).GetComponent<Spit>().transform;
            spit.position = transform.position + _shootDirection.normalized * 15;
            spit.rotation = quaternion.identity;

            spit.GetComponent<Rigidbody>().velocity =
                _shootDirection.normalized * spitPrefab.GetComponent<Projectile>().MovementSpeed;

            SwapWith(GameManager.Instance.SpawnEnemyAt(transform.position, Enemy.EnemyType.Assassin));
            Freeze();
        }

        protected override void Awake() {
            base.Awake();
            if (Instance != null) {
                Destroy(gameObject);
            }

            Instance = this;

            GlobalScope.ExecuteEveryInterval(
                1F,
                () => HealthPoints -= 0.5F,
                () => !IsAlive
            );

            GlobalScope.ExecuteEveryInterval(
                1F,
                () => _timeInBody += 1F,
                () => !IsAlive
            );

            _defaultRotation = Quaternion.LookRotation(transform.position - CameraScript.Instance.transform.position);
            Body.transform.rotation = _defaultRotation;
        }

        protected override void Start() { }

        protected override void Update() {
            var ray = CameraScript.Instance.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask)) {
                _shootDirection = raycastHit.point - transform.position;
                _shootDirection = new Vector3(_shootDirection.x, 0F, _shootDirection.z).normalized;
            }

            if (Input.GetKey(KeyCode.Mouse1) && _canPerformSoulBlast) {
                Time.timeScale = 0.6F;
                DrawLine();
            }
            else {
                Time.timeScale = 1F;
                ClearLine();
            }

            if (IsAlive && !IsFreezed) {
                ApplyControls(_appliedEnemyType);
            }
        }

        private void ApplyControls(Enemy.EnemyType enemyType) {
            if (enemyType == Enemy.EnemyType.Assassin) {
                ApplyAssassinControls();
            }
            else {
                ApplyTurretControls();
            }
        }

        private void ApplyAssassinControls() {
            var velocity = MovementState.Create(
                transform,
                MovementState.PossibleKeyList.Where(Input.GetKey)
            ).Direction * movementSpeed;

            if (velocity != Vector3.zero) {
                GetComponent<Rigidbody>().velocity = velocity;
            }

            if (Input.GetKey(KeyCode.Mouse0)) {
                Shoot();
            }

            if (Input.GetKeyUp(KeyCode.Mouse1)) {
                SoulBlast();
            }
        }

        private void ApplyTurretControls() {
            body.transform.Find("Center").transform.LookAt(transform.position + _shootDirection * 100);
        }

        private void OnCollisionEnter(Collision other) {
            if (other.transform.GetComponent<Enemy>()) {
                ReceiveDamage(1F);
            }
        }

        protected override void OnSwap() {
            _timeInBody = 0F;
        }

        private void DrawLine() {
            var lineRenderer = GameObject.Find("Line Renderer").GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;

            var maxLineLength = 500F;
            var positionList = new List<Vector3> {transform.position + _shootDirection.normalized * 10};
            var direction = _shootDirection;

            while (maxLineLength > 0) {
                if (!Physics.Raycast(
                    new Ray(positionList.LastElement(), direction),
                    out RaycastHit raycastHit,
                    maxLineLength, wallLayerMask)) {
                    break;
                }

                if (raycastHit.transform.CompareTag("Enemy")) {
                    positionList.Add(raycastHit.point);
                    break;
                }

                maxLineLength -= Vector3.Distance(positionList.LastElement(), raycastHit.point);
                direction = Vector3.Reflect(direction, raycastHit.normal);
                positionList.Add(raycastHit.point);
            }

            if (maxLineLength > 0) {
                positionList.Add(positionList.LastElement() + direction.normalized * maxLineLength);
            }

            lineRenderer.positionCount = positionList.Count;
            foreach (var i in 0.Until(positionList.Count)) {
                lineRenderer.SetPosition(i, positionList[i]);
            }
        }

        private static void ClearLine() {
            var lineRenderer = GameObject.Find("Line Renderer").GetComponent<LineRenderer>();
            lineRenderer.positionCount = 0;
        }
    }
}