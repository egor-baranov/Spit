using System.Collections.Generic;
using System.IO;
using System.Linq;
using Controllers.Projectiles;
using Core;
using UnityEngine;

namespace Controllers.Creatures {
    public class Player : Creature {
        public static Player Instance { get; private set; }

        public GameObject CameraHolder => transform.GetChild(1).gameObject;
        public GameObject Shadow => transform.GetChild(2).gameObject;

        [SerializeField] private float rotationSpeed;
        [SerializeField] private float rechargeTime;
        [SerializeField] private float shotCost;

        [SerializeField] private LayerMask layerMask;

        [SerializeField] private GameObject spitPrefab;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private float cameraAngle;

        private float _targetCameraHolderAngleX, _targetCameraHolderAngleY;
        private float _targetShadowScale;

        private bool _canShoot = true;
        private bool _canPerformSoulBlast = true;

        private Vector3 _shootDirection;

        public void MoveTo(Enemy enemy) {
            var tmpPosition = transform.position;
            transform.position = enemy.transform.position;
            enemy.transform.position = tmpPosition;

            var tmpSprite = Body.GetComponent<SpriteRenderer>().sprite;
            Body.GetComponent<SpriteRenderer>().sprite = enemy.Body.GetComponent<SpriteRenderer>().sprite;
            enemy.Body.GetComponent<SpriteRenderer>().sprite = tmpSprite;

            var tmpHp = HealthPoints;
            HealthPoints = enemy.HealthPoints;
            enemy.HealthPoints = tmpHp;

            var tmpMaxHp = MaxHp;
            MaxHp = enemy.MaxHp;
            enemy.MaxHp = tmpMaxHp;
        }

        public void RechargeSoulBlast() => _canPerformSoulBlast = true;
        public void Recharge() => _canShoot = true;

        private void Shoot() {
            if (!_canShoot) return;

            _canShoot = false;
            Instantiate(bulletPrefab, transform.position + _shootDirection.normalized,
                        Quaternion.identity)
                    .GetComponent<Rigidbody>().velocity =
                _shootDirection.normalized * bulletPrefab.GetComponent<Projectile>().MovementSpeed;

            ReceiveDamage(shotCost);

            GlobalScope.ExecuteWithDelay(
                rechargeTime,
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
                () => { ReceiveDamage(0.5F); },
                () => !IsAlive
            );
        }

        protected override void Start() {
            base.Start();
            Body.transform.LookAt(CameraScript.Instance.transform);
        }

        protected override void Update() {
            base.Update();
            if (!IsAlive) return;

            transform.Translate(
                MovementState.Create(
                    MovementState.PossibleKeyList.Where(Input.GetKey)
                ).Direction * (movementSpeed * Time.timeScale)
            );

            if (Input.GetKey(KeyCode.Q) ^ Input.GetKey(KeyCode.E)) {
                transform.Rotate(transform.up, (Input.GetKey(KeyCode.Q) ? -1 : 1) * rotationSpeed);
                _targetCameraHolderAngleY += (Input.GetKey(KeyCode.Q) ? -1 : 1) * rotationSpeed;
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

            Shadow.transform.localScale = Vector3.Lerp(
                Shadow.transform.localScale,
                Vector3.one * _targetShadowScale,
                10 * Time.deltaTime
            );

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask)) {
                Shadow.transform.LookAt(raycastHit.point);
                Shadow.transform.rotation = Quaternion.Euler(90F, Shadow.transform.rotation.eulerAngles.y, 0F);
                _shootDirection = raycastHit.point - transform.position;
                _shootDirection = new Vector3(_shootDirection.x, 0F, _shootDirection.z).normalized;
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

        private class MovementState {
            public static readonly List<KeyCode> PossibleKeyList =
                new List<KeyCode> {KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D};

            public List<KeyCode> KeyCodeList => _keyCodeList;
            public Vector3 Direction => _direction;

            private readonly List<KeyCode> _keyCodeList;
            private readonly Vector3 _direction;

            private MovementState(List<KeyCode> keyCodeList, Vector3 direction) {
                _keyCodeList = keyCodeList;
                _direction = direction;
            }

            private static Vector3 GetDirectionFromKey(KeyCode keyCode) {
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (keyCode) {
                    case KeyCode.W: return Vector3.forward;
                    case KeyCode.A: return Vector3.left;
                    case KeyCode.S: return Vector3.back;
                    case KeyCode.D: return Vector3.right;
                    default: throw new InvalidDataException();
                }
            }

            public static MovementState Create(params KeyCode[] keyCodes) => Create(keyCodes.ToList());

            public static MovementState Create(IEnumerable<KeyCode> keyCodes) {
                var keyList = keyCodes.Where(it => PossibleKeyList.Contains(it)).ToList();
                var result = Vector3.zero;
                keyList.ForEach(keyCode => result += GetDirectionFromKey(keyCode));

                if (result.sqrMagnitude > 1) {
                    result = result.normalized * 0.75F;
                }

                return new MovementState(keyList, result);
            }

            public enum MovementDirection {
                Up,
                Down,
                Left,
                Right,
                LeftUp,
                LeftDown,
                RightUp,
                RightDown,
                None
            }
        }
    }
}