using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace AssetChangeTracker
{
	/// <summary>
	/// Handles asset database notifications for an individual asset type.
	/// </summary>
	public class TrackedAssetType
	{
		private HashSet<string> AssetPaths = new HashSet<string>();
		private HashSet<IListener> Listeners = new HashSet<IListener>();

		/// <param name="assetType">Type of asset to track.</param>
		/// <param name="assetPaths">Set of paths to assets that exist at the time the tracker is created.</param>
		public TrackedAssetType(Type assetType, HashSet<string> assetPaths)
		{
			AssetPaths = assetPaths;
		}

		/// <summary>
		/// Adds listener for this asset type.
		/// </summary>
		public void AddListener(IListener listener)
		{
			Assert.IsFalse(Listeners.Contains(listener), "Attempted to add the same listener twice for the same type. This is not allowed.");
			Listeners.Add(listener);

			listener.OnExists(AssetPaths);
		}

		/// <summary>
		/// Removes listener for this asset type.
		/// Returns true if there still are other listeners present, or false if there are none.
		/// </summary>
		public bool RemoveListener(IListener listener)
		{
			Assert.IsTrue(Listeners.Contains(listener), "Attempted to remove a listener for type which is not currently registered. This is not allowed.");
			Listeners.Remove(listener);
			return Listeners.Count > 0;
		}

		public void OnReimported(string assetPath)
		{
			// Translate Reimported to either Added or Changed,
			//   depending on whether the asset already is known to the tracker
			if (!AssetPaths.Contains(assetPath))
			{
				AssetPaths.Add(assetPath);

				foreach (IListener listener in Listeners)
					listener.OnAdded(assetPath);
			}
			else
			{
				foreach (IListener listener in Listeners)
					listener.OnChanged(assetPath);
			}
		}

		public void OnMoved(string assetSourcePath, string assetTargetPath)
		{
			AssetPaths.Remove(assetSourcePath);
			AssetPaths.Add(assetTargetPath);

			foreach (IListener listener in Listeners)
				listener.OnMoved(assetSourcePath, assetTargetPath);
		}

		public void OnDeleted(string assetPath)
		{
			// TrackedAssetType.OnDeleted() will receive callbacks for all assets, regardless of their type
			// Therefore it must ignore delete operations on assets which it isn't currently tracking
			if (AssetPaths.Contains(assetPath))
			{
				AssetPaths.Remove(assetPath);

				foreach (IListener listener in Listeners)
					listener.OnDeleted(assetPath);
			}
		}
	}
}