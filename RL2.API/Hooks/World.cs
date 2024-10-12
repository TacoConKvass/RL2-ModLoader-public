using MonoMod.RuntimeDetour;
using RL2.ModLoader.API;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RL2.ModLoader;

public partial class RL2API
{
	/// <summary>
	/// Stores map icons as prefabs, accessed by a 
	/// </summary>
	internal static Dictionary<string, GameObject> TextureHashToPrefab = new Dictionary<string, GameObject>();

	/// <summary>
	/// Handles modifying the rooms icon on the map
	/// </summary>
	internal static Hook ModifyRoomIcon = new Hook(
		typeof(MapController).GetMethod("GetSpecialIconPrefab", BindingFlags.Public | BindingFlags.Static),
		(Func<GridPointManager, bool, bool, GameObject> orig, GridPointManager roomToCheck, bool getUsed, bool isMergeRoom) => {
			Texture2D? modMapIconTexture = World.Map.ModifyRoomIcon_Invoke(roomToCheck, getUsed, isMergeRoom);
			if (modMapIconTexture != null) {
				string textureHash = string.Concat(modMapIconTexture.GetPixels32());
				if (TextureHashToPrefab.ContainsKey(textureHash)) {
					return TextureHashToPrefab[textureHash];
				}
				GameObject modMapIconObject = UnityEngine.Object.Instantiate(MapController.m_instance.m_specialRoomIconPrefab);
				Sprite sprite = Sprite.Create(modMapIconTexture, new Rect(0f, 0f, modMapIconTexture.width, modMapIconTexture.height), new Vector2(.5f, .5f));
				modMapIconObject.GetComponent<SpriteRenderer>().sprite = sprite;
				UnityEngine.Object.DontDestroyOnLoad(modMapIconObject);
				modMapIconObject.SetActive(false);
				TextureHashToPrefab.Add(textureHash, modMapIconObject);
				return modMapIconObject;
			}
			return orig(roomToCheck, getUsed, isMergeRoom);
		}
	);
}