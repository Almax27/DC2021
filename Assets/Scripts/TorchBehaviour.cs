using UnityEngine;

public enum TorchType
{
    Ground,
    Ceiling
}


public class TorchBehaviour : MonoBehaviour
{
    public TorchType torchType = TorchType.Ground;
}
