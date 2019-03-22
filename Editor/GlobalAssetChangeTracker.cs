using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.Assertions;

namespace AssetChangeTracker
{
	public class GlobalAssetChangeTracker : AssetPostprocessor
	{

		private class AssetDatabaseAccess : AssetChangeTracker.IAssetDatabaseAccess
		{
			public HashSet<string> GetAssetsOfType(Type assetType)
			{
				Assert.IsNotNull(assetType);

				string[] assetGuids = AssetDatabase.FindAssets(string.Format("t:{0}", assetType));
				HashSet<string> assetPaths = new HashSet<string>(assetGuids.Select(assetGuid => AssetDatabase.GUIDToAssetPath(assetGuid)));
				return assetPaths;
			}

			public Type GetAssetType(string assetPath)
			{
				Assert.IsNotNull(assetPath);

				Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
				Assert.IsNotNull(assetType);
				return assetType;
			}
		}

		private static AssetChangeTracker Instance = new AssetChangeTracker(new AssetDatabaseAccess());

		public static void AddListener(Type assetType, IListener listener)
		{
			Assert.IsNotNull(assetType);
			Assert.IsNotNull(listener);

			Instance.AddListener(assetType, listener);
		}

		public static void RemoveListener(Type assetType, IListener listener)
		{
			Assert.IsNotNull(assetType);
			Assert.IsNotNull(listener);

			Instance.RemoveListener(assetType, listener);
		}


		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			Assert.IsNotNull(importedAssets);
			Assert.IsNotNull(deletedAssets);
			Assert.IsNotNull(movedAssets);
			Assert.IsNotNull(movedFromAssetPaths);

			Instance.OnReimported(importedAssets);
			Instance.OnDeleted(deletedAssets);
			Instance.OnMoved(movedFromAssetPaths, movedAssets);
		}
	}
}