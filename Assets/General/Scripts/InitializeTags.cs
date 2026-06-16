using UnityEngine;

public class InitializeTags : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool SelectByDefault = false;
    async void  Start()
    {
        PlaySetupController playSetupController = new();
        await playSetupController.GetTagsAsync();

        if (SelectByDefault)
        {
            PlaySetupController.SelectedTag = PlaySetupController.allTagsLoaded[4];
        }
    }

}
