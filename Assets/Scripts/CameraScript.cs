using Controllers.Creatures;
using UnityEngine;

public class CameraScript : MonoBehaviour {
    public static CameraScript Instance { get; private set; }
    private Transform CameraHolder => Player.Instance.CameraHolder.transform;

    private Vector3 _distanceFromPlayer;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float cameraDistance;
    [SerializeField] private float zoomOutScale;

    private float _targetCameraZ;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
        }

        Instance = this;
        _targetCameraZ = -cameraDistance;
    }

    private void Start() {
        _distanceFromPlayer = (transform.position - Player.Instance.transform.position).normalized * cameraDistance;
        transform.LookAt(Player.Instance.transform);
    }

    public void LateUpdate() {
        if (!Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.E)) {
            transform.position = Vector3.Lerp(
                transform.position,
                CameraHolder.position - _distanceFromPlayer,
                Time.deltaTime * movementSpeed
            );
        }

        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            _distanceFromPlayer *= zoomOutScale;
        }

        if (Input.GetKeyUp(KeyCode.Mouse1)) {
            _distanceFromPlayer /= zoomOutScale;
        }

        if (Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.E)) {
            _distanceFromPlayer = (transform.position - Player.Instance.transform.position).normalized *
                                  (cameraDistance * (Input.GetKey(KeyCode.Mouse1) ? zoomOutScale : 1));
            transform.parent = null;
        }

        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E)) {
            transform.parent = CameraHolder;
        }
    }
}