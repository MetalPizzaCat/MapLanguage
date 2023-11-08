
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