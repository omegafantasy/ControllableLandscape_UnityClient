## Controllable Procedural Generation of Landscapes - Unity Client

This is the Unity client repo for the ACM MM24 paper "Controllable Procedural Generation of Landscapes".

#### Environment

`Unity 2022.3.14f1c1` (not tested with other versions, but a 2022.xx version should be OK)

#### Structure

The landscape models used are located in `Assets/AssetStoreOriginals/APAC_Garden/Art`, which are mostly from third-parties.

`Assets/SceneGen` is the main directory. The necessary files for conversion (which are the outputs of the main generation process, see the other repo) should be put into `Assets/SceneGen/Inputs`. The scripts for conversion are put in `Assets/SceneGen/Scripts`. Other directories are mainly used for the terrain textures.

By default, `Assets/SceneGen/Image_Landscape`, `Assets/SceneGen/Scene_Landscape`, and `Assets/SceneGen/Mat_Landscape` are used for storing the outputs.

#### Usage

1. Put all outputs of the main generation process into `Assets/SceneGen/Inputs`. Alternatively, you can try using the files in `Assets/SceneGen/SampleInputs`. Just ensure that `height_map_xx.png`, `label_map_xx.png`, and `scene_xx.json` are all put into the directory.
2. Open Unity and open any saved scene. By default, you can just open `Assets/SceneGen/scene.unity`.
3. From the menu bar, select "Tools"->"Landscape Generator", and then click "Generate".
4. Wait a few seconds (or several minutes if there are many scenes).
5. By default, the generated scenes are stored in `Assets/SceneGen/Scene_Landscape`. Some images of these scenes are rendered and saved in `Assets/SceneGen/Image_Landscape` (you can disable it by modifying `Assets/SceneGen/Scripts/SceneRenderer.cs`).

#### Others

The scripts for conversion are generally straightforward. You can modify them if necessary.

If you encounter some bugs, please create a issue.