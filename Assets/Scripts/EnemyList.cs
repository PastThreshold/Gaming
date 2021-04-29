using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Array of list
/// Used for storing large amount of enemies where their specific script is needed
/// So that is does not require indexing through list to find enemies of that type
/// </summary>
public class EnemyList
{
    List<Robot> robotList;
    List<Protector> protectorList;
    List<RollerMine> rollermineList;
    List<Walker> walkerList;
    List<Assassin> assassinList;
    List<Commander> commanderList;

    public EnemyList()
    {
        robotList = new List<Robot>();
        protectorList = new List<Protector>();
        rollermineList = new List<RollerMine>();
        walkerList = new List<Walker>();
        assassinList = new List<Assassin>();
        commanderList = new List<Commander>();
    }

    public void Add(Enemy enemy)
    {
        switch (enemy.enemyType)
        {
            case Enemy.EnemyType.robot:
                robotList.Add(enemy.GetComponent<Robot>());
                break;
            case Enemy.EnemyType.protector:
                protectorList.Add(enemy.GetComponent<Protector>());
                break;
            case Enemy.EnemyType.roller:
                rollermineList.Add(enemy.GetComponent<RollerMine>());
                break;
            case Enemy.EnemyType.walker:
                walkerList.Add(enemy.GetComponent<Walker>());
                break;
            case Enemy.EnemyType.assassin:
                assassinList.Add(enemy.GetComponent<Assassin>());
                break;
            case Enemy.EnemyType.commander:
                commanderList.Add(enemy.GetComponent<Commander>());
                break;
            default:
                break;
        }
    }

    public bool Remove(Enemy enemy)
    {
        switch (enemy.enemyType)
        {
            case Enemy.EnemyType.robot:
                return robotList.Remove(enemy.GetComponent<Robot>());
            case Enemy.EnemyType.protector:
                return protectorList.Remove(enemy.GetComponent<Protector>());
            case Enemy.EnemyType.roller:
                return rollermineList.Remove(enemy.GetComponent<RollerMine>());
            case Enemy.EnemyType.walker:
                return walkerList.Remove(enemy.GetComponent<Walker>());
            case Enemy.EnemyType.assassin:
                return assassinList.Remove(enemy.GetComponent<Assassin>());
            case Enemy.EnemyType.commander:
                return commanderList.Remove(enemy.GetComponent<Commander>());
            default:
                return false;
        }
    }

    public bool Contains(Enemy enemy)
    {
        switch (enemy.enemyType)
        {
            case Enemy.EnemyType.robot:
                return robotList.Contains(enemy.GetComponent<Robot>());
            case Enemy.EnemyType.protector:
                return protectorList.Contains(enemy.GetComponent<Protector>());
            case Enemy.EnemyType.roller:
                return rollermineList.Contains(enemy.GetComponent<RollerMine>());
            case Enemy.EnemyType.walker:
                return walkerList.Contains(enemy.GetComponent<Walker>());
            case Enemy.EnemyType.assassin:
                return assassinList.Contains(enemy.GetComponent<Assassin>());
            case Enemy.EnemyType.commander:
                return commanderList.Contains(enemy.GetComponent<Commander>());
            default:
                return false;
        }
    }

    public int Count(Enemy.EnemyType type)
    {
        switch (type)
        {
            case Enemy.EnemyType.robot:
                return robotList.Count;
            case Enemy.EnemyType.protector:
                return protectorList.Count;
            case Enemy.EnemyType.roller:
                return rollermineList.Count;
            case Enemy.EnemyType.walker:
                return walkerList.Count;
            case Enemy.EnemyType.assassin:
                return assassinList.Count;
            case Enemy.EnemyType.commander:
                return commanderList.Count;
            default:
                Debug.Log("New Enemy Type Not Added???????????????");
                return -1;
        }
    }

    public int TotalCount()
    {
        return robotList.Count + protectorList.Count + 
            rollermineList.Count + walkerList.Count + 
            assassinList.Count + commanderList.Count;
    }

    public List<Robot> GetRobotList() { return robotList; }
    public List<Protector> GetProtectorList() { return protectorList; }
    public List<RollerMine> GetRollerMineList() { return rollermineList; }
    public List<Walker> GetWalkerList() { return walkerList; }
    public List<Assassin> GetAssassinList() { return assassinList; }
    public List<Commander> GetCommanderList() { return commanderList; }

    public void AddMultiple(List<Enemy> enemies)
    {
        for(int index = 0; index < enemies.Count; index++)
        {
            switch(enemies[index].enemyType)
            {
                case Enemy.EnemyType.robot:
                    robotList.Add(enemies[index].GetComponent<Robot>());
                    break;
                case Enemy.EnemyType.protector:
                    protectorList.Add(enemies[index].GetComponent<Protector>());
                    break;
                case Enemy.EnemyType.roller:
                    rollermineList.Add(enemies[index].GetComponent<RollerMine>());
                    break;
                case Enemy.EnemyType.walker:
                    walkerList.Add(enemies[index].GetComponent<Walker>());
                    break;
                case Enemy.EnemyType.assassin:
                    assassinList.Add(enemies[index].GetComponent<Assassin>());
                    break;
                case Enemy.EnemyType.commander:
                    commanderList.Add(enemies[index].GetComponent<Commander>());
                    break;
                default:
                    break;
            }
        }
    }
    public void AddMultiple(Enemy[] enemies)
    {
        for (int index = 0; index < enemies.Length; index++)
        {
            switch (enemies[index].enemyType)
            {
                case Enemy.EnemyType.robot:
                    robotList.Add(enemies[index].GetComponent<Robot>());
                    break;
                case Enemy.EnemyType.protector:
                    protectorList.Add(enemies[index].GetComponent<Protector>());
                    break;
                case Enemy.EnemyType.roller:
                    rollermineList.Add(enemies[index].GetComponent<RollerMine>());
                    break;
                case Enemy.EnemyType.walker:
                    walkerList.Add(enemies[index].GetComponent<Walker>());
                    break;
                case Enemy.EnemyType.assassin:
                    assassinList.Add(enemies[index].GetComponent<Assassin>());
                    break;
                case Enemy.EnemyType.commander:
                    commanderList.Add(enemies[index].GetComponent<Commander>());
                    break;
                default:
                    break;
            }
        }
    }
}
