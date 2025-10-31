using UnityEngine;

[CreateAssetMenu(fileName = "NewSkin", menuName = "Game/Skin Data Controller")]
public class SkinData : ScriptableObject
{
    [Tooltip("ID único para salvar. Ex: 'skin_default', 'skin_ninja'")]
    public string skinID;

    [Tooltip("Nome que aparece na loja")]
    public string skinName;

    [Tooltip("A imagem estática que aparece no botão da loja")]
    public Sprite shopIcon; // <-- Para mostrar na UI da loja

    [Tooltip("O 'Animator Controller' que contém as animações desta skin")]
    public RuntimeAnimatorController animatorController; // <-- A animação real

    public bool isUnlockedByDefault = false;
}