using Core;
using UnityEngine;

namespace Controllers.Creatures.Base {
    public abstract class Creature : MonoBehaviour {
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

        public virtual void ReceiveDamage(float damage) {
            HealthPoints -= damage;
            OnReceiveDamage();
        }

        public void SwapWith(Creature other) {
            AcceptIntent(other.AcceptIntent(Intent));
            OnSwap(other);
            other.OnSwap(this);
        }

        protected BodyIntent AcceptIntent(BodyIntent intent) {
            var prevIntent = Intent;

            _healthPoints = intent.HealthPoints;
            maxHp = intent.MaxHealthPoints;
            movementSpeed = intent.MovementSpeed;
            body = intent.Body;
            body.transform.SetParent(transform, false);
            transform.position = intent.Position;

            return prevIntent;
        }

        protected virtual void Awake() => HealthPoints = maxHp;

        protected abstract void Start();

        protected abstract void Update();

        protected virtual void OnDeath() {
            if (Player.Instance == this) {
                GameManager.OnDeath();
            }
            else {
                Destroy(gameObject);
            }
        }

        protected abstract void OnSwap(Creature other);
        protected abstract void OnReceiveDamage();

        protected class BodyIntent {
            public float HealthPoints { get; }
            public float MaxHealthPoints { get; }
            public float MovementSpeed { get; }
            public GameObject Body { get; }
            public Vector3 Position { get; }
            
            public static BodyIntent Create(Creature creature) =>
                new BodyIntent(
                    creature.HealthPoints,
                    creature.MaxHp,
                    creature.movementSpeed,
                    creature.Body,
                    creature.transform.position
                );

            private BodyIntent(float hp, float maxHp, float movementSpeed,
                GameObject body, Vector3 position) {
                HealthPoints = hp;
                MaxHealthPoints = maxHp;
                MovementSpeed = movementSpeed;
                Body = body;
                Position = position;
            }
        }
    }
}