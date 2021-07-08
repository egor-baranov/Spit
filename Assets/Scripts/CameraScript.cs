using System;
using UnityEngine;

public class CameraScript : MonoBehaviour {
    public static CameraScript Instance { get; private set; }
    private Vector3 _distanceFromPlayer;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
        }

        Instance = this;
    }

    private void Start() {
        _distanceFromPlayer = transform.position - Player.Instance.transform.position;
    }

    public void Update() {
        transform.position = Vector3.Lerp(
            transform.position,
            Player.Instance.transform.position + _distanceFromPlayer,
            Time.deltaTime * movementSpeed
        );
    }
}