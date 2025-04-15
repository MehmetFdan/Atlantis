using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Hareket ayarları varlığı oluşturmak için editor menü eklentisi
/// </summary>
public class CreateMovementSettings
{
    [MenuItem("Tools/Create Default Movement Settings")]
    public static void CreateDefaultMovementSettings()
    {
        // Resources klasörünü kontrol et, yoksa oluştur
        string resourcesPath = "Assets/Resources";
        if (!Directory.Exists(resourcesPath))
        {
            Directory.CreateDirectory(resourcesPath);
        }
        
        // Hareket ayarları varlığı oluştur
        MovementSettings settings = ScriptableObject.CreateInstance<MovementSettings>();
        
        // Varlığı kaydet
        string assetPath = "Assets/Resources/DefaultMovementSettings.asset";
        AssetDatabase.CreateAsset(settings, assetPath);
        AssetDatabase.SaveAssets();
        
        // Varlığı seç
        Selection.activeObject = settings;
        
        Debug.Log("DefaultMovementSettings created at " + assetPath);
    }
} 