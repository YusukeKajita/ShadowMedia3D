using UnityEngine;
using System.Collections;
using System.Threading;

public class script_CIPC_For_Unity : MonoBehaviour
{

    #region private Data
    private CIPC_CS_Unity.CLIENT.CLIENT client;
    private Thread thread;
    public bool IsThreadAlived = false;
    private byte[] data = new byte[1];
    public byte[] Data
    {
        get
        {
            return this.data;
        }
    }
    #endregion
    #region CIPC Setting Data
    public string Name = "CIPC_U";
    public int MyPort = 52000;
    public string ServerIP = "127.0.0.1";
    public int ServerPort = 12000;
    public bool IsSender = true;
    public int test = 0;
    #endregion

    // Use this for initialization
	void Start () {
        this.CIPC_Init();
        this.thread_Init();
	}

    private void thread_Init()
    {
        this.thread = new Thread(this.threadingtask);
        this.IsThreadAlived = true;
        this.thread.Start();
    }

    private void threadingtask(object obj)
    {
        try
        {
            if (!this.IsSender)
            {
                while (this.IsThreadAlived)
                {
                    while (this.client.IsAvailable > 0)
                    {
                        this.client.Update(ref this.data);
                        this.ReceiveFunction();
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private void ReceiveFunction()
    {
        test = this.data.Length;
    }
	
	// Update is called once per frame
	void Update () {

	}

    void OnApplicationQuit()
    {
        this.CIPC_Close();
    }

    void CIPC_Close()
    {
        this.IsThreadAlived = false;
        this.thread.Join(500);
        this.client.Close();
    }
    void CIPC_Init()
    {
        this.client = new CIPC_CS_Unity.CLIENT.CLIENT(this.MyPort, this.ServerIP, this.ServerPort, this.Name, Application.targetFrameRate);
        this.client.Setup(IsSender ? CIPC_CS_Unity.CLIENT.MODE.Sender : CIPC_CS_Unity.CLIENT.MODE.Receiver);
    }

}
