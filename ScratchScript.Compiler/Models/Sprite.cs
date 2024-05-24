namespace ScratchScript.Compiler.Models;

public record Sprite : Target
{
    public float Direction = 90;
    public bool Draggable;
    public string RotationStyle = "all around";
    public float Size = 100;
    public bool Visible = true;
    public float X;
    public float Y;
}