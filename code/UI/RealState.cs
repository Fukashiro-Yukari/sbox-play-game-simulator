using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class RealState : Panel
{
	Label IconHP;
	Label IconCold;
	Label IconStamina;
	Panel HealthBar;
	Panel ColdBar;
	Panel StaminaBar;

	public RealState()
	{
		var HealthBarBG = Add.Panel( "HealthBarBG" );

		IconHP = HealthBarBG.Add.Label( "favorite", "IconHP" );

		var HealthBarBack = HealthBarBG.Add.Panel( "HealthBarBack" );
		
		HealthBar = HealthBarBack.Add.Panel( "HealthBar" );

		var ColdBarBG = Add.Panel( "ColdBarBG" );

		IconCold = ColdBarBG.Add.Label( "ac_unit", "IconCold" );

		var ColdBarBack = ColdBarBG.Add.Panel( "ColdBarBack" );

		ColdBar = ColdBarBack.Add.Panel( "ColdBar" );

		var StaminaBarBG = Add.Panel( "StaminaBarBG" );

		IconStamina = StaminaBarBG.Add.Label( "bolt", "IconStamina" );

		var StaminaBarBack = StaminaBarBG.Add.Panel( "StaminaBarBack" );

		StaminaBar = StaminaBarBack.Add.Panel( "StaminaBar" );
	}

	public override void Tick()
	{
		SetClass( "IsInGamePlayer", !(Local.Pawn is RealPlayer) );

		if ( Local.Pawn is not RealPlayer ply ) return;

		HealthBar.Style.Width = Length.Percent( ply.Health );
		HealthBar.Style.Dirty();

		ColdBar.Style.Width = Length.Percent( ply.Cold );
		ColdBar.Style.Dirty();

		StaminaBar.Style.Width = Length.Percent( ply.Stamina );
		StaminaBar.Style.Dirty();
	}
}
