using System;
using Controllers;
using UnityEngine;

public class Spit : MonoBehaviour {
    private void Awake() {
        Destroy(gameObject, 5);
    }

    private void OnTriggerEnter(Collider other) {
        try {
            if (other.transform.parent.GetComponent<Enemy>()) {
                Player.Instance.MoveTo(other.transform.parent.GetComponent<Enemy>());
                Destroy(gameObject);
            }
        }
        catch (NullReferenceException) {
            Destroy(gameObject);
        }
    }

    private void OnDestroy() {
        Player.Instance.Recharge();
    }
}