using UnityEngine;

public class CameraScript: MonoBehaviour {
    public static CameraScript Instance { get; private set; }

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
        }

        Instance = this;
    }
}