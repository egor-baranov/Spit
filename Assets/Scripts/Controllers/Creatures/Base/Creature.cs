using Core;
using UnityEngine;

namespace Controllers.Creatures.Base {
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

        public float MaxHp => maxHp;

        protected GameObject Body => body;
        protected bool IsAlive => HealthPoints > 0;
        protected BodyIntent Intent => BodyIntent.Create(this);
        private SliderObject HpSlider => Body.transform.Find("HP Slider").GetComponent<SliderObject>();

        private float _healthPoints;
        [SerializeField] protected float maxHp;
        [SerializeField] protected float movementSpeed;
        [SerializeField] protected GameObject body;

        [SerializeField] protected float rechargeTime;

        public virtual void ReceiveDamage(float damage) => HealthPoints -= damage;

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
            body = intent.Body;
            body.transform.SetParent(transform, false);
            transform.position = intent.Position;
            rechargeTime = intent.RechargeTime;

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
            public GameObject Body { get; }
            public Vector3 Position { get; }
            public float RechargeTime { get; }

            public static BodyIntent Create(Creature creature) =>
                new BodyIntent(
                    creature.HealthPoints,
                    creature.MaxHp,
                    creature.movementSpeed,
                    creature.Body,
                    creature.transform.position,
                    creature.rechargeTime
                );

            private BodyIntent(float hp, float maxHp, float movementSpeed, GameObject body, Vector3 position, float rechargeTime) {
                HealthPoints = hp;
                MaxHealthPoints = maxHp;
                MovementSpeed = movementSpeed;
                Body = body;
                Position = position;
                RechargeTime = rechargeTime;
            }
        }
    }
}