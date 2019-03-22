using System.Collections.Generic;

namespace AssetChangeTracker
{
	/// <summary>
	/// Asset database activity listener interface.
	/// This provides a minimal API that allows monitoring of changes to a set of assets of a given type.
	/// </summary>
	public interface IAssetChangeListener
	{
		/// <summary>
		/// Called when a listener is registered.
		/// Provides the entire set of assets of the matching type that is present in the AssetDatabase at the time of registration.
		/// </summary>
		void OnExists(HashSet<string> assetPaths);

		/// <summary>
		/// Called whenever a new asset is added to the AssetDatabase.
		/// </summary>
		void OnAdded(string assetPath);

		/// <summary>
		/// Called whenever the contents of an asset (which already is known) may have changed.
		/// </summary>
		void OnChanged(string assetPath);

		/// <summary>
		/// Called whenever an asset is moved from one location to another.
		/// </summary>
		void OnMoved(string assetSourcePath, string assetTargetPath);

		/// <summary>
		/// Called whenever an asset has been removed from the AssetDatabase.
		/// This is either because the Unity editor has been instructed to delete the file, or because an external program has removed it.
		/// </summary>
		void OnDeleted(string assetPath);
	}
}
