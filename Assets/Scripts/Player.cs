using System;
using System.Collections.Generic;
using BeatWalker.Config;
using BeatWalker.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BeatWalker
{
    public class Player : MonoBehaviour
    {
        private static readonly int HitTrigger = Animator.StringToHash("Hit");
        private static readonly int HoldBool = Animator.StringToHash("Hold");
        private static readonly int LeftTapTrigger = Animator.StringToHash("LeftTap");
        private static readonly int RightTapTrigger = Animator.StringToHash("RightTap");

        private Animator _animator;

        public void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider.CompareTag("Enemy"))
                _animator.SetTrigger(HitTrigger);
        }

        public void Hold(bool hold)
        {
            _animator.SetBool(HoldBool, hold);
        }

        public void Tap(SongTimingType type)
        {
            _animator.SetTrigger(type switch
            {
                SongTimingType.LeftTap => LeftTapTrigger,
                SongTimingType.RightTap => RightTapTrigger,
                SongTimingType.Hold => throw new ArgumentOutOfRangeException(),
                _ => throw new ArgumentOutOfRangeException()
            });
        }
    }
}