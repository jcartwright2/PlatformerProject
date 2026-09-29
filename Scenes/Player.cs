using Godot;

public partial class Player : CharacterBody2D
{
	[Export]
	public float RunSpeed { get; set; } = 360.0f;

	[Export]
	public float JumpVelocity { get; set; } = -620.0f;

	[Export]
	public float Gravity { get; set; } = 1800.0f;

	[Export]
	public float RotationSpeed { get; set; } = 6.0f;

	private AnimatedSprite2D _sprite;

	private bool _hasShield = false;
	private float _shieldTimeRemaining = 0.0f;

	public bool HasShield
	{
		get { return _hasShield; }
	}

	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		_sprite.Play("run");

		AddToGroup("player");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Geometry Dash-style automatic forward movement.
		velocity.X = RunSpeed;

		// Gravity.
		if (!IsOnFloor())
		{
			velocity.Y += Gravity * (float)delta;

			// Only rotate the picture, NOT the physics body.
			_sprite.Rotation += RotationSpeed * (float)delta;
		}

		// Jump.
		if (
			Input.IsActionJustPressed("jump")
			&& IsOnFloor()
		)
		{
			velocity.Y = JumpVelocity;
		}

		Velocity = velocity;

		MoveAndSlide();

		// Snap the cube graphic to a 90-degree angle when landing.
		if (IsOnFloor())
		{
			float quarterTurn = Mathf.Pi / 2.0f;

			_sprite.Rotation =
				Mathf.Round(
					_sprite.Rotation / quarterTurn
				) * quarterTurn;
		}

		// If we run directly into the side of a solid block, die.
		for (
			int i = 0;
			i < GetSlideCollisionCount();
			i++
		)
		{
			KinematicCollision2D collision =
				GetSlideCollision(i);

			Vector2 normal =
				collision.GetNormal();

			if (Mathf.Abs(normal.X) > 0.7f)
			{
				Hit();
				return;
			}
		}

		// Temporary shield countdown.
		if (_hasShield)
		{
			_shieldTimeRemaining -= (float)delta;

			if (_shieldTimeRemaining <= 0.0f)
			{
				RemoveShield();
			}
		}

		// Restart.
		if (Input.IsActionJustPressed("restart"))
		{
			RestartLevel();
		}

		// Fell off the map.
		if (GlobalPosition.Y > 1200)
		{
			RestartLevel();
		}
	}

	public void Hit()
	{
		if (_hasShield)
		{
			RemoveShield();
			return;
		}

		RestartLevel();
	}

	public void ActivateShield(float duration)
	{
		_hasShield = true;
		_shieldTimeRemaining = duration;

		Modulate =
			new Color(
				0.4f,
				1.0f,
				1.0f
			);
	}

	private void RemoveShield()
	{
		_hasShield = false;
		_shieldTimeRemaining = 0.0f;

		Modulate = Colors.White;
	}

	public void RestartLevel()
	{
		GetTree().ReloadCurrentScene();
	}
}
