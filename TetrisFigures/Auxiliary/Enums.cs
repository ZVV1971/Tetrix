using System.ComponentModel;

namespace TetrisFigures.Auxiliary
{
    public enum MovementOutcomes
    {
        [Description("The movement is completely impossible, i.e. the moving figure would come out of borders or overlap the pile")]
        Impossible = 0,
        [Description("The movement is possible and the game won't be finalized by it")]
        Possible = 1,
        [Description("The movement is possible, but the figure will get frozen by it")]
        Frozen = 3,
        [Description("The movement is possible, but the figure will get frozen and the gameplay will be finalized by it")]
        EndOfPlay = 4
    }
}
