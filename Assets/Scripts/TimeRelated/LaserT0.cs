using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserT0 : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform laserStart;
    //public Transform laserEnd;
    Vector3 laserEndVec = new Vector3(0, 0, 0);
    public EdgeCollider2D edgeCollider;
    Transform target;
    public Vector3 direction = Vector3.zero;

    public float speed;


    public float laserLength = 0;


    public int mode = 0;
    //0 no laser
    //1 laser firing
    //2 laser 
    //3 laser ending
    //4 laser ended


    // Start is called before the first frame update
    void Start()
    {
        target = LoopData.player.transform;
    }

    // Update is called once per frame
    void Update()
    {

        switch (mode)
        {
            case 0:
                lineRenderer.enabled = false;
                break;
            case 1:
                lineRenderer.enabled = true;
                setLaserFiring();


                break;
            case 2:
                lineRenderer.enabled = true;
                setLaserGo();

                break;
            case 3:
                lineRenderer.enabled = true;
                lineRenderer.SetPosition(0, laserStart.position);
                lineRenderer.SetPosition(1, laserEndVec);
                break;
            case 4:
                lineRenderer.enabled = false;
                Destroy(gameObject);

                break;

        }
    }

    public void updateDirection()
    {
        direction = (target.position - laserStart.position).normalized;
    }
    void setLaserFiring()
    {
        laserLength += speed * Time.deltaTime;
        if (direction == Vector3.zero)
        {
            updateDirection();
        }
        //laserEndVec = direction * laserLength + laserStart.position;
        laserEndVec = direction * laserLength;
        setLinePosition();
    }
    void setLaserGo()
    {
        laserStart.position += direction * (speed * Time.deltaTime);
        setLinePosition();
    }

    void setLinePosition()
    {
        lineRenderer.SetPosition(0, new Vector3(0, 0, 0));
        lineRenderer.SetPosition(1, laserEndVec);


        Vector2[] points = edgeCollider.points;
        points.SetValue(new Vector2(laserEndVec.x, laserEndVec.y), 1);
        edgeCollider.points = points;
    }

}
