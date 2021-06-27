using Godot;
using System;

public class Map : Spatial{
    
    private PackedScene resHexagon; 
    private MeshInstance selector, overSelector; 
    private Hexagon[] hexagons; 
    public MapData mapData; 

    public override void _EnterTree(){
        resHexagon = ResourceLoader.Load("res://scenes/Hexagon.tscn") as PackedScene; 
        selector = GetNode<MeshInstance>("Selector");
        overSelector = GetNode<MeshInstance>("OverSelector");
        
    }
    public override void _Ready(){ 
        instanceAllMap(); 
        moveSelector(0,0);
        //debug vecinos
        iGeo = new ImmediateGeometry();
        AddChild(iGeo);
    }

    public override void _Process(float delta){ 
        //indicadorVecinosDebug();
    }
   
    public void instanceAllMap() { 
        //hide selector
        moveSelector(-1,-1);
        //borra lo anterior
        if (hexagons != null){
            foreach (Hexagon hx in hexagons) hx.QueueFree();
        }
        
        //nuevos hexagon
        hexagons = new Hexagon[mapData.datas.Length];

        // instanciamos y asignamos las referencias
        for (int i = 0; i< mapData.datas.Length;i++){ 
            Hexagon hexagon = (Hexagon)resHexagon.Instance(); 
            HexaData hexaData = mapData.datas[i]; 
            hexagon.hexData = hexaData; 
            hexagon.hexData.hexagon = hexagon; //link with node object
            hexagons[i] = hexagon;
        }

        //al arbol de escenas y transladar
        foreach (Hexagon hexagon in hexagons){
            AddChild(hexagon); 
            hexagon.Create(); 
            Vector3 pos = mapData.getHexPosition(hexagon.hexData.row, hexagon.hexData.col); 
            hexagon.Translation = pos; 
        }

        GD.Print("Mapa instanciado!");
    }

    // EDIT TERRAIN
    public void changeHex(HexaData newHxd){
        //old
        HexaData hxdOld = mapData.GetHexaData(newHxd.row,newHxd.col);
        if (hxdOld == null)return;

        //change datas
        hxdOld = newHxd;

        //get neibourgs
        HexaData[] affecteds = new HexaData[7];
        affecteds[0] = newHxd;
        for (int i = 0; i< newHxd.neighbours.Length;i++){
            affecteds[i+1] = newHxd.neighbours[i];
        } 

        //update
        foreach (Hexagon hx in hexagons){
            foreach (HexaData affected in affecteds){
                if (affected == null)continue;
                if (hx.hexData.row == affected.row && hx.hexData.col == affected.col){
                    hx.Create();
                }
            }
        }
        
    }

    // SELECTOR 
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

    public void moveOver(int row, int col){
        HexaData hdOver = mapData.GetHexaData(row,col);
        if (hdOver == null){
            overSelector.Visible = false;

        }else{
            overSelector.Visible = true;
            Vector3 pos = mapData.getHexPosition(hdOver.row,hdOver.col);
            float height = Hexagon.HEIGHT_VALUE * hdOver.height + 0.1f;
            pos = pos + new Vector3(0,height,0);
            overSelector.Translation = pos;
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
            HexaData hdVecino = data.neighbours[i];
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
    //const
    public static int MAX_HEIGHT = 10;
    public readonly int row, col; 
    public HexaData[] neighbours = new HexaData[6]; // primero SE y continua sentido horario 
    // aspect
    public int colorIndex = 0,  height = 1; 
    
    // riber system, 
    public bool riber = false;
    public HexaData[] ribersOut = new HexaData[6]; //first SE and clock way: S, SW...

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

}
