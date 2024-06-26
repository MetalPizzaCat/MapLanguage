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
using Avalonia.Media.Imaging;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Input;
using System;

namespace MapLanguage.Editor;

/// <summary>
/// Action that happened on the canvas
/// </summary>
/// <param name="newValue">Value that was written into the cell</param>
/// <param name="oldValue">Value that was present in the cell before</param>
/// <param name="cell">Location of the cell</param>
public record CanvasActionInfo(Operation NewValue, Operation OldValue, Vector2 Cell);

public class EditorCanvasControl : Control
{
    public static readonly DirectProperty<EditorCanvasControl, Dictionary<Operation, Bitmap>> OperationImagesProperty =
    AvaloniaProperty.RegisterDirect<EditorCanvasControl, Dictionary<Operation, Bitmap>>
    (
        nameof(OperationImages),
        o => o.OperationImages,
        (o, value) => o.OperationImages = value
    );

    public static readonly DirectProperty<EditorCanvasControl, bool> WasEditedProperty =
    AvaloniaProperty.RegisterDirect<EditorCanvasControl, bool>
    (
        nameof(WasEdited),
        o => o.WasEdited,
        (o, value) => o.WasEdited = value
    );

    public static readonly DirectProperty<EditorCanvasControl, Operation> OperationBrushProperty =
    AvaloniaProperty.RegisterDirect<EditorCanvasControl, Operation>
    (
        nameof(OperationBrush),
        o => o.OperationBrush,
        (o, value) => o.OperationBrush = value
    );

    public static readonly DirectProperty<EditorCanvasControl, Vector2?> ExecutorPositionProperty =
    AvaloniaProperty.RegisterDirect<EditorCanvasControl, Vector2?>
    (
        nameof(ExecutorPosition),
        o => o.ExecutorPosition,
        (o, value) => o.ExecutorPosition = value
    );

    public Dictionary<Operation, Bitmap> OperationImages
    {
        get => _operationImages;
        set => SetAndRaise(OperationImagesProperty, ref _operationImages, value);
    }

    public bool WasEdited
    {
        get => _wasEdited;
        set => SetAndRaise(WasEditedProperty, ref _wasEdited, value);
    }

    public Vector2? ExecutorPosition
    {
        get => _executorPosition;
        set => SetAndRaise(ExecutorPositionProperty, ref _executorPosition, value);
    }

    public Operation OperationBrush
    {
        get => _operationBrush;
        set => SetAndRaise(OperationBrushProperty, ref _operationBrush, value);
    }
    public int CellSize { get; private set; } = 64;

    /// <summary>
    /// Data about current canvas
    /// </summary>
    public ScriptCanvas Canvas
    {
        get => _canvas;
        private set
        {
            _canvas = value;
            MinHeight = _canvas.Height * 64;
            MinWidth = _canvas.Width * 64;
        }
    }

    private ScriptCanvas _canvas = new(6, 6);
    private Dictionary<Operation, Bitmap> _operationImages = new();
    private Operation _operationBrush = Operation.NoOperation;
    private Vector2? _executorPosition = null;
    private bool _wasEdited = false;

    public List<CanvasActionInfo> PlacedOperations { get; private set; } = new();
    private int _currentHistoryId = 0;

    public EditorCanvasControl()
    {
        ClipToBounds = true;
        MinHeight = _canvas.Height * 64;
        MinWidth = _canvas.Width * 64;

        PointerPressed += CellClicked;
    }

    public void CreateNewCanvas(int width, int height, int x = 0, int y = 0, byte[]? bytes = null)
    {
        Canvas = new ScriptCanvas(bytes ?? new byte[width * height], width, height);
        _currentHistoryId = 0;
        PlacedOperations = new();
    }

    public override void Render(DrawingContext context)
    {
        context.Custom(new ScriptDrawingOperation(new Rect(0, 0, Bounds.Width, Bounds.Height), _canvas, OperationImages, CellSize, _executorPosition));
        Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
    }

    public void Undo()
    {
        if (_currentHistoryId <= 0)
        {
            return;
        }
        _currentHistoryId--;
        _canvas.Operations[PlacedOperations[_currentHistoryId].Cell.X, PlacedOperations[_currentHistoryId].Cell.Y] = PlacedOperations[_currentHistoryId].OldValue;

    }

    public void Redo()
    {
        if (_currentHistoryId == PlacedOperations.Count)
        {
            return;
        }
        _canvas.Operations[PlacedOperations[_currentHistoryId].Cell.X, PlacedOperations[_currentHistoryId].Cell.Y] = PlacedOperations[_currentHistoryId].NewValue;
        _currentHistoryId++;
    }

    private void CellClicked(object? sender, PointerPressedEventArgs e)
    {
        Point clickPoint = e.GetPosition(this);
        int x = (int)clickPoint.X / CellSize;
        int y = (int)clickPoint.Y / CellSize;

        if (!_canvas.IsValidPoint(new Vector2(x, y)))
        {
            return;
        }
        Operation op = e.GetCurrentPoint(null).Properties.IsLeftButtonPressed ? OperationBrush : Operation.NoOperation;
        // override history with new items
        if (PlacedOperations.Count > _currentHistoryId)
        {
            PlacedOperations.RemoveRange(_currentHistoryId, PlacedOperations.Count - _currentHistoryId);
        }
        PlacedOperations.Add(new CanvasActionInfo(op, _canvas.Operations[x, y], new Vector2(x, y)));
        _canvas.Operations[x, y] = op;
        _currentHistoryId++;
        WasEdited = true;
    }
}