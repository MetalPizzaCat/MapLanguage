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
}