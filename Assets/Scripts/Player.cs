using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour {
    private void Update() {
        transform.Translate(
            MovementState.Create(
                MovementState.PossibleKeyList.Where(Input.GetKey)
            ).Direction
        );
    }

    private class MovementState {
        public static readonly List<KeyCode> PossibleKeyList =
            new List<KeyCode> {KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D};

        public List<KeyCode> KeyCodeList => _keyCodeList;
        public Vector2 Direction => _direction;

        private readonly List<KeyCode> _keyCodeList;
        private readonly Vector2 _direction;

        private MovementState(List<KeyCode> keyCodeList, Vector2 direction) {
            _keyCodeList = keyCodeList;
            _direction = direction;
        }

        private static Vector2 GetDirectionFromKey(KeyCode keyCode) {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (keyCode) {
                case KeyCode.W: return Vector2.up;
                case KeyCode.A: return Vector2.left;
                case KeyCode.S: return Vector2.down;
                case KeyCode.D: return Vector2.right;
                default: throw new InvalidDataException();
            }
        }

        public static MovementState Create(params KeyCode[] keyCodes) => Create(keyCodes.ToList());

        public static MovementState Create(IEnumerable<KeyCode> keyCodes) {
            var keyList = keyCodes.Where(it => PossibleKeyList.Contains(it)).ToList();
            var result = Vector2.zero;
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