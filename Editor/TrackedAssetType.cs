using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Assertions;

namespace AssetChangeTracker
{
	public class TrackedAssetType
	{
		public interface IAssetChangeNotifications
		{
			void OnExists(HashSet<string> assetPaths);
			void OnAdded(string assetPath);
			void OnChanged(string assetPath);
			void OnMoved(string assetSourcePath, string assetTargetPath);
			void OnDeleted(string assetPath);
		}

		private HashSet<string> AssetPaths = new HashSet<string>();
		private HashSet<IAssetChangeNotifications> Listeners = new HashSet<IAssetChangeNotifications>();

		public TrackedAssetType(Type assetType)
		{
			string[] assetGuids = AssetDatabase.FindAssets(string.Format("t:{0}", assetType));
			foreach (string assetGuid in assetGuids)
				AssetPaths.Add(AssetDatabase.GUIDToAssetPath(assetGuid));
		}

		public void AddListener(IAssetChangeNotifications listener)
		{
			Assert.IsFalse(Listeners.Contains(listener), "Attempted to add the same listener twice for the same type. This is not allowed.");
			Listeners.Add(listener);

			listener.OnExists(AssetPaths);
		}

		public void RemoveListener(IAssetChangeNotifications listener)
		{
			Assert.IsTrue(Listeners.Contains(listener), "Attempted to remove a listener for type which is not currently registered. This is not allowed.");
			Listeners.Remove(listener);
		}

		public bool HasListeners()
		{
			return Listeners.Count > 0;
		}

		public void OnReimported(string assetPath)
		{
			if (!AssetPaths.Contains(assetPath))
			{
				AssetPaths.Add(assetPath);

				foreach (IAssetChangeNotifications listener in Listeners)
					listener.OnAdded(assetPath);
			}
			else
			{
				foreach (IAssetChangeNotifications listener in Listeners)
					listener.OnChanged(assetPath);
			}
		}

		public void OnMoved(string assetSourcePath, string assetTargetPath)
		{
			AssetPaths.Remove(assetSourcePath);
			AssetPaths.Add(assetTargetPath);

			foreach (IAssetChangeNotifications listener in Listeners)
				listener.OnMoved(assetSourcePath, assetTargetPath);
		}

		public void OnDeleted(string assetPath)
		{
			// OnDeleted will receive callbacks for all assets, regardless of their type
			// Therefore it must ignore delete operations on assets which it isn't already tracking
			if (AssetPaths.Contains(assetPath))
			{
				AssetPaths.Remove(assetPath);

				foreach (IAssetChangeNotifications listener in Listeners)
					listener.OnDeleted(assetPath);
			}
		}
	}
}