using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dummything;

public class dummny : MonoBehaviour, ITest{

    [SerializeField]
    protected dummy2 body;

    [SerializeField]
    protected List<dummy2> listsOfDummy2;


    public dummy2 Body
    {
        get
        {
            return body;
        }
        set
        {
            body = value;
        }
    }
}
