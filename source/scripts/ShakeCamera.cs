using Godot;
using System;

public class ShakeCamera : Camera2D
{
    [Export] float decay = 0.8f;
    [Export] Vector2 maxOffset = new Vector2(100, 70);
    [Export] float MaxRotation = 0.01f;
    [Export] float maxShake = 1f;

    float shakeAmount = 0f;
    float shakePower = 2; // exponent between [2 , 3]
    Node2D target = null!;

    public override void _EnterTree()
    {
        target = GetParent<Node2D>();
    }

    public override void _Process(float delta)
    {
        if (target is not null)
            this.GlobalPosition = target.GlobalPosition;
        if (shakeAmount > 0)
        {
            shakeAmount = Mathf.Max(shakeAmount - decay * delta, 0);
            Shake();
        }
    }

    public void AddShake(float ammount, float maxShake = 1.0f)
        => shakeAmount = Math.Min(shakeAmount + ammount, 1.0f);

    void Shake()
    {
        var ammount = Mathf.Pow(shakeAmount, shakePower);
        this.Rotation = MaxRotation * ammount * Utils.RandomFloat(-1, 1);
        this.Offset = new Vector2(Utils.RandomFloat(-1, 1), Utils.RandomFloat(-1, 1)) * maxOffset * ammount;
    }



}
