using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{   
    const float scale = 5f;
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    public LODInfo[] detailLevels;
    public static float maxViewDist;
    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    static MapGenerator mapGenerator;

    int chunkSize;
    int chunksVisibleInViewDst;
    
    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
    void Start(){
        mapGenerator = FindObjectOfType<MapGenerator>();
        maxViewDist = detailLevels[detailLevels.Length-1].visibleDistanceThreshold;
        
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDist/chunkSize);
        UpdateVisibleChunks();

    }
    void Update(){
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z)/scale;
        if((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate){
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
        //UpdateVisibleChunks();
    }
    void UpdateVisibleChunks(){

        for(int i=0;i<terrainChunksVisibleLastUpdate.Count;i++){
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for(int yOffset = -chunksVisibleInViewDst; yOffset<= chunksVisibleInViewDst;yOffset++)
        {
            for(int xOffset = -chunksVisibleInViewDst; xOffset<= chunksVisibleInViewDst;xOffset++)
            {
                Vector2 viewedChunkCoord =new Vector2(currentChunkCoordX+xOffset, currentChunkCoordY+yOffset);
                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord)){
                        terrainChunkDictionary[viewedChunkCoord].UpdateTerrain();
           
                }else{
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk (viewedChunkCoord, chunkSize, detailLevels,transform, mapMaterial));
                }
            
            }
        }
    
    }

    public class TerrainChunk {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;
        
        //MapData mapData;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
    
        MapData mapData;
        bool mapDataRecieved;
        int prevLODIndex = -1;

        public TerrainChunk(Vector2 coord, int size,LODInfo[] detailLevels, Transform parent, Material material)
        {
            this.detailLevels = detailLevels;
            position = coord*size;
            bounds = new Bounds(position, Vector2.one *size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk ");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material  = material;
            meshObject.transform.position = positionV3*scale;
            meshObject.transform.localScale = Vector3.one*scale;
            meshObject.transform.parent = parent;
            SetVisible(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for(int i=0;i<detailLevels.Length;i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrain );
            }
        

            mapGenerator.RequestMapData(position, onMapDataRecieved);

        }

        void onMapDataRecieved(MapData mapData)
        {
            this.mapData = mapData;
            mapDataRecieved = true; 
            Texture2D texture = TextureGenerator.TextureFromColourMap(mapData.colourMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;
            UpdateTerrain(); 
            
        }

        

        public void UpdateTerrain(){
            if(mapDataRecieved){
                float viewerDstFrmNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDstFrmNearestEdge<= maxViewDist;

                if(visible){
                    int lodIndex = 0;
                    for(int i=0;i<detailLevels.Length - 1;i++)
                    {
                        if(viewerDstFrmNearestEdge>detailLevels[i].visibleDistanceThreshold){
                            lodIndex = i+1;
                        }else{
                            break;
                        }
                    }
                    if(lodIndex!=prevLODIndex){
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if(lodMesh.hasMesh){
                            prevLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }else if(!lodMesh.hasRequestedMesh){
                            lodMesh.RequestMesh(mapData);

                        }
                    }

                    terrainChunksVisibleLastUpdate.Add(this);
                }

                SetVisible(visible);
            }

        }
        public void SetVisible(bool visible){
            meshObject.SetActive(visible);
        }
        public bool isVisible()
        {
            return meshObject.activeSelf;
        }

    }

    class LODMesh {

        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback){
            this.lod = lod;
            this.updateCallback = updateCallback;
        }
        void onMeshDataRecieved(MeshData meshData){
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();
        }
        public void RequestMesh(MapData mapData){
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, onMeshDataRecieved);
        }
        


    }
    [System.Serializable]
    public struct LODInfo{
        public int lod;
        public float visibleDistanceThreshold;
    }


}
