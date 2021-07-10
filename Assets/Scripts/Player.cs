using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class Player : MonoBehaviour {
    public static Player Instance { get; private set; }

    public GameObject Body => transform.GetChild(0).gameObject;
    public GameObject CameraHolder => transform.GetChild(1).gameObject;
    public GameObject Shadow => transform.GetChild(2).gameObject;

    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;
    
    [SerializeField] private LayerMask layerMask;

    private float _targetCameraHolderAngleX = 50F, _targetCameraHolderAngleY;

    
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
                50 * Time.deltaTime
            );

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask)) {
            Shadow.transform.LookAt(raycastHit.point);
            Shadow.transform.rotation = Quaternion.Euler(90F, Shadow.transform.rotation.eulerAngles.y, 0F);
        }

        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            _targetCameraHolderAngleX = 89.9F;
            Shadow.transform.localScale = Vector3.one * 3;
        }

        if (Input.GetKeyUp(KeyCode.Mouse0)) {
            _targetCameraHolderAngleX = 50F;
            Shadow.transform.localScale = Vector3.zero;
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