using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputRecorderSave : InputRecorder
{
    public enum EMode
    {
        ERecord,
        EWrite,
        EPause,
    }
    public EMode mode;
    public int currentRecord;

    new void Start()
    {
        base.Start();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E) && mode != EMode.EWrite)
        {
            var c = GetComponents<InputRecorder>();
            foreach (var it in c)
                if (it != this)
                    it.enabled = false;
            mode = EMode.EWrite;
        }
        if (Input.GetKeyDown(KeyCode.R) && mode != EMode.ERecord)
        {
            var c = GetComponents<InputRecorder>();
            foreach (var it in c)
                it.enabled = true;
            mode = EMode.ERecord;
        }else if (Input.GetKeyDown(KeyCode.Q) && mode != EMode.EPause)
        {
            var c = GetComponents<InputRecorder>();
            foreach (var it in c)
                    it.enabled = true;
            mode = EMode.EPause;
        }
    }

    private void FixedUpdate()
    {
        if(mode == EMode.ERecord)
        {
            InputData data = new InputData();
            data.positionInput = inputHolder.positionInput;
            data.directionInput = inputHolder.directionInput;
            data.keys = new bool[3];
            data.keys[0] = inputHolder.keys[0];
            data.keys[1] = inputHolder.keys[1];
            data.keys[2] = inputHolder.keys[2];

            dataSaved.Add(data);
        }else if(mode == EMode.EWrite)
        {
            inputHolder.positionInput = dataSaved[currentRecord].positionInput;
            inputHolder.directionInput = dataSaved[currentRecord].directionInput;
            inputHolder.keys[0] = dataSaved[currentRecord].keys[0];
            inputHolder.keys[1] = dataSaved[currentRecord].keys[1];
            inputHolder.keys[2] = dataSaved[currentRecord].keys[2];

            currentRecord = (currentRecord + 1)%dataSaved.Count;
        }
    }

    [System.Serializable]
    struct InputData
    {
        public Vector2 positionInput;
        public Vector2 directionInput;
        public bool[] keys;
    }
    List<InputData> dataSaved = new List<InputData>();
}
