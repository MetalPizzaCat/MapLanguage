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
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Enums;

namespace Map;

public partial class MainWindow : Window
{
    /// <summary>
    /// List of all messages that were printed by the code
    /// </summary>
    public ObservableCollection<string> OutputMessages { get; private set; } = new();

    public ObservableCollection<MemoryCellControl> MemoryCells { get; private set; } = new();

    public static readonly DirectProperty<MainWindow, Operation> OperationBrushProperty =
    AvaloniaProperty.RegisterDirect<MainWindow, Operation>
    (
        nameof(OperationBrush),
        o => o.OperationBrush,
        (o, value) => o.OperationBrush = value
    );

    public static readonly DirectProperty<MainWindow, int> AccumulatorValueProperty =
    AvaloniaProperty.RegisterDirect<MainWindow, int>
    (
        nameof(AccumulatorValue),
        o => o.AccumulatorValue,
        (o, value) => o.AccumulatorValue = value
    );

    public static readonly DirectProperty<MainWindow, Vector2?> ExecutorPositionProperty =
    AvaloniaProperty.RegisterDirect<MainWindow, Vector2?>
    (
        nameof(ExecutorPosition),
        o => o.ExecutorPosition,
        (o, value) => o.ExecutorPosition = value
    );

    public static readonly DirectProperty<MainWindow, bool> WasEditedProperty =
    AvaloniaProperty.RegisterDirect<MainWindow, bool>
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

    public int AccumulatorValue
    {
        get => _accumulatorValue;
        set => SetAndRaise(AccumulatorValueProperty, ref _accumulatorValue, value);
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

    /// <summary>
    /// How much memory each program has 
    /// </summary>
    public static readonly int MemorySize = 64;

    private Operation _operationBrush = Operation.NoOperation;
    private Machine? _executionMachine = null;
    private Vector2? _executorPosition = null;
    private bool _shouldRun = false;
    private Uri? _currentFilePath = null;
    private int _accumulatorValue = 0;

    private bool _wasEdited = false;

    /// <summary>
    /// Used as a temporary memory storage between program runs
    /// </summary>
    private int[] _memoryStorage;

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
                InitMemory();
                CreateMemoryControls();
                Options.Add(button);
            }

            DataContext = this;
        }
    }

    private void InitMemory()
    {
        _memoryStorage = new int[MemorySize];
    }

    private void CreateMemoryControls()
    {
        MemoryCells.Clear();
        for (int i = 0; i < MemorySize; i++)
        {
            MemoryCellControl cell = new MemoryCellControl()
            {
                MemoryValue = _memoryStorage[0],
                CellName = i.ToString(),
                MemoryCellId = i
            };
            cell.MemoryCellValueChanged += MemoryCellValueChanged;
            MemoryCells.Add(cell);
        }
    }

    private void SelectNewOperationBrush(Operation op)
    {
        OperationBrush = op;
    }

    /// <summary>
    /// Create a new instance of the execution machine
    /// </summary>
    private void StartExecution()
    {
        _executionMachine = new Machine(EditorCanvas.Canvas, MemorySize, Vector2.Zero, _memoryStorage);
        CreateMemoryControls();
    }

    /// <summary>
    /// Attempt to perform execution and stop if execution has finished<para/>
    /// If execution machine has not yet been initialized it will will be created
    /// </summary>
    /// <returns>True if execution was not finished</returns>
    public bool TryStepExecution()
    {
        if (_executionMachine == null)
        {
            return false;
        }

        Operation? op = _executionMachine.Execute();
        if (op == null)
        {
            // we are done
            Debug.WriteLine("Finished execution by running out of bounds");
            return false;
        }
        if (op.Value == Operation.Exit)
        {
            Debug.WriteLine("Finished execution by exiting");
            return false;
        }

        Dispatcher.UIThread.Post(() =>
        {
            ExecutorPosition = _executionMachine.ProgramPoint;

            if (op.Value == Operation.Print)
            {
                OutputMessages.Add(_executionMachine.Accumulator.ToString());
            }
            if (op.Value == Operation.WriteFromAccumulator || op.Value == Operation.ReadToAccumulator)
            {
                // TODO: Find a better way to bind array values to memory cells
                MemoryCells[_executionMachine.CurrentStackPointer].MemoryValue = _executionMachine.Stack[_executionMachine.CurrentStackPointer];
            }
            AccumulatorValue = _executionMachine.Accumulator;
        });
        _executionMachine.MoveNext();
        return true;
    }

    /// <summary>
    /// Update stored memory values when user changes them via UI
    /// </summary>
    /// <param name="sender">Object where it came from</param>
    /// <param name="id">Id of the changed cell</param>
    /// <param name="val">New value for the cell</param>
    private void MemoryCellValueChanged(object? sender, int id, int val)
    {
        if (_executionMachine != null)
        {
            _executionMachine.Stack[id] = val;
        }
        _memoryStorage[id] = val;
    }
    public void StepExecution()
    {
        if (_executionMachine == null)
        {
            StopExecution();
            StartExecution();
        }
        TryStepExecution();
    }

    /// <summary>
    /// Stop execution and write down updated memory
    /// </summary>
    public void StopExecution()
    {
        _executionMachine = null;
        OutputMessages.Clear();
        ExecutorPosition = null;
        _shouldRun = false;
        int id = 0;
        foreach (MemoryCellControl cell in MemoryCells)
        {
            _memoryStorage[id++] = cell.MemoryValue;
        }
    }

    /// <summary>
    /// Run the execution loop with delays
    /// </summary>
    public async void RunExecutionTask()
    {
        while (TryStepExecution() && _shouldRun)
        {
            await Task.Delay(500);
        }
        Dispatcher.UIThread.Post(StopExecution);
    }

    public void RunExecution()
    {

        // in theory run should always run from a clean machine
        // but this could allow us to continue running a stepped code
        // created here instead of relying on step function to avoid having to reset the machine in a separate thread
        if (_executionMachine == null)
        {
            StopExecution();
            StartExecution();
        }
        _shouldRun = true;
        Task.Run(() => RunExecutionTask());
    }

    /// <summary>
    /// Open the dialog for new file creation
    /// </summary>
    public async void NewFile()
    {

        IMsBox<ButtonResult>? question = MessageBoxManager.GetMessageBoxStandard
        (
            "Save existing project?",
            "There is already a project opened, do you wish to save it?",
            MsBox.Avalonia.Enums.ButtonEnum.YesNoCancel
        );
        ButtonResult result = await question.ShowAsync();

        switch (result)
        {
            case ButtonResult.Yes:
                SaveFile();
                // save but we are changing file
                CurrentFilePath = null;
                WasEdited = false;
                break;
            case ButtonResult.Cancel:
                return;
            default:
                break;
        }
        StopExecution();
        FileCreationDialogWindow dialog = new FileCreationDialogWindow();
        await dialog.ShowDialog(this);
        if (!dialog.CreationAccepted)
        {
            // user quit -> we pretend nothing happened
            return;
        }

        EditorCanvas.CreateNewCanvas(dialog.FieldWidth, dialog.FieldHeight, 0, 0);
    }

    /// <summary>
    /// Try to write current code canvas into a file
    /// </summary>
    /// <param name="file">Destination</param>
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

    public void UndoLastAction()
    {
        EditorCanvas.Undo();
    }

    public void RedoLastAction()
    {
        EditorCanvas.Redo();
    }

    public async void QuitEditor()
    {
        IMsBox<ButtonResult>? question = MessageBoxManager.GetMessageBoxStandard
        (
            "Save existing project?",
            "There is already a project opened, do you wish to save it?",
            MsBox.Avalonia.Enums.ButtonEnum.YesNoCancel
        );
        ButtonResult result = await question.ShowAsync();

        switch (result)
        {
            case ButtonResult.Yes:
                SaveFile();
                // save but we are changing file
                CurrentFilePath = null;
                WasEdited = false;
                
                break;
            case ButtonResult.Cancel:
                return;
            default:
                break;
        }
        Close();
    }

    public void ClearMemory()
    {
        if (_shouldRun || _executionMachine != null)
        {
            return;
        }
        InitMemory();
        CreateMemoryControls();
    }
}