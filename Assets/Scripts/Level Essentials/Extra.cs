using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The purpose of this class is for extra methods like sorting, setting vector values, and debug drawing. It cleans
/// up part of the code in other place where used frequently
/// </summary>
public class Extra : MonoBehaviour
{






    // TRANFORM SETTING METHODS --------------------------------------------------------------------
    public static Vector3 SetYToTransform(Vector3 vector, Transform tranform)
    {
        vector.y = tranform.position.y;
        return vector;
    }

    public static Vector3 SetYToZero(Vector3 vector)
    {
        vector.y = 0;
        return vector;
    }

    public static Vector3 SetYToValue(Vector3 vector, float value)
    {
        vector.y = value;
        return vector;
    }

    // VECTOR MATH METHODS ------------------------------------------------------------------------------------
    /// <summary> Subtracts parameter value from every component of vector </summary>
    public static Vector3 SubtractValueFromVector(Vector3 vector, float value)
    {
        vector = new Vector3(vector.x - value, vector.y - value, vector.z - value);
        return vector;
    }
    public static Vector3 SubtractValueFromVector(Vector3 vector, int value)
    {
        vector = new Vector3(vector.x - value, vector.y - value, vector.z - value);
        return vector;
    }
    /// <summary> Add parameter value to every component of vector </summary>
    public static Vector3 AddValueToVector(Vector3 vector, int value)
    {
        vector = new Vector3(vector.x + value, vector.y + value, vector.z + value);
        return vector;
    }
    public static Vector3 AddValueToVector(Vector3 vector, float value)
    {
        vector = new Vector3(vector.x + value, vector.y + value, vector.z + value);
        return vector;
    }

    /// <summary> Returns a vector with x and z components with random values between -value and value. </summary>
    public static Vector3 CreateRandomVector(float value)
    {
        return new Vector3(Random.Range(-value, value), 0, Random.Range(-value, value));
    }

    /// <summary> Returns a vector with random x and z components but has fixed magnitude between lowerBound and upperBound. 
    /// Creates a vector in any direction with the given parameters of magnitude </summary>
    public static Vector3 CreateRandomVectorWithMagnitude(float lowerBound, float upperBound)
    {
        return new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * Random.Range(lowerBound, upperBound);
    }

    /// <summary> Returns a vector with random x and z components with total magnitude of magnitude </summary>
    public static Vector3 CreateRandomVectorWithMagnitude(float magnitude)
    {
        return CreateRandomVectorWithMagnitude(magnitude, magnitude);
    }

    /// <summary> Returns a vector with x y z and components that are between lower and upperBound </summary>
    public static Vector3 CreateCompletlyRandomVector(float lowerBound, float upperBound)
    {
        return new Vector3(Random.Range(lowerBound, upperBound), Random.Range(lowerBound, upperBound), Random.Range(lowerBound, upperBound));
    }

    /// <summary> Return the squared distance between two vector3 without their y component </summary>
    public static float SquaredDistanceWithoutY(Vector3 vector1, Vector3 vector2)
    {
        Vector2 v1 = new Vector2(vector1.x, vector1.z);
        Vector2 v2 = new Vector2(vector2.x, vector2.z);
        return (v1 - v2).sqrMagnitude;
    }

    /// <summary> Return the distance between two vector3 without their y component </summary>
    public static float DistanceWithoutY(Vector3 vector1, Vector3 vector2)
    {
        Vector2 v1 = new Vector2(vector1.x, vector1.z);
        Vector2 v2 = new Vector2(vector2.x, vector2.z);
        return (v1 - v2).magnitude;
    }

    public static Quaternion RandomVectorYRotation(float minAngle, float maxAngle)
    {
        return Quaternion.Euler(0f, Random.Range(minAngle, maxAngle), 0f);
    }

    /// <summary> Returns whether value in inbetween num1 and num2 or not </summary>
    public static bool Within1DCoord(float num1, float num2, float value)
    {
        if (num1 > num2)
        {
            if (value <= num1 && value >= num2)
                return true;
        }
        else
        {
            if (value >= num1 && value <= num2)
                return true;
        }
        return false;
    }

    public static float GetCloserPoint(float num1, float num2, float value)
    {
        float totalOne = Mathf.Abs(num1 - value);
        float totalTwo = Mathf.Abs(num2 - value);
        if (totalOne < totalTwo)
            return num1;
        else
            return num2;
    }














    // WIDE USE HELPFUL METHODS --------------------------------------------------------------------------------------------

    /// <summary> Will create a random number from 0-100, takes in a percentage float value.
    /// If the roll is less than or equal it will return true (ex. rolls 23.4 percentage was 45 returns true) </summary>
    /// <param name="percentageTrue">Percent of time it should return true</param>
    /// <returns>Outcome of the roll</returns>
    public static bool RollChance(float percentageTrue)
    {
        float roll = Random.Range(0f, 100f);
        return roll <= percentageTrue;
    }

    public static bool RandomBoolean()
    {
        return Random.value > 0.5f;
    }

    /// <summary>
    /// Calculates the midpoint "center of mass" of a group of enemies
    /// </summary>
    /// <param name="enemies">list conataining enemies</param>
    /// <returns>Vector3 center of enemies</returns>
    public static Vector3 FindCenterOfListOfEnemies(List<Enemy> enemies)
    {
        float centroidOfX = 0;
        float centroidOfZ = 0;
        for (int i = 0; i < enemies.Count; i++)
        {
            centroidOfX += enemies[i].transform.position.x;
            centroidOfZ += enemies[i].transform.position.z;
        }
        centroidOfX /= enemies.Count;
        centroidOfZ /= enemies.Count;
        return new Vector3(centroidOfX, 0, centroidOfZ);
    }

    /// <summary> Returns a RaycastHit array that is sorted by distance Note: uses bubble sort </summary>
    public static RaycastHit[] SortHitsByDistance(RaycastHit[] hits)
    {
        RaycastHit[] sorted = new RaycastHit[hits.Length];
        for (int i = 0; i < hits.Length; i++)
        {
            sorted[i] = hits[i];
        }
        RaycastHit temp;
        int outerIndex, innerIndex;
        for (outerIndex = 0; outerIndex < hits.Length; outerIndex++)
        {
            for (innerIndex = 0; innerIndex < hits.Length - outerIndex - 1; innerIndex++)
            {
                if (sorted[innerIndex].distance > sorted[innerIndex + 1].distance)
                {
                    temp = sorted[innerIndex];
                    sorted[innerIndex] = sorted[innerIndex + 1];
                    sorted[innerIndex + 1] = temp;
                }
            }
        }
        return sorted;
    }

    /// <summary> Takes in a List of Enemy scripts and returns the closest Enemy script to the players current position </summary>
    public static Enemy ReturnClosestEnemyFromList(List<Enemy> list)
    {
        Enemy closest = null;
        float distance = Mathf.Infinity;
        float distanceBetween;
        for (int i = 0; i < list.Count; i++)
        {
            distanceBetween = (list[i].transform.position - GlobalClass.playerPos).sqrMagnitude;
            if (distanceBetween < distance)
            {
                closest = list[i];
                distance = distanceBetween;
            }
        }
        return closest;
    }

    /// <summary> Takes in a collider array and returns the root gameobjects of each. Will exclude duplicate gameobjects from multiple colliders </summary>
    public static List<GameObject> TestSingleTriggerArray(Collider[] colliders)
    {
        List<GameObject> gameObjects = new List<GameObject>();
        bool alreadyExists;
        foreach (Collider col in colliders)
        {

            GameObject rootObj = col.transform.root.gameObject;
            alreadyExists = false;
            foreach (GameObject obj in gameObjects)
            {
                if (rootObj == obj)
                {
                    alreadyExists = true;
                    break;
                }
            }

            if (!alreadyExists)
            {
                gameObjects.Add(rootObj);
            }
        }
        return gameObjects;
    }

    /// <summary> Calculates the bezier curve point based on the given time and vector values </summary>
    public static Vector3 CalculateQuadraticBezierCurve(float t, Vector3 start, Vector3 midpoint, Vector3 endPosition)
    {
        // return = (1-t)^2 * P0 + 2(1-t) * t * P1 + t^2 * P2
        //     uu * start +  u * 2  * t * midpoint + tt * endPosition

        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 resultPosition = uu * start;
        resultPosition += 2 * u * t * midpoint;
        resultPosition += tt * endPosition;
        return resultPosition;
    }

    /// <summary>
    /// Uses other methods within this class and physics overlap sphere to go from the colliders, to the root 
    /// GameObjects, to the Enemy scripts in a List. returns the enemy list HOW FREAKING NEAT!!!!!!!!! :DDDDDDDDDDDDDDDDDDDDD
    /// </summary>
    public static List<Enemy> GetEnemiesFromPhysicsOverLapSphere(Vector3 position, float radius, LayerMask layerMask)
    {
        Collider[] collidersHit = Physics.OverlapSphere(position, radius, layerMask);
        List<GameObject> gameObjectsHit = TestSingleTriggerArray(collidersHit);
        List<Enemy> enemiesHit = CovertListOfGameObjectsToListOfEnemy(gameObjectsHit);
        return enemiesHit;
    }












    // DATA TYPE CONVERSION METHODS ------------------------------------------------------------------------------------------

    /// <summary> Takes in a List of gameobjects and will return an EnemyList data type </summary>
    public static EnemyList CovertListOfGameObjectsToEnemyList(List<GameObject> list)
    {
        EnemyList resultList = new EnemyList();
        foreach (GameObject obj in list) // Sift out the enemies from the list of GameObjects
        {
            if (obj.CompareTag(GlobalClass.ENEMY_TAG)) // If its an enemy
            {
                resultList.Add(obj.GetComponent<Enemy>());
            }
        }
        return resultList;
    }

    /// <summary> Takes in List of gameobjects and will return a List of Enemy scripts </summary>
    public static List<Enemy> CovertListOfGameObjectsToListOfEnemy(List<GameObject> list)
    {
        List<Enemy> resultList = new List<Enemy>();
        foreach (GameObject obj in list) // Sift out the enemies from the list of GameObjects
        { 
            print("TAG: " + obj.tag);
            if (obj.CompareTag(GlobalClass.ENEMY_TAG)) // If its an enemy
            {
                print("Added");
                resultList.Add(obj.GetComponent<Enemy>());
            }
        }
        return resultList;
    }

    public static List<Enemy> ConvertFromRobotToEnemies(List<Robot> list)
    {
        List<Enemy> resultList = new List<Enemy>();
        for (int i = 0; i < list.Count; i++)
        {
            resultList.Add(list[i]);
        }
        return resultList;
    }


    /// <summary> Takes in a weapon type and converts to its "global" number that should remain static </summary>
    public static int ConvertFromWeaponTypeToGlobalNumber(BasicWeapon.WeaponType type)
    {
        switch (type)
        {
            case BasicWeapon.WeaponType.assaultRifle: return 0;
            case BasicWeapon.WeaponType.sniperRifle: return 1;
            case BasicWeapon.WeaponType.shotgun: return 2;
            case BasicWeapon.WeaponType.deagle: return 3;
            case BasicWeapon.WeaponType.stickyBombLauncher: return 4;
            case BasicWeapon.WeaponType.shredder: return 5;
            case BasicWeapon.WeaponType.laser: return 6;
            case BasicWeapon.WeaponType.chargeRifle: return 7;
            case BasicWeapon.WeaponType.rpg: return 8;
            default: return -1;
        }
    }

    /// <summary> Takes in a weapon type and converts to its "global" number that should remain static </summary>
    public static BasicWeapon.WeaponType ConvertFromGlobalNumberToWeaponType(int number)
    {
        switch (number)
        {
            case 0: return BasicWeapon.WeaponType.assaultRifle;
            case 1: return BasicWeapon.WeaponType.sniperRifle;
            case 2: return BasicWeapon.WeaponType.shotgun;
            case 3: return BasicWeapon.WeaponType.deagle;
            case 4: return BasicWeapon.WeaponType.stickyBombLauncher;
            case 5: return BasicWeapon.WeaponType.shredder;
            case 6: return BasicWeapon.WeaponType.laser;
            case 7: return BasicWeapon.WeaponType.chargeRifle;
            case 8: return BasicWeapon.WeaponType.rpg;
            default: return BasicWeapon.WeaponType.nullVal;
        }
    }










    // DRAWING/DEBUGGING METHODS -------------------------------------------------------------------------------------------------

    /// <summary> Draws a box with Debug.DrawLine </summary>
    public static void DrawBox(Vector3 center, float size, Color color, float time)
    {
        float radius = size / 2;
        Vector3 cornerBBL = SubtractValueFromVector(center, radius);
        Vector3 cornerBFL = new Vector3(center.x - radius, center.y - radius, center.z + radius);
        Vector3 cornerBBR = new Vector3(center.x + radius, center.y - radius, center.z - radius);
        Vector3 cornerBFR = new Vector3(center.x + radius, center.y - radius, center.z + radius);
        Vector3 cornerTBL = new Vector3(center.x - radius, center.y + radius, center.z - radius);
        Vector3 cornerTFL = new Vector3(center.x - radius, center.y + radius, center.z + radius);
        Vector3 cornerTBR = new Vector3(center.x + radius, center.y + radius, center.z - radius);
        Vector3 cornerTFR = AddValueToVector(center, radius);

        Debug.DrawLine(cornerBBL, cornerBFL, color, time); // Draws Bottom
        Debug.DrawLine(cornerBFL, cornerBFR, color, time);
        Debug.DrawLine(cornerBFR, cornerBBR, color, time);
        Debug.DrawLine(cornerBBR, cornerBBL, color, time);

        Debug.DrawLine(cornerBBL, cornerTBL, color, time); // Draws Verticals
        Debug.DrawLine(cornerBFL, cornerTFL, color, time);
        Debug.DrawLine(cornerBFR, cornerTFR, color, time);
        Debug.DrawLine(cornerBBR, cornerTBR, color, time);

        Debug.DrawLine(cornerTBL, cornerTFL, color, time); // Draws Top
        Debug.DrawLine(cornerTFL, cornerTFR, color, time);
        Debug.DrawLine(cornerTFR, cornerTBR, color, time);
        Debug.DrawLine(cornerTBR, cornerTBL, color, time);
    }

    /// <summary> Draws a box with radius 0.5, black color, for 5 seconds </summary>
    public static void DrawBox(Vector3 center)
    {
        DrawBox(center, 0.5f, Color.black, 5f);
    }

    /// <summary> Draws a filled in box with diagonal lines connecting corners </summary>
    public static void DrawBoxFilled(Vector3 center, float size, Color color, float time)
    {
        float radius = size / 2;
        Vector3 cornerBBL = SubtractValueFromVector(center, radius);
        Vector3 cornerBFL = new Vector3(center.x - radius, center.y - radius, center.z + radius);
        Vector3 cornerBBR = new Vector3(center.x + radius, center.y - radius, center.z - radius);
        Vector3 cornerBFR = new Vector3(center.x + radius, center.y - radius, center.z + radius);
        Vector3 cornerTBL = new Vector3(center.x - radius, center.y + radius, center.z - radius);
        Vector3 cornerTFL = new Vector3(center.x - radius, center.y + radius, center.z + radius);
        Vector3 cornerTBR = new Vector3(center.x + radius, center.y + radius, center.z - radius);
        Vector3 cornerTFR = AddValueToVector(center, radius);

        Debug.DrawLine(cornerBBL, cornerBFL, color, time); // Draws Bottom
        Debug.DrawLine(cornerBFL, cornerBFR, color, time);
        Debug.DrawLine(cornerBFR, cornerBBR, color, time);
        Debug.DrawLine(cornerBBR, cornerBBL, color, time);

        Debug.DrawLine(cornerBBL, cornerTBL, color, time); // Draws Verticals
        Debug.DrawLine(cornerBFL, cornerTFL, color, time);
        Debug.DrawLine(cornerBFR, cornerTFR, color, time);
        Debug.DrawLine(cornerBBR, cornerTBR, color, time);

        Debug.DrawLine(cornerTBL, cornerTFL, color, time); // Draws Top
        Debug.DrawLine(cornerTFL, cornerTFR, color, time);
        Debug.DrawLine(cornerTFR, cornerTBR, color, time);
        Debug.DrawLine(cornerTBR, cornerTBL, color, time);


        Debug.DrawLine(cornerBBL, cornerTFL, color, time);  // Draws Diagonals
        Debug.DrawLine(cornerTBL, cornerBFL, color, time);
        Debug.DrawLine(cornerBBL, cornerTBR, color, time);
        Debug.DrawLine(cornerTBL, cornerBBR, color, time);
        Debug.DrawLine(cornerBBR, cornerTFR, color, time);
        Debug.DrawLine(cornerTBR, cornerBFR, color, time);
        Debug.DrawLine(cornerBFL, cornerTFR, color, time);
        Debug.DrawLine(cornerTFL, cornerBFR, color, time);
        Debug.DrawLine(cornerBBL, cornerBFR, color, time);
        Debug.DrawLine(cornerBBR, cornerBFL, color, time);
        Debug.DrawLine(cornerTBL, cornerTFR, color, time);
        Debug.DrawLine(cornerTBR, cornerTFL, color, time);
    }

    /// <summary> Draws a thing around a center point almost like an inverted circle </summary>
    public static void DrawStrangeOutCenterThing(Vector3 center, float size, Color color, float time)
    {
        float radius = size / 2;
        Debug.DrawRay(center, Vector3.forward * radius, color, time);
        Debug.DrawRay(center, Vector3.left * radius, color, time);
        Debug.DrawRay(center, Vector3.right * radius, color, time);
        Debug.DrawRay(center, Vector3.back * radius, color, time);
        Debug.DrawRay(center, Vector3.up * radius, color, time);
        Debug.DrawRay(center, Vector3.down * radius, color, time);
    }

    /// <summary> Every while loop has this to check whether it has looped too many times, in which it will break to avert from an infinite loop </summary>
    public static bool CheckSafetyBreak(int safetyBreak, int breakValue)
    {
        return safetyBreak >= breakValue;
    }












    // DATA TYPES -------------------------------------------------------------------
    /// <summary>
    /// Holds two Vector3s with only x and z coordinates. They create a 2D box for tranform calculations
    /// </summary>
    public class Box2D
    {
        public Vector3 topLeftBound;
        public Vector3 bottomRightBound;

        public Box2D(Vector3 first, Vector3 second)
        {
            topLeftBound = first;
            bottomRightBound = second;
        }

        public bool IsInBounds(Vector3 position)
        {
            if (position.x >= topLeftBound.x && position.z <= topLeftBound.z &&
                position.x <= bottomRightBound.x && position.z >= bottomRightBound.z)
                return true;
            return false;
        }

        /// <summary> Returns the closest point in the bounds to the given position </summary>
        public Vector3 ClosestPointInBounds(Vector3 position)
        {
            if (IsInBounds(position)) return position;

            Vector3 closest;
            float coordNeeded;
            if (Within1DCoord(topLeftBound.x, bottomRightBound.x, position.x))
            {
                coordNeeded = GetCloserPoint(topLeftBound.z, bottomRightBound.z, position.z);
                closest = new Vector3(position.x, position.y, coordNeeded);
            }
            else if (Within1DCoord(topLeftBound.z, bottomRightBound.z, position.z))
            {
                coordNeeded = GetCloserPoint(topLeftBound.x, bottomRightBound.x, position.x);
                closest = new Vector3(coordNeeded, position.y, position.z);
            }
            else
            {
                coordNeeded = GetCloserPoint(topLeftBound.x, bottomRightBound.x, position.x);
                float coordNeededTwo = GetCloserPoint(topLeftBound.z, bottomRightBound.z, position.z);
                closest = new Vector3(coordNeeded, position.y, coordNeededTwo);
            }
            return closest;
        }
    }
}
