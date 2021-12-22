using Sandbox;
using System;

[Library( "pg_snowballpistol", Title = "Snow ball pistol", Spawnable = true )]
[Hammer.EditorModel( "weapons/rust_pistol/rust_pistol.vmdl" )]
partial class SnowBallPistol : Weapon
{ 
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
	public override string WorldModelPath => "weapons/rust_pistol/rust_pistol.vmdl";
	public override BaseViewModel ViewModel => new InGamePlayerViewModel();

	public override int ClipSize => 18;
	public override int Bucket => 1;
	public override float PrimaryRate => 15.0f;
	public override float SecondaryRate => 1.0f;
	public override float ReloadTime => 3.0f;
	public override bool Automatic => false;
	public override CType Crosshair => CType.Pistol;
	public override string Icon => "ui/weapons/weapon_pistol.png";
	public override string ShootSound => "rust_pistol.shoot";
	public override bool CanDischarge => true;
	public override float Spread => 0.05f;
	public override float Force => 1.5f;
	public override float Damage => 9.0f;
	public override float BulletSize => 3.0f;
	public override ScreenShake ScreenShake => new ScreenShake { };

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 1 );
		anim.SetParam( "aimat_weight", 1.0f );
		anim.SetParam( "holdtype_handedness", 0 );
	}

	public override Func<Vector3, Vector3, Vector3, float, float, float, Entity> CreateEntity => CreateSnowBall;

	private Entity CreateSnowBall( Vector3 pos, Vector3 dir, Vector3 forward, float spread, float force, float damage )
	{
		if ( IsClient ) return null;
		using ( Prediction.Off() )
		{
			var rand = Rand.Float( 1 );

			if ( rand < 0.1 )
			{
				var man = new SnowMan();
				man.Position = pos + Owner.EyeRot.Forward * 100;
				man.Rotation = Owner.EyeRot;
				man.Owner = Owner;
				man.Velocity = Owner.EyeRot.Forward * 200000;

				return man;
			}
			else
			{
				var ball = new SnowBall();
				ball.Position = pos + Owner.EyeRot.Forward * 100;
				ball.Rotation = Owner.EyeRot;
				ball.Owner = Owner;
				ball.Velocity = Owner.EyeRot.Forward * 200000;

				return ball;
			}
		}
	}
}
