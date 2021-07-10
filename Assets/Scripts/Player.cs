using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour {
    public static Player Instance { get; private set; }

    public GameObject Body => transform.GetChild(0).gameObject;
    public GameObject CameraHolder => transform.GetChild(1).gameObject;
    public GameObject Shadow => transform.GetChild(2).gameObject;

    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float spitSpeed;

    [SerializeField] private bool canShoot = true;

    [SerializeField] private LayerMask layerMask;

    [SerializeField] private GameObject spitPrefab;

    private float _targetCameraHolderAngleX = 75F, _targetCameraHolderAngleY;
    private float _targetShadowScale;

    public void MoveTo(Enemy enemy) {
        var tmpPosition = transform.position;
        transform.position = enemy.transform.position;
        enemy.transform.position = tmpPosition;

        var tmpSprite = Body.GetComponent<SpriteRenderer>().sprite;
        Body.GetComponent<SpriteRenderer>().sprite = enemy.Body.GetComponent<SpriteRenderer>().sprite;
        enemy.Body.GetComponent<SpriteRenderer>().sprite = tmpSprite;
    }

    public void Recharge() => canShoot = true;

    private void Shoot() {
        if (!canShoot) return;

        canShoot = false;
        Instantiate(spitPrefab, transform.position, Quaternion.identity).GetComponent<Rigidbody>().velocity =
            (Shadow.transform.GetChild(0).transform.position - Shadow.transform.position).normalized * spitSpeed;
    }


    private void Update() {
        transform.Translate(
            MovementState.Create(
                MovementState.PossibleKeyList.Where(Input.GetKey)
            ).Direction * movementSpeed
        );

        if (Input.GetKey(KeyCode.Q) ^ Input.GetKey(KeyCode.E)) {
            transform.Rotate(transform.up, (Input.GetKey(KeyCode.Q) ? -1 : 1) * rotationSpeed);
            _targetCameraHolderAngleY += (Input.GetKey(KeyCode.Q) ? -1 : 1) * rotationSpeed;
        }

        Body.transform.LookAt(CameraScript.Instance.transform);

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
        }

        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            _targetCameraHolderAngleX = 89.9F;
            _targetShadowScale = 3;
        }

        if (Input.GetKeyUp(KeyCode.Mouse0)) {
            _targetCameraHolderAngleX = 75F;
            _targetShadowScale = 0;

            Shoot();
        }
    }

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
        }

        Instance = this;
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