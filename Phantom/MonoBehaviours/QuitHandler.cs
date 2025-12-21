using DearImGuiInjection;
using MelonLoader;
using UnityEngine;

namespace Phantom.MonoBehaviours;

[RegisterTypeInIl2Cpp]
internal class QuitHandler : MonoBehaviour
{
    private static void OnApplicationQuit()
    {
        ImGuiInjector.Dispose();
    }
}
