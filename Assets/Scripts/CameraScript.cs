﻿using Controllers.Creatures;
using UnityEngine;
using Util.Data;

public class CameraScript : MonoBehaviour {
    public static CameraScript Instance { get; private set; }
    private static Transform CameraHolder => Player.Instance.CameraHolder.transform;

    private Vector3 _distanceFromTarget;
    private readonly Vector3 _shootingDistance = -Vector3.up * 240;
    [SerializeField] private float cameraDistance;
    [SerializeField] private float zoomOutScale;

    [SerializeField] private MinMaxFloat cameraRangeX, cameraRangeZ;
    [SerializeField] private LayerMask layerMask;

    private Transform _target;
    private float _movementSpeed;

    public void SetTarget(Transform targetTransform, float speed) {
        _target = targetTransform;
        _movementSpeed = speed;
    }

    public void LateUpdate() {
        if (_target != null) {
            var mousePosition = _target.position;
            var ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask)) {
                mousePosition = new Vector3(raycastHit.point.x, _target.position.y, raycastHit.point.z);
            }

            transform.position = Vector3.Lerp(
                transform.position,
                Fit((_target.position * 7 + mousePosition) / 8 +
                    _target.GetComponentInParent<Rigidbody>().velocity.normalized * 15 -
                    (Input.GetKey(KeyCode.Mouse1) ? _shootingDistance : _distanceFromTarget)),
                Time.deltaTime * _movementSpeed
            );
        }

        transform.rotation = Quaternion.RotateTowards(
            CameraHolder.transform.rotation,
            Quaternion.Euler(
                Input.GetKey(KeyCode.Mouse1) ? 90 : 73,
                transform.rotation.y,
                transform.rotation.z
            ),
            Time.deltaTime
        );
    }

    private Vector3 Fit(Vector3 position) =>
        new Vector3(cameraRangeX.Fit(position.x), position.y, cameraRangeZ.Fit(position.z));

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
        }

        Instance = this;
    }

    private void Start() {
        _distanceFromTarget = (transform.position - CameraHolder.position).normalized *
                              (cameraDistance / Mathf.Pow(zoomOutScale, 2));
        transform.LookAt(CameraHolder);
        SetTarget(CameraHolder, 5);
    }
}