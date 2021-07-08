using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class Player : MonoBehaviour {
    public static Player Instance { get; private set; }

    public GameObject Body => transform.GetChild(0).gameObject;

    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;

    private void Update() {
        transform.Translate(
            MovementState.Create(
                MovementState.PossibleKeyList.Where(Input.GetKey)
            ).Direction * movementSpeed
        );

        if (Input.GetKey(KeyCode.Q) ^ Input.GetKey(KeyCode.E)) {
            transform.Rotate(transform.up, (Input.GetKey(KeyCode.Q) ? -1 : 1) * rotationSpeed);
        }

        Body.transform.LookAt(CameraScript.Instance.transform);
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

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
        }

        Instance = this;
    }
}