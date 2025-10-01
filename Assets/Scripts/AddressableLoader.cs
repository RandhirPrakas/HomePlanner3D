using System;
using UnityEngine.AddressableAssets;

public static class AddressableLoader
{

    public static async void LoadAndAssign<T>(string key, System.Action<T> onLoaded) where T : class
    {
        var handle = Addressables.LoadAssetAsync<T>(key);
        await handle.Task;

        if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
        {
            onLoaded?.Invoke(handle.Result);
        }
        else
        {
            Console.WriteLine($"[AddressablesLoader] Failed to load {typeof(T)} with key {key}");
        }
    }
}
