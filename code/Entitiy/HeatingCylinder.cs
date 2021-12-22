using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

[Library( "ent_heating_cylinder", Title = "Change Player Computer" )]
[Hammer.EditorModel( "models/sbox_props/gas_cylinder_fat/gas_cylinder_fat.vmdl", FixedBounds = true )]
public partial class HeatingCylinder: Prop, IUse
{
	private TimeSince timeSinceHeatingFuel;
	private TimeSince timeSinceHeatingCold;
	private float MaxHeatingFuel = 100f;
	private HeatingCylinderTags tags;
	private int need;

	[Net] public float HeatingFuel { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/sbox_props/gas_cylinder_fat/gas_cylinder_fat.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );

		timeSinceHeatingFuel = 0;
		timeSinceHeatingCold = 0;
		HeatingFuel = MaxHeatingFuel;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		tags = new HeatingCylinderTags( this );
	}

	[Event.Tick.Server]
	public void OnTick()
	{
		need = (int)(MaxHeatingFuel - HeatingFuel) * 2;

		var ents = Physics.GetEntitiesInSphere( Position, 150 );
		var plys = ents.OfType<RealPlayer>().Where( p => p.LifeState == LifeState.Alive ).ToList();

		if ( timeSinceHeatingCold > 0.5 && HeatingFuel > 0 )
		{
			timeSinceHeatingCold = 0;

			foreach ( var ply in plys )
			{
				if ( ply.Cold <= 100 )
					ply.Cold += Math.Max( 5f / plys.Count, 1f );
			}
		}

		if ( timeSinceHeatingFuel > 1 )
		{
			timeSinceHeatingFuel = 0;

			if ( HeatingFuel > 0 )
				HeatingFuel -= 1f;
		}
	}

	[Event.Tick.Client]
	public void OnClientTick()
	{
		var ply = Local.Pawn;

		if ( tags == null || ply == null ) return;

		var ang = ply.Rotation.Angles();
		var pos = Position + Vector3.Up * 45;

		tags.Transform = new Transform( pos, Rotation.From( ang + new Angles( 0, 180, 0 ) ) );
	}

	public bool IsUsable( Entity user )
	{
		return HeatingFuel < MaxHeatingFuel && user is RealPlayer ply;
	}

	public bool OnUse( Entity user )
	{
		if ( user is RealPlayer ply )
		{
			if ( ply.Money >= need )
			{
				HeatingFuel = MaxHeatingFuel;

				ply.LossMoney( need );
			}
			else if ( ply.Money > 0 )
			{
				var add = ply.Money;

				HeatingFuel = add / 2;

				ply.LossMoney( add );
			}
			else
				Hint.Add( To.Single( ply ), "You have to play game to make money." );
		}

		return false;
	}

	public override void TakeDamage( DamageInfo info )
	{
		// Avoid explosion
		//base.TakeDamage( info );
	}
}
