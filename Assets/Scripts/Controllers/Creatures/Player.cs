using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Controllers.Projectiles;
using Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Controllers.Creatures {
    public class Player : Creature {
        public static Player Instance { get; private set; }

        public bool IsFreezed { get; private set; }

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

        public GameObject CameraHolder => transform.GetChild(1).gameObject;
        private GameObject Shadow => transform.GetChild(2).gameObject;
        private GameObject Highlight => transform.Find("Highlight").gameObject;

        [SerializeField] private float rotationSpeed;
        [SerializeField] private float rechargeTime;
        [SerializeField] private float shotCost;

        [SerializeField] private LayerMask layerMask;

        [SerializeField] private GameObject spitPrefab;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private float cameraAngle;

        private float _targetCameraHolderAngleX, _targetCameraHolderAngleY;
        private float _targetShadowScale;

        private bool _canShoot = true, _canPerformSoulBlast = true, _isInvincible = false;

        private Vector3 _shootDirection;

        public void RechargeSoulBlast() => _canPerformSoulBlast = true;
        private void Recharge() => _canShoot = true;

        private void Freeze() {
            IsFreezed = true;
            Body.SetActive(false);
            GetComponent<CapsuleCollider>().enabled = false;
            Highlight.SetActive(false);
        }

        public void UnFreeze() {
            IsFreezed = false;
            GetComponent<CapsuleCollider>().enabled = true;
            Body.SetActive(true);
            Highlight.SetActive(true);
        }

        private void Shoot() {
            if (!_canShoot) return;

            _canShoot = false;
            var bullet = Instantiate(
                    bulletPrefab,
                    transform.position + _shootDirection.normalized * 15,
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
            if (!_canPerformSoulBlast) return;

            _canPerformSoulBlast = false;
            Instantiate(spitPrefab, transform.position + _shootDirection.normalized,
                        Quaternion.identity)
                    .GetComponent<Rigidbody>().velocity =
                _shootDirection.normalized * spitPrefab.GetComponent<Projectile>().MovementSpeed;

            SwapWith(GameManager.Instance.SpawnEnemyAt(transform.position));
            Freeze();
        }

        protected override void Awake() {
            base.Awake();
            if (Instance != null) {
                Destroy(gameObject);
            }

            Instance = this;
            _targetCameraHolderAngleX = cameraAngle;

            GlobalScope.ExecuteEveryInterval(
                1F,
                () => HealthPoints -= 0.5F,
                () => !IsAlive
            );
        }

        protected override void Start() {
            base.Start();
            Body.transform.LookAt(CameraScript.Instance.transform);
        }

        protected override void Update() {
            base.Update();

            Shadow.transform.localScale = Vector3.Lerp(
                Shadow.transform.localScale,
                Vector3.one * _targetShadowScale,
                10 * Time.deltaTime
            );

            var ray = CameraScript.Instance.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask)) {
                Shadow.transform.LookAt(raycastHit.point);
                Shadow.transform.rotation = Quaternion.Euler(90F, Shadow.transform.rotation.eulerAngles.y, 0F);
                _shootDirection = raycastHit.point - transform.position;
                _shootDirection = new Vector3(_shootDirection.x, 0F, _shootDirection.z).normalized;
            }

            CameraHolder.transform.rotation =
                Quaternion.RotateTowards(
                    CameraHolder.transform.rotation,
                    Quaternion.Euler(
                        _targetCameraHolderAngleX,
                        _targetCameraHolderAngleY,
                        CameraHolder.transform.rotation.z
                    ),
                    100 * Time.deltaTime
                );

            if (IsAlive && !IsFreezed) {
                PerformControls();
            }
        }

        private void PerformControls() {
            var velocity =
                MovementState.Create(
                    transform,
                    MovementState.PossibleKeyList.Where(Input.GetKey)
                ).Direction * movementSpeed;

            if (velocity != Vector3.zero) {
                GetComponent<Rigidbody>().velocity = velocity;
            }

            if (Input.GetKey(KeyCode.Mouse0)) {
                Shoot();
            }

            if (Input.GetKeyDown(KeyCode.Mouse1)) {
                _targetCameraHolderAngleX = 89.9F;
                _targetShadowScale = 3;
            }

            if (Input.GetKeyUp(KeyCode.Mouse1)) {
                _targetCameraHolderAngleX = cameraAngle;
                _targetShadowScale = 0;

                SoulBlast();
            }
        }

        private void OnCollisionEnter(Collision other) {
            if (other.transform.GetComponent<Enemy>()) {
                ReceiveDamage(1F);
            }
        }

        private class MovementState {
            public static readonly List<KeyCode> PossibleKeyList =
                new List<KeyCode> {KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D};

            public List<KeyCode> KeyCodeList => _keyCodeList;
            public Vector3 Direction => _direction;

            private readonly Transform _transform;
            private readonly List<KeyCode> _keyCodeList;
            private readonly Vector3 _direction;

            private MovementState(Transform transform, List<KeyCode> keyCodeList, Vector3 direction) {
                _transform = transform;
                _keyCodeList = keyCodeList;
                _direction = direction;
            }

            private static Vector3 GetDirectionFromKey(Transform transform, KeyCode keyCode) {
                switch (keyCode) {
                    case KeyCode.W: return transform.forward;
                    case KeyCode.A: return -transform.right;
                    case KeyCode.S: return -transform.forward;
                    case KeyCode.D: return transform.right;
                    default: throw new InvalidDataException();
                }
            }

            public static MovementState Create(Transform transform, params KeyCode[] keyCodes) =>
                Create(transform, keyCodes.ToList());

            public static MovementState Create(Transform transform, IEnumerable<KeyCode> keyCodes) {
                var keyList = keyCodes.Where(it => PossibleKeyList.Contains(it)).ToList();
                var result = Vector3.zero;
                keyList.ForEach(keyCode => result += GetDirectionFromKey(transform, keyCode));

                if (result.sqrMagnitude > 1) {
                    result = result.normalized * 0.75F;
                }

                return new MovementState(transform, keyList, result);
            }
        }
    }
}