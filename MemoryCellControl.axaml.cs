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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MapLanguage.Editor;

public partial class MemoryCellControl : UserControl
{
    public delegate void MemoryCellValueChangedEventHandler(object? sender, int id, int value);
    public event MemoryCellValueChangedEventHandler? MemoryCellValueChanged;

    public static readonly DirectProperty<MemoryCellControl, string> CellNameProperty =
    AvaloniaProperty.RegisterDirect<MemoryCellControl, string>
    (
        nameof(CellName),
        o => o.CellName,
        (o, value) => o.CellName = value
    );

    public static readonly DirectProperty<MemoryCellControl, int> MemoryValueProperty =
    AvaloniaProperty.RegisterDirect<MemoryCellControl, int>
    (
        nameof(MemoryValue),
        o => o.MemoryValue,
        (o, value) => o.MemoryValue = value
    );

    public string CellName
    {
        get => _cellName;
        set => SetAndRaise(CellNameProperty, ref _cellName, value);
    }

    public int MemoryValue
    {
        get => _memoryValue;
        set
        {
            SetAndRaise(MemoryValueProperty, ref _memoryValue, value);
            MemoryCellValueChanged?.Invoke(this, MemoryCellId, value);
        }
    }

    public int MemoryCellId { get; set; } = 0;


    private string _cellName = "BAD";
    private int _memoryValue = 0;

    public MemoryCellControl()
    {
        InitializeComponent();
        DataContext = this;
    }
}