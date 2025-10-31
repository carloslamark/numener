using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SkinDatabase", menuName = "Game/Skin Database")]
public class SkinDatabase : ScriptableObject
{
    public List<SkinData> allSkins;
}