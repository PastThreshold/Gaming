using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The purpose of this class is for extra methods like sorting, setting vector values, and debug drawing. It cleans
/// up part of the code in other place where used frequently
/// </summary>
public class Extra
{
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

    /// <summary>
    /// Returns a vector with x and z components with random values between -value and value.
    /// </summary>
    public static Vector3 CreateRandomVector(float value)
    {
        return new Vector3(Random.Range(-value, value), 0, Random.Range(-value, value));
    }

    /// <summary>
    /// Returns a vector with random x and z components but has fixed magnitude between lowerBound and upperBound
    /// </summary>
    public static Vector3 CreateRandomVectorWithMagnitude(float lowerBound, float upperBound)
    {
        return new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * Random.Range(lowerBound, upperBound);
    }

    public static Vector3 CreateRandomVectorWithMagnitude(float bound)
    {
        return CreateRandomVectorWithMagnitude(bound, bound);
    }

    /// <summary>
    /// Returns a vector with x y z and components that are between lower and upperBound
    /// </summary>
    public static Vector3 CreateCompletlyRandomVector(float lowerBound, float upperBound)
    {
        return new Vector3(Random.Range(lowerBound, upperBound), Random.Range(lowerBound, upperBound), Random.Range(lowerBound, upperBound));
    }

    /// <summary>
    /// Return the squared distance between two vector3 without their y component
    /// </summary>
    public static float SquaredDistanceWithoutY(Vector3 vector1, Vector3 vector2)
    {
        Vector2 v1 = new Vector2(vector1.x, vector1.z);
        Vector2 v2 = new Vector2(vector2.x, vector2.z);
        return (v1 - v2).sqrMagnitude;
    }

    /// <summary>
    /// Return the distance between two vector3 without their y component
    /// </summary>
    public static float DistanceWithoutY(Vector3 vector1, Vector3 vector2)
    {
        Vector2 v1 = new Vector2(vector1.x, vector1.z);
        Vector2 v2 = new Vector2(vector2.x, vector2.z);
        return (v1 - v2).magnitude;
    }

    /// <summary>
    /// Will create a random number from 0-100, takes in a percentage float value.
    /// If the roll is less than or equal it will return true (ex. rolls 23.4 percentage was 45 returns true)
    /// </summary>
    /// <param name="percentageTrue">Percent of time it should return true</param>
    /// <returns>Outcome of the roll</returns>
    public static bool RollChance(float percentageTrue)
    {
        float roll = Random.Range(0f, 100f);
        return roll <= percentageTrue;
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

    /// <summary>
    /// Returns a RaycastHit array that is sorted by distance
    /// Note: uses bubble sort
    /// </summary>
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

    /// <summary>
    /// Takes in a collider array and returns the root gameobjects of each. Will exclude duplicate gameobjects from multiple colliders
    /// </summary>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static EnemyList ListOfGameObjectsToEnemyList(List<GameObject> list)
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

    public static List<Enemy> ListOfGameObjectsToListOfEnemies(List<GameObject> list)
    {
        List<Enemy> resultList = new List<Enemy>();
        foreach (GameObject obj in list) // Sift out the enemies from the list of GameObjects
        {
            if (obj.CompareTag(GlobalClass.ENEMY_TAG)) // If its an enemy
            {
                resultList.Add(obj.GetComponent<Enemy>());
            }
        }
        return resultList;
    }

    /// <summary>
    /// Uses other methods within this class and physics overlap sphere to go from the colliders, to the root 
    /// gameobjects, to the enemy scripts in a list. returns the enemy list HOW FREAKING NEAT!!!!!!!!! :DDDDDDDDDDDDDDDDDDDDD
    /// </summary>
    public static List<Enemy> GetEnemiesFromPhysicsOverLapSphere(Vector3 position, float radius, LayerMask layerMask)
    {
        Collider[] collidersHit = Physics.OverlapSphere(position, radius, layerMask);
        List<GameObject> gameObjectsHit = TestSingleTriggerArray(collidersHit);
        List<Enemy> enemiesHit = ListOfGameObjectsToListOfEnemies(gameObjectsHit);
        return enemiesHit;
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

    /// <summary>
    /// Calculates the bezier curve point based on the given time and vector values
    /// </summary>
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


    // DRAWING/DEBUGGING METHODS -------------------------------------------------------------------------------------------------

    /// <summary>
    /// Draws a box with Debug.DrawLine
    /// </summary>
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

    public static void DrawBox(Vector3 center)
    {
        DrawBox(center, 0.5f, Color.black, 5f);
    }

    /// <summary>
    /// Draws a filled in box with diagonal lines connecting corners
    /// </summary>
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

    /// <summary>
    /// Draws a thing around a center point almost like an inverted circle
    /// </summary>
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

    /// <summary>
    /// Every while loop has this to check whether it has looped too many times, in which it will break 
    /// to avert from an infinite loop
    /// </summary>
    public static bool CheckSafetyBreak(int safetyBreak, int breakValue)
    {
        return safetyBreak >= breakValue;
    }
}
