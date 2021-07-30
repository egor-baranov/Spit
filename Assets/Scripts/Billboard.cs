using Controllers;
using Controllers.Creatures;
using UnityEngine;

public class Billboard : MonoBehaviour {
    [SerializeField] private bool shouldRotate;

    private void Start() {
        if (shouldRotate) {
            transform.rotation = Player.Instance.Body.transform.rotation;
        }
    }
}