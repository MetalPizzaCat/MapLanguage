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

/// <summary>
/// Class that executes code
/// </summary>
public class Machine
{
    /// <summary>
    /// Which direction will the execution goblin go in
    /// </summary>
    protected enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    /// <summary>
    /// Current code loaded into memory
    /// </summary>
    public ScriptCanvas Data { get; }

    /// <summary>
    /// Current location of the execution goblin 
    /// </summary>
    public Vector2 ProgramPoint { get; private set; }
    private Direction _currentDirection = Direction.Down;
    private bool _isConditionFlagChecked = false;

    /// <summary>
    /// Current memory stack
    /// </summary>
    public int[] Stack { get; private set; }

    /// <summary>
    /// Value in the accumulator
    /// </summary>
    public int Accumulator { get; set; } = 0;

    /// <summary>
    /// Id of the current memory cell 
    /// </summary>
    public int CurrentStackPointer { get; set; } = 0;

    /// <summary>
    /// Result of the last check
    /// </summary>
    public bool IsConditionFlagChecked
    {
        get => _isConditionFlagChecked;
        set => _isConditionFlagChecked = value;
    }

    public Machine(ScriptCanvas data, int stackSize, Vector2 start)
    {
        Stack = new int[stackSize];
        Data = data;
        ProgramPoint = start;
    }

    /// <summary>
    /// Move execution goblin to the next cell in current direction
    /// </summary>
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
                Accumulator += Stack[CurrentStackPointer];
                break;
            case Operation.Sub:
                Accumulator -= Stack[CurrentStackPointer];
                break;
            case Operation.Mul:
                Accumulator *= Stack[CurrentStackPointer];
                break;
            case Operation.Div:
                Accumulator /= Stack[CurrentStackPointer];
                break;
            case Operation.Increment:
                Accumulator++;
                break;
            case Operation.Decrement:
                Accumulator--;
                break;
            case Operation.IsLess:
                IsConditionFlagChecked = Accumulator < Stack[CurrentStackPointer];
                break;
            case Operation.IsMore:
                IsConditionFlagChecked = Accumulator > Stack[CurrentStackPointer];
                break;
            case Operation.MoveLeftIfTrue:
                if (IsConditionFlagChecked)
                {
                    _currentDirection = Direction.Left;
                }
                break;
            case Operation.MoveRightIfTrue:
                if (IsConditionFlagChecked)
                {
                    _currentDirection = Direction.Right;
                }
                break;
            case Operation.MoveUpIfTrue:
                if (IsConditionFlagChecked)
                {
                    _currentDirection = Direction.Up;
                }
                break;
            case Operation.MoveDownIfTrue:
                if (IsConditionFlagChecked)
                {
                    _currentDirection = Direction.Down;
                }
                break;
            case Operation.IsLessOrEqual:
                IsConditionFlagChecked = Accumulator <= Stack[CurrentStackPointer];
                break;
            case Operation.IsMoreOrEqual:
                IsConditionFlagChecked = Accumulator >= Stack[CurrentStackPointer];
                break;
            case Operation.IsEqual:
                IsConditionFlagChecked = Accumulator == Stack[CurrentStackPointer];
                break;
            case Operation.IsNotEqual:
                IsConditionFlagChecked = Accumulator != Stack[CurrentStackPointer];
                break;
            case Operation.MoveStackDown:
                CurrentStackPointer++;
                break;
            case Operation.MoveStackUp:
                CurrentStackPointer--;
                break;
            case Operation.WriteFromAccumulator:
                Stack[CurrentStackPointer] = Accumulator;
                break;
            case Operation.ReadToAccumulator:
                Accumulator = Stack[CurrentStackPointer];
                break;
            case Operation.NoOperation:
                // no operation :3
                break;
            case Operation.Print:
                // handled by the class that uses this object
                break;
            case Operation.Exit:
                // TODO: End execution here
                break;
            case Operation.IsZero:
                IsConditionFlagChecked = Accumulator == 0;
                break;
            default:
                break;
        }
        return Data[ProgramPoint];
    }
}