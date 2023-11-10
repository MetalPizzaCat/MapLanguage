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
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MapLanguage;
using MapLanguage.Editor;

namespace Map;

public partial class MainWindow : Window
{
    /// <summary>
    /// List of all messages that were printed by the code
    /// </summary>
    public ObservableCollection<string> OutputMessages { get; private set; } = new();

    public ObservableCollection<MemoryCellControl> MemoryCells { get; private set; } = new();

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

    public static readonly DirectProperty<EditorCanvasControl, bool> WasEditedProperty =
    AvaloniaProperty.RegisterDirect<EditorCanvasControl, bool>
    (
        nameof(WasEdited),
        o => o.WasEdited,
        (o, value) => o.WasEdited = value
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
    public Uri? CurrentFilePath
    {
        get => _currentFilePath;
        set
        {
            _currentFilePath = value;
            Title = $"MapLang editor {_currentFilePath?.ToString() ?? string.Empty} {(WasEdited ? '*' : ' ')}";
        }
    }

    public bool WasEdited
    {
        get => _wasEdited;
        set
        {
            SetAndRaise(WasEditedProperty, ref _wasEdited, value);
            Title = $"MapLang editor {_currentFilePath?.ToString() ?? string.Empty} {(WasEdited ? '*' : ' ')}";
        }
    }

    private Operation _operationBrush = Operation.NoOperation;
    private Machine? _executionMachine = null;
    private Vector2? _executorPosition = null;
    private bool _shouldRun = false;
    private Uri? _currentFilePath = null;
    private bool _wasEdited = false;

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

    public bool TryStepExecution()
    {
        if (_executionMachine == null)
        {
            StopExecution();
            _executionMachine = new Machine(EditorCanvas.Canvas, 32, Vector2.Zero);
            MemoryCells.Clear();
            for (int i = 0; i < _executionMachine.Stack.Length; i++)
            {
                MemoryCellControl cell = new MemoryCellControl()
                {
                    MemoryValue = _executionMachine.Stack[0],
                    CellName = i.ToString(),
                    MemoryCellId = 0
                };
                MemoryCells.Add(cell);
            }
        }

        Operation? op = _executionMachine.Execute();
        ExecutorPosition = _executionMachine.ProgramPoint;
        if (op == null)
        {
            // we are done
            Debug.WriteLine("Finished execution by running out of bounds");
            return false;
        }
        if (op.Value == Operation.Print)
        {
            OutputMessages.Add(_executionMachine.Accumulator.ToString());
        }
        _executionMachine.MoveNext();
        return true;
    }

    public void StepExecution()
    {
        TryStepExecution();
    }

    public void StopExecution()
    {
        _executionMachine = null;
        OutputMessages.Clear();
        ExecutorPosition = null;
        _shouldRun = false;
    }

    public async Task RunExecutionTask()
    {
        while (TryStepExecution() && _shouldRun)
        {
            await Task.Delay(500);
        }
        StopExecution();
    }

    public void RunExecution()
    {
        _shouldRun = true;
        Dispatcher.UIThread.Post(() => RunExecutionTask(), DispatcherPriority.Background);
    }

    public async void SaveCanvasToFile(IStorageFile file)
    {
        await using Stream? stream = await file.OpenWriteAsync();
        using BinaryWriter streamWriter = new BinaryWriter(stream);
        streamWriter.Write(EditorCanvas.Canvas.GenerateFile().ToArray());
        CurrentFilePath = file.Path;
        WasEdited = false;
    }
    public async void SaveFile()
    {
        // redirect to save file as if file is already recorded
        if (CurrentFilePath == null)
        {
            SaveFileAs();
            return;
        }
        IStorageFile? file = await StorageProvider.TryGetFileFromPathAsync(CurrentFilePath);
        if (file != null)
        {
            SaveCanvasToFile(file);
        }
    }

    public async void SaveFileAs()
    {
        IStorageFile? file = await StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions()
        {
            Title = "Save file",
            DefaultExtension = "mlb"
        });
        if (file != null)
        {
            SaveCanvasToFile(file);
        }
    }

    public async void LoadFile()
    {
        IReadOnlyList<IStorageFile>? files = await StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions()
        {
            Title = "Open file",
            AllowMultiple = false
        });
        if (files.Count > 0)
        {

            await using Stream? stream = await files[0].OpenReadAsync();
            using BinaryReader? streamReader = new BinaryReader(stream);
            int width = streamReader.ReadInt32();
            int height = streamReader.ReadInt32();
            int x = streamReader.ReadInt32();
            int y = streamReader.ReadInt32();
            byte[] canvasBuffer = streamReader.ReadBytes(width * height);
            EditorCanvas.CreateNewCanvas(width, height, x, y, canvasBuffer);
            CurrentFilePath = files[0].Path;
            WasEdited = false;
        }
    }
}