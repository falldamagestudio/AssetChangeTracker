using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Assertions;

namespace AssetChangeTracker
{
	/// <summary>
	/// AssetChangeTracker implementation.
	/// This class handles tracking of all asset database changes, performs any asset database API calls necessary,
	///   and routes changes to more specialized TrackedAssetType objects.
	/// </summary>
	public class AssetChangeTracker
	{
		/// <summary>
		/// Asset database API specification.
		/// Abstracting this out of the AssetChangeTracker makes it easy to test without a need for the AssetDatabase to have particular contents.
		/// </summary>
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

		public void AddListener(Type assetType, IAssetChangeListener listener)
		{
			if (!trackedAssetTypes.ContainsKey(assetType))
			{
				// This is the first listener for this particular asset type; create a tracker for the type
				trackedAssetTypes[assetType] = new TrackedAssetType(assetType, assetDatabaseAccess.GetAssetsOfType(assetType));
			}

			// Add listener to tracker
			trackedAssetTypes[assetType].AddListener(listener);
		}

		public void RemoveListener(Type assetType, IAssetChangeListener listener)
		{
			Assert.IsTrue(trackedAssetTypes.ContainsKey(assetType), string.Format("Attempted to remove a listener for type {0} which is not currently registered. This is not allowed.", assetType));

			// Remove listener from tracker
			if (!trackedAssetTypes[assetType].RemoveListener(listener))
			{
				// There are no more listeneres for this particular asset type; remove the tracker as well
				trackedAssetTypes.Remove(assetType);
			}
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