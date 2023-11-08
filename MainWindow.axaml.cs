using System;
using System.Collections.Generic;
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
    public List<OperationSelectionButton> Options { get; } = new();

    private Bitmap _icons;
    public MainWindow()
    {
        InitializeComponent();

        // foreach (Operation operation in Enum.GetValues(typeof(Operation)))
        // {
        //     operations.Add(new OperationInfo(operation, $"/Assets/{operation}.png", null));
        // }
        // File.WriteAllText("./Assets/OperationInfo.json", JsonSerializer.Serialize(operations));

        // Options.Add(new Button()
        // {
        //     Content = new Image()
        //     {
        //         Source = new Bitmap(AssetLoader.Open(new Uri("avares://Map/Assets/MoveRight.png"))),
        //         Width = 64,
        //         Height = 64
        //     }
        // });
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
                OperationSelectionButton button = new OperationSelectionButton()
                {
                    Description = info?.IconName ?? string.Empty,
                    OperationImage = new Bitmap(AssetLoader.Open(path))
                };
                Options.Add(button);
            }
            DataContext = this;
        }
    }
}