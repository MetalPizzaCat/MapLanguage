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

namespace MapLanguage;

public class Machine
{
    public Operation[,] Operations { get; private set; }
    public Vector2 ProgramPoint { get; private set; }


    public int Width { get; }
    public int Height { get; }

    public int[] Stack { get; private set; }

    public int Accumulator { get; set; } = 0;

    public Machine(int width, int height, IEnumerable<Operation> operations, int stackSize, Vector2 start)
    {
        Width = width;
        Height = height;
        Stack = new int[stackSize];
        ProgramPoint = start;
        Operations = new Operation[width, height];
        int counter = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Operations[x, y] = operations.ElementAt(counter);
                counter++;
            }
        }
    }
}