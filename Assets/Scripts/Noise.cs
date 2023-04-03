using UnityEngine;
public static class Noise
{
    public enum NormalizeMode {Local, Global};
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight,int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode){
        float [,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;


        for(int i = 0; i < octaves;i++) 
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY  = prng.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY); 

            maxPossibleHeight += amplitude;
            amplitude*=persistance;
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }
        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth/2f;
        float halfHeight = mapHeight/2f;

        for(int i = 0;i<mapHeight;i++)
        {
            for(int j=0;j<mapWidth;j++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;


                for(int k=0;k<octaves;k++)
                {
                    float sampleX = (j - halfWidth + octaveOffsets[k].x)/scale * frequency;
                    float sampleY = (i - halfHeight + octaveOffsets[k].y)/scale * frequency;

                    float perlinVal = Mathf.PerlinNoise(sampleX, sampleY)*2-1;
                    noiseHeight+=perlinVal * amplitude;
                    amplitude*= persistance;
                    frequency*= lacunarity;
                }
                if (noiseHeight > maxLocalNoiseHeight){
                    maxLocalNoiseHeight = noiseHeight;
                } else if(noiseHeight<minLocalNoiseHeight){

                    minLocalNoiseHeight = noiseHeight;
                     
                }
                noiseMap[j, i] = noiseHeight;
            }
        }
        for(int j = 0;j<mapHeight;j++)
        {
            for(int i=0;i<mapWidth;i++)
            {
                if(normalizeMode == NormalizeMode.Local){
                    noiseMap[i, j] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[i, j]);

                }else{
                    float normalizedHeight = (noiseMap[i,j]+1)/(2f*maxPossibleHeight/1.5f);
                //noiseMap[i,j] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, normalizedHeight);
                noiseMap[i, j] = Mathf.Clamp(normalizedHeight, 0, 99999);
                }
            }
        }
        return noiseMap;
    }

}



