namespace Utils
{
    [System.Serializable]
    public struct MinMaxFloat
    {
        public float min;
        public float max;

        public MinMaxFloat(float _min = 0, float _max = 0)
        {
            min = _min;
            max = _max;
        }

        public float Random => UnityEngine.Random.Range(min, max);
    }

    [System.Serializable]
    public struct MinMaxInt
    {
        public int min;
        public int max;

        public MinMaxInt(int _min = 0, int _max = 0)
        {
            min = _min;
            max = _max;
        }

        public int Random => UnityEngine.Random.Range(min, max);
    }
}
