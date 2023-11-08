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