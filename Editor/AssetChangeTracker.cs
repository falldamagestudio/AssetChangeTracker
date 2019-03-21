using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Assertions;

namespace AssetChangeTracker
{
	public class AssetChangeTracker
	{

		public interface IAssetDatabaseAccess
		{
			HashSet<string> GetAssetsOfType(Type assetType);
			Type GetAssetType(string assetPath);
		}

		private IAssetDatabaseAccess assetDatabaseAccess;


		private Dictionary<Type, TrackedAssetType> trackedAssetTypes = new Dictionary<Type, TrackedAssetType>();

		public AssetChangeTracker(IAssetDatabaseAccess assetDatabaseAccess)
		{
			Assert.IsNotNull(assetDatabaseAccess);
			this.assetDatabaseAccess = assetDatabaseAccess;
		}

		public void AddListener(Type assetType, TrackedAssetType.IAssetChangeNotifications listener)
		{
			if (!trackedAssetTypes.ContainsKey(assetType))
			{
				trackedAssetTypes[assetType] = new TrackedAssetType(assetType, assetDatabaseAccess.GetAssetsOfType(assetType));
			}

			trackedAssetTypes[assetType].AddListener(listener);
		}

		public void RemoveListener(Type assetType, TrackedAssetType.IAssetChangeNotifications listener)
		{
			Assert.IsTrue(trackedAssetTypes.ContainsKey(assetType), string.Format("Attempted to remove a listener for type {0} which is not currently registered. This is not allowed.", assetType));
			trackedAssetTypes[assetType].RemoveListener(listener);

			if (!trackedAssetTypes[assetType].HasListeners())
				trackedAssetTypes.Remove(assetType);
		}

		public void OnReimported(string[] assetPaths)
		{
			foreach (string assetPath in assetPaths)
			{
				Type assetType = assetDatabaseAccess.GetAssetType(assetPath);
				if (trackedAssetTypes.ContainsKey(assetType))
					trackedAssetTypes[assetType].OnReimported(assetPath);
			}
		}

		public void OnMoved(string[] assetSourcePaths, string[] assetTargetPaths)
		{
			for (int assetIndex = 0; assetIndex < assetSourcePaths.Length; assetIndex++)
			{
				string assetSourcePath = assetSourcePaths[assetIndex];
				string assetTargetPath = assetTargetPaths[assetIndex];

				Type assetType = assetDatabaseAccess.GetAssetType(assetTargetPath);
				if (trackedAssetTypes.ContainsKey(assetType))
					trackedAssetTypes[assetType].OnMoved(assetSourcePath, assetTargetPath);
			}
		}

		public void OnDeleted(string[] assetPaths)
		{
			foreach (string assetPath in assetPaths)
				foreach (TrackedAssetType trackedAssetGroup in trackedAssetTypes.Values)
					trackedAssetGroup.OnDeleted(assetPath);
		}
	}
}