using TMPro;
using UnityEngine;

public class SetupSubjectOnPlayerPrefs : MonoBehaviour
{
    public void GetSubjectFromDropdown(TMP_Dropdown inputField)
    {
        var selectedValue = inputField.options[inputField.value].text;

        PlaySetupController.SelectedTag = GetTagModelFromName(selectedValue);
    }

    public static void SetTagModelFromName(string tagName)
    {
        PlaySetupController.SelectedTag = GetTagModelFromName(tagName);

        // print($"Procurando nome {tagName}");

        // print($"Selecionado {PlaySetupController.SelectedTag.id}");
    }

    private static TagModel GetTagModelFromName(string tagName)
    {
        print($"Length da array {PlaySetupController.allTagsLoaded.Count}");
        foreach (var tag in PlaySetupController.allTagsLoaded)
        {
            if(tagName.Equals(tag.label))
                return tag;
        }

        return null;
    }
    
}
