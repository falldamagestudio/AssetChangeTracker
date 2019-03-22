using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.Assertions;

namespace AssetChangeTracker
{
	/// <summary>
	/// Application-wide interface for the AssetChangeTracker
	/// </summary>
	public class GlobalAssetChangeTracker : AssetPostprocessor
	{

		/// <summary>
		/// Unity API adapter: asset queries are routed to the AssetDatabase.
		/// </summary>
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

		/// <summary>
		/// Adds a listener object to the AssetChangeTracker.
		///
		/// The same listener can be added for several different types.
		/// Multiple different listeners can be added for the same type.
		/// The same listener cannot be added twice for the same type.
		/// </summary>
		public static void AddListener(Type assetType, IListener listener)
		{
			Assert.IsNotNull(assetType);
			Assert.IsNotNull(listener);

			Instance.AddListener(assetType, listener);
		}

		/// <summary>
		/// Removes a listener object from the AssetChangeTracker.
		/// </summary>
		public static void RemoveListener(Type assetType, IListener listener)
		{
			Assert.IsNotNull(assetType);
			Assert.IsNotNull(listener);

			Instance.RemoveListener(assetType, listener);
		}


		/// <summary>
		/// Unity API callback: This is called whenever the set of assets change - regardless of type.
		/// </summary>
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