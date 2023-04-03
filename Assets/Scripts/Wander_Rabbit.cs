using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wander_Rabbit : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float rotationSpeed = 100f;

    private bool isWandering=  false;
    private bool isRotatingLeft= false;
    private bool isRotatingRight= false;
    private bool isRunning= false;
    // Update is called once per frame
    void Update()
    {
        if(isWandering == false){
            StartCoroutine(Wander());
        }
        if(isRotatingLeft == true)
        {
            gameObject.GetComponent<Animator>().Play("idle");
            transform.Rotate(transform.up*Time.deltaTime*rotationSpeed);
        }
        if(isRotatingRight == true){
            gameObject.GetComponent<Animator>().Play("idle");
            transform.Rotate(transform.up*Time.deltaTime*rotationSpeed);
        }
        if(isRunning == true){
            gameObject.GetComponent<Animator>().Play("run");
            transform.position += transform.forward*Time.deltaTime*moveSpeed;
        }
    }

    IEnumerator Wander()
    {
        int rotTime  = Random.Range(1, 3);
        int rotWait = Random.Range(1, 4);
        int rotLorR = Random.Range(1, 2);
        int runWait = Random.Range(1, 5);
        int runTime = Random.Range(1, 6);

        isWandering = true;

        yield return new WaitForSeconds(runWait);
        isRunning = true;
        yield return new WaitForSeconds(runTime);
        isRunning = false;
        yield return new WaitForSeconds(rotWait);

        if (rotLorR == 1){
            isRotatingLeft = true;
            yield return new WaitForSeconds(rotTime);
            isRotatingLeft= false;
        }
        if (rotLorR == 2){
            isRotatingRight = true;
            yield return new WaitForSeconds(rotTime);
            isRotatingRight= false;
        }

        isWandering = false;

    }
}
