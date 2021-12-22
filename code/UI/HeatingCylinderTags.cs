using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class HeatingCylinderTags : WorldPanel
{
	public Label IconFuel;
	public Label IconMoney;
	public Panel FuelBar;
	public Label Money;
	public HeatingCylinder Heating;

	public HeatingCylinderTags( HeatingCylinder heating )
	{
		Heating = heating;

		StyleSheet.Load( "/UI/HeatingCylinderTags.scss" );

		var BG = Add.Panel( "BG" );
		var FuelBG = BG.Add.Panel( "FuelBG" );

		IconFuel = FuelBG.Add.Label( "flash_on", "IconFuel" );

		var FuelBarBack = FuelBG.Add.Panel( "FuelBarBack" );

		FuelBar = FuelBarBack.Add.Panel( "FuelBar" );

		var MoneyBG = BG.Add.Panel( "MoneyBG" );

		IconMoney = MoneyBG.Add.Label( "attach_money", "IconMoney" );
		Money = MoneyBG.Add.Label( "100", "Money" );
	}

	public override void Tick()
	{
		if ( !Heating.IsValid() ) return;

		var need = (100 - Heating.HeatingFuel) * 2;

		FuelBar.Style.Width = Length.Percent( Heating.HeatingFuel );
		Money.Text = $"{need}";

		if ( Local.Pawn is RealPlayer ply && ply.Money < need )
		{
			if ( ply.Money > 0 )
			{
				Money.Style.FontColor = Color.Yellow;
				Money.Style.Dirty();
			}
			else
			{
				Money.Style.FontColor = Color.Red;
				Money.Style.Dirty();
			}
		}
		else
		{
			Money.Style.FontColor = Color.White;
			Money.Style.Dirty();
		}

		if ( Heating.HeatingFuel <= 0 )
		{
			IconFuel.Style.FontColor = Color.Red;
			IconFuel.Style.Dirty();
		}
		else
		{
			IconFuel.Style.FontColor = new Color32( 255, 90, 0 ).ToColor();
			IconFuel.Style.Dirty();
		}
	}
}
