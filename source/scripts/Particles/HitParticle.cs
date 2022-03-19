using Godot;
using System;

// Cżąstki przy uderzeniu w coś
public class HitParticle : CPUParticles2D
{
    public bool DestroyAfter { get; set; } = false;

    void _on_Timer_timeout()
    {
        this.SetProcess(false);
        this.SetPhysicsProcess(false);
        this.SetProcessInput(false);
        this.SetProcessInternal(false);
        this.SetProcessUnhandledInput(false);
        this.SetProcessUnhandledKeyInput(false);

        if (DestroyAfter) //TODO particle pool?
            QueueFree();
    }
}
