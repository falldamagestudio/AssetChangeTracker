using System.Collections.Generic;

namespace AssetChangeTracker
{
    public interface IListener
    {
        void OnExists(HashSet<string> assetPaths);
        void OnAdded(string assetPath);
        void OnChanged(string assetPath);
        void OnMoved(string assetSourcePath, string assetTargetPath);
        void OnDeleted(string assetPath);
    }
}
