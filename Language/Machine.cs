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
using MapLanguage.Editor;

namespace MapLanguage;

public class Machine
{
    protected enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public ScriptCanvas Data { get; }
    public Vector2 ProgramPoint { get; private set; }
    private Direction _currentDirection = Direction.Down;

    public int[] Stack { get; private set; }

    public int Accumulator { get; set; } = 0;

    public int CurrentStackPointer { get; set; } = 0;

    public Machine(ScriptCanvas data, int stackSize, Vector2 start)
    {
        Stack = new int[stackSize];
        Data = data;
        ProgramPoint = start;
    }

    public void MoveNext()
    {
        switch (_currentDirection)
        {
            case Direction.Up:
                ProgramPoint += new Vector2(0, -1);
                break;
            case Direction.Down:
                ProgramPoint += new Vector2(0, 1);
                break;
            case Direction.Left:
                ProgramPoint += new Vector2(-1, 0);
                break;
            case Direction.Right:
                ProgramPoint += new Vector2(1, 0);
                break;
        }
    }
    /// <summary>
    /// Move to the next cell and execute action inside
    /// </summary>
    /// <returns>Action that was executed or null if out of bounds</returns>
    public Operation? Execute()
    {
        if (!Data.IsValidPoint(ProgramPoint))
        {
            return null;
        }
        switch (Data[ProgramPoint])
        {
            case Operation.MoveLeft:
                _currentDirection = Direction.Left;
                break;
            case Operation.MoveRight:
                _currentDirection = Direction.Right;
                break;
            case Operation.MoveUp:
                _currentDirection = Direction.Up;
                break;
            case Operation.MoveDown:
                _currentDirection = Direction.Down;
                break;
            case Operation.Add:
                break;
            case Operation.Sub:
                break;
            case Operation.Mul:
                break;
            case Operation.Div:
                break;
            case Operation.Increment:
                Accumulator++;
                break;
            case Operation.Decrement:
                Accumulator--;
                break;
            case Operation.IsLess:
                break;
            case Operation.IsMore:
                break;
            case Operation.MoveLeftIfTrue:
                break;
            case Operation.MoveRightIfTrue:
                break;
            case Operation.MoveUpIfTrue:
                break;
            case Operation.MoveDownIfTrue:
                break;
            case Operation.IsLessOrEqual:
                break;
            case Operation.IsMoreOrEqual:
                break;
            case Operation.IsEqual:
                break;
            case Operation.IsNotEqual:
                break;
            case Operation.MoveStackDown:
                break;
            case Operation.MoveStackUp:
                break;
            case Operation.WriteFromAccumulator:
                break;
            case Operation.ReadToAccumulator:
                break;
            default:
                break;
        }
        return Data[ProgramPoint];
    }
}