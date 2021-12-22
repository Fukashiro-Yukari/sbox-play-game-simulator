
using Sandbox;
using Sandbox.UI;
using System;

public partial class KillFeed : Panel
{
	public static KillFeed Current;

	public KillFeed()
	{
		Current = this;

		StyleSheet.Load( "/UI/KillFeed.scss" );
	}

	private bool GetIcon( string method, KillFeedEntry e )
	{
		try
		{
			if ( method != null && method.StartsWith( "pg_" ) || method == "crossbow_bolt" )
			{
				var killWeapon = Library.Create<Entity>( method );

				if ( killWeapon is Carriable car )
				{
					if ( !string.IsNullOrEmpty( car.Icon ) )
					{
						e.Icon.Style.BackgroundImage = Texture.Load( car.Icon );
						e.Icon.SetClass( "close", false );
						killWeapon.Delete();

						return true;
					}
				}
				else if ( killWeapon is CrossbowBolt cb )
				{
					if ( !string.IsNullOrEmpty( cb.Icon ) )
					{
						e.Icon.Style.BackgroundImage = Texture.Load( cb.Icon );
						e.Icon.SetClass( "close", false );
						killWeapon.Delete();

						return true;
					}
				}
			}
		}
		catch ( Exception ) { }

		return false;
	}

	public virtual Panel AddEntry( long lsteamid, string left, long rsteamid, string right, string method, bool isHeadShot = false )
	{
		var e = Current.AddChild<KillFeedEntry>();

		e.Left.Text = left;
		e.Left.SetClass( "me", lsteamid == (Local.PlayerId) );

		if ( !GetIcon( method, e ) )
		{
			e.Method.Text = method;
			e.Icon.SetClass( "close", true );
		}

		e.HeadShotIcon.Style.BackgroundImage = Texture.Load( "ui/headshot.png" );
		e.HeadShotIcon.SetClass( "close", !isHeadShot );

		e.Right.Text = right;
		e.Right.SetClass( "me", rsteamid == (Local.PlayerId) );

		return e;
	}

	public virtual Panel AddEntry( string left, long rsteamid, string right, string method, bool isHeadShot = false )
	{
		var e = Current.AddChild<KillFeedEntry>();

		e.Left.Text = left;
		e.Left.SetClass( "me", false );

		if ( !GetIcon( method, e ) )
		{
			e.Method.Text = method;
			e.Icon.SetClass( "close", true );
		}

		e.HeadShotIcon.Style.BackgroundImage = Texture.Load( "ui/headshot.png" );
		e.HeadShotIcon.SetClass( "close", !isHeadShot );

		e.Right.Text = right;
		e.Right.SetClass( "me", rsteamid == (Local.Client?.PlayerId) );

		return e;
	}

	public virtual Panel AddEntry( long lsteamid, string left, string right, string method, bool isHeadShot = false )
	{
		var e = Current.AddChild<KillFeedEntry>();

		e.Left.Text = left;
		e.Left.SetClass( "me", lsteamid == (Local.Client?.PlayerId) );

		if ( !GetIcon( method, e ) )
		{
			e.Method.Text = method;
			e.Icon.SetClass( "close", true );
		}

		e.HeadShotIcon.Style.BackgroundImage = Texture.Load( "ui/headshot.png" );
		e.HeadShotIcon.SetClass( "close", !isHeadShot );

		e.Right.Text = right;
		e.Right.SetClass( "me", false );

		return e;
	}

	public virtual Panel AddEntry( string left, string right, string method, bool isHeadShot = false )
	{
		var e = Current.AddChild<KillFeedEntry>();

		e.Left.Text = left;
		e.Left.SetClass( "me", false );

		if ( !GetIcon( method, e ) )
		{
			e.Method.Text = method;
			e.Icon.SetClass( "close", true );
		}

		e.HeadShotIcon.Style.BackgroundImage = Texture.Load( "ui/headshot.png" );
		e.HeadShotIcon.SetClass( "close", !isHeadShot );

		e.Right.Text = right;
		e.Right.SetClass( "me", false );

		return e;
	}

	[ClientRpc]
	public static void OnKilledMessage( long leftid, string left, long rightid, string right, string method, bool isHeadShot )
	{
		Current?.AddEntry( leftid, left, rightid, right, method, isHeadShot );
	}

	[ClientRpc]
	public static void OnKilledMessage( long leftid, string left, string right, string method )
	{
		Current?.AddEntry( leftid, left, right, method );
	}

	[ClientRpc]
	public static void OnKilledMessage( string left, long rightid, string right, string method )
	{
		Current?.AddEntry( left, rightid, right, method );
	}

	[ClientRpc]
	public static void OnKilledMessage( string left, string right, string method )
	{
		Current?.AddEntry( left, right, method );
	}
}
