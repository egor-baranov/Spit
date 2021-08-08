using System;
using Core;
using UnityEngine.Events;

namespace Util.Classes {
    public class GameThread {
        private bool _isAlive = true;

        public GameThread Subscribe(float timeInterval, UnityAction action, Func<bool> stopCondition = null) {
            GlobalScope.ExecuteEveryInterval(
                timeInterval,
                action,
                () => stopCondition == null ? !_isAlive : !_isAlive || stopCondition.Invoke()
            );
            return this;
        }

        public GameThread Subscribe(float timeInterval, UnityAction action, int count, float preDelay = 0F) {
            GlobalScope.ExecuteMultipleTimes(
                timeInterval, action, count, preDelay, () => !_isAlive
            );
            return this;
        }

        public void Destroy(float delay = 0F) => GlobalScope.ExecuteWithDelay(delay, () => _isAlive = false);

        public GameThread Unsubscribe(float delay = 0F) {
            Destroy(delay);
            return Create();
        }

        public static GameThread Create() => new GameThread();
    }
}