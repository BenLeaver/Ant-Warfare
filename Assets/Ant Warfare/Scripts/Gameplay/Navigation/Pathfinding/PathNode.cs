
namespace Ant.Navigation.Pathfinding
{
    public class PathNode
    {
        public NavigationCell Cell;

        public float GCost;
        public float HCost;

        public float FCost => GCost + HCost;

        public PathNode Parent;

        public PathNode(NavigationCell cell)
        {
            Cell = cell;
        }
    }

}

