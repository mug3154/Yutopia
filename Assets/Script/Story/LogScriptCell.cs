using TMPro;
using UnityEngine;

public class LogScriptCell : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _NameText;
    [SerializeField] TextMeshProUGUI _ScriptText;

    public void SetText(string name, string script)
    {
        _NameText.text = name;
        _ScriptText.text = script;
    }
}
