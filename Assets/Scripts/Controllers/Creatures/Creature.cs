using Core;
using UnityEngine;

namespace Controllers.Creatures {
    public class Creature : MonoBehaviour {
        public float HealthPoints {
            get => _healthPoints;
            set {
                if (value <= 0 && IsAlive) {
                    OnDeath();
                }

                _healthPoints = value;
                HpSlider.Value = value;
            }
        }

        public float MaxHp {
            get => maxHp;
            set => maxHp = value;
        }

        public GameObject Body => transform.Find("Body").gameObject;
        protected SliderObject HpSlider => Body.transform.Find("HP Slider").GetComponent<SliderObject>();

        private float _healthPoints;
        [SerializeField] protected float maxHp;
        [SerializeField] protected float movementSpeed;

        public bool IsAlive => HealthPoints > 0;
        public void ReceiveDamage(float damage) => HealthPoints -= damage;

        protected virtual void Awake() => HealthPoints = maxHp;

        protected virtual void Start() { }

        protected virtual void Update() { }

        protected virtual void OnDeath() {
            if (Player.Instance == this) {
                GameManager.Instance.OnDeath();
            }
            else {
                Destroy(gameObject);
            }
        }
    }
}