using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MapLanguage.Editor;

public partial class FileCreationDialogWindow : Window
{

    public bool CreationAccepted { get; private set; } = false;
    public int FieldWidth { get; private set; } = 2;
    public int FieldHeight { get; private set; } = 2;
    public FileCreationDialogWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    public void Accept()
    {
        CreationAccepted = true;
        Close();
    }

    public void Cancel()
    {
        Close();
    }
}