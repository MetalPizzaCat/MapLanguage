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
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Rendering.SceneGraph;

namespace MapLanguage.Editor;
public sealed class ScriptDrawingOperation : ICustomDrawOperation
{
    public Rect Bounds { get; }

    public ScriptCanvas Canvas { get; }
    public Dictionary<Operation, Bitmap> OperationImages { get; }
    public int CellSize { get; }
    public Rect FieldRect { get; }

    public ScriptDrawingOperation(Rect bounds, ScriptCanvas canvas, Dictionary<Operation, Bitmap> operationImages, int cellSize = 64)
    {
        Canvas = canvas;
        OperationImages = operationImages;
        CellSize = cellSize;
        FieldRect = new Rect(0, 0, cellSize * canvas.Width, cellSize * canvas.Height);
        Bounds = bounds;
    }

    public void Dispose()
    {
        // nothing to dispose of 
    }

    public bool Equals(ICustomDrawOperation? other) => false;

    public bool HitTest(Point p)
    {
        if (!Bounds.Contains(p) || !FieldRect.Contains(p))
        {
            return false;
        }
        return true;
    }

    public void Render(ImmediateDrawingContext context)
    {
        for (int x = 0; x < Canvas.Width; x++)
        {
            for (int y = 0; y < Canvas.Height; y++)
            {
                Bitmap? bitmap;
                OperationImages.TryGetValue(Canvas.Operations[x, y], out bitmap);
                context.DrawBitmap(bitmap ?? OperationImages.Values.First(), new Rect(x * CellSize, y * CellSize, CellSize, CellSize));
            }
        }
    }
}