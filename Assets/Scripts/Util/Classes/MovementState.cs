using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Util.Classes {
    public class MovementState {
        public static readonly List<KeyCode> PossibleKeyList =
            new List<KeyCode> {KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D};

        public Vector3 Direction { get; }

        private readonly Transform _transform;

        private MovementState(Transform transform, List<KeyCode> keyCodeList, Vector3 direction) {
            _transform = transform;
            Direction = direction;
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