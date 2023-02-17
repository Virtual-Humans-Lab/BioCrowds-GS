using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Biocrowds.Core;
using System.Linq;

public class LevelEditorUIController : MonoBehaviour
{
    public SceneController sceneController;
    public EventSystem eventSystem;

    [SerializeField] private World simulationWorld;
    [SerializeField] private RuntimeOBJImporter objImporter;
    [SerializeField] private LevelExporter levelExporter;
    [SerializeField] private LevelImporter levelImporter;
    [SerializeField] private ManagerScript levelEditorManager;
    [SerializeField] private ObjectEditor objEditor;

    //--------------------------------------
    // Save/Load/Run
    [Header("Save/Load/Run")]
    public CustomPointerHandler saveSceneButton;
    public CustomPointerHandler loadSceneButton;
    public CustomPointerHandler runSceneButton;
    public RectTransform simulationRunningLabel;
    public CustomPointerHandler createAlternativesButton;
    public CustomPointerHandler removeAlternativeButton;

    public CustomPointerHandler confirmLoadSaveSceneButton;
    public CustomPointerHandler confirmLoadLoadAnywayButton;
    public CustomPointerHandler confirmLoadCancelButton;

    public CustomPointerHandler saveFailedContinueButton;
    public CustomPointerHandler loadFailedContinueButton;

    public CustomPointerHandler simulationRunningContinueButton;

    //--------------------------------------
    // Clear
    public CustomPointerHandler clearSceneButton;
    public CustomPointerHandler confirmClearButton;
    public CustomPointerHandler cancelClearButton;

    //--------------------------------------
    // Actions
    [Header("Actions")]
    public List<Toggle> actionToggles;

    //--------------------------------------
    // Create Objects/Presets
    [Header("Create Objects/Presets")]
    public CustomPointerHandler importOBJButton;
    public CustomPointerHandler clearOBJButton;
    public CustomPointerHandler confirmPresetButton;
    public CustomPointerHandler cancelPresetButton;

    public ToggleGroup presetToggleGroup;
    public List<Toggle> presetToggles;

    //--------------------------------------
    // Edit Objects
    [Header("Edit Objects")]
    public TMP_InputField agentNumberInputField;

    //--------------------------------------
    // Hints
    [Header("Terrain Options")]
    public RectTransform editTerrainSizePanel;
    public TMP_InputField terrainWidthInputField;
    public TMP_InputField terrainHeightInputField;
    //--------------------------------------
    // Panels
    [Header("Panels")]
    public RectTransform loadPresetPanel;
    public RectTransform confirmLoadPanel;
    public RectTransform confirmClearPanel;
    public RectTransform saveFailedPanel;
    public RectTransform loadFailedPanel;
    public RectTransform simulationRunningPanel;
    public RectTransform objectsPanel;
    public RectTransform editSpawnerPanel;
    public RectTransform editObstaclePanel;

    //--------------------------------------
    // Hints
    public RectTransform editObjectHint;
    public RectTransform editGoalHint;

    [Header("Cameras")]
    public List<Camera> cameras;
    public Camera currrentCamera;
    public Slider cameraSpeed;
    public ManagerScript ms;

    [Header("Test Level")]
    public SimulationScenario mainSimulationScenario;
    public List<SimulationScenario> simulationScenarios;
    public GameObject simulationScenarioPrefab;

    [HideInInspector]
    public bool isZoom { get; private set; }



    private void Awake()
    {
        levelEditorManager.world = sceneController.world;
        loadPresetPanel.gameObject.SetActive(false);
        confirmLoadPanel.gameObject.SetActive(false);
        confirmClearPanel.gameObject.SetActive(false);
        saveFailedPanel.gameObject.SetActive(false);
        loadFailedPanel.gameObject.SetActive(false);
        simulationRunningPanel.gameObject.SetActive(false);
        removeAlternativeButton.gameObject.SetActive(false);

        editObjectHint.gameObject.SetActive(false);
        editGoalHint.gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (sceneController == null)
            sceneController = FindObjectOfType<SceneController>();


        importOBJButton.OnPointerDownEvent += ImportOBJButton_OnPointerDownEvent;
        clearOBJButton.OnPointerDownEvent += ClearOBJButton_OnPointerDownEvent;
        runSceneButton.OnPointerDownEvent += RunSceneButton_OnPointerDownEvent;
        saveSceneButton.OnPointerDownEvent += SaveSceneButton_OnPointerDownEvent;
        loadSceneButton.OnPointerDownEvent += LoadSceneButton_OnPointerDownEvent;
        createAlternativesButton.OnPointerDownEvent += CreateAlternativesButton_OnPointerDownEvent;
        removeAlternativeButton.OnPointerDownEvent += RemoveAlternativeButton_OnPointerDownEvent;

        confirmLoadSaveSceneButton.OnPointerDownEvent += SaveSceneButton_OnPointerDownEvent;
        confirmLoadLoadAnywayButton.OnPointerDownEvent += ConfirmLoadLoadAnywayButton_OnPointerDownEvent;
        confirmLoadCancelButton.OnPointerDownEvent += ConfirmLoadCancelButton_OnPointerDownEvent;

        clearSceneButton.OnPointerDownEvent += ClearSceneButton_OnPointerDownEvent;
        confirmClearButton.OnPointerDownEvent += ConfirmClearButton_OnPointerDownEvent;
        cancelClearButton.OnPointerDownEvent += CancelClearButton_OnPointerDownEvent;

        saveFailedContinueButton.OnPointerDownEvent += SaveFailedContinueButton_OnPointerDownEvent;
        loadFailedContinueButton.OnPointerDownEvent += LoadFailedContinueButton_OnPointerDownEvent;

        simulationRunningContinueButton.OnPointerDownEvent += SimulationRunningContinueButton_OnPointerDownEvent;

        confirmPresetButton.OnPointerDownEvent += ConfirmPresetButton_OnPointerDownEvent;
        cancelPresetButton.OnPointerDownEvent += CancelPresetButton_OnPointerDownEvent;

        

        if (simulationScenarios.Count == 0)
        {
            if (mainSimulationScenario != null)
                simulationScenarios.Add(mainSimulationScenario);
            else
                simulationScenarios.Add(CreateNewAlternative());
        }

        if (!cameras.Contains(mainSimulationScenario.GetComponentInChildren<Camera>()))
            cameras.Add(mainSimulationScenario.GetComponentInChildren<Camera>());

        isZoom = false;

        terrainWidthInputField.onEndEdit.AddListener(OnEditTerrainWidth);
        terrainHeightInputField.onEndEdit.AddListener(OnEditTerrainHeight);
        foreach (SimulationScenario scenario in simulationScenarios)
        {
            scenario.world.UpateTerrainSize(new Vector3(30f, 600f, 30f));
        }

    }




    private void Update()
    {
        if (!levelEditorManager.HasInputFieldFocused() && !IsPopUpPanelOpen())
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                actionToggles[0].isOn = true;
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                actionToggles[1].isOn = true;
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                actionToggles[2].isOn = true;
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                actionToggles[3].isOn = true;
        }

        if (Input.GetKeyDown(KeyCode.Escape) && IsPopUpPanelOpen())
        {
            confirmLoadPanel.gameObject.SetActive(false);
            saveFailedPanel.gameObject.SetActive(false);
            confirmClearPanel.gameObject.SetActive(false);
            simulationRunningPanel.gameObject.SetActive(false);
            loadFailedPanel.gameObject.SetActive(false);
        }

        var mode = levelEditorManager.user.manipulatorOption;

        objectsPanel.gameObject.SetActive(mode == MouseScript.LevelManupulator.Create ? true : false);
        if ((mode == MouseScript.LevelManupulator.Edit || mode == MouseScript.LevelManupulator.Link) &&
            levelEditorManager.user.oe.GetSelected())
        {
            if (levelEditorManager.user.oe.GetSelectedItemType() == MouseScript.ItemList.Spawner)
            {
                editSpawnerPanel.gameObject.SetActive(true);
                editObstaclePanel.gameObject.SetActive(false);
            }
            else if (levelEditorManager.user.oe.GetSelectedItemType() == MouseScript.ItemList.Obstacle)
            {
                editSpawnerPanel.gameObject.SetActive(false);
                editObstaclePanel.gameObject.SetActive(true);
            }
        }
        else
        {
            editSpawnerPanel.gameObject.SetActive(false);
            editObstaclePanel.gameObject.SetActive(false);
        }


        if (mode == MouseScript.LevelManupulator.Edit && !objEditor.GetSelected())
        {
            editObjectHint.gameObject.SetActive(true);
        }
        else
        {
            editObjectHint.gameObject.SetActive(false);
        }

        editGoalHint.gameObject.SetActive(mode == MouseScript.LevelManupulator.Link ? true : false);



        importOBJButton.gameObject.SetActive(objImporter.loadedModels.Count == 0);
        clearOBJButton.gameObject.SetActive(objImporter.loadedModels.Count > 0);


        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.T))
        {
            editTerrainSizePanel.gameObject.SetActive(!editTerrainSizePanel.gameObject.activeSelf);
            terrainWidthInputField.text = mainSimulationScenario.terrain.terrainData.size.x.ToString();
            terrainHeightInputField.text = mainSimulationScenario.terrain.terrainData.size.z.ToString();
        }
    }



    private void ImportOBJButton_OnPointerDownEvent(PointerEventData eventData)
    {
        eventSystem.SetSelectedGameObject(null);
        loadPresetPanel.gameObject.SetActive(true);
    }

    private void ClearOBJButton_OnPointerDownEvent(PointerEventData obj)
    {
        eventSystem.SetSelectedGameObject(null);
        //objImporter.ClearLoadedModels();
        objImporter.ClearAllLoadedModels();
    }

    private void ConfirmPresetButton_OnPointerDownEvent(PointerEventData obj)
    {

        eventSystem.SetSelectedGameObject(null);
        var selected = presetToggleGroup.ActiveToggles().FirstOrDefault();
        objImporter.LoadPreset(presetToggles.IndexOf(selected));
        loadPresetPanel.gameObject.SetActive(false);
    }

    private void CancelPresetButton_OnPointerDownEvent(PointerEventData obj)
    {
        eventSystem.SetSelectedGameObject(null);
        loadPresetPanel.gameObject.SetActive(false);
    }


    private void LoadSceneButton_OnPointerDownEvent(PointerEventData obj)
    {
        eventSystem.SetSelectedGameObject(null);
        actionToggles[0].isOn = true;
        levelEditorManager.user.oe.UnselectObject();
        levelEditorManager.user.mo.isSelected = false;
        confirmLoadPanel.gameObject.SetActive(true);
    }

    private void SaveSceneButton_OnPointerDownEvent(PointerEventData obj)
    {
        eventSystem.SetSelectedGameObject(null);
        if (levelExporter.IsValidExport(sceneController.world, simulationScenarios))
        {
            levelExporter.ExportLevel(sceneController.world, objImporter, LevelExporter.ExportType.Download, simulationScenarios);
        }
        else
        {
            saveFailedPanel.gameObject.SetActive(true);
        }
    }

    private void RunSceneButton_OnPointerDownEvent(PointerEventData obj)
    {
        eventSystem.SetSelectedGameObject(null);

        if (levelExporter.IsValidExport(sceneController.world, simulationScenarios))
        {
            levelExporter.ExportLevel(sceneController.world, objImporter, LevelExporter.ExportType.RunScene, simulationScenarios);
            simulationRunningPanel.gameObject.SetActive(true);
            // runSceneButton.gameObject.SetActive(false);
            // simulationRunningLabel.gameObject.SetActive(true);
        }
        else
        {
            saveFailedPanel.gameObject.SetActive(true);
        }
    }

    private void CreateAlternativesButton_OnPointerDownEvent(PointerEventData obj)
    {
        if (simulationScenarios.Count == 4)
            throw new System.Exception("It is not possible to create more alternatives");

        GameObject newTestLevel = Instantiate(mainSimulationScenario.gameObject, new Vector3(simulationScenarios.Count * 1000, 0, 0), new Quaternion());
        newTestLevel.name = "SimulationScenario" + (simulationScenarios.Count + 1);
        simulationScenarios.Add(newTestLevel.GetComponent<SimulationScenario>());

        cameras.Add(newTestLevel.GetComponentInChildren<Camera>());
        ResizeCameras();

        removeAlternativeButton.gameObject.SetActive(true);
        if (simulationScenarios.Count == 4)
            createAlternativesButton.gameObject.SetActive(false);
    }

    private void RemoveAlternativeButton_OnPointerDownEvent(PointerEventData obj)
    {
        SimulationScenario removedScenario = simulationScenarios[simulationScenarios.Count - 1];
        cameras.Remove(cameras[cameras.Count - 1]);
        simulationScenarios.Remove(removedScenario);
        Destroy(removedScenario.gameObject);

        ResizeCameras();

        createAlternativesButton.gameObject.SetActive(true);
        if (simulationScenarios.Count == 1)
            removeAlternativeButton.gameObject.SetActive(false);
    }

    public void RemoveAllAlternatives()
    {
        foreach (SimulationScenario sm in simulationScenarios)
        {
            Destroy(sm.gameObject);
        }
        mainSimulationScenario = null;
        simulationScenarios.Clear();
        cameras.Clear();
    }

    public SimulationScenario CreateNewAlternative()
    {
        createAlternativesButton.gameObject.SetActive(true);

        GameObject newAlternative = Instantiate(simulationScenarioPrefab, new Vector3(simulationScenarios.Count * 1000, 0, 0), new Quaternion());
        newAlternative.name = "SimulationScenario" + (simulationScenarios.Count + 1);
        var simulationScenario = newAlternative.GetComponent<SimulationScenario>();
        simulationScenarios.Add(simulationScenario);

        var cam = simulationScenario.editorCamera.GetComponent<CameraScript>();
        cam.ms = ms;
        cam.cameraSpeed = cameraSpeed;

        cameras.Add(newAlternative.GetComponentInChildren<Camera>());
        ResizeCameras();

        removeAlternativeButton.gameObject.SetActive(true);
        if (simulationScenarios.Count == 4)
            createAlternativesButton.gameObject.SetActive(false);

        if (mainSimulationScenario == null)
            mainSimulationScenario = simulationScenario;

        return simulationScenario;
    }

    private void ConfirmLoadLoadAnywayButton_OnPointerDownEvent(PointerEventData obj)
    {
        levelImporter.ImportLevel(objImporter);
        confirmLoadPanel.gameObject.SetActive(false);
    }


    private void ConfirmLoadCancelButton_OnPointerDownEvent(PointerEventData obj)
    {
        confirmLoadPanel.gameObject.SetActive(false);
    }
    private void SaveFailedContinueButton_OnPointerDownEvent(PointerEventData obj)
    {
        saveFailedPanel.gameObject.SetActive(false);
    }

    private void LoadFailedContinueButton_OnPointerDownEvent(PointerEventData obj)
    {
        loadFailedPanel.gameObject.SetActive(false);
    }

    private void SimulationRunningContinueButton_OnPointerDownEvent(PointerEventData obj)
    {
        simulationRunningPanel.gameObject.SetActive(false);
    }

    public void SimulationFinishedRunning()
    {
        runSceneButton.gameObject.SetActive(true);
        simulationRunningLabel.gameObject.SetActive(false);
    }

    public void InvalidImport()
    {
        loadFailedPanel.gameObject.SetActive(true);
    }

    private void ClearSceneButton_OnPointerDownEvent(PointerEventData obj)
    {
        eventSystem.SetSelectedGameObject(null);
        confirmClearPanel.gameObject.SetActive(true);
    }
    private void ConfirmClearButton_OnPointerDownEvent(PointerEventData obj)
    {
        actionToggles[0].isOn = true;
        eventSystem.SetSelectedGameObject(null);
        objImporter.ClearAllLoadedModels();
        RemoveAllAlternatives();
        CreateNewAlternative();
        foreach (SimulationScenario scenario in simulationScenarios)
        {
            scenario.world.UpateTerrainSize(new Vector3(30f, 600f, 30f));
        }
        removeAlternativeButton.gameObject.SetActive(false);
        createAlternativesButton.gameObject.SetActive(true);
        confirmClearPanel.gameObject.SetActive(false);
    }
    private void CancelClearButton_OnPointerDownEvent(PointerEventData obj)
    {
        eventSystem.SetSelectedGameObject(null);
        confirmClearPanel.gameObject.SetActive(false);
    }

    public bool IsPopUpPanelOpen()
    {
        if (loadPresetPanel.gameObject.activeSelf || confirmLoadPanel.gameObject.activeSelf
            || saveFailedPanel.gameObject.activeSelf || simulationRunningPanel.gameObject.activeSelf
            || loadFailedPanel.gameObject.activeSelf || confirmClearPanel.gameObject.activeSelf)
            return true;
        return false;
    }

    public void ResizeCameras()
    {
        isZoom = false;

        foreach (Camera camera in cameras)
        {
            if (!camera.enabled)
                camera.enabled = true;

        }

        switch (cameras.Count)
        {
            case 1:
                ResizeCamera(cameras[0], 0.0f, 0.0f, 1.0f, 1.0f);
                break;
            case 2:
                ResizeCamera(cameras[0], 0.0f, 0.0f, 0.5f, 1.0f);
                ResizeCamera(cameras[1], 0.5f, 0.0f, 0.5f, 1.0f);
                break;
            case 3:
                ResizeCamera(cameras[0], 0.0f, 0.5f, 0.5f, 0.5f);
                ResizeCamera(cameras[1], 0.5f, 0.5f, 0.5f, 0.5f);
                ResizeCamera(cameras[2], 0.0f, 0.0f, 1.0f, 0.5f);
                break;
            case 4:
                ResizeCamera(cameras[0], 0.0f, 0.5f, 0.5f, 0.5f);
                ResizeCamera(cameras[1], 0.5f, 0.5f, 0.5f, 0.5f);
                ResizeCamera(cameras[2], 0.0f, 0.0f, 0.5f, 0.5f);
                ResizeCamera(cameras[3], 0.5f, 0.0f, 0.5f, 0.5f);
                break;
        }
    }

    private void ResizeCamera(Camera camera, float x, float y, float width, float height)
    {
        camera.rect = new Rect(x, y, width, height);
    }

    public void ZoomCamera(Camera camera)
    {
        isZoom = true;

        camera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);

        foreach (Camera cameraref in cameras)
        {
            if (camera != cameraref)
                cameraref.enabled = false;
        }
    }

    public void OnEditTerrainWidth(string text)
    {
        Vector3 terrainSize = mainSimulationScenario.terrain.terrainData.size;

        if (int.TryParse(text, out int value))
        {
            terrainSize.x = Mathf.Clamp(value, 10, 200);
            terrainWidthInputField.text = terrainSize.x.ToString();
        }

        else if (string.IsNullOrEmpty(text))
        {
            terrainSize.x = 10;
            terrainWidthInputField.text = terrainSize.x.ToString();
        }
        
        foreach (SimulationScenario scenario in simulationScenarios)
        {
            scenario.world.UpateTerrainSize(terrainSize);
        }
    }

    public void OnEditTerrainHeight(string text)
    {
        Vector3 terrainSize = mainSimulationScenario.terrain.terrainData.size;

        if (int.TryParse(text, out int value))
        {
            terrainSize.z = Mathf.Clamp(value, 10, 200);
            terrainHeightInputField.text = terrainSize.z.ToString();
        }

        else if (string.IsNullOrEmpty(text))
        {
            terrainSize.z = 10;
            terrainHeightInputField.text = terrainSize.z.ToString();
        }

        foreach (SimulationScenario scenario in simulationScenarios)
        {
            scenario.world.UpateTerrainSize(terrainSize);
        }
    }
}
