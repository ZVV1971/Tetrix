using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TetrisFigures.Interfaces
{
    public class ElementaryCell
    {
        public Rectangle rect;
        public bool IsFrozen;
        public bool NeedsFreeze;

        public void Freeze()
        {
            if (NeedsFreeze)
            {
                IsFrozen = true;
                NeedsFreeze = false;
            }
        }

        public void Reset()
        {
            IsFrozen = false;
            NeedsFreeze = false;
            rect.Visibility = Visibility.Hidden;
        }
    }
}
