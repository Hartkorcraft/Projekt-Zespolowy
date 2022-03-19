using Godot;
using System;

// Cżąstki przy uderzeniu w coś
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
