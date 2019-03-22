# AssetChangeTracker

Simple interface for monitoring Unity's Asset Database for changes

## Purpose

Sometimes you want to write a bit of editor logic that wants to display information about a set of assets, but you do not want to query the AssetDatabase, and load and inspect the assets over and over. This class makes it simple to keep track of the assets, and only re-inspect them when necessary.

For example: Perhaps you want to make an editor window, which shows a list of all the textures in the folder `Assets/Textures` folder that are not a power-of-2 in size?
If the editor window queries the database all the time, it will be slow. Instead, the editor window can use the AssetChangeTracker interface to listen to asset changes, and only inspect an individual texture when it is added or re-imported.

## How to use

- Add this project to your code.
- Implement the `AssetChangeTracker.IListener` interface in one of your classes.
- Register an instance of your class using the `GlobalAssetChangeTracker.AddListener()` interface.

```
public class ExampleListener : AssetChangeTracker.IListener
{
    public void OnExists(HashSet<string> assetPaths)
    {
        Debug.Log("Existing assets at initialization time:");
        foreach (string assetPath in assetPaths)
            Debug.Log("---- " + assetPath);
    }

    public void OnAdded(string assetPath)
    {
        Debug.Log("New asset added: " + assetPath);
    }

    public void OnChanged(string assetPath)
    {
        Debug.Log("Existing asset changed: " + assetPath);
    }

    public void OnMoved(string assetSourcePath, string assetTargetPath)
    {
        Debug.Log("Existing asset moved from " + assetSourcePath + " to " + assetTargetPath);
    }

    public void OnDeleted(string assetPath)
    {
        Debug.Log("Existing asset deleted: " + assetPath);
    }
}


...

ExampleListener myListener = new ExampleListener;

GlobalAssetChangeTracker.addListener(typeof(MyScriptableObject), exampleListener);
```

There is an example project available at https://github.com/falldamagestudio/AssetChangeTracker-Example.
