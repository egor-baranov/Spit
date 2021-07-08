using System;
using UnityEngine;

public class Billboard : MonoBehaviour {
    [SerializeField] private bool shouldRotate;

    private void LateUpdate() {
        if (shouldRotate) {
            transform.rotation = Player.Instance.Body.transform.rotation;
        }
    }
}