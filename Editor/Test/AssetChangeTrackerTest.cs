using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace AssetChangeTracker
{
    public static class AssetChangeTrackerTest
    {
        private class TestAssetType
        {
        }

        private class TestAssetType2
        {
        }

        private class TestListener : TrackedAssetType.IAssetChangeNotifications
        {
            public HashSet<string> AssetPaths = new HashSet<string>();
            public int OnExistsCallCount = 0;
            public int OnAddedCallCount = 0;
            public int OnChangedCallCount = 0;
            public int OnMovedCallCount = 0;
            public int OnDeletedCallCount = 0;

            public int CallbackCount = 0;

			public void OnExists(HashSet<string> assetPaths)
            {
                foreach (string assetPath in assetPaths)
                    AssetPaths.Add(assetPath);

                OnExistsCallCount++;
                CallbackCount++;
            }

			public void OnAdded(string assetPath)
            {
                AssetPaths.Add(assetPath);
                OnAddedCallCount++;
                CallbackCount++;
            }

			public void OnChanged(string assetPath)
            {
                OnChangedCallCount++;
                CallbackCount++;
            }

			public void OnMoved(string assetSourcePath, string assetTargetPath)
            {
                AssetPaths.Remove(assetSourcePath);
                AssetPaths.Add(assetTargetPath);
                OnMovedCallCount++;
                CallbackCount++;
            }

			public void OnDeleted(string assetPath)
            {
                AssetPaths.Remove(assetPath);
                OnDeletedCallCount++;
                CallbackCount++;
            }
        }

        private const string file1 = "file1";
        private const string file2 = "file2";
        private const string file3 = "file3";
        private const string file4 = "file4";
        private const string file5 = "file5";

        private class MockAssetDatabaseAccess : AssetChangeTracker.IAssetDatabaseAccess
        {
			public HashSet<string> GetAssetsOfType(Type assetType)
			{
                if (assetType == typeof(TestAssetType))
                    return new HashSet<string> { file1, file2 };
                else
                    return new HashSet<string> { file5 };
			}

			public Type GetAssetType(string assetPath)
			{
                if (assetPath != file5)
                    return typeof(TestAssetType);
                else
                    return typeof(TestAssetType2);
			}
        }

        [Test]
        public static void TestRegistrationAndUnRegistration()
        {
            AssetChangeTracker tracker = new AssetChangeTracker(new MockAssetDatabaseAccess());
            TestListener listener = new TestListener();

            // Subscribe listener
            tracker.AddListener(typeof(TestAssetType), listener);
            Assert.AreEqual(1, listener.OnExistsCallCount);
            Assert.AreEqual(1, listener.CallbackCount);

            // Reimport notifies the listener
            tracker.OnReimported(new string[] { file2 });
            Assert.AreEqual(2, listener.CallbackCount);

            // Unsubscribe listener
            tracker.RemoveListener(typeof(TestAssetType), listener);
            Assert.AreEqual(2, listener.CallbackCount);

            // Reimport notifies the listener
            tracker.OnReimported(new string[] { file2 });
            Assert.AreEqual(2, listener.CallbackCount);
        }

        [Test]
        public static void TestNotifications()
        {
            AssetChangeTracker tracker = new AssetChangeTracker(new MockAssetDatabaseAccess());
            TestListener listener = new TestListener();

            // Registration triggers OnExists()
            tracker.AddListener(typeof(TestAssetType), listener);
            Assert.AreEqual(1, listener.OnExistsCallCount);
            Assert.AreEqual(1, listener.CallbackCount);
            Assert.IsTrue(listener.AssetPaths.Contains(file1));
            Assert.IsTrue(listener.AssetPaths.Contains(file2));
            Assert.IsFalse(listener.AssetPaths.Contains(file3));
            Assert.IsFalse(listener.AssetPaths.Contains(file4));

            // Reimporting an already-existing file triggers OnChanged()
            tracker.OnReimported(new string[] { file2 });
            Assert.AreEqual(1, listener.OnChangedCallCount);
            Assert.AreEqual(2, listener.CallbackCount);
            Assert.IsTrue(listener.AssetPaths.Contains(file1));
            Assert.IsTrue(listener.AssetPaths.Contains(file2));
            Assert.IsFalse(listener.AssetPaths.Contains(file3));
            Assert.IsFalse(listener.AssetPaths.Contains(file4));

            // Reimporting a new file triggers OnAdded()
            tracker.OnReimported(new string[] { file3 });
            Assert.AreEqual(1, listener.OnAddedCallCount);
            Assert.AreEqual(3, listener.CallbackCount);
            Assert.IsTrue(listener.AssetPaths.Contains(file1));
            Assert.IsTrue(listener.AssetPaths.Contains(file2));
            Assert.IsTrue(listener.AssetPaths.Contains(file3));
            Assert.IsFalse(listener.AssetPaths.Contains(file4));

            // Deleting a file triggers OnDeleted()
            tracker.OnDeleted(new string[] { file2 });
            Assert.AreEqual(1, listener.OnDeletedCallCount);
            Assert.AreEqual(4, listener.CallbackCount);
            Assert.IsTrue(listener.AssetPaths.Contains(file1));
            Assert.IsFalse(listener.AssetPaths.Contains(file2));
            Assert.IsTrue(listener.AssetPaths.Contains(file3));
            Assert.IsFalse(listener.AssetPaths.Contains(file4));

            // Moving a file triggers OnMoved()
            tracker.OnMoved(new string[] { file1 }, new string[] { file4 });
            Assert.AreEqual(1, listener.OnMovedCallCount);
            Assert.AreEqual(5, listener.CallbackCount);
            Assert.IsFalse(listener.AssetPaths.Contains(file1));
            Assert.IsFalse(listener.AssetPaths.Contains(file2));
            Assert.IsTrue(listener.AssetPaths.Contains(file3));
            Assert.IsTrue(listener.AssetPaths.Contains(file4));
        }
 
 
        [Test]
        public static void TestMultipleListeners()
        {
            AssetChangeTracker tracker = new AssetChangeTracker(new MockAssetDatabaseAccess());
            TestListener listener1 = new TestListener();
            TestListener listener2 = new TestListener();
            TestListener listener3 = new TestListener();

            // Registration triggers OnExists(), with assets for the corresponding type
            tracker.AddListener(typeof(TestAssetType), listener1);
            Assert.AreEqual(2, listener1.AssetPaths.Count);
            Assert.AreEqual(1, listener1.OnExistsCallCount);
            tracker.AddListener(typeof(TestAssetType), listener2);
            Assert.AreEqual(2, listener2.AssetPaths.Count);
            Assert.AreEqual(1, listener2.OnExistsCallCount);
            tracker.AddListener(typeof(TestAssetType2), listener3);
            Assert.AreEqual(1, listener3.AssetPaths.Count);
            Assert.AreEqual(1, listener3.OnExistsCallCount);

            // Adding an asset of type TestAssetType notifies the first two listeners only
            tracker.OnReimported(new string[] { file3 });
            Assert.AreEqual(1, listener1.OnAddedCallCount);
            Assert.AreEqual(1, listener2.OnAddedCallCount);
            Assert.AreEqual(0, listener3.OnAddedCallCount);

            // Deleting an asset of type TestAssetType2 notifies the last listener only
            tracker.OnDeleted(new string[] { file5 });
            Assert.AreEqual(0, listener1.OnDeletedCallCount);
            Assert.AreEqual(0, listener2.OnDeletedCallCount);
            Assert.AreEqual(1, listener3.OnDeletedCallCount);
        }
   }
}