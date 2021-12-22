using Sandbox;
using System;

public partial class InGamePlayer : Player
{
	private TimeSince timeSinceDropped;
	private TimeSince timeSinceHealthing;
	private TimeSince timeSinceGiveMoney;

	private DamageInfo lastDamage;

	[Net] public RealPlayer PlayerTwe { get; set; }
	[Net] public PawnController VehicleController { get; set; }
	[Net] public PawnAnimator VehicleAnimator { get; set; }
	[Net, Predicted] public ICamera VehicleCamera { get; set; }
	[Net, Predicted] public Entity Vehicle { get; set; }

	public ICamera LastCamera { get; set; }
	public int Healthing { get; set; }

	public bool IsHeadShot { get; private set; }
	public bool SupressPickupNotices { get; private set; }

	public InGamePlayer()
	{
		Inventory = new InGameInventory( this );
	}

	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		Healthing = 0;
		timeSinceGiveMoney = 0;
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

		SupressPickupNotices = true;

		Inventory.Add( new InGameHand(), true );

		SupressPickupNotices = false;

		Dress();

		base.Respawn();
	}

	public override void OnKilled()
	{
		IsHeadShot = GetHitboxGroup( lastDamage.HitboxIndex ) == 1;

		base.OnKilled();

		if ( lastDamage.Flags.HasFlag( DamageFlags.Vehicle ) )
		{
			Particles.Create( "particles/impact.flesh.bloodpuff-big.vpcf", lastDamage.Position );
			Particles.Create( "particles/impact.flesh-big.vpcf", lastDamage.Position );
			PlaySound( "kersplat" );
		}

		VehicleController = null;
		VehicleAnimator = null;
		VehicleCamera = null;
		Vehicle = null;

		BecomeRagdollOnClient( Velocity, lastDamage.Flags, lastDamage.Position, lastDamage.Force, GetHitboxBone( lastDamage.HitboxIndex ) );

		Camera = new SpectateRagdollCamera();
		Controller = null;

		EnableAllCollisions = false;
		EnableDrawing = false;

		var inv = Inventory as InGameInventory;

		inv.DropAll();
		inv.DeleteContents();
	}

	Rotation lastCameraRot = Rotation.Identity;

	public override void PostCameraSetup( ref CameraSetup setup )
	{
		base.PostCameraSetup( ref setup );

		if ( lastCameraRot == Rotation.Identity )
			lastCameraRot = setup.Rotation;

		var angleDiff = Rotation.Difference( lastCameraRot, setup.Rotation );
		var angleDiffDegrees = angleDiff.Angle();
		var allowance = 20.0f;

		if ( angleDiffDegrees > allowance )
		{
			// We could have a function that clamps a rotation to within x degrees of another rotation?
			lastCameraRot = Rotation.Lerp( lastCameraRot, setup.Rotation, 1.0f - (allowance / angleDiffDegrees) );
		}
		else
		{
			//lastCameraRot = Rotation.Lerp( lastCameraRot, Camera.Rotation, Time.Delta * 0.2f * angleDiffDegrees );
		}

		// uncomment for lazy cam
		//camera.Rotation = lastCameraRot;

		if ( setup.Viewer != null )
		{
			AddCameraEffects( ref setup );
		}
	}

	float walkBob = 0;
	float lean = 0;
	float fov = 0;

	private void AddCameraEffects( ref CameraSetup setup )
	{
		var speed = Velocity.Length.LerpInverse( 0, 320 );
		var forwardspeed = Velocity.Normal.Dot( setup.Rotation.Forward );

		var left = setup.Rotation.Left;
		var up = setup.Rotation.Up;

		if ( GroundEntity != null )
		{
			walkBob += Time.Delta * 25.0f * speed;
		}

		setup.Position += up * MathF.Sin( walkBob ) * speed * 2;
		setup.Position += left * MathF.Sin( walkBob * 0.6f ) * speed * 1;

		// Camera lean
		lean = lean.LerpTo( Velocity.Dot( setup.Rotation.Right ) * 0.03f, Time.Delta * 15.0f );

		var appliedLean = lean;
		appliedLean += MathF.Sin( walkBob ) * speed * 0.2f;
		setup.Rotation *= Rotation.From( 0, 0, appliedLean );

		speed = (speed - 0.7f).Clamp( 0, 1 ) * 3.0f;

		fov = fov.LerpTo( speed * 20 * MathF.Abs( forwardspeed ), Time.Delta * 2.0f );

		setup.FieldOfView += fov;
	}

	public override void TakeDamage( DamageInfo info )
	{
		if ( GetHitboxGroup( info.HitboxIndex ) == 1 )
		{
			info.Damage *= 2.0f;
		}

		lastDamage = info;

		base.TakeDamage( info );

		if ( info.Attacker != null && (info.Attacker is InGamePlayer || info.Attacker.Owner is InGamePlayer) )
		{
			InGamePlayer attacker = info.Attacker as InGamePlayer;

			if ( attacker == null )
				attacker = info.Attacker.Owner as InGamePlayer;

			// Note - sending this only to the attacker!
			if ( attacker != this )
				attacker.DidDamage( To.Single( attacker ), info.Position, info.Damage, Health.LerpInverse( 100, 0 ), Health <= 0 );
		}
	}

	[ClientRpc]
	public void DidDamage( Vector3 pos, float amount, float healthinv, bool isdeath )
	{
		Sound.FromScreen( "dm.ui_attacker" )
			.SetPitch( 1 + healthinv * 1 );

		HitIndicator.Current?.OnHit( pos, amount, isdeath );
	}

	public override PawnController GetActiveController()
	{
		if ( VehicleController != null ) return VehicleController;
		if ( DevController != null ) return DevController;

		return base.GetActiveController();
	}

	public override PawnAnimator GetActiveAnimator()
	{
		if ( VehicleAnimator != null ) return VehicleAnimator;

		return base.GetActiveAnimator();
	}

	public ICamera GetActiveCamera()
	{
		if ( VehicleCamera != null ) return VehicleCamera;

		return Camera;
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( Input.ActiveChild != null )
		{
			ActiveChild = Input.ActiveChild;
		}

		if ( LifeState != LifeState.Alive )
			return;

		if ( VehicleController != null && DevController is NoclipController )
		{
			DevController = null;
		}

		var controller = GetActiveController();
		if ( controller != null )
			EnableSolidCollisions = !controller.HasTag( "noclip" );

		TickPlayerUse();
		SimulateActiveChild( cl, ActiveChild );

		Camera = GetActiveCamera();

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

		if ( IsClient )
			CanUseEntityGlow();

		if ( Input.Pressed( InputButton.View ) )
			ChangePlayer();

		if ( timeSinceGiveMoney > 60 && PlayerTwe.IsValid() )
		{
			timeSinceGiveMoney = 0;
			PlayerTwe.GiveMoney( 10 );
		}
	}

	[Event.Tick.Server]
	public void OnTick()
	{
		if ( Client.IsValid() && Client.Pawn.IsValid() && Client.Pawn == this && PlayerTwe.IsValid() && PlayerTwe.LifeState != LifeState.Alive )
		{
			ChangePlayer();
		}

		if ( LifeState == LifeState.Alive )
		{
			if ( Healthing > 0 && timeSinceHealthing > 0.3 )
			{
				timeSinceHealthing = 0;
				Healthing--;

				if ( Health < 100 )
					Health++;
			}
		}
	}

	public override void StartTouch( Entity other )
	{
		if ( timeSinceDropped < 0.1 ) return;

		base.StartTouch( other );
	}

	[ServerCmd( "inventory_current" )]
	public static void SetInventoryCurrent( string entName )
	{
		var target = ConsoleSystem.Caller.Pawn;
		if ( target == null ) return;

		var inventory = target.Inventory;
		if ( inventory == null )
			return;

		for ( int i = 0; i < inventory.Count(); ++i )
		{
			var slot = inventory.GetSlot( i );
			if ( !slot.IsValid() )
				continue;

			if ( !slot.ClassInfo.IsNamed( entName ) )
				continue;

			inventory.SetActiveSlot( i, false );

			break;
		}
	}

	public void ChangePlayer()
	{
		if ( !Client.Pawn.IsValid() || PlayerTwe == null || !PlayerTwe.IsValid() )
			return;

		Velocity = new Vector3();

		var rot = PlayerTwe.Rotation;

		Hint.Add( To.Single( this ), "Leaving game" );

		Client.Pawn = null;
		Client.Pawn = PlayerTwe;
		Client.Pawn.Rotation = rot;
	}
}
