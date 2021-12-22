using Sandbox;
using System;

[Library( "ent_ammobox", Title = "Ammo Box" )]
[Hammer.EditorModel( "models/AmmoBox/ammobox.vmdl", FixedBounds = true )]
public class AmmoBox : Prop, IUse, IRespawnableEntity
{
	public int AmmoCount { get; private set; } = 200;
	private TimeSince timeSincePickup;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/AmmoBox/ammobox.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
	}

	public bool IsUsable( Entity user )
	{
		return user is Player && timeSincePickup > 0.5;
	}

	public bool OnUse( Entity user )
	{
		timeSincePickup = 0;

		if ( user is Player ply && ply.ActiveChild is Weapon wep )
		{
			var ammo = Math.Min( AmmoCount, wep.ClipSize * 2 );

			wep.AmmoCount += ammo;
			AmmoCount -= ammo;
			Sound.FromWorld( "dm.pickup_ammo", wep.Position );
			PickupFeed.OnPickupAmmo( To.Single( ply ), wep, ammo );

			if ( AmmoCount <= 0 )
			{
				ItemRespawn.Taken( this );
				Delete();
			}
		}

		return false;
	}
}
