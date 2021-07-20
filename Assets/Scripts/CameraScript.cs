using Controllers.Creatures;
using UnityEngine;
using Util.Data;

public class CameraScript : MonoBehaviour {
    public static CameraScript Instance { get; private set; }
    private static Transform CameraHolder => Player.Instance.CameraHolder.transform;

    private Vector3 _distanceFromTarget;
    [SerializeField] private float cameraDistance;
    [SerializeField] private float zoomOutScale;

    [SerializeField] private MinMaxFloat cameraRangeX, cameraRangeZ;

    private Transform _target;
    private float _movementSpeed;

    public void SetTarget(Transform targetTransform, float speed) {
        _target = targetTransform;
        _movementSpeed = speed;

        if (_target == CameraHolder) {
            _distanceFromTarget *= Mathf.Pow(zoomOutScale, 2);
        }
        else {
            _distanceFromTarget /= Mathf.Pow(zoomOutScale, 2);
        }
    }

    public void LateUpdate() {
        if (transform.parent == CameraHolder.transform && CameraHolder.transform.rotation.eulerAngles.x - 75 <= 0.01F) {
            transform.parent = null;
        }
        
        if (!Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.E) || _target != CameraHolder) {
            if (_target != null && transform.parent == null) {
                transform.position = Vector3.Lerp(
                    transform.position,
                    Fit(_target.position - _distanceFromTarget),
                    Time.deltaTime * _movementSpeed
                );
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            _distanceFromTarget *= zoomOutScale;
            transform.parent = CameraHolder;
        }

        if (Input.GetKeyUp(KeyCode.Mouse1)) {
            _distanceFromTarget /= zoomOutScale;
        }

        if ((Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E)) && _target == CameraHolder) {
            transform.parent = CameraHolder;
        }
        
        if ((Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.E)) && _target == CameraHolder) {
            _distanceFromTarget = (transform.position - CameraHolder.position).normalized *
                                  (cameraDistance * (Input.GetKey(KeyCode.Mouse1) ? zoomOutScale : 1));
            transform.parent = null;
        }
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
        SetTarget(CameraHolder, 2);
    }
}