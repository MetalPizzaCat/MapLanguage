/*
A graphical esolang that looks like a map
Copyright (C) 2023  Sofia "MetalPizzaCat"

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
namespace MapLanguage.Editor;

/// <summary>
/// Class that stores information about current script canvas field
/// </summary>
public class ScriptCanvas
{
    public ScriptCanvas(int width, int height)
    {
        Width = width;
        Height = height;

        Operations = new Operation[Width, Height];
    }

    /// <summary>
    /// All of the operations on the canvas
    /// </summary>
    /// <value></value>
    public Operation[,] Operations { get; private set; }

    /// <summary>
    /// Current canvas width
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Current canvas height
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Is given point inside the canvas field
    /// </summary>
    /// <param name="point">Point to check</param>
    /// <returns>True if given point does have a valid cell underneath</returns>
    public bool IsValidPoint(Vector2 point) => point.X >= 0 && point.Y >= 0 && point.X < Width && point.Y < Height;

    public Operation this[Vector2 point] => Operations[point.X, point.Y];
}