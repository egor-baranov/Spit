using Controllers.Creatures;
using Controllers.Creatures.Enemies.Base;
using Spine.Unity;
using UnityEngine;

namespace Controllers.Animation {
    public class CreatureAnimationController : MonoBehaviour {
        [SerializeField] private SkeletonAnimation skeletonAnimation;
        [SerializeField] private AnimationReferenceAsset idle, movement;

        private AnimationState _state = AnimationState.Idle;

        private bool _faceRight = false;

        private void Start() {
            SetState(AnimationState.Idle);
        }

        private void FixedUpdate() {
            if (GetComponent<Player>()) {
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) {
                    SetState(AnimationState.Movement, faceRight: Input.GetKey(KeyCode.D));
                } else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) {    
                    SetState(AnimationState.Movement, faceRight: _faceRight);
                } 
                else {
                    SetState(AnimationState.Idle, _faceRight);
                }
            }

            if (GetComponent<Enemy>()) {
                var velocity = GetComponent<Rigidbody>().velocity;
                if (velocity.magnitude > 0.001F) {
                    SetState(AnimationState.Movement, faceRight: velocity.x > 0);
                }
                else {
                    SetState(AnimationState.Idle);
                }
            }
        }

        public void SetState(AnimationState animationState, bool faceRight = false) {
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
            }

            _state = animationState;
            _faceRight = faceRight;
        }

        private void SetAnimation(AnimationReferenceAsset referenceAsset, bool shouldLoop = true,
            float timeScale = 1F, bool flipX = false) {
            skeletonAnimation.state.SetAnimation(0, referenceAsset, shouldLoop).TimeScale = timeScale;
            skeletonAnimation.skeleton.FlipX = flipX;
        }

        public enum AnimationState {
            Idle,
            Movement
        }
    }
}