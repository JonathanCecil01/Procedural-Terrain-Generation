using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;
public class MapGenerator : MonoBehaviour
{

    public enum DrawMode {NoiseMap, ColourMap, Mesh, FallOffMap};
    public DrawMode drawMode;
    public Noise.NormalizeMode normalizeMode;

    public const int mapChunkSize = 241;
    [Range(0, 6)]
    public int editorPreviewLOD;
    //public int mapChunkSize;
    //public int mapChunkSize;
    public float noiseScale;

    public int octaves;
  
    public float lacunarity;
    [Range(0, 1)]
    public float persistance;

    public int seed;
    public Vector2 offset;


    public bool useFallOff;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve; 
    public bool autoUpdate;

    public TerrainTypes[] regions; 

    float [,] fallOfMap;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();
    

    public GameObject Rabbit;

    public GameObject Carrot;

    void Awake(){
        fallOfMap = FallOffMapGenerator.GenerateFallOffMap(mapChunkSize);
    }
    
    public void DrawMapInEditor(){
        MapData mapData = GenerateMap(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay> ();

        if(drawMode == DrawMode.NoiseMap){
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if(drawMode == DrawMode.ColourMap){
            display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize ));

        }else if(drawMode == DrawMode.Mesh){
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD),TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize ));
        }else if(drawMode == DrawMode.FallOffMap){
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FallOffMapGenerator.GenerateFallOffMap(mapChunkSize)));
        }
    }


    public void RequestMapData(Vector2 centre, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate{
            MapDataThread(centre, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 centre, Action<MapData> callback)
    {
        MapData mapData = GenerateMap(centre);
        lock(mapDataThreadInfoQueue){
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }  
     public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate{
            MeshDataThread(mapData,lod, callback);
        };

        new Thread(threadStart).Start();   
    }

    void MeshDataThread(MapData mapData,int lod, Action<MeshData> callback)
    {
        
            MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve,lod);
            lock(meshDataThreadInfoQueue){
                meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
            }
    }

    void Update(){
        if(mapDataThreadInfoQueue.Count>0)
        {
            for(int i=0;i<mapDataThreadInfoQueue.Count;i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
        if(meshDataThreadInfoQueue.Count>0){
            for(int i=0;i<meshDataThreadInfoQueue.Count;i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }


   

    

    MapData GenerateMap(Vector2 centre){
        fallOfMap = FallOffMapGenerator.GenerateFallOffMap(mapChunkSize);
        float [,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, centre+offset, normalizeMode);
        
        Color[] colourMap = new Color[mapChunkSize*mapChunkSize];

        for(int y =0 ;y<mapChunkSize;y++){
            for(int x = 0 ;x<mapChunkSize;x++){
                if(useFallOff){
                    noiseMap[x, y] = Mathf.Clamp(noiseMap[x ,y]-fallOfMap[x, y], 0, 1);
                }
                float currentHeight = noiseMap[x, y];
                
                for(int i=0;i<regions.Length;i++){
                    if(currentHeight >= regions [i].height){
                        colourMap[ y*mapChunkSize+x] = regions[i].colour;
                    }else{
                        break;
                    }
                }
            }
        }
        return new MapData(noiseMap, colourMap);

        
    }

    void onValidate(){
        if(lacunarity<1){
            lacunarity = 1;
        }
        if(octaves<0){
            octaves = 0;
        }

        
        fallOfMap = FallOffMapGenerator.GenerateFallOffMap(mapChunkSize);
    }

    struct MapThreadInfo<T>{
        public readonly Action<T> callback;
        public readonly T parameter; 

        public MapThreadInfo(Action<T> callback, T parameter){
            this.callback = callback;
            this.parameter = parameter;
        }
    }

}


[System.Serializable]
public struct TerrainTypes{
    public string name;
    public float height;
    public Color colour;

}

public struct MapData{
    public float[,] heightMap;
    public Color[] colourMap;
    public MapData(float[, ] heightMap, Color[] colourMap)
    {
        this.colourMap = colourMap;
        this.heightMap = heightMap;
    }
}