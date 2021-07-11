﻿using UnityEngine;

namespace Core.Managers {
    public class UiManager : MonoBehaviour {
        public static UiManager Instance { get; private set; }

        [SerializeField] private GameObject deathPanel;

        public void OnDeath() => deathPanel.SetActive(true);

        private void Awake() {
            if (Instance != null) {
                Destroy(gameObject);
            }

            Instance = this;
        }
    }
}