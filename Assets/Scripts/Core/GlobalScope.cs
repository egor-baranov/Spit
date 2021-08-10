using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Util.ExtensionMethods;

namespace Core {
    public static class GlobalScope {
        private sealed class RelatedMonoBehaviour : MonoBehaviour {
            internal void DoWithDelay(
                float delay,
                UnityAction action,
                UnityAction preAction = null) =>
                StartCoroutine(
                    DoWithDelayCoroutine(delay, action, preAction)
                );

            internal void DoEveryInterval(
                float timeInterval,
                UnityAction action,
                Func<bool> stopCondition = null) =>
                StartCoroutine(
                    DoEveryIntervalCoroutine(timeInterval, action, stopCondition)
                );

            internal void DoMultipleTimes(
                float timeSeparator,
                UnityAction action,
                int count,
                float preDelay = 0F,
                UnityAction endAction = null,
                Func<bool> stopCondition = null) =>
                StartCoroutine(
                    DoMultipleTimesCoroutine(timeSeparator, action, count, preDelay, endAction, stopCondition)
                );

            private static IEnumerator DoWithDelayCoroutine(float time, UnityAction action,
                UnityAction preAction = null) {
                preAction?.Invoke();
                yield return new WaitForSecondsRealtime(time);
                try {
                    action.Invoke();
                }
                catch {
                    // ignored
                }
            }

            private static IEnumerator DoEveryIntervalCoroutine(
                float timeInterval,
                UnityAction action,
                Func<bool> stopCondition) {
                while (stopCondition == null || !stopCondition()) {
                    yield return new WaitForSecondsRealtime(timeInterval);

                    try {
                        action.Invoke();
                    }
                    catch (Exception) {
                        yield break;
                    }
                }
            }

            private static IEnumerator DoMultipleTimesCoroutine(
                float timeInterval,
                UnityAction action,
                int repeatTimes,
                float preDelay,
                UnityAction endAction = null,
                Func<bool> stopCondition = null
            ) {
                yield return new WaitForSecondsRealtime(preDelay);
                foreach (var i in 0.Until(repeatTimes)) {
                    if (stopCondition != null && stopCondition.Invoke()) {
                        break;
                    }

                    action.Invoke();
                    if (i != repeatTimes - 1) {
                        yield return new WaitForSecondsRealtime(timeInterval);
                    }
                }

                endAction?.Invoke();
            }

            private void OnDestroy() {
                GlobalScope._relatedMonoBehaviour = null;
            }
        }

        private static RelatedMonoBehaviour _relatedMonoBehaviour;

        public static void ExecuteWithDelay(float delay, UnityAction action, UnityAction preAction = null) {
            Init();
            try {
                _relatedMonoBehaviour.DoWithDelay(delay, action, preAction);
            }
            catch (MissingReferenceException) {
                Init();
            }
        }

        public static void ExecuteEveryInterval(
            float timeInterval,
            UnityAction action,
            Func<bool> stopCondition = null) {
            Init();
            try {
                _relatedMonoBehaviour.DoEveryInterval(timeInterval, action, stopCondition);
            }
            catch (MissingReferenceException) {
                Init();
            }
        }

        public static void ExecuteMultipleTimes(
            float timeInterval,
            UnityAction action,
            int repeatTimes,
            UnityAction endAction = null,
            float preDelay = 0F,
            Func<bool> stopCondition = null
        ) {
            Init();
            try {
                _relatedMonoBehaviour.DoMultipleTimes(
                    timeInterval,
                    action,
                    repeatTimes,
                    preDelay,
                    endAction,
                    stopCondition
                );
            }
            catch (MissingReferenceException) {
                Init();
            }
        }

        private static void Init() {
            if (_relatedMonoBehaviour is null) {
                _relatedMonoBehaviour =
                    new GameObject("RelatedMonoBehaviour").AddComponent<RelatedMonoBehaviour>();
            }
        }
    }
}