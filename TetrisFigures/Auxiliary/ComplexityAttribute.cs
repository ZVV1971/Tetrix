using System;

namespace TetrisFigures.Auxiliary
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ComplexityAttribute: Attribute
    {
        public GameComplexity Complexity { get; set; }
    }
}