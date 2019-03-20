using System;
using UnityEditor;

namespace AssetChangeTracker
{
	public class GlobalAssetChangeTracker : AssetPostprocessor
	{
		private static AssetChangeTracker Instance = new AssetChangeTracker();

		public static void AddListener(Type assetType, TrackedAssetType.IAssetChangeNotifications listener)
		{
			Instance.AddListener(assetType, listener);
		}

		public static void RemoveListener(Type assetType, TrackedAssetType.IAssetChangeNotifications listener)
		{
			Instance.RemoveListener(assetType, listener);
		}

		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			Instance.OnReimported(importedAssets);
			Instance.OnDeleted(deletedAssets);
			Instance.OnMoved(movedFromAssetPaths, movedAssets);
		}
	}
}