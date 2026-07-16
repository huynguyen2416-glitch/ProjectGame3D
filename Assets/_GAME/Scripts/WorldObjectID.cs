using UnityEngine;


public class WorldObjectID : MonoBehaviour
{
    public string id;

    [ContextMenu("Generate New ID")]
    private void GenerateNewId()
    {
        id = System.Guid.NewGuid().ToString();
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private void Reset()
    {
        if (string.IsNullOrEmpty(id)) GenerateNewId();
    }
}