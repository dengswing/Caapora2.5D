using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IsoTools;


/// <summary>
/// C�digo encontrado em tutorial no Youtube postado por Sebastian Lague
/// </summary>
namespace PathFinding {
    public class NPC : MonoBehaviour {

    private IsoObject seekerIso;
    public GameObject _targetPos;
    public Vector3 cachedSeekerPos, cachedTargetPos;
    public Grid grid;
    private bool move = true, canStart = true;
    public IEnumerator updatePosition;
        public IEnumerator animatePath;
      
       
        public Vector3 targetPos
        {

      
            get { return _targetPos.GetComponent<IsoObject>().position; }
            set {
                        _targetPos.GetComponent<IsoObject>().position = value;
            }
        }





        void Start()
        {


            seekerIso = GetComponent<IsoObject>();
            // posicao no modo isometrico
            cachedSeekerPos = seekerIso.position;
            cachedTargetPos = _targetPos.GetComponent<IsoObject>().position;



        }

       void find()
       {

                if (cachedSeekerPos != seekerIso.position)
                {
                    cachedSeekerPos = seekerIso.position;
                    FindPath(seekerIso.position, _targetPos.GetComponent<IsoObject>().position);
                }
                if (cachedTargetPos != _targetPos.GetComponent<IsoObject>().position)
                {
                    cachedTargetPos = _targetPos.GetComponent<IsoObject>().position;
                    FindPath(seekerIso.position, _targetPos.GetComponent<IsoObject>().position);
                }
          


        }



       void Update()
       {

            if (GameManager.npc_start) { 

                        // Enquanto n�o move calcula o caminho
                        if (!move)
                        {
                            find();
           
                        }

                            AnimatePath();


                        }

             }

        void FindPath(Vector3 startPos, Vector3 targetPos) {

           
		Node startNode = grid.NodeFromWorldPoint(startPos);
		Node targetNode = grid.NodeFromWorldPoint(targetPos);

            List<Node> openSet = new List<Node>();

        HashSet<Node> closedSet = new HashSet<Node>();
		openSet.Add(startNode);
		
		while (openSet.Count > 0) {
			Node currentNode = openSet[0];
			for (int i = 1; i < openSet.Count; i ++) {
				if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost) {
					currentNode = openSet[i];
				}
			}
			
			openSet.Remove(currentNode);
			closedSet.Add(currentNode);
			
			if (currentNode == targetNode) {
				RetracePath(startNode,targetNode);
				return;
			}
			
			foreach (Node neighbour in grid.GetNeighbours(currentNode)) {
				if (!neighbour.walkable || closedSet.Contains(neighbour)) {
					continue;
				}
				
				int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
				if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) {
					neighbour.gCost = newMovementCostToNeighbour;
					neighbour.hCost = GetDistance(neighbour, targetNode);
					neighbour.parent = currentNode;
					
					if (!openSet.Contains(neighbour))
						openSet.Add(neighbour);
				}
			}
		}
	}
	
	void RetracePath(Node startNode, Node endNode) {
		List<Node> path = new List<Node>();
		Node currentNode = endNode;
		
		while (currentNode != startNode) {
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}
		path.Reverse();
		
		grid.path = path;
		
	}
	
	int GetDistance(Node nodeA, Node nodeB) {
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
		
		if (dstX > dstY)
			return 14*dstY + 10* (dstX-dstY);
		return 14*dstX + 10 * (dstY-dstX);
	}

      

        void AnimatePath()
        {
            // Enquanto estiver animando para de checar a posi��o
            move = false;
        
            Vector3 currentPos = seekerIso.position;


            if (canStart) { 
              updatePosition = UpdatePosition(currentPos, grid.path[0], 0);
              StartCoroutine(updatePosition);
            }

        }


     
       
        IEnumerator UpdatePosition(Vector3 currentPos, Node n, int index)
        {
   
            float t = 0.0f;
            // Vector3 correctedPathPos = new Vector3(n.GetWorldPos().x, 1, n.GetWorldPos().z);

            // n�o sei porque, mas foi necess�rio fazer a covers�o
            // foi necess�rio criar uma vari�vel apenas para essa finalizade para n�o sobrescrever a posi��o original
            Vector3 tmpWorldPosition  = seekerIso.isoWorld.ScreenToIso(n.worldPosition);



            Vector3 correctedPathPos = tmpWorldPosition;

            while (t < 0.5f)
            {
                t += Time.deltaTime;

            
                seekerIso.position =   Vector3.Lerp(currentPos, correctedPathPos, t);


                NPCController.stopWalking = false;

                // Apenas para o caipora, seta a posi��o anterior para movimenta��o autom�tica
                NPCController.prevPosition = currentPos;



                // Vector3.MoveTowards(currentPos, correctedPathPos, t);

                yield return null;
            }


            seekerIso.position = correctedPathPos;

     
            currentPos = correctedPathPos;

      
            // Para cada ponto do caminho executa novamente este m�todo
            index++;
            if (index < grid.path.Count)
            {

                  updatePosition = UpdatePosition(currentPos, grid.path[index], index);

                  StartCoroutine(updatePosition);
                  // grid.path.Remove(grid.path[index]);
                  Debug.Log("Caminho " + index + " Alcan�ado");
       
            }
               
            else
            {

                move = false;
                GameManager.npc_start = false;
                Debug.Log("UpdatePositio finalizado");
                StopCoroutine(updatePosition);
                               

            }
                
        } 


    } // end Pathfinding 
} // end namespace Pathfinding