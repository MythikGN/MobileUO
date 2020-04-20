using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
class BuildSettings
{
    static BuildSettings()
    {
//WebGL
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Asm;
        PlayerSettings.WebGL.threadsSupport = false;
        PlayerSettings.WebGL.memorySize = 2032;//1024;
    }
}