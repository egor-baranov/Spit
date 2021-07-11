using System;
using UnityEngine;

namespace Util.Data {
    [Serializable]
    public struct MinMax {
        public int Min => min;

        public int Max => max;

        [SerializeField] private int min;
        [SerializeField] private int max;

        public MinMax(int min = -1, int max = -1) {
            this.min = min;
            this.max = max;
        }

        public bool Contains(int value) => min <= value && max >= value;
    }
}