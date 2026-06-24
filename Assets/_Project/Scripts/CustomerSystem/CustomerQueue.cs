using UnityEngine;
using System.Collections.Generic;

public class CustomerQueue : MonoBehaviour
{
   [SerializeField] private Transform lineStartTransform;
   [SerializeField] private Transform lineEndTransform;
   [SerializeField , Range(1f, 5)] private float spacing;

   [SerializeField] private bool firstLineInXAxis;

   private List<Vector3> _queuePointList;
   private List<Customer> _customerList;
   
   private void Start()
   {
      _queuePointList = new List<Vector3>();
      _customerList = new List<Customer>();
    
      GenerateQueuePoints();
   }

   private void GenerateQueuePoints()
   {
      
      Vector3 startPosition = lineStartTransform.position + Vector3.up;
      Vector3 endPosition = lineEndTransform.position + Vector3.up;

      Vector3 firstLineEnd = endPosition;
      
      bool twoLine = false;

      if (firstLineInXAxis) twoLine = Mathf.Abs(endPosition.z - startPosition.z) > 1;
      else if (firstLineInXAxis == false) twoLine = Mathf.Abs(endPosition.x - startPosition.x) > 1;
      
      if (twoLine)
      {
         if(firstLineInXAxis) firstLineEnd = new Vector3(endPosition.x, 1, startPosition.z);
         else if (firstLineInXAxis == false) firstLineEnd = new Vector3(startPosition.x, 1, endPosition.z);
      }

      int distance = 0;
      
      if(firstLineInXAxis) distance = Mathf.FloorToInt(Mathf.Abs(startPosition.x - firstLineEnd.x));
      else if (firstLineInXAxis == false) distance = Mathf.FloorToInt(Mathf.Abs(startPosition.z - firstLineEnd.z));
      
      for (float i = 0; i < distance; i+= spacing)
      {
         Vector3 point = Vector3.Lerp(startPosition, firstLineEnd, i / distance);
         _queuePointList.Add(point);
      }

      if (twoLine)
      {
         int dis = 0;

         if(firstLineInXAxis) dis = Mathf.FloorToInt(Mathf.Abs(firstLineEnd.z - endPosition.z));
         else if (firstLineInXAxis == false) dis = Mathf.FloorToInt(Mathf.Abs(firstLineEnd.x - endPosition.x));
         
         for (float i = 0; i < dis; i+= spacing)
         {
            Vector3 point = Vector3.Lerp(firstLineEnd, endPosition, i / dis);
            _queuePointList.Add(point);
         }
      }
   }

   public bool CanAddCustomer()
   {
      if (_customerList.Count >= _queuePointList.Count) return false;
      return true;
   }
   
   public void AddCustomerToQueue(Customer customer)
   {
      if (CanAddCustomer() == false) return;
      if (_customerList.Contains(customer)) return;
      
      _customerList.Add(customer);
      int index = _customerList.IndexOf(customer);
      
      Vector3 targetPoint = _queuePointList[index];
      customer.MoveTo(targetPoint);
   }

   public void RemoveCustomerFromQueue(Customer customer)
   {
      _customerList.Remove(customer);
      UpdateCustomerInQueue();
   }

   private void UpdateCustomerInQueue()
   {
      foreach (var customer in _customerList)
      {
         int index = _customerList.IndexOf(customer);
         Vector3 targetPoint = _queuePointList[index];

         customer.MoveTo(targetPoint);
      }
   }
   
   private void OnDrawGizmos()
   {
      Gizmos.color = Color.red;
      
      Vector3 startPosition = lineStartTransform.position + Vector3.up;
      Vector3 endPosition = lineEndTransform.position + Vector3.up;

      Vector3 lineEnd = endPosition;

      bool twoLine = false;

      if (firstLineInXAxis) twoLine = Mathf.Abs(endPosition.z - startPosition.z) > 1;
      else if (firstLineInXAxis == false) twoLine = Mathf.Abs(endPosition.x - startPosition.x) > 1;
      
      if (twoLine)
      {
         if(firstLineInXAxis) lineEnd = new Vector3(endPosition.x, 1, startPosition.z);
         else if (firstLineInXAxis == false) lineEnd = new Vector3(startPosition.x, 1, endPosition.z);
            
         Gizmos.DrawLine(lineEnd, endPosition);
      }
      
      Gizmos.DrawLine(startPosition,lineEnd);

      if (Application.isPlaying == false) return;
      if (_queuePointList.Count <= 0) return;

      Gizmos.color = Color.white;
      
      foreach (var p in _queuePointList)
      {
         Gizmos.DrawWireSphere(p, .2f);
      }
   }
}
