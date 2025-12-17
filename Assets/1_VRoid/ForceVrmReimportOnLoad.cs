using UnityEngine;
using UnityEditor;
using System.IO;

// エディタ拡張として、ロード時に実行
[InitializeOnLoad]
public class ForceVrmReimportOnLoad
{
    static ForceVrmReimportOnLoad()
    {
        // Unityエディタが立ち上がり、コンパイルが終わったタイミングで実行
        EditorApplication.delayCall += CheckAndReimportVrms;
    }

    static void CheckAndReimportVrms()
    {
        // 1_VRoidフォルダ内の全VRMファイルを探す
        // ※フォルダ名は実際のパスに合わせて調整してください
        string[] guids = AssetDatabase.FindAssets("t:DefaultAsset", new[] { "Assets/1_VRoid" });

        bool reimported = false;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // 拡張子が .vrm なのに DefaultAsset (白い紙アイコン) として認識されている場合
            if (path.EndsWith(".vrm", System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"[AutoFix] 未認識のVRMファイルを検出しました。再インポートします: {path}");
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                reimported = true;
            }
        }

        if (reimported)
        {
            Debug.Log("[AutoFix] VRMファイルの再インポートが完了しました。");
        }
    }
}