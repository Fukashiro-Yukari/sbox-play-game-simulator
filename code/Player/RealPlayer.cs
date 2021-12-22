using Sandbox;
using System;

public partial class RealPlayer : Player
{
	private TimeSince timeSinceDropped;
	private TimeSince timeSinceCold;
	private TimeSince timeSinceColdSound;
	private TimeSince timeSinceStamina;
	private TimeSince timeSinceHealthing;

	private DamageInfo lastDamage;

	public bool SupressPickupNotices { get; private set; }
	public bool IsHeadShot { get; private set; }
	public ModelEntity Ragdoll { get; set; }
	public int Healthing { get; set; }

	public Clothing.Container Clothing = new();

	[Net] public InGamePlayer PlayerTwe { get; set; }
	[Net] public float Cold { get; set; }
	[Net] public int Money { get; set; }
	[Net] public float Stamina { get; set; }

	public RealPlayer()
	{
		Inventory = new RealInventory( this );
	}

	public RealPlayer( Client cl ) : this()
	{
		Clothing.LoadFromClient( cl );
	}

	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		Cold = 100f;
		Healthing = 0;

		if ( Money > 0 )
			LossMoney( 100 );

		Stamina = PlayGames.MaxPlayerStamina;
		timeSinceCold = 0;
		Controller = new WalkController();
		Animator = new StandardPlayerAnimator();

		Camera = new FirstPersonCamera();

		if ( DevController is NoclipController )
		{
			DevController = null;
		}

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableLagCompensation = true;

		Clothing.DressEntity( this );

		SupressPickupNotices = true;

		Inventory.Add( new Flashlight() );
		Inventory.Add( new RealHand(), true );

		SupressPickupNotices = false;

		base.Respawn();
	}

	public override void OnKilled()
	{
		IsHeadShot = GetHitboxGroup( lastDamage.HitboxIndex ) == 1;

		timeSinceDied = 0;

		base.OnKilled();

		if ( lastDamage.Flags.HasFlag( DamageFlags.Vehicle ) )
		{
			Particles.Create( "particles/impact.flesh.bloodpuff-big.vpcf", lastDamage.Position );
			Particles.Create( "particles/impact.flesh-big.vpcf", lastDamage.Position );
			PlaySound( "kersplat" );
		}

		if ( lastDamage.Flags.HasFlag( DamageFlags.Shock ) )
		{
			Hint.Add( To.Single( this ), "You fell to the ground because you couldn't bear the cold" );
		}

		BecomeRagdollOnClient( Velocity, lastDamage.Flags, lastDamage.Position, lastDamage.Force, GetHitboxBone( lastDamage.HitboxIndex ) );

		Camera = new RealityCamera();
		Controller = null;

		EnableAllCollisions = false;
		EnableDrawing = false;

		Inventory.DropActive();
		Inventory.DeleteContents();
	}

	public override void TakeDamage( DamageInfo info )
	{
		if ( GetHitboxGroup( info.HitboxIndex ) == 1 )
		{
			info.Damage *= 2.0f;
		}

		lastDamage = info;

		base.TakeDamage( info );

		if ( info.Flags.HasFlag( DamageFlags.Shock ) ) return;

		PlaySound( $"pain-{Rand.Int( 1, 4 )}" );
	}

	TimeSince timeSinceDied;

	public override void Simulate( Client cl )
	{
		if ( LifeState != LifeState.Alive )
		{
			if ( timeSinceDied > 8 && IsServer )
			{
				Respawn();
			}

			return;
		}

		var controller = GetActiveController();
		controller?.Simulate( cl, this, GetActiveAnimator() );

		if ( Input.ActiveChild != null )
		{
			ActiveChild = Input.ActiveChild;
		}

		if ( LifeState != LifeState.Alive )
			return;

		if ( IsClient && Ragdoll != null )
			Ragdoll = null;

		if ( controller != null )
			EnableSolidCollisions = !controller.HasTag( "noclip" );

		TickPlayerUse();
		SimulateActiveChild( cl, ActiveChild );

		if ( Input.Pressed( InputButton.Drop ) )
		{
			var dropped = Inventory.DropActive();
			if ( dropped != null )
			{
				dropped.PhysicsGroup.ApplyImpulse( Velocity + EyeRot.Forward * 80.0f + Vector3.Up * 100.0f, true );
				dropped.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );

				timeSinceDropped = 0;
			}
		}

		if ( Input.Down( InputButton.Run ) )
		{
			timeSinceStamina = 0;

			if ( Stamina > 0 )
				Stamina -= 0.2f;
		}

		if ( Input.Pressed( InputButton.Jump ) )
		{
			timeSinceStamina = 0;

			if ( Stamina > 0 )
				Stamina -= 20f;
		}

		if ( IsClient )
			CanUseEntityGlow();
	}

	// When the player entity does not have a client, the Simulate method will not run
	[Event.Tick.Server]
	public void OnTick()
	{
		if ( LifeState == LifeState.Alive )
		{
			if ( timeSinceCold > 1 )
			{
				timeSinceCold = 0;

				if ( Cold > 0 )
					Cold--;

				if ( Cold < 20 )
				{
					TakeDamage( DamageInfo.Generic( 2 ).WithFlag( DamageFlags.Shock ) );
				}
			}

			if ( timeSinceColdSound > 5 )
			{
				timeSinceColdSound = 0;

				if ( Cold < 20 )
				{
					PlaySound( "sneeze" );
					PlaySoundOnClient( To.Single( this ) );
				}
			}

			if ( Stamina < PlayGames.MaxPlayerStamina && timeSinceStamina > 0.5 )
			{
				Stamina += 1f;
			}

			if ( Healthing > 0 && timeSinceHealthing > 0.3 )
			{
				timeSinceHealthing = 0;
				Healthing--;

				if ( Health < 100 )
					Health++;
			}
		}
	}

	[ClientRpc]
	public void PlaySoundOnClient()
	{
		PlaySound( "sneeze-ui" );
	}

	public override void StartTouch( Entity other )
	{
		if ( timeSinceDropped < 0.1 ) return;

		base.StartTouch( other );
	}

	public void ChangePlayer()
	{
		if ( !Client.Pawn.IsValid() || PlayerTwe == null || !PlayerTwe.IsValid() )
			return;

		Velocity = new Vector3();

		var rot = PlayerTwe.Rotation;

		Hint.Add( To.Single( this ), "Joining a deathmatch game.." );

		Client.Pawn = null;
		Client.Pawn = PlayerTwe;
		Client.Pawn.Rotation = rot;
	}

	[Event.BuildInput]
	public override void BuildInput( InputBuilder input )
	{
		base.BuildInput( input );

		if ( Local.Pawn != this ) return;
		if ( Stamina <= 20 )
		{
			input.ClearButton( InputButton.Jump );
		}

		if ( Stamina <= 0 )
		{
			input.ClearButton( InputButton.Run );
		}
	}

	public void GiveMoney( int add )
	{
		Money += add;
		MoneyFeed.OnGiveMoney( To.Single( this ), add );
	}

	public void LossMoney( int loss )
	{
		Money -= Math.Min( Money, loss );
		MoneyFeed.OnLossMoney( To.Single( this ), loss );
	}

	[AdminCmd("pg_givememoney")]
	public static void GiveMeMoney()
	{
		if ( ConsoleSystem.Caller.Pawn is not RealPlayer ply ) return;

		ply.Money += 10000;
	}
}
