using UnityEngine;

public class InitializeTags : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void  Start()
    {
        PlaySetupController playSetupController = new();
        await playSetupController.GetTagsAsync();
    }

}
