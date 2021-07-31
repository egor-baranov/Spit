using System;
using System.Collections.Generic;
using System.Linq;
using Controllers.Animation;
using Controllers.Creatures.Base;
using Controllers.Creatures.Enemies.Base;
using Controllers.Projectiles;
using Controllers.Projectiles.Base;
using Core;
using Unity.Mathematics;
using UnityEngine;
using Util.Classes;
using Util.ExtensionMethods;
using Random = UnityEngine.Random;

namespace Controllers.Creatures {
    public class Player : Creature {
        public static Player Instance { get; private set; }
        public bool CanPerformSoulBlast { get; private set; } = true;

        public Quaternion DefaultRotation { get; private set; }

        public override void ReceiveDamage(float damage) {
            if (_isInvincible) return;
            GetComponent<Animator>().Play("Receive Damage");

            base.ReceiveDamage(damage);
            _isInvincible = true;

            GlobalScope.ExecuteWithDelay(1, () => {
                _isInvincible = false;
                GetComponent<Animator>().Play("Idle");
            });
        }

        public GameObject CameraHolder => transform.Find("Camera Holder").gameObject;
        private bool IsFreezed { get; set; }
        private Vector3 ShootPosition => transform.position + _shootDirection.normalized * 25;

        [SerializeField] private float shotCost;
        [SerializeField] private LayerMask layerMask, wallLayerMask;

        [SerializeField] private GameObject spitPrefab;
        [SerializeField] private GameObject bulletPrefab;

        private float _timeInBody = 2F;
        private Enemy.EnemyType _appliedEnemyType = Enemy.EnemyType.Assassin;

        private bool _canShoot = true, _isInvincible;

        private Vector3 _shootDirection;
        private Func<Vector3, Bullet.Builder> _bulletBuilderAction;

        public void RechargeSoulBlast() => CanPerformSoulBlast = true;
        private void Recharge() => _canShoot = true;

        private void Freeze() {
            IsFreezed = true;
            GetComponent<CapsuleCollider>().enabled = false;
            body.SetActive(false);
        }

        public void UnFreeze() {
            IsFreezed = false;
            GetComponent<CapsuleCollider>().enabled = true;
            body.SetActive(true);
        }

        private Bullet.Builder BulletBuilder(Vector3 bulletPosition) => _bulletBuilderAction.Invoke(bulletPosition);

        private void Shoot() {
            if (!_canShoot) return;

            _canShoot = false;
            var bullet = BulletBuilder(ShootPosition)
                .SetTarget(Bullet.BulletTarget.Enemy)
                .SetColor(Color.red)
                .Result();

            bullet.gameObject.GetComponent<Rigidbody>().velocity =
                _shootDirection.normalized * bullet.GetComponent<Bullet>().MovementSpeed;
            HealthPoints -= shotCost;

            GlobalScope.ExecuteWithDelay(
                rechargeTime * Random.Range(0.8F, 1.2F),
                Recharge
            );
        }

        private void SoulBlast() {
            if (!CanPerformSoulBlast || _timeInBody < 2F) return;

            CanPerformSoulBlast = false;
            var spit = Instantiate(spitPrefab).GetComponent<Spit>().transform;
            spit.position = transform.position + _shootDirection.normalized * 15;
            spit.rotation = quaternion.identity;

            spit.GetComponent<Rigidbody>().velocity =
                _shootDirection.normalized * spitPrefab.GetComponent<Projectile>().MovementSpeed;

            SwapWith(GameManager.Instance.SpawnEnemyAt(transform.position, _appliedEnemyType));
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
                () => _timeInBody += IsFreezed ? 0F : 1F,
                () => !IsAlive
            );

            DefaultRotation = Quaternion.LookRotation(transform.position - CameraScript.Instance.transform.position);
            Body.transform.rotation = DefaultRotation;
        }

        protected override void Start() {
            _bulletBuilderAction = bulletPosition => new Bullet.Builder(
                Instantiate(bulletPrefab, bulletPosition, Quaternion.identity).GetComponent<Bullet>()
            );
        }

        protected override void Update() {
            var ray = CameraScript.Instance.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var raycastHit, float.MaxValue, layerMask)) {
                _shootDirection = raycastHit.point - transform.position;
                _shootDirection = new Vector3(_shootDirection.x, 0F, _shootDirection.z).normalized;
            }

            if (Input.GetKey(KeyCode.Mouse1) && CanPerformSoulBlast) {
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

            if (Input.GetKey(KeyCode.Mouse0)) {
                Shoot();
            }

            if (Input.GetKeyUp(KeyCode.Mouse1)) {
                SoulBlast();
            }
        }

        private void OnCollisionEnter(Collision other) {
            if (other.transform.GetComponent<Enemy>()) {
                ReceiveDamage(1F);
            }
        }

        protected override void OnSwap(Creature other) {
            _timeInBody = 0F;
            _appliedEnemyType = ((Enemy) other).Type;
            _bulletBuilderAction = bulletPosition => ((Enemy) other).BulletBuilder(bulletPosition);
            Body.GetComponent<CreatureAnimationController>()?.SetColor(Color.white, 2);
        }

        protected override void OnReceiveDamage() =>
            Body.GetComponent<CreatureAnimationController>()?.OnReceiveDamage();

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