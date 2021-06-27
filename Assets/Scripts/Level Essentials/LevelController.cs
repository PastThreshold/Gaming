using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

/// <summary>
/// Responsible for keeping track of enemies in scene and scene setup
/// </summary>
public class LevelController : MonoBehaviour
{
    public static List<Enemy> allEnemiesInScene;
    public static EnemyList allEnemiesInSceneList;
    public static List<Enemy> enemiesInDanger;
    public static EnemyList enemiesWithoutActiveBehavior;
    public static List<Protector> allProtectorsInScene;
    public static Projectile[] activeProjectiles;
    private int creationIndex = 0; // only used on start for activeProjectiles
    public static AltProjectile[] activeAlternateProjectiles;
    private int secondCreationIndex = 0;
    public static List<GravityWell> activeGravityWells;
    public static Extra.Box2D[] wallBounds;
    public static Extra.Box2D[] inBounds;

    LevelProgression levelProgression;
    [SerializeField] RoomData thisRoomData;
    public static RoomData roomData;
    [SerializeField] GameObject player;
    [SerializeField] Transform playerSpawn;
    [SerializeField] GameObject hud;
    [SerializeField] Transform[] boundsNotConverted;
    [SerializeField] Transform[] inBoundsNotConverted;
    static CameraFollow mainCamera;

    [Header("Enemy Spawning")]
    public static int totalEnemiesToSpawn;
    public static int enemiesLeftToSpawn;
    static int maxEnemiesAtOnce;
    static public int totalEnemies = 0;
    static public bool canSpawn;

    private void Awake()
    {
        roomData = thisRoomData;
        if (FindObjectOfType<Player>())
            FindObjectOfType<Player>().transform.position = playerSpawn.transform.position;
        else
            Instantiate(player, playerSpawn.transform.position, Quaternion.identity);

        if (!FindObjectOfType<HeadsUpDisplay>())
            Instantiate(hud, Vector3.zero, Quaternion.identity);

        allEnemiesInScene = new List<Enemy>();
        allEnemiesInSceneList = new EnemyList();
        enemiesInDanger = new List<Enemy>();
        enemiesWithoutActiveBehavior = new EnemyList();
        allProtectorsInScene = new List<Protector>();
        activeGravityWells = new List<GravityWell>();
    }

    private void Start()
    {
        levelProgression = FindObjectOfType<LevelProgression>();
        mainCamera = Camera.main.GetComponent<CameraFollow>();
        canSpawn = true;
        totalEnemies = 0;
        totalEnemiesToSpawn = roomData.GetTotalEnemies();
        enemiesLeftToSpawn = totalEnemiesToSpawn;
        maxEnemiesAtOnce = roomData.GetMaxEnemiesAtTime();
        GetProjectiles();

        wallBounds = GetWallBounds(boundsNotConverted);
        inBounds = GetWallBounds(inBoundsNotConverted);
        for (int i = 0; i < wallBounds.Length; i++)
        {
            Debug.DrawLine(wallBounds[i].topLeftBound, wallBounds[i].bottomRightBound, Color.cyan, 5f);
            Vector3 test1 = new Vector3(wallBounds[i].topLeftBound.x, 0, wallBounds[i].bottomRightBound.z);
            Debug.DrawLine(wallBounds[i].topLeftBound, test1, Color.cyan, 5f);
            Debug.DrawLine(wallBounds[i].bottomRightBound, test1, Color.cyan, 5f);
            Vector3 test2 = new Vector3(wallBounds[i].bottomRightBound.x, 0, wallBounds[i].topLeftBound.z);
            Debug.DrawLine(wallBounds[i].topLeftBound, test2, Color.cyan, 5f);
            Debug.DrawLine(wallBounds[i].bottomRightBound, test2, Color.cyan, 5f);
            Debug.DrawLine(test1, test2, Color.cyan, 5f);
        }

    }

    private void Update()
    {

        if (totalEnemies <= 0 && enemiesLeftToSpawn <= 0)
        {
            Door[] doors = FindObjectsOfType<Door>();
            foreach (Door door in doors)
            {
                door.SetActive();
            }
        }
    }

    public bool CanSpawn()
    {
        return canSpawn;
    }

    public void NextLevel()
    {
        int chance = Random.Range(0, 2);
        if (chance == 0)
        {
            if (SceneManager.GetActiveScene().buildIndex == 3)
                SceneManager.LoadScene(2);
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            if (SceneManager.GetActiveScene().buildIndex == 3)
                SceneManager.LoadScene(levelProgression.previousLevel + 1);
            else
                SceneManager.LoadScene(3);
        }
    }

    public static void AddEnemy(Enemy enem)
    {
        allEnemiesInScene.Add(enem);
        allEnemiesInSceneList.Add(enem);
        enemiesWithoutActiveBehavior.Add(enem);
        if (enem.GetComponent<Protector>())
            allProtectorsInScene.Add(enem.GetComponent<Protector>());
    }

    public static void RemoveEnemy(Enemy enem)
    {
        allEnemiesInScene.Remove(enem);
        allEnemiesInSceneList.Remove(enem);
        enemiesWithoutActiveBehavior.Remove(enem);
        if (enem.GetComponent<Protector>())
            allProtectorsInScene.Remove(enem.GetComponent<Protector>());
        totalEnemies--;
        mainCamera.Shake();
    }

    public static int GetEnemyCountOfType(int enemyNum)
    {
        switch (enemyNum)
        {
            case 0:
                return allEnemiesInSceneList.GetRobotList().Count;
            case 1:
                return allEnemiesInSceneList.GetAssassinList().Count;
            case 2:
                return allEnemiesInSceneList.GetWalkerList().Count;
            case 3:
                return allEnemiesInSceneList.GetProtectorList().Count;
            case 4:
                return allEnemiesInSceneList.GetRollerMineList().Count;
            case 5:
                return allEnemiesInSceneList.GetCommanderList().Count;
            default:
                break;
        }
        return 0;
    }

    public static void AddEnemyInDanger(Enemy enem)
    {
        enemiesInDanger.Add(enem);
    }

    public static void RemoveEnemyInDanger(Enemy enem)
    {
        enemiesInDanger.Remove(enem);
    }


    /// <summary>
    /// Called at start to add all the projectiles that are instantiated by the pools into a array
    /// meant to work with bullet time alt ability
    /// </summary>
    private void GetProjectiles()
    {
        ProjectilePool[] pools = FindObjectsOfType<ProjectilePool>();
        int totalLengthRequired = 0;
        int totalAltLengthRequired = 0;
        foreach (ProjectilePool pool in pools)
        {
            if (pool.notAltType)
                totalLengthRequired += pool.GetPool().Length;
            else
                totalAltLengthRequired += pool.GetAltPool().Length;
        }
        activeProjectiles = new Projectile[totalLengthRequired];
        activeAlternateProjectiles = new AltProjectile[totalAltLengthRequired];
        foreach (ProjectilePool pool in pools)
        {
            if (pool.notAltType)
                AddToProjectileArray(pool.GetPool());
            else
                AddToAlternateProjectileArray(pool.GetAltPool());
        }
    }

    /// <summary>
    /// Called for every projectile pool in the scene, creationIndex is the index of the activeProjectiles array
    /// The next function called continues where the last left off based on creationIndex
    /// </summary>
    private void AddToProjectileArray(Projectile[] array)
    {
        int index;
        for (index = 0; index < array.Length; index++)
        {
            activeProjectiles[creationIndex] = array[index];
            creationIndex++;
        }
    }

    private void AddToAlternateProjectileArray(AltProjectile[] array)
    {
        int index;
        for (index = 0; index < array.Length; index++)
        {
            activeAlternateProjectiles[secondCreationIndex] = array[index];
            secondCreationIndex++;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static void FreezeAllProjectiles()
    {
        foreach(Projectile proj in activeProjectiles)
        {
            if (proj.beingUsed)
                proj.StopMoving();
        }
    }

    public static void UnFreezeAllProjectiles()
    {
        foreach (Projectile proj in activeProjectiles)
        {
            if (proj.beingUsed)
                proj.ContinueMoving();
        }
    }

    private Extra.Box2D[] GetWallBounds(Transform[] bounds)
    {
        Extra.Box2D[] array = new Extra.Box2D[bounds.Length / 2];
        Extra.Box2D temp = null;
        for (int i = 0; i < bounds.Length; i += 2)
        {
            if (i + 1 >= bounds.Length)
                Debug.LogError("Missing Extra.Box2D Transform Point (In LevelController).");
            temp = new Extra.Box2D(bounds[i].position, bounds[i + 1].position);
            array[i / 2] = temp;
        }
        return array;
    }

    public static bool CheckTransformOutOfBounds(Vector3 objPos)
    {
        Vector3 tl;
        Vector3 br;
        for (int i = 0; i < wallBounds.Length; i++)
        {
            tl = wallBounds[i].topLeftBound;
            br = wallBounds[i].bottomRightBound;
            if (Extra.Within1DCoord(tl.x, br.x, objPos.x) && Extra.Within1DCoord(tl.z, br.z, objPos.z))
            {
                Extra.DrawBox(objPos, 0.5f, Color.red, 3f);
                return true;
            }
        }
        return false;
    }
    public static Extra.Box2D[] GetAllInBounds()
    {
        return inBounds;
    }

    public static Vector3 GetInBoundsPositions(Vector3 position)
    {
        Vector3 newPosition = position;
        return newPosition;
    }
}