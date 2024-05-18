using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BeatWalker
{
    public class TimingManager : MonoBehaviour
    {
        [SerializeField] private string song;
        [SerializeField] private GameObject linePrefab;
        [SerializeField] private int initLineCapacity;
        [SerializeField] private float startY;
        [SerializeField] private float endY;
        [SerializeField] private float lineSpeed;

        [SerializeField] private int earlyPoints;
        [SerializeField] private int perfectPoints;
        [SerializeField] private int latePoints;
        
        [SerializeField] private float earlyTime; // The amount of time before a line, which counts for early points.
        [SerializeField] private float perfectTime; // The amount of time after early time finishes, that counts for perfect points.
        [SerializeField] private float lateTime; // The amount of time after perfect time, that counts for late points.
        
        private Queue<Line> _reserveLines;
        private float _travelTime;
        private SongConfig _songConfig;
        private int _index;
        private float _startTime;
        private float _lineReferenceTime;
        private bool _isEnteringLine;

        private float _lateTimeOffset;
        private float _missedTimeOffset;
        
        private void Awake()
        {
            _reserveLines = new Queue<Line>(Enumerable.Range(0, initLineCapacity).Select(_ => CreateNewLine())
                .ToList());
            
            _songConfig = new SongConfig(song);
            
            var player = FindObjectOfType<Player>();
            player.Init(this);
            
            _travelTime = (startY - player.transform.position.y) / lineSpeed;
            _lateTimeOffset = earlyTime + perfectTime;
            _missedTimeOffset = _lateTimeOffset + lateTime;
        }

        private void Start()
        {
            _startTime = Time.timeSinceLevelLoad;
        }

        private void Update()
        {
            if (_reserveLines.Count > 0 && _index < _songConfig.Count)
            {
                StartCoroutine(NextLine());
            }
        }

        private IEnumerator NextLine()
        {
            if (_index >= _songConfig.Count)
                yield break;

            var nextTiming = _songConfig[_index++];
            var line = _reserveLines.Dequeue();

            yield return new WaitForSeconds(Mathf.Clamp(nextTiming.StartTime - Time.timeSinceLevelLoad - _startTime - _travelTime, 0.0f, float.MaxValue));
            line.Go(nextTiming.Duration);
        }
        
        public void NotGo(Line line)
        {
            _reserveLines.Enqueue(line);
        }

        private Line CreateNewLine()
        {
            var obj = Instantiate(linePrefab);
            var line = obj.GetComponent<Line>();
            line.Init(this, startY, endY, lineSpeed, earlyTime, earlyTime + perfectTime + lateTime);
            return line;
        }

        public void OnLineEnter()
        {
            _isEnteringLine = true;
            _lineReferenceTime = Time.timeSinceLevelLoad;
        }
 
        public void OnLineExit()
        {
            _isEnteringLine = false;
            _lineReferenceTime = Time.timeSinceLevelLoad;
        }

        public void OnAttack(InputValue button)
        {
            var attackTime = Time.timeSinceLevelLoad - _lineReferenceTime;

            if ((_isEnteringLine && !button.isPressed) || (!_isEnteringLine && button.isPressed) || attackTime > _missedTimeOffset)
            {
                Debug.Log("Missed");
                return;
            }
            
            if (attackTime > _lateTimeOffset)
            {
                Debug.Log("Late");
                return;
            }
                
            if (attackTime > earlyTime)
            {
                Debug.Log("Perfect");
                return;
            }
                
            Debug.Log("Early");
        }
    }
}