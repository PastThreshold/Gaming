using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorController : MonoBehaviour
{
    public enum Behavior
    {
        none,
        groupProt,
        robotFSquad,
        assassinTriangle,
        mountedWalker,
        overProtected //two protectors on one and a commander boosting the one
    }

    private class EnemyInPosition
    {
        public Enemy enemy;
        public bool inPosition;

        public EnemyInPosition(Enemy enemy)
        {
            this.enemy = enemy;
            inPosition = false;
        }
    }

    /// <summary>
    /// Component that handles a specific behavior or formation that was intiated by the behaviorcontroller
    /// </summary>
    public class BehaviorHandler : MonoBehaviour
    {
        Behavior behavior;
        EnemyList enemies;
        EnemyInPosition[] allEnemies;
        bool started = false;
        bool initiationPhase;
        bool secondIntiation;
        bool updatePhase;
        bool endPhase;
        int commanderNum = 1;
        int protectorNum = 2;
        int robotNum = 5;
        int walkerNum = 0;
        int totalEnemyNum = 8;

        /// <summary>
        /// Every behavior will have its own variables, all behaviors have their specific variables above them
        /// This is organized by Behaviors, its variables, its methods.
        /// </summary>

        // ROBOT F SQUAD ================================================================================================

        float distBetweenRobots = 1.25f;
        Vector3 middleVector;

        /// <summary>
        /// Call a commander to be behind, two protectors to cast a large shield, and 
        /// around 5 robots to line up beside each other
        /// </summary>
        private void StartRobotFSquad(List<Enemy> robots)
        {
            commanderNum = 1;
            protectorNum = 2;
            robotNum = Random.Range(5, Mathf.Clamp(robots.Count, 5, 8));
            
            totalEnemyNum = robotNum + commanderNum + protectorNum;
            // Below takes in the enemies from the levelcontroller and puts them into the allEnemies array and the enemies EnemyList
            allEnemies = new EnemyInPosition[totalEnemyNum];
            List<Robot> origRobotList = LevelController.enemiesWithoutActiveBehavior.GetRobotList();
            List<Protector> origProtList = LevelController.enemiesWithoutActiveBehavior.GetProtectorList();
            List<Commander> origCommList = LevelController.enemiesWithoutActiveBehavior.GetCommanderList();
            int listRemoveIndex; // Removes from the top for more efficiency
            int allEnemIndex = 0;
            for (listRemoveIndex = robotNum - 1; listRemoveIndex >= 0; listRemoveIndex--)
            {
                enemies.Add(robots[listRemoveIndex]);
                allEnemies[allEnemIndex] = new EnemyInPosition(robots[listRemoveIndex]);
                origRobotList.RemoveAt(listRemoveIndex);
                allEnemIndex++;
            }
            for (listRemoveIndex = protectorNum - 1; listRemoveIndex >= 0; listRemoveIndex--)
            {
                enemies.Add(origProtList[listRemoveIndex]);
                allEnemies[allEnemIndex] = new EnemyInPosition(origProtList[listRemoveIndex]);
                origProtList.RemoveAt(listRemoveIndex);
                allEnemIndex++;
            }
            for (listRemoveIndex = commanderNum - 1; listRemoveIndex >= 0; listRemoveIndex--)
            {
                enemies.Add(origCommList[listRemoveIndex]);
                allEnemies[allEnemIndex] = new EnemyInPosition(origCommList[listRemoveIndex]);
                origCommList.RemoveAt(listRemoveIndex);
                allEnemIndex++;
            }

            // Below Sets all the positions for the enemies, the formation looks kinda like .-----.
            Vector3 middlePos = GlobalClass.playerPos;
            middlePos += Extra.CreateRandomVector(1f).normalized * 8f;
            Vector3 dirToPlayer = (GlobalClass.playerPos - middlePos).normalized;
            Vector3 oppDir = Quaternion.Euler(0, -90f, 0) * dirToPlayer * distBetweenRobots;
            Vector3 robotPosition = middlePos + 
                Quaternion.Euler(0, 90f, 0) * dirToPlayer * distBetweenRobots * ((robotNum - 1) / 2);

            if (robotNum % 2 == 0)
                robotPosition += Quaternion.Euler(0, 90f, 0) * dirToPlayer * (distBetweenRobots / 2);

            Extra.DrawBox(middlePos, 0.5f, Color.blue, 5f);
            

            for (int i = 0; i < robotNum; i++)
            {
                Extra.DrawBox(robotPosition, 0.5f, Color.black, 5f);
                enemies.GetRobotList()[i].StartFSquad(robotPosition);
                robotPosition += oppDir;
            }
            // Set protector index 0 and index 1 to their positions and protect eachother
            Vector3 protectorPos = middlePos + (dirToPlayer * 1.5f) + oppDir * 4f;
            enemies.GetProtectorList()[0].StartFSquad(protectorPos, enemies.GetProtectorList()[1], true);
            protectorPos += -oppDir * 8f;
            enemies.GetProtectorList()[1].StartFSquad(protectorPos, enemies.GetProtectorList()[0], false);
            // Set the commander behind all the robots
            Vector3 commanderPos = middlePos + -dirToPlayer * 5f;
            enemies.GetCommanderList()[0].StartFSquad(commanderPos, Extra.ConvertFromRobotToEnemies(enemies.GetRobotList()));
            middleVector = enemies.GetRobotList()[robotNum / 2].transform.position;
        }

        /// <summary>
        /// After all the enemies have made it to their positions, start firing
        /// </summary>
        private void SecondStartRobotFSquad()
        {
            StartCoroutine("BeginFiring");
        }

        IEnumerator BeginFiring()
        {
            yield return new WaitForSeconds(1f);
            int randomNumber;
            for (int i = 0; i < 100; i++)
            {
                Vector3 positionToShoot = GlobalClass.playerPos;
                positionToShoot += new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
                randomNumber = Random.Range(0, robotNum);
                if (enemies.GetRobotList()[randomNumber] != null)
                    enemies.GetRobotList()[randomNumber].FireInSquad(positionToShoot);
                yield return new WaitForSeconds(0.02f);
            }
            EndFormation();
        }

        // GROUP PROTECTION ==================================================================================================

        Vector3 groupCenter;
        Vector3 protPosOutFromCenter;
        Vector3 protLeft;
        Vector3 protRight;

        /// <summary>
        /// Adds all the enemies within the circle and keeps their movement, calls the two protectors to cast large shield between them
        /// </summary>
        private void StartGroupProtection(List<Enemy> enemiesNearPosition)
        {
            protectorNum = 2;
            totalEnemyNum = protectorNum + enemiesNearPosition.Count;

            allEnemies = new EnemyInPosition[totalEnemyNum];
            List<Protector> origProtList = LevelController.enemiesWithoutActiveBehavior.GetProtectorList();
            int listRemoveIndex; // Removes from the top for more efficiency
            int allEnemIndex = 0;
            for (listRemoveIndex = protectorNum - 1; listRemoveIndex >= 0; listRemoveIndex--)
            {
                enemies.Add(origProtList[listRemoveIndex]);
                allEnemies[allEnemIndex] = new EnemyInPosition(origProtList[listRemoveIndex]);
                origProtList.RemoveAt(listRemoveIndex);
                allEnemIndex++;
            }
            for (int i = 0; i < enemiesNearPosition.Count; i++)
            {
                allEnemies[allEnemIndex] = new EnemyInPosition(enemiesNearPosition[i]);
                LevelController.enemiesWithoutActiveBehavior.Remove(enemiesNearPosition[i]);
                allEnemIndex++;
            }

            for (int i = 0; i < allEnemies.Length; i++)
            {
                Extra.DrawBox(allEnemies[i].enemy.transform.position);
                allEnemies[i].enemy.StartGroupProtection(allEnemies[i].enemy.transform.position);
            }

            groupCenter = Extra.FindCenterOfListOfEnemies(enemiesNearPosition);
            Vector3 left = Quaternion.Euler(0, 90, 0) * (GlobalClass.playerPos - groupCenter).normalized;
            protPosOutFromCenter = (GlobalClass.playerPos - groupCenter).normalized * 3;
            Vector3 protectorPos = groupCenter + protPosOutFromCenter;
            protLeft = left * 4;
            protectorPos += protLeft;
            enemies.GetProtectorList()[0].StartFSquad(protectorPos, enemies.GetProtectorList()[1], true);
            protRight = -left * 8;
            protectorPos += protRight;
            enemies.GetProtectorList()[1].StartFSquad(protectorPos, enemies.GetProtectorList()[0], false);
        }

        private void SecondStartGroupProtection()
        {
            for (int i = 0; i < allEnemies.Length; i++)
            {
                allEnemies[i].enemy.SecondStartGroupProtection();
            }
        }

        /// <summary>
        /// Updates the two protectors to stay in front of the player while keeping the shield distance
        /// Always makes a T shape with the player at the bottom and the protectors at the two top ends
        /// </summary>
        private void UpdateGroupProtection()
        {
            
            Vector3 dirToPlayer = GlobalClass.playerPos - groupCenter;
            Vector3 protCenterPos = dirToPlayer.normalized * 3 + groupCenter;
            Vector3 protectorPos = Quaternion.Euler(0, 90f, 0) * dirToPlayer.normalized * 4 + protCenterPos;
            enemies.GetProtectorList()[0].UpdateGroupProtection(protectorPos);
                    protectorPos = Quaternion.Euler(0, -90f, 0) * dirToPlayer.normalized * 4 + protCenterPos;
            enemies.GetProtectorList()[1].UpdateGroupProtection(protectorPos);
        }

        // MOUNTED WALKER ========================================================================================================

        private void StartMountedWalker()
        {
            commanderNum = 1;
            walkerNum = 1;
            totalEnemyNum = protectorNum + walkerNum;

            allEnemies = new EnemyInPosition[totalEnemyNum];
            List<Commander> origCommList = LevelController.enemiesWithoutActiveBehavior.GetCommanderList();
            List<Walker> origWalkerList = LevelController.enemiesWithoutActiveBehavior.GetWalkerList();
            int listRemoveIndex; // Removes from the top for more efficiency. I think?
            int allEnemIndex = 0;
            for (listRemoveIndex = commanderNum - 1; listRemoveIndex >= 0; listRemoveIndex--)
            {
                enemies.Add(origCommList[listRemoveIndex]);
                allEnemies[allEnemIndex] = new EnemyInPosition(origCommList[listRemoveIndex]);
                origCommList.RemoveAt(listRemoveIndex);
                allEnemIndex++;
            }
            for (listRemoveIndex = walkerNum - 1; listRemoveIndex >= 0; listRemoveIndex--)
            {
                enemies.Add(origWalkerList[listRemoveIndex]);
                allEnemies[allEnemIndex] = new EnemyInPosition(origWalkerList[listRemoveIndex]);
                origWalkerList.RemoveAt(listRemoveIndex);
                allEnemIndex++;
            }

            Vector3 walkerPos = enemies.GetWalkerList()[0].transform.position;
            Vector3 randDir = Extra.CreateRandomVectorWithMagnitude(3f);
            enemies.GetCommanderList()[0].StartMountedWalker(walkerPos + randDir);
            //enemies.GetWalkerList()[0].StartMountedWalker(walkerPos);
        }

        private void SecondStartMountedWalker()
        {
            enemies.GetCommanderList()[0].SecondStartMountedWalker(enemies.GetWalkerList()[0]);
        }

        // GLOBAL METHODS =========================================================================================================

        /// <summary>
        /// Intitate the formation based on parameter behavior
        /// </summary>
        public void Initiate(Behavior behavior, List<Enemy> enemiesNearPosition)
        {
            enemies = new EnemyList();
            this.behavior = behavior;
            started = true;
            initiationPhase = true;
            secondIntiation = false;

            switch (behavior)
            {
                case Behavior.robotFSquad:
                    StartRobotFSquad(enemiesNearPosition);
                    break;
                case Behavior.groupProt:
                    StartGroupProtection(enemiesNearPosition);
                    break;
                case Behavior.mountedWalker:
                    StartMountedWalker();
                    break;
                default:
                    started = false;
                    Debug.Log("Unimplemented Behavior");
                    break;
            }
            IncludeAllEnemies();
        }

        private void Update()
        {
            if (!started)
                return;
            if (initiationPhase)
                CheckAllPositions();
            else if (secondIntiation)
            {
                switch(behavior)
                {
                    case Behavior.robotFSquad:
                        SecondStartRobotFSquad();
                        break;
                    case Behavior.groupProt:
                        SecondStartGroupProtection();
                        break;
                    case Behavior.mountedWalker:
                        SecondStartMountedWalker();
                        break;
                    default:
                        break;
                }
                secondIntiation = false;
                updatePhase = true;
            }
            else if (updatePhase)
            {
                switch (behavior)
                {
                    case Behavior.groupProt:
                        UpdateGroupProtection();
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Will check to see if all enemies within the formation are in position or not
        /// </summary>
        private void CheckAllPositions()
        {
            bool allInPosition = true;
            for (int i = 0; i < allEnemies.Length; i++)
            {
                if (allEnemies[i] == null)
                    continue;

                if (!allEnemies[i].inPosition)
                {
                    if (allEnemies[i].enemy.CheckIfAtEndOfPath())
                    {
                        allEnemies[i].inPosition = true;
                        allEnemies[i].enemy.StopMovement();
                    }
                    else
                    {
                        allInPosition = false;
                    }
                }
            }
            if (allInPosition)
            {
                initiationPhase = false;
                secondIntiation = true;
            }
            else
                Debug.Log("There is an enemy not in position");
        }

        /// <summary>
        /// Set the behavior handler variable on each enemy in the formation
        /// </summary>
        private void IncludeAllEnemies()
        {
            for (int i = 0; i < allEnemies.Length; i++)
            {
                if (allEnemies[i] != null)
                    allEnemies[i].enemy.SetBehaviorHandler(this);
            }
        }

        /// <summary>
        /// Called by enemies before death to see if the formation or behavior should carry on or not
        /// </summary>
        public void EnemyInFormationDied(Enemy enemy)
        {
            // TODO
            switch (behavior)
            {
                case Behavior.robotFSquad:
                    enemies.Remove(enemy);
                    if (enemies.GetRobotList().Count <= 3 || enemies.GetProtectorList().Count <= 1)
                        EndFormation();
                    break;
                case Behavior.groupProt:
                    enemies.Remove(enemy);
                    int protCount = enemies.GetProtectorList().Count;
                    if (protCount <= 1 || enemies.TotalCount() - protCount <= 5)
                        EndFormation();
                    break;
                case Behavior.mountedWalker:
                    enemies.Remove(enemy);
                    EndFormation();
                    break;
                default:
                    started = false;
                    Debug.Log("No Behavior for enemy death");
                    break;
            }
        }

        private void EndFormation()
        {
            for (int i = 0; i < allEnemies.Length; i++)
            {
                if (allEnemies[i] != null)
                    if (allEnemies[i].enemy != null)
                        allEnemies[i].enemy.EndFomation();
            }
            Destroy(this);
        }
    }
    // END OF BEHAVIOR HANDLER SCRIPT ------------------------------------------------------------------------------------





















    [SerializeField] float rollCooldown = 5f;
    bool rollOnCooldown = false;
    [Range(0f, 100f)] [SerializeField] float robotFSquadChance = 10f;
    [Range(0f, 100f)] [SerializeField] float groupProtectionChance = 40f;

    /// <summary>
    /// Will roll every x seconds to see if to activate a formation, creates a BehaviorHandler 
    /// which will handle its own update and start as a seperate component
    /// </summary>
    void Update()
    {
        
        if (!rollOnCooldown)
        {

            StartCoroutine("RollCooldown");
            EnemyList lcList = LevelController.enemiesWithoutActiveBehavior;
            List<Robot> robotList = lcList.GetRobotList();
            List<Protector> protectorList = lcList.GetProtectorList();
            List<Commander> commanderList = lcList.GetCommanderList();
            List<Walker> walkerList = lcList.GetWalkerList();

            RobotFSquadCheck(robotList, protectorList, commanderList);
            GroupProtectionCheck(lcList, protectorList);

            /*
            if (walkerList.Count >= 1 && commanderList.Count >= 1)
            {
                BehaviorHandler handler = gameObject.AddComponent<BehaviorHandler>();
                handler.Initiate(Behavior.mountedWalker, null);
            }*/
        }
    }

    private void RobotFSquadCheck(List<Robot> robotList, List<Protector> protectorList, List<Commander> commanderList)
    {
        if (robotList.Count >= 5 && protectorList.Count >= 2 && commanderList.Count >= 1)
        {
            if (Extra.RollChance(robotFSquadChance))
            {
                List<Robot> enemies = LevelController.enemiesWithoutActiveBehavior.GetRobotList();
                int rand = Random.Range(0, enemies.Count); // Get a random enemy
                Collider[] collidersHit = Physics.OverlapSphere(enemies[rand].transform.position, 11f, GlobalClass.exD.enemiesOnlyLM); // Get the colliders near the randomly chosen enemies

                Extra.DrawStrangeOutCenterThing(enemies[rand].transform.position, 11f, Color.red, 10f);

                if (collidersHit.Length >= 5) // Are there at least 5 colliders for three possible enemies
                {
                    List<GameObject> gameObjectsHit = CollisionHandler.TestSingleTriggerArray(collidersHit); // Sift out extra colliders
                    EnemyList uniqueEnemies = Extra.ListOfGameObjectsToEnemyList(gameObjectsHit); // Sift out the enemies
                    List<Robot> robotsDetected = uniqueEnemies.GetRobotList();
                    print("There are " + robotsDetected.Count + " in the list before sifting");
                    List<Enemy> finalRobots = new List<Enemy>();
                    if (robotsDetected.Count >= 5)
                    {
                        for (int i = 0; i < robotsDetected.Count; i++)
                        {
                            if (!robotsDetected[i].HasActiveBehavior())
                            {
                                finalRobots.Add(robotsDetected[i]);
                            }
                        }
                        print("There are " + robotsDetected.Count + " in the list after sifting");
                        if (finalRobots.Count >= 5)
                        {
                            BehaviorHandler handler = gameObject.AddComponent<BehaviorHandler>();
                            handler.Initiate(Behavior.robotFSquad, finalRobots);
                        }
                    }
                }
            }
        }
    }

    private void GroupProtectionCheck(EnemyList lcList, List<Protector> protectorList)
    {
        if (!Extra.RollChance(groupProtectionChance))
            return;
        if (lcList.TotalCount() - protectorList.Count >= 3 && protectorList.Count >= 2)
        {
            List<Enemy> enemies = LevelController.allEnemiesInScene;
            int rand = UnityEngine.Random.Range(0, enemies.Count); // Get a random enemy
            Collider[] collidersHit = Physics.OverlapSphere(enemies[rand].transform.position, 6f, GlobalClass.exD.enemiesOnlyLM); // Get the colliders near the randomly chosen enemies

            Extra.DrawStrangeOutCenterThing(enemies[rand].transform.position, 6f, Color.red, 10f);

            if (collidersHit.Length > 3) // Are there at least 3 colliders for three possible enemies
            {
                List<GameObject> temp = CollisionHandler.TestSingleTriggerArray(collidersHit); // Sift out extra colliders
                List<Enemy> uniqueEnemies = new List<Enemy>();
                foreach (GameObject obj in temp) // Sift out the enemies from the list of GameObjects
                {
                    if (obj.CompareTag(GlobalClass.ENEMY_TAG)) // If its an enemy
                    {
                        Enemy enemy = obj.GetComponent<Enemy>();
                        if (enemy.enemyType != Enemy.EnemyType.protector || enemy.enemyType != Enemy.EnemyType.assassin) // If not protectors or assasins add them
                            uniqueEnemies.Add(enemy);
                    }
                }
                if (uniqueEnemies.Count > 3) // If there are at least 3 enemies
                {
                    BehaviorHandler handler = gameObject.AddComponent<BehaviorHandler>();
                    handler.Initiate(Behavior.groupProt, uniqueEnemies);
                }
            }
        }
    }

    IEnumerator RollCooldown()
    {
        rollOnCooldown = true;
        yield return new WaitForSeconds(rollCooldown);
        rollOnCooldown = false;
    }
}
