using System;
using Avalonia.Media.Imaging;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace MapLanguage.Editor;

public partial class OperationSelectionButton : UserControl
{
    public static readonly DirectProperty<OperationSelectionButton, string> DescriptionProperty =
     AvaloniaProperty.RegisterDirect<OperationSelectionButton, string>(nameof(Description), o => o.Description, (o, value) => o.Description = value);


    public static readonly DirectProperty<OperationSelectionButton, IImage> OperationImageProperty =
     AvaloniaProperty.RegisterDirect<OperationSelectionButton, IImage>(nameof(OperationImage), o => o.OperationImage, (o, value) => o.OperationImage = value);


    public string Description
    {
        get => _description;
        set => SetAndRaise(DescriptionProperty, ref _description, value);
    }

    public IImage OperationImage
    {
        get => _operationImage;
        set => SetAndRaise(OperationImageProperty, ref _operationImage, value);
    }
    public OperationSelectionButton()
    {
        InitializeComponent();
        DataContext = this;
    }

    private string _description = string.Empty;
    private IImage _operationImage;
}