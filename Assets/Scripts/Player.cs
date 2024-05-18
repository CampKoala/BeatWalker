using System;
using UnityEngine;

namespace BeatWalker
{
    public class Player : MonoBehaviour
    {
        private TimingManager _tm;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("LineEnter"))
            {
                _tm.OnLineEnter();
            }

            if (other.CompareTag("LineExit"))
            {
                _tm.OnLineExit();
            }
        }

        public void Init(TimingManager timingManager)
        {
            _tm = timingManager;
        }
    }
}