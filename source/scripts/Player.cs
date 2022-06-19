using Godot;
using System;

public class Player : Entity, IMovement
{
    public ShakeCamera ShakeCamera { get; private set; } = null!;

    [Export] float bulletTimeSpeed = 0.2f;
    [Export] float bulletTimeDecay = 0.2f;

    Arm arm = null!;
    MusicPlayer curMusicPlayer = null!;
    MusicPlayer musicPlayer = null!;
    MusicPlayer deathMusicPlayer = null!;

    public override void _EnterTree()
    {
        base._EnterTree();
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer") ?? throw new Exception("Animation player is null");
        arm = GetNode<Arm>("Arm");
        ShakeCamera = GetNode<ShakeCamera>("Camera2D");
        curMusicPlayer = musicPlayer = this.GetScene<MusicPlayer>(ScenesPaths.MUSIC_PLAYER);
        RLMapGenerator.OnMapGenerationFinished += () => curMusicPlayer.Play();

        deathMusicPlayer = this.GetScene<MusicPlayer>(ScenesPaths.DEATH_MUSIC_PLAYER);
    }

    public override void _Ready()
    {
        base._Ready();
        var bloodParticleScene = (PackedScene)ResourceLoader.Load(Imports.BLOOD_PARTICLE_DEATH_PATH);
        var healthBar = this.GetScene<HealthBar>(ScenesPaths.HEALTH_BAR);

        HealthSystem = new HealtSystem_Player(100, 100, () => GlobalPosition, this, healthBar, bloodParticleScene);

        healthBar.Visible = true;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (HealthSystem.Health <= 0)
        {
            if (deathMusicPlayer.Playing is false)
            {
                curMusicPlayer.Stop();
                curMusicPlayer = deathMusicPlayer;
                deathMusicPlayer.Play();
            }
            Movement.Update(this, Vector2.Zero, delta);
            BulletTime(false);
            return;
        }

        Movement.Update(this, GetMovementAxis(), delta);

        if (animationPlayer is not null)
            Movement.AnimateWalking(animationPlayer, this);

        var usedItem = TryToUseItemInHand(delta);
        BulletTime(usedItem);
    }

    private void BulletTime(bool usedItem = false)
    {
        if (Movement.Motion == Vector2.Zero && usedItem is false) Engine.TimeScale = bulletTimeSpeed;
        else Engine.TimeScale = Mathf.Min(Engine.TimeScale + bulletTimeDecay, 1.0f);
        curMusicPlayer.PitchScale = Mathf.Max(Engine.TimeScale, 0.7f);
    }

    protected Vector2 GetMovementAxis()
    {
        var axis = Vector2.Zero;
        axis.x = (Input.IsActionPressed(InputActions.MOVE_RIGHT_INPUT) ? 1 : 0) - (Input.IsActionPressed(InputActions.MOVE_LEFT_INPUT) ? 1 : 0);
        axis.y = (Input.IsActionPressed(InputActions.MOVE_DOWN_INPUT) ? 1 : 0) - (Input.IsActionPressed(InputActions.MOVE_UP_INPUT) ? 1 : 0);
        if (axis == Vector2.Zero) return axis;
        return axis.Normalized();
    }

    bool TryToUseItemInHand(float delta)
    {
        var tryingToShoot = Input.IsActionPressed(InputActions.SHOOT_INPUT);
        if (tryingToShoot)
            return arm.TryToUseItemInHand(delta);
        return false;
    }

}
