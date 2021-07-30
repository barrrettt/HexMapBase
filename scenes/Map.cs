using Godot;
using System;

public class Map : Spatial{
    
    //resources
    private PackedScene resHexagon; 

    public ShaderMaterial matSea,matRiber,matRock,matGrass,matTree;

    private MeshInstance selector, overSelector; 
    private Hexagon[] hexagons; 
    public MapData mapData; 
    public Random random;

    public Vector3 cameraRayPosition = Vector3.Zero;

    public override void _EnterTree(){
        //resources
        resHexagon = ResourceLoader.Load("res://scenes/Hexagon.tscn") as PackedScene;
        matSea = ResourceLoader.Load("res://src/shaders_materials/sea_vs.material") as ShaderMaterial;
        matRiber = ResourceLoader.Load("res://src/shaders_materials/riber_vs.material") as ShaderMaterial;
        matRock = ResourceLoader.Load("res://src/shaders_materials/rock_vs.material") as ShaderMaterial;
        matGrass = ResourceLoader.Load("res://src/shaders_materials/grass.material") as ShaderMaterial;
        matTree = ResourceLoader.Load("res://src/shaders_materials/tree_vs.material") as ShaderMaterial;
        
        //references to selectors
        selector = GetNode<MeshInstance>("Selector");
        overSelector = GetNode<MeshInstance>("OverSelector");
        
        //random for procedural generation
        random = new Random();
    }
    
    public override void _Ready(){ 
        //debug vecinos
        //iGeo = new ImmediateGeometry();
        //AddChild(iGeo);
    }
    public override void _Process(float delta){ 
        //indicadorVecinosDebug();
    }
    
    
    public void instanceAllMap(Random random) {
        this.random = random;

        //hide indicators
        moveSelector(-1,-1);
        moveOver(-1,-1);

        // delete old map
        if (hexagons != null){
            foreach (Hexagon hx in hexagons) hx.QueueFree();
        }
        
        //new array hexagons
        hexagons = new Hexagon[mapData.datas.Length];

        // referencing and instancing...
        for (int i = 0; i< mapData.datas.Length;i++){ 
            Hexagon hexagon = (Hexagon)resHexagon.Instance(); 
            
            HexaData hexaData = mapData.datas[i]; 
            hexagon.hexData = hexaData; 
            hexagon.CreateHexMetrics();//main geometry now

            hexagon.hexData.hexagon = hexagon; 
            hexagons[i] = hexagon;
            hexagon.Name = String.Format("H {0},{1}",hexaData.row,hexaData.col);
        }

        // to scene tree and translate
        foreach (Hexagon hexagon in hexagons){
            AddChild(hexagon); 
            Vector3 pos = mapData.getHexPosition(hexagon.hexData.row, hexagon.hexData.col); 
            hexagon.Translation = pos; 
            hexagon.Create(this);//rest of geometry 
        }

        GD.Print("Map ready!");
    }

    // EDIT TERRAIN
    private void CreateAffectedHex(HexaData newHxd){
        //old
        HexaData hxdOld = mapData.GetHexaData(newHxd.row,newHxd.col);
        if (hxdOld == null)return;

        //change datas
        newHxd.hexagon = hxdOld.hexagon;
        hxdOld = newHxd;

        //get neibourgs
        HexaData[] affecteds = new HexaData[7];
        affecteds[0] = newHxd;
        for (int i = 0; i< newHxd.neighbours.Length;i++){
            affecteds[i+1] = newHxd.neighbours[i];
        }

        // update neigourgs
        foreach (HexaData affected in affecteds){
            if (affected == null)continue;
            affected.hexagon.CreateHexMetrics();
            affected.hexagon.Create(this);
           
        }
    }

    public void upTerrain(HexaData hxd){
        if (hxd == null) return;
        if(hxd.setHeight(hxd.getHeight()+1)){
            hxd.clearRivers();
            CreateAffectedHex(hxd);
        }
    }

    public void downTerrain(HexaData hxd){
        if (hxd == null) return;
        if(hxd.setHeight(hxd.getHeight()-1)){
            hxd.clearRivers();
            CreateAffectedHex(hxd);
        }
    }

    public void up2Terrain(HexaData hxd){
        if (hxd == null) return;
        HexaData[] terrain = new HexaData[7];
        terrain[0] = hxd;
        for (int i = 1; i<terrain.Length;i++){
            terrain[i] = hxd.neighbours[i-1];
        }

        foreach (HexaData hdt in terrain){
            upTerrain(hdt);
        }
    }

    public void down2Terrain(HexaData hxd){
        if (hxd == null) return;
        HexaData[] terrain = new HexaData[7];
        terrain[0] = hxd;
        for (int i = 1; i<terrain.Length;i++){
            terrain[i] = hxd.neighbours[i-1];
        }

        foreach (HexaData hdt in terrain){
            downTerrain(hdt);
        }
    }

    public void createRiver(HexaData hxdOrigin, HexaData hxdEnd){
        if (hxdOrigin == null || hxdEnd == null)return;
        //river cant up
        if (hxdOrigin.getHeight()< hxdEnd.getHeight()) 
            return;
        //river not in water
        if (hxdOrigin.water) 
            return;

        bool found = false;
        for (int i = 0; i< hxdOrigin.neighbours.Length; i++){
            if (hxdEnd == hxdOrigin.neighbours[i]) {
                //Origin and end are Neighbours
                hxdOrigin.river = true;
                hxdEnd.river = true;
                hxdOrigin.riversOut[i] = hxdOrigin.neighbours[i];
                found = true;
                break;
            }
        }
        if (!found){ 
            return;
        };

        CreateAffectedHex(hxdOrigin);
        CreateAffectedHex(hxdEnd);
    }

    public void cleanRivers(HexaData hxd){
        if (hxd == null) return;
        hxd.clearRivers();
        CreateAffectedHex(hxd);
    }

    // SELECTOR 
    public void moveSelector(int row, int col){
        HexaData hdSelected = mapData.GetHexaData(row,col);
        if (hdSelected == null){
            selector.Visible = false;

        }else{
            selector.Visible = true;
            Vector3 pos = mapData.getHexPosition(hdSelected.row,hdSelected.col);
            float height = hdSelected.hexagon.getRealHeight() + 0.1f;
            pos = pos + new Vector3(0,height,0);
            selector.Translation = pos;
        }
    }

    public void moveOver(int row, int col){
        HexaData hdOver = mapData.GetHexaData(row,col);
        if (hdOver == null){
            overSelector.Visible = false;

        }else{
            overSelector.Visible = true;
            Vector3 pos = mapData.getHexPosition(hdOver.row,hdOver.col);
            float height = hdOver.hexagon.getRealHeight() + 0.1f;
            pos = pos + new Vector3(0,height,0);
            overSelector.Translation = pos;
        }
    }

    // DEBUG CONEXIONES VECINOS 
    /*
    private ImmediateGeometry iGeo;  //debug vecinos 
    private int indexActualDebug = 0; 
    private uint lastTime = 0; 
    private void indicadorVecinosDebug(){
        iGeo.Clear();
        //vertices
        uint t = OS.GetTicksMsec();
        if (t-lastTime>500){
            indexActualDebug++;
            lastTime = t;
            if (indexActualDebug >= mapData.datas.Length) indexActualDebug = 0;
        }
        HexaData data = mapData.datas[indexActualDebug];
        if (data == null) return;
        Vector3 me = mapData.getHexPosition(data.row,data.col);
        me.y = data.hexagon.getRealHeight() +0.2f;
        me = me-Transform.origin;

        iGeo.Begin(Mesh.PrimitiveType.Lines);
        //pinta vecinos E
        for (int i=0;i<6;i++){
            HexaData hdVecino = data.neighbours[i];
            if (hdVecino != null){
                //GD.Print("Vecino "+ i+": "+hdVecino.ToString());
                iGeo.SetUv(new Vector2());
                iGeo.AddVertex(me);
                iGeo.SetUv(new Vector2(1,1));
                Vector3 pos = mapData.getHexPosition(hdVecino.row, hdVecino.col);
                pos.y = data.hexagon.getRealHeight() +0.1f;
                pos = pos-Transform.origin;
                iGeo.AddVertex(pos);
            }
        }
        iGeo.End();
    }
*/

}

//clase para los datos del mapa
public class MapData{
    private int sizeMap;
    public HexaData[] datas; 

    public MapData(int sizeMap){
        this.sizeMap = sizeMap;
        this.datas = new HexaData[sizeMap*sizeMap];
        
        // Creamos hexdatas
        for (int row = 0;row<sizeMap;row++){
            for(int col = 0;col<sizeMap;col++){
                HexaData data = new HexaData(row,col);
                int index = coordToListIndex(row,col);
                this.datas[index] = data;
            }
        }

        // Conectamos vecinos: 
        for (int row = 0;row<sizeMap;row++){
            for(int col = 0;col<sizeMap;col++){

                // Centro
                HexaData centro = GetHexaData(row,col); 

                // Columnas impares estan desplazadas por debajo y tienen distintos vecinos
                bool colimpar = (col%2 != 0);

                // Centro con otros
                int oRow = col,oCol = row; 
                if (!colimpar){ 
                    oRow = row; oCol = col+1; 
                }else{ 
                    oRow = row+1; oCol = col+1; 
                } 
                HexaData SE = GetHexaData(oRow,oCol); 
                centro.neighbours[0] = SE; 

                HexaData S = GetHexaData(row+1,col); 
                centro.neighbours[1] = S; 

                if (!colimpar){ 
                    oRow = row; oCol = col-1; 
                }else{ 
                    oRow = row+1; oCol = col-1; 
                } 
                HexaData SW = GetHexaData(oRow,oCol); 
                centro.neighbours[2] = SW; 

                if (!colimpar){ 
                    oRow = row-1; oCol = col-1; 
                }else{ 
                    oRow = row; oCol = col-1; 
                } 
                HexaData NW = GetHexaData(oRow,oCol); 
                centro.neighbours[3] = NW; 

                HexaData N = GetHexaData(row-1,col); 
                centro.neighbours[4] = N; 

                if (!colimpar){ 
                    oRow = row-1; oCol = col+1; 
                }else{ 
                    oRow = row; oCol = col+1; 
                } 
                HexaData NE = GetHexaData(oRow,oCol);  
                centro.neighbours[5] = NE; 

            }
        }
        GD.Print("Datos creados");
    }

    private int coordToListIndex(int row, int col){
        int index = (row * sizeMap) + (col);
        return index;
    }

    public HexaData GetHexaData(int row, int col){
        if (row<0 || row>=sizeMap || col<0 || col>=sizeMap) return null;
        int index = coordToListIndex(row,col);
        return this.datas[index];
    }

    public Vector3 getHexPosition(int row, int col){
        HexaData data = GetHexaData(row,col);
        if (data == null) return Vector3.Zero;

        //circunferencias del hexagono 
        float outerRadius = 1.0f;
        float innerRadius = outerRadius * (Mathf.Sqrt(3)/2); 
        float sin60 = Mathf.Sin(Mathf.Pi/3); 

        //posiciones cuadriculadas 
        Vector3 z = new Vector3(0,0, 2 * outerRadius * sin60); 
        Vector3 x = new Vector3(( 1.5f * outerRadius), 0,0); 
        
        //a vector r3 
        Vector3 pos = new Vector3(); 
        pos = (col * x) + (row * z); 

        //columnas impares bajan el inner
        if (col %2 != 0){ 
            pos.z += innerRadius;
        }

        return pos; 
    }

    public int getSize(){return sizeMap;}
} 

// clase para datos del tile 
public class HexaData{ 
    //const
    public static int MAX_HEIGHT = 10;
    public static int WATER_LEVEL = 2;
    public readonly int row, col; 
    public HexaData[] neighbours = new HexaData[6]; // primero SE y continua sentido horario 
    // aspect
    public int colorIndex = 0;
    public int height = 1; 
    public bool water = false;
    
    // river system, 
    public bool river = false; 
    public HexaData[] riversOut = new HexaData[6]; //first SE and clock way: S, SW...

    //reference with escene object
    public Hexagon hexagon;

    public HexaData(int row, int col) { 
        this.row = row; 
        this.col = col; 
    }

    public override String ToString(){
        String str = String.Format("({0},{1})",this.row,this.col);
        return str;
    }

    public bool setHeight(int newheight){
        if (newheight<0 || newheight > MAX_HEIGHT) return false;
        water = (newheight < WATER_LEVEL); 
        colorIndex = newheight;
        this.height = newheight;
        return true;
    }

    public int getHeight(){
        return height;
    }

    public void clearRivers(){
        river = false;
        riversOut = new HexaData[6];
    }

}
