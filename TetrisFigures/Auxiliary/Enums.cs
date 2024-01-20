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
        NeedsFreezing = 3,
        [Description("The movement is possible, but the figure will get frozen and the gameplay will be finalized by it")]
        EndOfPlay = 4
    }

    public enum TetrisColors
    {
        Red = 0,    //Colors.Red,
        Blue = 1,    //Colors.Blue,
        Green = 2,    //Colors.Green,
        Yellow = 3,    //Colors.Yellow,
        Cyan = 4,    //Colors.Cyan,
        Brown = 5,    //Colors.Brown
    }
}
