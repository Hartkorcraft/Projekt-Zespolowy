using Godot;
using System;

public abstract class HitParticle : CPUParticles2D
{
    void _on_Timer_timeout()
    {
        this.SetProcess(false);
        this.SetPhysicsProcess(false);
        this.SetProcessInput(false);
        this.SetProcessInternal(false);
        this.SetProcessUnhandledInput(false);
        this.SetProcessUnhandledKeyInput(false);
    }
}
