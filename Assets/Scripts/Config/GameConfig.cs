using UnityEngine;

namespace BeatWalker.Config
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "ScriptableObjects/GameConfig", order = 1)]
    public class GameConfig : ScriptableObject
    {
        [SerializeField] private int earlyPoints;
        public int EarlyPoints => earlyPoints;
        
        [SerializeField] private int perfectPoints;
        public int PerfectPoints => perfectPoints;
        
        [SerializeField] private int latePoints;
        public int LatePoints => latePoints;
        
        [Header("Lines")]
        [SerializeField] private GameObject linePrefab;
        public GameObject LinePrefab => linePrefab;
        
        [SerializeField] private float tapLineLength;
        public float TapLineLength => tapLineLength;
        
        [SerializeField] private float lineStartY;
        public float LineStartY => lineStartY;
        
        [SerializeField] private float lineEndY;
        public float LineEndY => lineEndY;
        
        [SerializeField] private float lineSpeed;
        public float LineSpeed => lineSpeed;
        
        [SerializeField] private float earlyTime; // The amount of time before a line, which counts for early points.
        public float EarlyTime => earlyTime;
        
        [SerializeField] private float perfectTime; // The amount of time after early time finishes, that counts for perfect points.
        public float PerfectTime => perfectTime;
        
        [SerializeField] private float lateTime; // The amount of time after perfect time, that counts for late points.
        public float LateTime => lateTime;

        public float LateTimeOffset => earlyTime + perfectTime;
        public float MissedTimeOffset => LateTimeOffset + lateTime;


        [Header("Enemies")]
        [SerializeField] private EnemyConfig holdEnemy;
        public EnemyConfig HoldEnemy => holdEnemy;
        
        [SerializeField] private EnemyConfig tapEnemy;
        public EnemyConfig TapEnemy => tapEnemy;
    }
}