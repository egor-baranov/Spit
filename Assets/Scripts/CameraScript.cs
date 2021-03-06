using Controllers.Creatures;
using UnityEngine;
using Util.Data;

public class CameraScript : MonoBehaviour {
    public static CameraScript Instance { get; private set; }
    private static Transform CameraHolder => Player.Instance.CameraHolder.transform;

    private Vector3 _distanceFromTarget;
    private readonly Vector3 _shootingDistance = -Vector3.up * 240;
    [SerializeField] private float cameraDistance;
    [SerializeField] private float zoomOutScale;
    [SerializeField] private float feedbackScale;

    [SerializeField] private MinMaxFloat cameraRangeX, cameraRangeZ;
    [SerializeField] private LayerMask layerMask;

    private Transform _target;
    private float _movementSpeed;
    private float _zoomCoefficient;

    private Vector3 _movementPosition;

    public void SetTarget(Transform targetTransform, float speed, float zoomCoefficient = 1F) {
        _target = targetTransform;
        _movementSpeed = speed;
        _zoomCoefficient = zoomCoefficient;
    }

    public void Feedback(Vector3 direction) {
        transform.position -= direction.normalized * feedbackScale;
    }

    public void LateUpdate() {
        if (Player.Instance.HealthPoints <= 0) {
            return;
        }

        if (_target != null) {
            var mousePosition = _target.position;
            var ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask)) {
                mousePosition = new Vector3(raycastHit.point.x, _target.position.y, raycastHit.point.z);
            }

            _movementPosition =
                Fit((_target.position * 7 + mousePosition) / 8 -
                    (Input.GetKey(KeyCode.Mouse1) && Player.Instance.CanPerformSoulBlast || _target != CameraHolder
                        ? _shootingDistance
                        : _distanceFromTarget) * _zoomCoefficient
                );
            
            var viewPortDistance = Vector2.Distance(
                GetComponent<Camera>().WorldToViewportPoint(Player.Instance.transform.position),
                GetComponent<Camera>().ScreenToViewportPoint(Input.mousePosition));

            transform.position = Vector3.Lerp(
                transform.position,
                _movementPosition,
                Time.deltaTime * _movementSpeed / (_target == CameraHolder ? viewPortDistance : 1)
            );
        }

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.Euler(
                Input.GetKey(KeyCode.Mouse1) && Player.Instance.CanPerformSoulBlast || _target != CameraHolder
                    ? 90
                    : 73,
                transform.rotation.y,
                transform.rotation.z
            ),
            Time.deltaTime * 5
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
                              (cameraDistance / Mathf.Pow(zoomOutScale, 3));
        transform.LookAt(CameraHolder);
        SetTarget(CameraHolder, 3);
    }
}