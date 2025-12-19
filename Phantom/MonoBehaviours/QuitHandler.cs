using DearImGuiInjection;
using UnityEngine;

namespace Phantom.MonoBehaviours;

internal class QuitHandler : MonoBehaviour
{
    private static void OnApplicationQuit()
    {
        ImGuiInjector.Dispose();
    }
}
