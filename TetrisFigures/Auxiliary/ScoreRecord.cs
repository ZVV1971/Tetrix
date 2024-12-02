using System;

namespace TetrisFigures.Auxiliary
{
    [Serializable]
    public class ScoreRecord
    {
        public string playerName { get; set; }
        public int score { get; set; }
        public int level { get; set; }
        public DateTime recordTimestamp { get; set; }
        public string gameFieldSize { get; set; }
        public GameComplexity complexityLevel { get; set; }
    }
}