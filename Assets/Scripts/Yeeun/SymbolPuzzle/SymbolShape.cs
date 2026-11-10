namespace Yeeun.SymbolPuzzle
{
    /// <summary>
    /// Shapes used by the outline symbol puzzles (wall reliefs, plates, pieces, sockets, pillars).
    /// Every shape is a simple silhouette so it reads clearly as an outline.
    /// Append new values at the end only: existing scenes store these as numbers.
    /// </summary>
    public enum SymbolShape
    {
        Circle = 0,
        Triangle = 1,
        Square = 2,
        Diamond = 3,
        Star = 4,
        Cross = 5,
        Hexagon = 6,
        XMark = 7,
    }

    public static class SymbolShapeExtensions
    {
        /// <summary>Korean display name, for interaction prompts and debug text.</summary>
        public static string DisplayName(this SymbolShape shape)
        {
            switch (shape)
            {
                case SymbolShape.Circle: return "원";
                case SymbolShape.Triangle: return "삼각형";
                case SymbolShape.Square: return "사각형";
                case SymbolShape.Diamond: return "마름모";
                case SymbolShape.Star: return "별";
                case SymbolShape.Cross: return "십자";
                case SymbolShape.Hexagon: return "육각형";
                case SymbolShape.XMark: return "X";
                default: return shape.ToString();
            }
        }
    }
}
