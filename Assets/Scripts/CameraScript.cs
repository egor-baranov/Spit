using System;
using Controllers;
using UnityEngine;

public class CameraScript : MonoBehaviour {
    public static CameraScript Instance { get; private set; }
    private Vector3 _distanceFromPlayer;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;

    private float _targetCameraZ = -100F;

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
        transform.LookAt(Player.Instance.transform);

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            new Vector3(0, 0, _targetCameraZ),
            30 * Time.deltaTime
        );

        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            _targetCameraZ = -200F;
        }

        if (Input.GetKeyUp(KeyCode.Mouse0)) {
            _targetCameraZ = -100F;
        }
    }
}