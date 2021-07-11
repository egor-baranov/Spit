using System;
using UnityEngine;

namespace Util.Data {
    /// <summary>
    ///  A class with active and passive int states, represented as fields.
    /// </summary>
    [Serializable]
    public class TwoState {
        public float Passive => passive;
        public float Active => active;

        [SerializeField] private float active;
        [SerializeField] private float passive;

        public TwoState(float passive, float active) {
            this.passive = passive;
            this.active = active;
        }
    }
}