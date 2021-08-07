using System;
using Controllers.Creatures;
using UnityEngine;
using Util.Data;

namespace Controllers.Objects {
    public class Eye : MonoBehaviour {
        private Transform Pupil => transform.Find("Pupil");

        [SerializeField] private MinMaxFloat xRange;

        private void Update() {
            var angle = Vector3.SignedAngle(
                transform.forward,
                Player.Instance.transform.position - transform.position,
                transform.up
            );
            Debug.Log(angle);
            Pupil.transform.localPosition =
                new Vector2(
                    Mathf.Pow(Mathf.Abs(angle) / 90, 2) * (angle > 0 ? xRange.Max : xRange.Min),
                    0
                );
        }
    }
}