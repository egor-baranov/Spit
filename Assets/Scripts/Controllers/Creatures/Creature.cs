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
        protected bool IsAlive => HealthPoints > 0;
        protected BodyIntent Intent => BodyIntent.Create(this);
        private SliderObject HpSlider => Body.transform.Find("HP Slider").GetComponent<SliderObject>();

        private float _healthPoints;
        [SerializeField] protected float maxHp;
        [SerializeField] protected float movementSpeed;

        public void ReceiveDamage(float damage) => HealthPoints -= damage;

        public void SwapWith(Creature other) {
            AcceptIntent(other.AcceptIntent(Intent));
            OnSwap();
            other.OnSwap();
        }

        protected BodyIntent AcceptIntent(BodyIntent intent) {
            var prevIntent = Intent;

            _healthPoints = intent.HealthPoints;
            maxHp = intent.MaxHealthPoints;
            movementSpeed = intent.MovementSpeed;
            Body.GetComponent<SpriteRenderer>().sprite = intent.Sprite;
            transform.position = intent.Position;

            return prevIntent;
        }

        protected virtual void Awake() => HealthPoints = maxHp;

        protected virtual void Start() { }

        protected virtual void Update() { }

        protected virtual void OnDeath() {
            if (Player.Instance == this) {
                GameManager.OnDeath();
            }
            else {
                Destroy(gameObject);
            }
        }

        protected virtual void OnSwap() { }

        protected class BodyIntent {
            public float HealthPoints { get; }
            public float MaxHealthPoints { get; }
            public float MovementSpeed { get; }
            public Sprite Sprite { get; }
            public Vector3 Position { get; }

            public static BodyIntent Create(Creature creature) =>
                new BodyIntent(
                    creature.HealthPoints,
                    creature.MaxHp,
                    creature.movementSpeed,
                    creature.Body.GetComponent<SpriteRenderer>().sprite,
                    creature.transform.position
                );

            private BodyIntent(float hp, float maxHp, float movementSpeed, Sprite sprite, Vector3 position) {
                HealthPoints = hp;
                MaxHealthPoints = maxHp;
                MovementSpeed = movementSpeed;
                Sprite = sprite;
                Position = position;
            }
        }
    }
}