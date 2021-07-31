using System;
using System.Collections;
using Controllers.Creatures;
using Controllers.Creatures.Enemies.Base;
using Spine.Unity;
using UnityEngine;
using Util.ExtensionMethods;

namespace Controllers.Animation {
    public class CreatureAnimationController : MonoBehaviour {
        [SerializeField] private SkeletonAnimation skeletonAnimation;
        [SerializeField] private AnimationReferenceAsset idle, movement;

        private AnimationState _state = AnimationState.Idle;
        private bool _faceRight;
        private Vector3 _prevPosition;

        private float _timeLeft, _totalTime;
        private Color _endColor, _startColor;

        public void OnReceiveDamage() => StartCoroutine(ReceiveDamageCoroutine());

        public void SetColor(Color color, float timeLeft) {
            if (timeLeft < Time.deltaTime) {
                skeletonAnimation.skeleton.SetColor(color);
            }

            _timeLeft = timeLeft;
            _totalTime = timeLeft;
            _startColor = skeletonAnimation.skeleton.GetColor();
            _endColor = color;
        }

        private void Start() {
            SetState(AnimationState.Idle);
            _prevPosition = transform.parent.position;
            skeletonAnimation.skeleton.SetColor(Color.white);
        }

        private void Update() {
            if (_timeLeft < Time.deltaTime) return;
            _timeLeft -= Time.deltaTime;

            float F(float end, float start) => (end * (_totalTime - _timeLeft) + start * _timeLeft) / _totalTime;
            skeletonAnimation.skeleton.SetColor(
                new Color(
                    F(_endColor.r, _startColor.r), F(_endColor.g, _startColor.g),
                    F(_endColor.b, _startColor.b), F(_endColor.a, _startColor.a)
                )
            );
        }

        private void FixedUpdate() {
            if (transform.parent.GetComponent<Player>()) {
                ApplyPlayerControls();
            }

            if (transform.parent.GetComponent<Enemy>()) {
                ApplyEnemyControls();
            }
        }

        private void ApplyPlayerControls() {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) {
                SetState(AnimationState.Movement, faceRight: Input.GetKey(KeyCode.D));
            }
            else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) {
                SetState(AnimationState.Movement, faceRight: _faceRight);
            }
            else {
                SetState(AnimationState.Idle, _faceRight);
            }
        }

        private void ApplyEnemyControls() {
            var position = transform.parent.position;
            if ((position - _prevPosition).magnitude > 0.0001F) {
                SetState(AnimationState.Movement, faceRight: position.x > _prevPosition.x);
            }
            else {
                SetState(AnimationState.Idle);
            }

            skeletonAnimation.skeleton.SetColor(new Color(0.4F, 1F, 0.4F, 1F));
            _prevPosition = position;
        }

        private void SetState(AnimationState animationState, bool faceRight = false) {
            if (_state == animationState && faceRight == _faceRight) {
                return;
            }

            switch (animationState) {
                case AnimationState.Idle:
                    SetAnimation(idle, flipX: faceRight);
                    break;
                case AnimationState.Movement:
                    SetAnimation(movement, flipX: faceRight);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(animationState), animationState, null);
            }

            _state = animationState;
            _faceRight = faceRight;
        }

        private void SetAnimation(AnimationReferenceAsset referenceAsset, bool shouldLoop = true,
            float timeScale = 1F, bool flipX = false) {
            skeletonAnimation.state.SetAnimation(0, referenceAsset, shouldLoop).TimeScale = timeScale;
            skeletonAnimation.skeleton.FlipX = flipX;
        }

        private IEnumerator ReceiveDamageCoroutine() {
            foreach (var i in 0.Until(10)) {
                SetColor(i % 2 == 1 ? Color.white : new Color(0.5F, 0.5F, 0.5F, 0.2F), 0.15F);
                yield return new WaitForSeconds(0.1F);
            }

            SetColor(Color.white, 1);
        }

        private enum AnimationState {
            Idle,
            Movement
        }
    }
}