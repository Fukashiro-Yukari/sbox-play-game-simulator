using Sandbox;
using System;
using System.Linq;

partial class InGameInventory : BaseInventory
{
	public InGameInventory( Player player ) : base( player )
	{
	}

	public override bool CanAdd( Entity entity )
	{
		if ( !entity.IsValid() )
			return false;

		if ( !base.CanAdd( entity ) )
			return false;

		if ( entity is Weapon weapon )
		{
			if ( weapon.Bucket >= 0 && weapon.Bucket <= 2 )
			{
				foreach ( var wep in List )
				{
					if ( wep is Weapon w )
					{
						if ( w.Bucket == weapon.Bucket ) return false;
					}
				}
			}
		}

		return !IsCarryingType( entity.GetType() );
	}

	public virtual bool CanReplace( Entity entity )
	{
		if ( !entity.IsValid() )
			return false;

		if ( !base.CanAdd( entity ) )
			return false;

		if ( entity is Weapon weapon )
		{
			if ( weapon.Bucket >= 0 && weapon.Bucket <= 2 )
			{
				foreach ( var wep in List )
				{
					if ( wep is Weapon w )
					{
						if ( w.Bucket == weapon.Bucket ) return true;
					}
				}
			}
		}

		return false;
	}

	public Entity GetReplaceEntity( Entity entity )
	{
		if ( entity is Weapon weapon )
		{
			if ( weapon.Bucket >= 0 && weapon.Bucket <= 2 )
			{
				foreach ( var wep in List )
				{
					if ( wep is Weapon w )
					{
						if ( w.Bucket == weapon.Bucket ) return w;
					}
				}
			}
		}

		return null;
	}

	public virtual Entity Replace( Entity entity )
	{
		if ( !Host.IsServer ) return null;

		var repent = GetReplaceEntity( entity );

		if ( repent != null && repent.IsValid )
		{
			var ac = Owner.ActiveChild;
			var needActive = ac is Carriable wep && entity is Carriable wep2 && wep.Bucket == wep2.Bucket;

			if ( Drop( repent ) )
			{
				if ( ac != null && ac == repent )
				{
					Owner.ActiveChild = null;
				}

				Add( entity, needActive );

				return repent;
			}
		}

		return null;
	}

	public override bool Add( Entity entity, bool makeActive = false )
	{
		if ( !entity.IsValid() )
			return false;

		var player = Owner as InGamePlayer;
		var weapon = entity as Weapon;
		var notices = !player.SupressPickupNotices;

		if ( weapon != null && IsCarryingType( entity.GetType() ) && !entity.Owner.IsValid() )
		{
			var ammo = weapon.AmmoClip + weapon.AmmoCount;

			if ( ammo > 0 )
			{
				var wep = List.Where( x => x?.GetType() == weapon.GetType() ).ToArray()[0] as Weapon;

				wep.AmmoCount += ammo;

				if ( notices )
				{
					Sound.FromWorld( "dm.pickup_ammo", weapon.Position );
					PickupFeed.OnPickupAmmo( To.Single( player ), weapon, ammo );
				}
			}

			ItemRespawn.Taken( entity );

			weapon.Delete();

			return false;
		}

		if ( !CanAdd( entity ) ) return false;

		if ( weapon != null && notices && entity.Owner == null )
		{
			Sound.FromWorld( "dm.pickup_weapon", weapon.Position );

			PickupFeed.OnPickup( To.Single( player ), weapon );
		}

		ItemRespawn.Taken( entity );

		return base.Add( entity, makeActive );
	}

	public bool IsCarryingType( Type t )
	{
		return List.Any( x => x?.GetType() == t );
	}

	public override Entity DropActive()
	{
		if ( !Host.IsServer ) return null;

		var ac = Owner.ActiveChild;
		if ( ac == null ) return null;

		if ( Drop( ac ) )
		{
			Owner.ActiveChild = null;
			return ac;
		}

		return null;
	}

	public virtual bool DropAll()
	{
		if ( !Host.IsServer ) return false;

		Owner.ActiveChild = null;

		for ( int i = 0; i < List.Count; i++ )
		{
			var wep = List[i];

			Drop( wep );
		}

		return true;
	}

	public override bool Drop( Entity ent )
	{
		if ( !Host.IsServer )
			return false;

		if ( !Contains( ent ) )
			return false;

		if ( ent is InGameHand || ent is RealHand )
			return false;

		ent.OnCarryDrop( Owner );

		return ent.Parent == null;
	}
}
