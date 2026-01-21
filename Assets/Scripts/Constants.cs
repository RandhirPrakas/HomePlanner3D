using TMPro;
using UnityEngine;

public static class Constants
{
    #region Tags

    public static string TAG_DOOR = "Door";
    public static string TAG_WINDOW = "Window";
    public static string TAG_GROUND = "Ground";
    public static string TAG_WALL = "Wall";
    public static string TAG_PLACABLES = "Placables";
    public static string TAG_ROOM = "Room";
    public static string TAG_RESIZE_HANDLE = "ResizeHandle";
    public static string TAG_WORLD_CANVAS = "WorldCanvas";
    #endregion

    #region Layers

    public static string LAYER_WALL = "Wall";
    public static string LAYER_FlOOR = "Floor";

    #endregion


    #region Addressables Addresses

    public static string PATH_DOOR_VISUALIZER = "Assets/Prefabs/Door/DoorVisualizer.prefab";
    public static string PATH_WINDOW_VISUALIZER = "Assets/Prefabs/Door/WindowVisualizer.prefab";
    public static string PATH_OBJECT_DISTANCE_LABEL_PREFAB = "Assets/Prefabs/ObjectDistanceLabel.prefab";

    public static string PATH_DEFAULT_LR_MATERIAL = "Assets/Prefabs/ProceduralMaterials/DefaultLRmaterial.mat";
    public static string PATH_VALID_PLACAMENT_MATERIAL = "Assets/Prefabs/ProceduralMaterials/ValidPlacement.mat";
    public static string PATH_INVALID_PLACAMENT_MATERIAL = "Assets/Prefabs/ProceduralMaterials/InvalidPlacement.mat";
    public static string PATH_DEFAULT_FLOOR_MATERIAL = "Assets/Prefabs/ProceduralMaterials/DefaultFloorMaterial.mat";
    public static string PATH_DEFAULT_QUAD_MATERIAL = "Assets/Prefabs/ProceduralMaterials/QuadMaterial.mat";
    public static string PATH_HIGHLIGHTED_WALL_MATERIAL = "Assets/Prefabs/ProceduralMaterials/HighlightedMaterialColor.mat";
    public static string PATH_OBJECT_DISTANCE_LR_MATERIAL = "Assets/Prefabs/ProceduralMaterials/ObjectDistanceLrMat.mat";

    public static string PATH_WALL_LENGTH_LABEL = "Assets/Prefabs/Wall/WallLengthLabel.prefab";

    public static string PATH_DEFAULT_DOOR = "Assets/Prefabs/Opening/Door/DefaultDoor.prefab";
    public static string PATH_DEFAULT_Window= "Assets/Prefabs/Opening/Door/DefaultWindow.prefab";
    #endregion

    public static Material DEFAULT_LINERENDERER_MATERIAL;
    public static Material DEFAULT_FLOOR_MATERIAL;
    public static Material DEFAULT_INVALID_PLACAMENT_MATERIAL;
    public static Material DEFAULT_VALID_PLACAMENT_MATERIAL;
    public static Material DEFAULT_QUAD_MATERIAL;
    public static Material DEFAULT_OBJECT_DISTANCE_MATERIAL;
    public static Material DEFAULT_HIGHLIGHTED_WALL_MATERIAL;

    public static TMP_Text DEFAULT_WALL_LENGTH_LABEL;

    public static GameObject DOOR_VISUALIZER;
    public static GameObject WINDOW_VISUALIZER;
    public static GameObject OBJECT_DISTANCE_LABEL_PREFAB;
    public static GameObject DEFAULT_DOOR;
    public static GameObject DEFAULT_WINDOW;
}
