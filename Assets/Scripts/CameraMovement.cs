using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    private float cameraSpeed;
    private float xMin, xMax, yMin, yMax;


    void Update()
    {
        GetInput();
    }

    void GetInput() {
        if (Input.GetKey(KeyCode.W))
            transform.Translate(Vector3.up * cameraSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.A))
            transform.Translate(Vector3.left * cameraSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.S))
            transform.Translate(Vector3.down * cameraSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.D))
            transform.Translate(Vector3.right * cameraSpeed * Time.deltaTime);

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, xMin , xMax),
            Mathf.Clamp(transform.position.y, yMin, yMax),-10);
    }

    public void SetLimits(Vector3 maxTile, Vector3 minTile) {
        Vector3 wp = Camera.main.ViewportToWorldPoint(new Vector3(1, 0));  //Bottom Right
        Vector3 wp2 = Camera.main.ViewportToWorldPoint(new Vector3(0, 1)); //Top left
        
        //LevelManager.Instance.TilemapMove(new Vector3(wp2.x - minTile.x, wp2.y - maxTile.y));
        xMin = minTile.x - wp2.x;
        xMax = maxTile.x - wp.x;
        yMin = minTile.y - wp.y;
        yMax = maxTile.y - wp2.y;
        transform.position = new Vector3(xMin, yMax);
    }

}
