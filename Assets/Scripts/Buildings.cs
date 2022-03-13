using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buildings : MonoBehaviour
{
    private GameObject buildingToPlace, roadToPlace, initialToPlace;
    public CustomCursor customCursor, customCursorRoad, customCursorInitial;
    public Grid grid;
    public GameObject[,] tiles;
    public Camera camera;
    public GameObject initialShadow, roadShadow, buildingShadow;
    public AudioSource buildingPlaceSound, buildingRotateSound, deleteBuildingSound;
    public ParticleSystem buildingPlaceParticles;

    GameObject nearNode;
    bool isDeleting;
    public GameManager gameManager;//need to change naming convention for this to be somthing else rather than gameManager
    private GameObject selectedObjectToDelete;
    private Material[] originalMaterial;
    public Material[] deletingMaterial;
    private Vector3 buildPos;
    private BuildingCost buildingShadowScript, initialShadowScript;

    // Start is called before the first frame update
    void Start()
    {
        isDeleting = false;
        buildingShadowScript = buildingShadow.GetComponent<BuildingCost>();
        initialShadowScript = initialShadow.GetComponent<BuildingCost>();
    }

    // Update is called once per frame
    void Update()
    {
        if(grid == null)
            grid = GameObject.FindGameObjectWithTag("Grid").GetComponent<Grid>();

        if (tiles == null)
            tiles = grid.getGrid();


        if (initialShadow.activeInHierarchy)
        {
            nearNode = getNearestNode(customCursorInitial.gameObject);
            if (grid.areNodesFree(initialShadowScript.getGridWidth(), initialShadowScript.getGridHeight(), nearNode.GetComponent<Node>()))
            {
                buildPos = buildCentered(grid.getNodes(initialShadowScript.getGridWidth(), initialShadowScript.getGridHeight(), nearNode.GetComponent<Node>()));
                initialShadow.transform.position = new Vector3(buildPos.x, 2f, buildPos.z);
                //initialShadow.transform.position = new Vector3(nearNode.transform.position.x, 0.1f, nearNode.transform.position.z);
                if (Input.GetKeyDown(KeyCode.R))
                {
                    rotateAroundY(initialShadow, 90);
                    buildingRotateSound.Play();
                    initialShadowScript.changeWH();
                }
            }
            
        }

        if (roadShadow.activeInHierarchy)
        {
            nearNode = getNearestNode(customCursorRoad.gameObject);
            roadShadow.transform.position = new Vector3(nearNode.transform.position.x, 0.1f, nearNode.transform.position.z);
            if (Input.GetKeyDown(KeyCode.R))
            {
                rotateAroundY(roadShadow, 90);
                buildingRotateSound.Play();
            }
        }

        if (buildingShadow.activeInHierarchy)
        {
            nearNode = getNearestNode(customCursor.gameObject);
            if(grid.areNodesFree(buildingShadowScript.getGridWidth(), buildingShadowScript.getGridHeight(), nearNode.GetComponent<Node>()))
            {
                buildPos = buildCentered(grid.getNodes(buildingShadowScript.getGridWidth(), buildingShadowScript.getGridHeight(), nearNode.GetComponent<Node>()));
                buildingShadow.transform.position = new Vector3(buildPos.x, 2f, buildPos.z);
                if (Input.GetKeyDown(KeyCode.R))
                {
                    rotateAroundY(buildingShadow, 90);
                    buildingRotateSound.Play();
                }
            }
            
        }

        // Cancel construction with escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            buildingToPlace = null;
            customCursor.gameObject.SetActive(false);
            Cursor.visible = true;
            grid.setTilesActive(false);
            roadToPlace = null;
            customCursorRoad.gameObject.SetActive(false);
            initialToPlace = null;
            customCursorInitial.gameObject.SetActive(false);
            initialShadow.SetActive(false);
            roadShadow.SetActive(false);
            buildingShadow.SetActive(false);
            isDeleting = false;
        }

        // Create building
        if (Input.GetKeyDown(KeyCode.Mouse0) && buildingToPlace != null)
        {
            nearNode = getNearestNode(customCursor.gameObject);

            
            buildingPlaceSound.Play();
            buildingPlaceParticles.transform.position = new Vector3(nearNode.transform.position.x, 0, nearNode.transform.position.z);
            buildingPlaceParticles.Play();
            //nearNode.GetComponent<Node>().setOcupied(true);
            buildPos = buildCentered(grid.getNodes(buildingToPlace.GetComponent<BuildingCost>().getGridWidth(), buildingToPlace.GetComponent<BuildingCost>().getGridHeight(), nearNode.GetComponent<Node>()));
            grid.setNodesOccupied(buildingToPlace.GetComponent<BuildingCost>().getGridWidth(), buildingToPlace.GetComponent<BuildingCost>().getGridHeight(), nearNode.GetComponent<Node>());
            Instantiate(buildingToPlace, new Vector3(buildPos.x, 2f, buildPos.z), buildingShadow.transform.rotation);
            gameManager.BuyBuilding(buildingToPlace.GetComponent<BuildingCost>());
            buildingToPlace = null;
            customCursor.gameObject.SetActive(false);
            Cursor.visible = true;
            gameManager.SetNoBuilding(gameManager.GetNoBuildings() + 1);
            grid.setTilesActive(false);
            buildingShadow.SetActive(false);
        }

        // Create road
        if (Input.GetKeyDown(KeyCode.Mouse0) && roadToPlace != null)
        {
            nearNode = getNearestNode(customCursorRoad.gameObject);

            Instantiate(roadToPlace, new Vector3(nearNode.transform.position.x, 0, nearNode.transform.position.z), roadShadow.transform.rotation);
            buildingPlaceSound.Play();
            buildingPlaceParticles.transform.position = new Vector3(nearNode.transform.position.x, 0, nearNode.transform.position.z);
            buildingPlaceParticles.Play();
            nearNode.GetComponent<Node>().setOcupied(true);
            nearNode.GetComponent<Node>().setRoad(true);
            roadToPlace = null;
            customCursorRoad.gameObject.SetActive(false);
            Cursor.visible = true;
            grid.setTilesActive(false);
            grid.checkTilesRoads();
            roadShadow.SetActive(false);
        }

        // Create initial building
        if (Input.GetKeyDown(KeyCode.Mouse0) && initialToPlace != null)
        {
            nearNode = getNearestNode(customCursorInitial.gameObject);

            initialToPlace.GetComponent<BuildingCost>().setWH(initialShadowScript.getGridWidth(), initialShadowScript.getGridHeight());
            buildPos = buildCentered(grid.getNodes(initialToPlace.GetComponent<BuildingCost>().getGridWidth(), initialToPlace.GetComponent<BuildingCost>().getGridHeight(), nearNode.GetComponent<Node>()));
            grid.setNodesOccupied(initialToPlace.GetComponent<BuildingCost>().getGridWidth(), initialToPlace.GetComponent<BuildingCost>().getGridHeight(), nearNode.GetComponent<Node>());
            Instantiate(initialToPlace, new Vector3(buildPos.x, 0f, buildPos.z), initialShadow.transform.rotation);
            //Instantiate(initialToPlace, new Vector3(nearNode.transform.position.x, 0, nearNode.transform.position.z), initialShadow.transform.rotation);
            buildingPlaceSound.Play();
            buildingPlaceParticles.transform.position = new Vector3(nearNode.transform.position.x, 0, nearNode.transform.position.z);
            buildingPlaceParticles.Play();
            nearNode.GetComponent<Node>().setOcupied(true);
            nearNode.GetComponent<Node>().setInitial(true);
            initialToPlace = null;
            customCursorInitial.gameObject.SetActive(false);
            Cursor.visible = true;
            grid.setTilesActive(false);
            grid.checkTilesRoads();
            initialShadow.SetActive(false);
        }

        // Delete building or road
        if(selectedObjectToDelete != null)
        {
            selectedObjectToDelete.GetComponent<Renderer>().materials = originalMaterial;
            selectedObjectToDelete = null;
        }

        if (isDeleting) // If Im deleting
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);     // Throw a ray in the mouse position
            if (Physics.Raycast(ray, out RaycastHit hitInfo))           // If we get a hit with that ray
            {
                if (hitInfo.collider.gameObject != null && (hitInfo.collider.gameObject.tag == "Buildings" ||      // We see if the gameobject hitted
                                                            hitInfo.collider.tag == "Road"))                        // is a good one
                {
                    selectedObjectToDelete = hitInfo.collider.gameObject;
                    originalMaterial = selectedObjectToDelete.GetComponent<Renderer>().materials;
                    selectedObjectToDelete.GetComponent<Renderer>().materials = deletingMaterial;

                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        if (selectedObjectToDelete.tag == "Buildings")
                        {
                            gameManager.SetNoBuilding(gameManager.GetNoBuildings() - 1);
                            grid.setNodesUnoccupied(selectedObjectToDelete.GetComponent<BuildingCost>().getGridWidth(), selectedObjectToDelete.GetComponent<BuildingCost>().getGridHeight(), grid.getTile(selectedObjectToDelete.transform.position).GetComponent<Node>());
                        }
                        grid.getTile(selectedObjectToDelete.transform.position).GetComponent<Node>().setOcupied(false);
                        grid.checkTilesRoads();
                        Destroy(selectedObjectToDelete);
                        deleteBuildingSound.Play();
                        selectedObjectToDelete = null;
                    }
                }
            }
        }

    }

    /********************************************************************************************************************************/

    // Private functions
    // Return the gameobject node nearest to the gameobject given
    private GameObject getNearestNode(GameObject gObject)
    {
        GameObject res = null;
        float distanceNode = float.MaxValue;
        float dist = 0;
        
        for (int i = 0; i < tiles.GetLength(0); i++)
        {
            for (int j = 0; j < tiles.GetLength(1); j++)
            {
                if (!tiles[i, j].GetComponent<Node>().isOcupied() && tiles[i, j].activeInHierarchy)
                {
                    dist = Vector3.Distance(tiles[i, j].transform.position, gObject.transform.position);
                    if (dist < distanceNode)
                    {
                        distanceNode = dist;
                        res = tiles[i, j];
                    }
                }
            }
        }

        return res;
    }

    // Rotate a gameobject around the axis y
    private void rotateAroundY(GameObject go, float degrees)
    {
        go.transform.Rotate(0, degrees, 0);
    }

    // Build the building centered between the nodes
    private Vector3 buildCentered(List<GameObject> nodes)
    {
        float x = 0, z = 0;

        for(int i=0; i< nodes.Count; i++)
        {
            x += nodes[i].GetComponent<Node>().transform.position.x;
            z += nodes[i].GetComponent<Node>().transform.position.z;
        }

        x /= nodes.Count;
        z /= nodes.Count;

        Vector3 res = new Vector3(x, 0, z);
        return res;
    }

    /********************************************************************************************************************************/

    // Button event to create a building
    public void createBuilding(GameObject building)
    {
        grid.setTilesNearRoadActive(true);
        customCursor.gameObject.SetActive(true);
        Cursor.visible = false;
        buildingToPlace = building;
        isDeleting = false;
        buildingShadow.SetActive(true);
    }

    // Button event to create a road
    public void createRoad(GameObject road)
    {
        grid.setTilesAdyacentRoadActive(true);
        customCursorRoad.gameObject.SetActive(true);
        Cursor.visible = false;
        roadToPlace = road;
        isDeleting = false;
        roadShadow.SetActive(true);
    }

    // Button event to create initial building
    public void createInitial(GameObject building)
    {
        grid.setTilesActive(true);
        customCursorInitial.gameObject.SetActive(true);
        Cursor.visible = false;
        initialToPlace = building;
        isDeleting = false;
        initialShadow.SetActive(true);
    }

    // Button event to delete a building or a road
    public void deleteBuilding()
    {
        if (isDeleting)
            isDeleting = false;
        else
            isDeleting = true;
    }
}
