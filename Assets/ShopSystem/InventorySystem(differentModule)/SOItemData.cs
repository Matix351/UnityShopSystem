#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public abstract class SOItemData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private Sprite _image;
    [SerializeField] private int _maxStackAmmount = 64;
 
    public string Name => _name;
    public Sprite image => _image;
    public int maxStackAmmount => _maxStackAmmount;

#if UNITY_EDITOR
    public void SetID(int newId)
    {


        EditorApplication.delayCall += () =>
        {
            if (this != null)
            {
                EditorUtility.SetDirty(this);
                Debug.Log("saved");

                AssetDatabase.SaveAssetIfDirty(this);

            }
        };
    }
#endif

}


