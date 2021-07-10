using System;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int enemyCount;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
        }

        Instance = this;

        for (var i = 0; i < enemyCount; i++) {
            Instantiate(enemyPrefab);
        }
    }
}