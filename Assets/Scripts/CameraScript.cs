using Controllers.Creatures;
using UnityEngine;
using Util.Data;

public class CameraScript : MonoBehaviour {
    public static CameraScript Instance { get; private set; }
    private static Transform CameraHolder => Player.Instance.CameraHolder.transform;

    private Vector3 _distanceFromTarget;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float cameraDistance;
    [SerializeField] private float zoomOutScale;

    [SerializeField] private MinMaxFloat cameraRangeX, cameraRangeZ;
    
    public void LateUpdate() {
        if (!Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.E)) {
            transform.position = Vector3.Lerp(
                transform.position,
                Fit(CameraHolder.position - _distanceFromTarget),
                Time.deltaTime * movementSpeed
            );
        }

        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            _distanceFromTarget *= zoomOutScale;
        }

        if (Input.GetKeyUp(KeyCode.Mouse1)) {
            _distanceFromTarget /= zoomOutScale;
        }

        if (Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.E)) {
            _distanceFromTarget = (transform.position - CameraHolder.position).normalized *
                                  (cameraDistance * (Input.GetKey(KeyCode.Mouse1) ? zoomOutScale : 1));
            transform.parent = null;
        }

        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E)) {
            transform.parent = CameraHolder;
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
        _distanceFromTarget = (transform.position - CameraHolder.position).normalized * cameraDistance;
        transform.LookAt(CameraHolder);
    }
}