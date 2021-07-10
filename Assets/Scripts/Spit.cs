using System;
using UnityEngine;

public class Spit : MonoBehaviour {
    private void Awake() {
        Destroy(gameObject, 5);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.transform.parent == null) {
            Destroy(gameObject);
        }

        if (other.transform.parent.GetComponent<Enemy>()) {
            Player.Instance.MoveTo(other.transform.parent.GetComponent<Enemy>());
            Destroy(gameObject);
        }
    }

    private void OnDestroy() {
        Player.Instance.Recharge();
    }
}