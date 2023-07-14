using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public class HumanScript : MonoBehaviour
{
    [SerializeField]
    GameObject buildingContainer;
    [SerializeField]
    GameResourceSO woodResourceSO;
    [SerializeField]
    GameResourceSO chairResourceSO;
    GameResourceSO holdedResourceType;
    [SerializeField]
    GameResourcesList holdingResource;

    Transform target;
    GameObject targetedGameObject;

    public Animator anim;

    bool isHoldingBasicResource = false;
    bool isHoldingComplexResource = false;
    int largesAmountOfSimpleResource = -1;
    int largestAmountOfComplexResource = 0;

    public bool warehouseIsActive=false;

    float nearestWarehouseDistance = float.MaxValue;
    float nearestProductionDistance = float.MaxValue;
    private void Awake()
    {
        buildingContainer = GameObject.FindWithTag("Respawn");
    }
    private void Update()
    {
        if (isHoldingComplexResource)
        {
            CarryToWarehouse();
        }
        else if (isHoldingBasicResource)
        {
            CarryToProduction();
        }
        //setting target to extract items from buildings
        else if (target == null)
        {
            foreach (Transform child in buildingContainer.transform)
            {

                if (child.GetComponent<ProductionBuilding>() != null && warehouseIsActive)
                    GoToProduction(child.GetComponent<ProductionBuilding>());
                else if(child.GetComponent<ExtractionBuilding>() != null)
                    GoToExtraction();
            }

        }

        if (target != null)
        {
            MoveToPosition();
        }
        
    }
    void MoveToPosition()
    {
        transform.position = Vector3.MoveTowards(transform.position, target.position, 2 * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.position - transform.position),Time.deltaTime*4);
        if (transform.position != target.position)
        {
            anim.SetBool("IsMoving", true);
        }
        if (transform.position == target.position)
        {
            anim.SetBool("IsMoving", false);
            if (isHoldingBasicResource || isHoldingComplexResource)
            {
                //dlaczego nie wiem ale dzia³a (animacja), powinno byæ na odwrót ale siê nie znam na animacji wiêc zostaje jak jest
                anim.SetBool("IsCaring", false);
                targetedGameObject.GetComponent<PullOutResource>().resourcesList.Add(holdedResourceType, 1);
                isHoldingBasicResource = false;
                isHoldingComplexResource = false;
                target = null;
                nearestWarehouseDistance = float.MaxValue;
                nearestProductionDistance = float.MaxValue;
                holdingResource.Remove(holdedResourceType, 1);
            }
            else
            {
                anim.SetBool("IsCaring", true);
                PullOutResource();
            }

        }

    }
    //getting out resources from building
    void PullOutResource()
    {
        GameResourcesList list = targetedGameObject.GetComponent<PullOutResource>().resourcesList;
        var resource = list.resources.Find((x) => x.resourceSO == holdedResourceType);
        if (holdedResourceType == woodResourceSO)
        {
            if (resource.amount >= 1)
            {
                list.Remove(holdedResourceType,1);
                isHoldingBasicResource = true;
                largesAmountOfSimpleResource = -1;
                target = null;
                holdingResource.Add(holdedResourceType, 1);
            }
        }
        else if(holdedResourceType == chairResourceSO)
        {
            if (resource.amount >= 1)
            {
                list.Remove(holdedResourceType, 1);
                isHoldingComplexResource = true;
                largesAmountOfSimpleResource = -1;
                largestAmountOfComplexResource = 0;
                target = null;
                holdingResource.Add(holdedResourceType, 1);
            }
        }
    }
    //moving human to extraction building if he got nothing to do
    void GoToExtraction()
    {
        foreach (ExtractionBuilding extraction in buildingContainer.transform.GetComponentsInChildren<ExtractionBuilding>())
        {

            if (extraction != null)
            {
                var simpleResource = extraction.resourcesList.resources.Find((x) => x.resourceSO == woodResourceSO);
                if (simpleResource.amount > largesAmountOfSimpleResource)
                {
                    holdedResourceType = woodResourceSO;
                    largesAmountOfSimpleResource = simpleResource.amount;
                    target = extraction.transform;
                    targetedGameObject = extraction.gameObject;
                }

            }
        }
    }
    //moving human to production building 
    void GoToProduction(ProductionBuilding production)
    {
        var coplexResource = production.resourcesList.resources.Find((x) => x.resourceSO == chairResourceSO);
        if (coplexResource.amount <= 0 )
        {
            GoToExtraction();
        }
        else if (coplexResource.amount > largestAmountOfComplexResource)
        {
            holdedResourceType = chairResourceSO;
            largestAmountOfComplexResource = coplexResource.amount;
            target = production.transform;
            targetedGameObject = production.gameObject;
        }
    }
    void CarryToWarehouse()
    {
        foreach (Transform child in buildingContainer.transform)
        {
            if (child.GetComponent<Warehouse>() != null)
            {
                float warehouseDistance = (child.position - this.transform.position).sqrMagnitude;
                if (warehouseDistance < nearestWarehouseDistance)
                {
                    nearestWarehouseDistance = warehouseDistance;

                    Warehouse warehouse = child.GetComponent<Warehouse>();
                    target = warehouse.transform;
                    targetedGameObject = warehouse.gameObject;
                }
            }
        }
    }
    void CarryToProduction()
    {
        foreach (Transform child in buildingContainer.transform)
        {
            if (child.GetComponent<ProductionBuilding>() != null)
            {
                float productionDistance = (child.position - this.transform.position).sqrMagnitude;
                if (productionDistance < nearestProductionDistance)
                {
                    nearestProductionDistance = productionDistance;

                    ProductionBuilding production = child.GetComponent<ProductionBuilding>();
                    target = production.transform;
                    targetedGameObject = production.gameObject;
                }
            }
        }
    }

}
