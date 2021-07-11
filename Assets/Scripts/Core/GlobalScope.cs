using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Util.ExtensionMethods;

namespace Core {
    public static class GlobalScope {
        private sealed class RelatedMonoBehaviour : MonoBehaviour {
            internal void DoWithDelay(float delay, UnityAction action, UnityAction preAction = null) =>
                StartCoroutine(DoWithDelayCoroutine(delay, action, preAction));

            internal void DoEveryInterval(float timeInterval, UnityAction action,
                Func<bool> stopCondition = null) =>
                StartCoroutine(DoEveryIntervalCoroutine(timeInterval, action, stopCondition));

            internal void DoMultipleTimes(float timeSeparator, UnityAction action, int count, float preDelay = 0F) =>
                StartCoroutine(DoMultipleTimesCoroutine(timeSeparator, action, count, preDelay));

            private static IEnumerator DoWithDelayCoroutine(float time, UnityAction action,
                UnityAction preAction = null) {
                preAction?.Invoke();
                yield return new WaitForSeconds(time);
                action.Invoke();
            }

            private static IEnumerator DoEveryIntervalCoroutine(float timeInterval, UnityAction action,
                Func<bool> stopCondition) {
                while (!stopCondition()) {
                    yield return new WaitForSeconds(timeInterval);
                    action.Invoke();
                }
            }

            private static IEnumerator DoMultipleTimesCoroutine(float timeSeparator,
                UnityAction action, int repeatTimes, float preDelay) {
                yield return new WaitForSeconds(preDelay);
                foreach (var i in 0.Until(repeatTimes)) {
                    action.Invoke();
                    if (i != repeatTimes - 1) {
                        yield return new WaitForSeconds(timeSeparator);
                    }
                }
            }
        }

        private static RelatedMonoBehaviour _relatedMonoBehaviour;

        public static void ExecuteWithDelay(float delay, UnityAction action, UnityAction preAction = null) {
            Init();
            _relatedMonoBehaviour.DoWithDelay(delay, action, preAction);
        }

        public static void ExecuteEveryInterval(
            float timeInterval, 
            UnityAction action,
            Func<bool> stopCondition = null) {
            Init();
            _relatedMonoBehaviour.DoEveryInterval(timeInterval, action, stopCondition);
        }

        public static void ExecuteMultipleTimes(
            float timeSeparator,
            UnityAction action,
            int count,
            float preDelay = 0F) {
            Init();
            _relatedMonoBehaviour.DoMultipleTimes(timeSeparator, action, count, preDelay);
        }

        private static void Init() {
            if (_relatedMonoBehaviour is null) {
                _relatedMonoBehaviour =
                    new GameObject("RelatedMonoBehaviour").AddComponent<RelatedMonoBehaviour>();
            }
        }
    }
}