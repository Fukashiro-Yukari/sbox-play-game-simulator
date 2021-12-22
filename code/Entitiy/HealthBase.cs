using Sandbox;
using System;

public class HealthBase : Prop, IUse, IRespawnableEntity
{
	public virtual int AddHealth => 50;
	public virtual string ModelPath => "";
	public virtual string SoundName => "health-shot";

	public override void Spawn()
	{
		base.Spawn();

		SetModel( ModelPath );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
	}

	public bool IsUsable( Entity user )
	{
		return user is Player && user.Health < 100;
	}

	public bool OnUse( Entity user )
	{
		if ( user is RealPlayer ply )
			ply.Healthing = Math.Min( ply.Healthing + AddHealth, 100 );
			
		else if ( user is InGamePlayer ply2 )
			ply2.Healthing = Math.Min( ply2.Healthing + AddHealth, 100 );

		user.PlaySound( SoundName );
		ItemRespawn.Taken( this );
		Delete();

		return false;
	}
}
