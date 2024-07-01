namespace Maths;

public enum Direction {
    Up,
    Down,
    North,
    East,
    South,
    West
}

public static class DirectionExtensions {
    public static void Deconstruct(this Direction direction, out int x, out int y, out int z) {
        x = direction.GetXComponent();
        y = direction.GetYComponent();
        z = direction.GetZComponent();
    }
    public static int GetXComponent(this Direction direction) {
        return direction switch {
            Direction.East => -1,
            Direction.West => 1,
            _ => 0
        };
    }

    public static Direction Opposate(this Direction direction) {
        return direction switch {
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.North => Direction.South,
            Direction.South => Direction.North,
            Direction.East => Direction.West,
            Direction.West => Direction.East,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }

    public static int GetYComponent(this Direction direction) {
        return direction switch {
            Direction.Up => -1,
            Direction.Down => 1,
            _ => 0
        };
    }

    public static int GetZComponent(this Direction direction) {
        return direction switch {
            Direction.North => 1,
            Direction.South => -1,
            _ => 0
        };
    }
    public static Direction GetFacing(float x, float y, float z) {
        Direction direction = Direction.North;
        float f = float.MaxValue;

        foreach(Direction curDir in Enum.GetValues<Direction>()) {
            float g = x * curDir.GetXComponent() + y * curDir.GetYComponent() + z * curDir.GetZComponent();
            if (!(g > f)) continue;
            f = g;
            direction = curDir;
        }

        return direction;
    }
}