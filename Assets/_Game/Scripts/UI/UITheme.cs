using UnityEngine;

/// <summary>
/// Single source of truth for the pixel-art skin: palette, type ladder and grid.
/// Every UI number is a multiple of 4 (1 art pixel = 4 canvas units on a 1920x1080 canvas).
/// Font sizes are multiples of 18 (the native sampling size of the pixel font).
/// </summary>
public static class UITheme
{
	// ---- Palette (10 colors, no more) -------------------------------------
	public static readonly Color Bg     = Hex("0B1418");
	public static readonly Color Panel  = Hex("122024");
	public static readonly Color Panel2 = Hex("182B31");
	public static readonly Color Line   = Hex("2C464E");
	public static readonly Color Bone   = Hex("E8E1D5");
	public static readonly Color Muted  = Hex("8A9290");
	public static readonly Color Surv   = Hex("6FA85C");
	public static readonly Color Sci    = Hex("4FA3A5");
	public static readonly Color Mor    = Hex("E8A33D");
	public static readonly Color Danger = Hex("C4442E");

	// ---- Type ladder ------------------------------------------------------
	// Silkscreen is drawn on an 8px grid, VT323 on a 16px grid. Only sizes that
	// are exact multiples of those grids stay crisp, so the ladder is 16/24/32/48/64/96.
	public const float FsMicro   = 16f;  // tags, role, hints        (Silkscreen)
	public const float FsBody    = 24f;  // labels, effects, buttons (Silkscreen)
	public const float FsProse   = 32f;  // quotes, event body       (VT323)
	public const float FsName    = 48f;  // card titles, HUD numbers (Silkscreen)
	public const float FsTitle   = 64f;
	public const float FsDisplay = 96f;

	// ---- Layout grid ------------------------------------------------------
	public const float Grid = 4f;

	/// <summary>1 art pixel expressed in canvas units at the 4x working scale.</summary>
	public const float Px = 4f;

	/// <summary>Image.pixelsPerUnitMultiplier that turns 1 sprite pixel into 4 canvas units.</summary>
	public const float SpriteScale = 0.25f;

	public static readonly string[] RoleNames =
	{
		"BIOLOG", "ENGINEER", "SOLDIER", "MEDIC", "CHILD", "SICK", "FARMER", "MUTANT"
	};

	static readonly Color[] RoleColors =
	{
		new Color(0.42f, 0.66f, 0.36f), // Biolog
		new Color(0.31f, 0.64f, 0.65f), // Engineer
		new Color(0.72f, 0.36f, 0.30f), // Soldier
		new Color(0.91f, 0.64f, 0.24f), // Medic
		new Color(0.85f, 0.75f, 0.55f), // Child
		new Color(0.45f, 0.45f, 0.50f), // Sick
		new Color(0.55f, 0.60f, 0.35f), // Farmer
		new Color(0.60f, 0.40f, 0.65f), // Mutant
	};

	public static Color RoleColor(CardRole role) => RoleColors[(int)role % RoleColors.Length];

	public static string RoleLabel(CardRole role) => RoleNames[(int)role % RoleNames.Length];

	// ---- helpers ----------------------------------------------------------
	public static Color Hex(string hex)
	{
		ColorUtility.TryParseHtmlString("#" + hex, out Color c);
		return c;
	}

	public static string Html(Color c) => ColorUtility.ToHtmlStringRGB(c);

	/// <summary>Per-resource accent used by cards, HUD and event results.</summary>
	public static Color StatColor(int index) => index switch
	{
		0 => Surv,
		1 => Sci,
		_ => Mor
	};
}
