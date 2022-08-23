using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserT1 : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform laserStart;
    Vector3 laserEndVec = new Vector3(0, 0, 0);
    public EdgeCollider2D edgeCollider;
    //Transform target;
    public Vector3 directionVec = Vector3.zero;


    public float rotationSpeed;
    public float startAngle;
    public float endAngle;
    public bool clockwise;
    float angleDiff;
    float operationTotalTime;



    //public float laserLength = 0;


    public int mode = 1;
    //0 stopped laser
    //1 laser firing
    //2 laser ended


    // Start is called before the first frame update
    void Start()
    {
        operationTotalTime = Zaman.quarterBeatDuration * GetComponent<Attack>().data.duration;
        startAngle = GetComponent<Attack>().data.f1;
        endAngle = GetComponent<Attack>().data.f2;
        clockwise = GetComponent<Attack>().data.b1;
        angleDiff = (endAngle - startAngle) / operationTotalTime * Mathf.PI / 180;
        directionVec = angleToVector(startAngle * Mathf.PI / 180);
    }

    // Update is called once per frame
    void Update()
    {

        switch (mode)
        {
            case 0:
                lineRenderer.enabled = true;
                break;
            case 1:
                lineRenderer.enabled = true;
                setLaserFiring();

                break;
            case 2:
                //lineRenderer.enabled = false;
                Destroy(gameObject);
                break;

        }
    }

    Vector3 angleToVector(float angle)
    {//radian to vector
        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
    }
    float vectorToAngle(Vector3 vec)
    {//vector to radian
        return Mathf.Atan2(vec.y, vec.x);
    }

    public void updateDirection()
    {
        Debug.Log("updateDirection: " + vectorToAngle(directionVec) + ". " + angleDiff + " " + rotationSpeed + " " + Time.deltaTime + " : " + (angleDiff * rotationSpeed * Time.deltaTime));
        //directionVec = Quaternion.AngleAxis(angleDiff * rotationSpeed * Time.deltaTime * Mathf.PI / 180, directionVec) * Vector3.right;

        directionVec = angleToVector(vectorToAngle(directionVec) + angleDiff * rotationSpeed * Time.deltaTime);
        //directionVec = (target.position - laserStart.position).normalized;
    }
    void setLaserFiring()
    {
        laserEndVec = directionVec * 10;
        setLinePosition();
        updateDirection();

    }
    /*void setLaserGo()
    {
        //laserStart.position += direction * (speed * Time.deltaTime);
        setLinePosition();
    }*/

    void setLinePosition()
    {
        lineRenderer.SetPosition(0, new Vector3(0, 0, 0));
        lineRenderer.SetPosition(1, laserEndVec);


        Vector2[] points = edgeCollider.points;
        points.SetValue(new Vector2(laserEndVec.x, laserEndVec.y), 1);
        edgeCollider.points = points;
    }

}
