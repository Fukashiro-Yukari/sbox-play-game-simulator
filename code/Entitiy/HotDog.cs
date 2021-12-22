using Sandbox;
using System;

[Library( "ent_hotdog", Title = "HotDog" )]
[Hammer.EditorModel( "models/HotDog/hotdog.vmdl", FixedBounds = true )]
public class HotDog : HealthBase
{
	public override int AddHealth => 25;
	public override string ModelPath => "models/HotDog/hotdog.vmdl";
	public override string SoundName => "eat";
}
