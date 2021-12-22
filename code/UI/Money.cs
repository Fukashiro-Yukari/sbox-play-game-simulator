using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Money : Panel
{
	Label Icon;
	Label MoneyText;

	public Money()
	{
		Icon = Add.Label( "attach_money", "Icon" );
		MoneyText = Add.Label( "0", "Text" );
	}

	public override void Tick()
	{
		if ( Local.Pawn is RealPlayer ply )
		{
			MoneyText.Text = $"{ply.Money}";
			Style.Bottom = 110;
			Style.Dirty();
		}
		else if ( Local.Pawn is InGamePlayer ply2 )
		{
			MoneyText.Text = $"{ply2.PlayerTwe.Money}";
			Style.Bottom = 60;
			Style.Dirty();
		}
	}
}
