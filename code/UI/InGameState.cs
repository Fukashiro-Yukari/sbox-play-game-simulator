using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class InGameState : Panel
{
	Label IconHP;
	Panel HealthBar;

	public InGameState()
	{
		var HealthBarBG = Add.Panel( "HealthBarBG" );

		IconHP = HealthBarBG.Add.Label( "favorite", "IconHP" );

		var HealthBarBack = HealthBarBG.Add.Panel( "HealthBarBack" );
		
		HealthBar = HealthBarBack.Add.Panel( "HealthBar" );
	}

	public override void Tick()
	{
		SetClass( "IsRealPlayer", !(Local.Pawn is InGamePlayer) );

		if ( Local.Pawn is not InGamePlayer ply ) return;

		HealthBar.Style.Width = Length.Percent( ply.Health );
		HealthBar.Style.Dirty();
	}
}
