using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Handles spawning ants for a given species and team. 
/// </summary>
public class AntSpawner : NetworkBehaviour
{
    /// <summary>
    /// Will call the correct spawn method on the queen to spawn the correct ant. Must be updated
    /// whenever a new species is added or new ant types are added.
    /// </summary>
    /// <param name="speciesIndex">
    /// Indicates which ant species to spawn:
    /// 0 = Black
    /// 1 = Fire
    /// </param>
    /// <param name="antIndex">Indicates which ant type to spawn.</param>
    /// <param name="team">The team that is spawning an ant.</param>
    [Rpc(SendTo.Everyone)]
    public void SpawnAntClientRpc(int speciesIndex, int antIndex, int team)
    {
        if(speciesIndex == 0)
        {
            GameObject queen = GameObject.Find("MBlackAntQueen" + team.ToString());
            if(antIndex == 0) //Worker
            {
                queen.GetComponent<MBaseAntQueenAI>().SpawnWorker();
            }
            else if (antIndex == 1) //Soldier
            {
                queen.GetComponent<MBaseAntQueenAI>().SpawnSoldier();
            }
        }
        else if(speciesIndex == 1)
        {
            GameObject queen = GameObject.Find("MFireAntQueen" + team.ToString());
            if (antIndex == 0) //Worker
            {
                queen.GetComponent<MBaseAntQueenAI>().SpawnWorker();
            }
            else if (antIndex == 1) //Soldier
            {
                queen.GetComponent<MBaseAntQueenAI>().SpawnSoldier();
            }
            else if (antIndex == 2) //Supersoldier
            {
                queen.GetComponent<MBaseAntQueenAI>().SpawnSuperSoldier();
            }
        }
    }
}
