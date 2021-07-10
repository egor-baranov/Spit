using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour {
    public GameObject Body => transform.GetChild(0).gameObject;

    [SerializeField] private float movementSpeed;
    [SerializeField] private float changeTarget;

    [SerializeField] private Vector3 _targetPosition;

    public Vector3 GenerateNextPoint() {
        return new Vector3(Random.Range(-94, 94), 6, Random.Range(-142, 150));
    }

    private void Start() {
        transform.position = GenerateNextPoint();
        _targetPosition = GenerateNextPoint();
    }

    private void Update() {
        if (Vector3.Distance(transform.position, _targetPosition) < changeTarget) {
            _targetPosition = GenerateNextPoint();
        }

        GetComponent<Rigidbody>().velocity = (_targetPosition - transform.position).normalized * movementSpeed;
    }
}