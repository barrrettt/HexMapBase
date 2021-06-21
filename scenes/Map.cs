using Godot;
using System;
using System.Diagnostics;

public class Map : Spatial{
    
    private PackedScene resHexagon; 
    private MeshInstance selector;
    private Hexagon[] hexagons; 
    public MapData mapData; 

    public override void _EnterTree(){
        resHexagon = ResourceLoader.Load("res://scenes/Hexagon.tscn") as PackedScene; 
        selector = GetNode<MeshInstance>("Selector");
        
    }
    public override void _Ready(){ 
        //para pruebas
        generateDebugMap(); 
        instanceAllMap(); 
        moveSelector(0,0);
        //debug vecinos
        iGeo = new ImmediateGeometry();
        AddChild(iGeo);
    }

    public override void _Process(float delta){
        //indicadorVecinosDebug();
    }

    private void generateDebugMap(){
        mapData = new MapData(10);
        hexagons = new Hexagon[mapData.datas.Length];
        int index = 0; 

        for (int i = 0; i< mapData.datas.Length;i++){ 
            mapData.datas[i].colorIndex = index;
            mapData.datas[i].height = index+1; 
            index++; 
            if (index >= 10){
                index = 0;
            }
                
        }
    }

    private void instanceAllMap() { 
        
        if (hexagons == null){
            mapData = new MapData(1);
            hexagons = new Hexagon[1];
        }

        // Actualizar vista con los datos
        for (int i = 0; i< mapData.datas.Length;i++){ 
            Hexagon hexagon = (Hexagon)resHexagon.Instance(); 
            HexaData hexaData = mapData.datas[i]; 
            hexagon.hexData = hexaData; 
            hexagons[i] = hexagon;

            AddChild(hexagon); 
            hexagon.CreateHexagon(); 
            Vector3 pos = mapData.getHexPosition(hexaData.row,hexaData.col); 
            hexagon.Translation = pos; 
        }

        GD.Print("Mapa instanciado!");
    }

    //SELECTOR
    private HexaData hdSelected = null;
    public void moveSelector(int row, int col){
        HexaData hdSelected = mapData.GetHexaData(row,col);
        if (hdSelected == null){
            selector.Visible = false;

        }else{
            selector.Visible = true;
            Vector3 pos = mapData.getHexPosition(hdSelected.row,hdSelected.col);
            float height = Hexagon.HEIGHT_VALUE * hdSelected.height + 0.1f;
            pos = pos + new Vector3(0,height,0);
            selector.Translation = pos;
        }
    }

    // DEBUG CONEXIONES VECINOS 
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
        me.y = data.height * Hexagon.HEIGHT_VALUE +0.1f;
        me = me-Transform.origin;

        iGeo.Begin(Mesh.PrimitiveType.Lines);
        //pinta vecinos E
        for (int i=0;i<6;i++){
            HexaData hdVecino = data.vecinos[i];
            if (hdVecino != null){
                //GD.Print("Vecino "+ i+": "+hdVecino.ToString());
                iGeo.SetUv(new Vector2());
                iGeo.AddVertex(me);
                iGeo.SetUv(new Vector2(1,1));
                Vector3 pos = mapData.getHexPosition(hdVecino.row, hdVecino.col);
                pos.y = hdVecino.height * Hexagon.HEIGHT_VALUE + 0.1f;
                pos = pos-Transform.origin;
                iGeo.AddVertex(pos);
            }
        }
        iGeo.End();
    }

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
                centro.vecinos[0] = SE; 

                HexaData S = GetHexaData(row+1,col); 
                centro.vecinos[1] = S; 

                if (!colimpar){ 
                    oRow = row; oCol = col-1; 
                }else{ 
                    oRow = row+1; oCol = col-1; 
                } 
                HexaData SW = GetHexaData(oRow,oCol); 
                centro.vecinos[2] = SW; 

                if (!colimpar){ 
                    oRow = row-1; oCol = col-1; 
                }else{ 
                    oRow = row; oCol = col-1; 
                } 
                HexaData NW = GetHexaData(oRow,oCol); 
                centro.vecinos[3] = NW; 

                HexaData N = GetHexaData(row-1,col); 
                centro.vecinos[4] = N; 

                if (!colimpar){ 
                    oRow = row-1; oCol = col+1; 
                }else{ 
                    oRow = row; oCol = col+1; 
                } 
                HexaData NE = GetHexaData(oRow,oCol);  
                centro.vecinos[5] = NE; 

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
        float offset = 1f; //Hexagon.SIZE_TOP; 
        float outerRadius = 1.0f * offset; 
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
    public readonly int row, col; 
    public int colorIndex = 0,  height = 1; 

    // el primero es SE y continua sentido horario 
    public HexaData[] vecinos = new HexaData[6]; 

    public HexaData(int row, int col) { 
        this.row = row; 
        this.col = col; 
    }

    public override String ToString(){
        String str = String.Format("({0},{1})",this.row,this.col);
        return str;
    }
}