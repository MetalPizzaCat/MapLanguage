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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MapLanguage;
using MapLanguage.Editor;

namespace Map;

public partial class MainWindow : Window
{
    /// <summary>
    /// List of all messages that were printed by the code
    /// </summary>
    public ObservableCollection<string> OutputMessages { get; private set; } = new();
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

    /// <summary>
    /// Current brush used for placing operation
    /// </summary>
    /// <value></value>
    public Operation OperationBrush
    {
        get => _operationBrush;
        set => SetAndRaise(OperationBrushProperty, ref _operationBrush, value);
    }

    public Vector2? ExecutorPosition
    {
        get => _executorPosition;
        set => SetAndRaise(ExecutorPositionProperty, ref _executorPosition, value);
    }

    public List<OperationSelectionButton> Options { get; } = new();

    public Dictionary<Operation, Bitmap> OperationImages { get; }
    private Operation _operationBrush = Operation.NoOperation;
    private Machine? _executionMachine = null;
    private Vector2? _executorPosition = null;

    public MainWindow()
    {
        InitializeComponent();

        // foreach (Operation operation in Enum.GetValues(typeof(Operation)))
        // {
        //     operations.Add(new OperationInfo(operation, $"/Assets/{operation}.png", null));
        // }
        // File.WriteAllText("./Assets/OperationInfo.json", JsonSerializer.Serialize(operations));
        OperationImages = new();
        using (StreamReader reader = new StreamReader(AssetLoader.Open(new Uri("avares://Map/Assets/OperationInfo.json"))))
        {
            List<OperationInfo>? operations = JsonSerializer.Deserialize<List<OperationInfo>>(reader.ReadToEnd());
            if (operations == null)
            {
                throw new NullReferenceException("Missing or malformed information about available operations");
            }
            foreach (OperationInfo info in operations)
            {
                Uri path = new Uri($"avares://Map{info.IconName}");
                if (!AssetLoader.Exists(path))
                {
                    continue;
                }
                OperationImages.Add(info.Operation, new Bitmap(AssetLoader.Open(path)));
                OperationSelectionButton button = new OperationSelectionButton()
                {
                    Description = info.Description ?? info.Operation.ToString(),
                    OperationImage = OperationImages[info.Operation],
                    Operation = info.Operation
                };
                button.PrimaryBrushOperationSelected += SelectNewOperationBrush;

                Options.Add(button);
            }
            DataContext = this;
        }
    }

    private void SelectNewOperationBrush(Operation op)
    {
        OperationBrush = op;
    }

    public void StepExecution()
    {
        if (_executionMachine == null)
        {
            StopExecution();
            _executionMachine = new Machine(EditorCanvas.Canvas, 32, Vector2.Zero);
        }

        Operation? op = _executionMachine.Execute();
        ExecutorPosition = _executionMachine.ProgramPoint;
        if (op == null)
        {
            // we are done
            Debug.WriteLine("Finished execution by running out of bounds");
            return;
        }
        if (op.Value == Operation.Print)
        {
            OutputMessages.Add(_executionMachine.Accumulator.ToString());
        }
        _executionMachine.MoveNext();
    }

    public void StopExecution()
    {
        _executionMachine = null;
        OutputMessages.Clear();
        ExecutorPosition = null;
    }
}