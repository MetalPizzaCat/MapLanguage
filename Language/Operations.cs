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
namespace MapLanguage;

/// <summary>
/// Enumeration of all possible actions that machine can perform
/// </summary>
public enum Operation
{
    /// <summary>
    /// Do nothing
    /// </summary>
    NoOperation,
    MoveLeft,
    MoveRight,
    MoveUp,
    MoveDown,
    Add,
    Sub,
    Mul,
    Div,
    Increment,
    Decrement,
    IsLess,
    IsMore,
    MoveLeftIfTrue,
    MoveRightIfTrue,
    MoveUpIfTrue,
    MoveDownIfTrue,
    IsLessOrEqual,
    IsMoreOrEqual,
    IsEqual,
    IsNotEqual,
    MoveStackDown,
    MoveStackUp,
    WriteFromAccumulator,
    ReadToAccumulator,
    /// <summary>
    /// Prints value in the accumulator to the output
    /// </summary>
    Print,
    /// <summary>
    /// Ends the execution
    /// </summary>
    Exit,
    IsZero,
}