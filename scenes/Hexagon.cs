using Godot;
using System;
using System.Collections.Generic;

public class Hexagon : MeshInstance{
    public HexaData hexData;
    public Boolean isPC = true;

    // 10 colors
    public static Color[] colors = new Color[]{
        new Color("#022340"),//blue dark
        new Color("#115999"),//blue
        new Color("#859911"),//yellow
        new Color("#518513"),//y-green
        new Color("#1f5404"),//g
        new Color("#0b4003"), //green d
        new Color("#2e2a0a"), //broown
        new Color("#2e210a"), //b dark
        new Color("#5e4e38"), //b light
        new Color("#91816a"), //white
        new Color("#a3947e"), //white w
    };
    
    public static Color colorRiver = new Color("#115999"); //blue
    
    // STATICS METRICS 
    public static float SIZE_TOP = 0.75f; //0.75f; //radius top hex (1 max) 
    public static float HEIGHT_RIBER_OFFSET = 0.05f;
    public static float SIZE_RIBER = 0.50f;
    public static float HEIGHT_REAL_SEA = 0.40f;

    // Children geometry 
    private Spatial geo;
    private MeshInstance river;
    private MeshInstance sea;
    
    // Spawn valid detail zones 
    private bool[] detailZonesFree = new bool[]{true,true,true,true,true,true,true};
    private List<DetaillPlazed> detaillsPositions = new List<DetaillPlazed>();

    public override void _EnterTree(){
        //refs
        geo = (Spatial)GetNode("geo");
        river = (MeshInstance)geo.GetNode("River");
        sea = (MeshInstance)geo.GetNode("Sea");

        // mobil o PC
        string osname = Godot.OS.GetName();
        if (osname=="Android" || osname=="iOS" ) {
            isPC = false;
        }
       
    }

    public override void _Process(float delta){
        if (map == null || isPC) return;
        float dist = (map.cameraRayPosition - GlobalTransform.origin).LengthSquared();
        if (dist < 625){
            Visible = true;
        }else{
            Visible = false;
        }   
    }

    public static float getRealHeight(int heightIndex){
        float heightValue = 0.0f;
        switch (heightIndex){
            case 0: heightValue = 0.1f; break;
            case 1: heightValue = 0.2f; break;//water
            case 2: heightValue = 0.5f; break;//beach
            case 3: heightValue = 0.8f; break;//grass
            case 4: heightValue = 1.2f; break;//grass2
            case 5: heightValue = 1.5f; break;//forest
            case 6: heightValue = 2.0f; break;//mountain 1
            case 7: heightValue = 2.8f; break; 
            case 8: heightValue = 3.2f; break;
            case 9: heightValue = 3.6f; break;
            case 10: heightValue = 4.0f; break;
            default: heightValue = 4.6f+ (heightIndex-10)*0.4f; break;
        }
        return heightValue;
    }
    
    // Metrics 
    public float innerRadius;
    public float ang30;

    // Main vertex 
    public Vector3[] vertex = new Vector3[13];
    
    // Main Links vertex 
    public Vector3 pNEv1 = new Vector3(), pNEv2 = new Vector3(), pNEv3 = new Vector3(), pNEv4 = new Vector3(), 
    pSEv1 = new Vector3(), pSEv2= new Vector3(), pSEv3 = new Vector3(), pSEv4= new Vector3(), 
    pSv1= new Vector3(), pSv2 = new Vector3(), pSv3= new Vector3(), pSv4= new Vector3();
    
    // CREATE HEXAGON:
    private Map map;

     //Basic terrain geometry, before create!
    public void CreateHexMetrics(){
        //metics
        ang30 = (Mathf.Pi/6);// 30º slides
        float hValue = Hexagon.getRealHeight(hexData.height);
        innerRadius = (Mathf.Sqrt(3)/2)*SIZE_TOP; //med

        //MAIN vertices: TOP
        vertex[0] = new Vector3(0,hValue,0);//CENTRO
        vertex[1] = new Vector3(Mathf.Cos(ang30 * 1)* innerRadius, hValue, Mathf.Sin(ang30*1)* innerRadius);// med E y SE
        vertex[2] = new Vector3(Mathf.Cos(ang30 * 2)* SIZE_TOP, hValue, Mathf.Sin(ang30*2)* SIZE_TOP) ;//SE
        vertex[3] = new Vector3(Mathf.Cos(ang30 * 3)* innerRadius, hValue, Mathf.Sin(ang30*3)* innerRadius);// med
        vertex[4] = new Vector3(Mathf.Cos(ang30 * 4)* SIZE_TOP, hValue, Mathf.Sin(ang30*4)* SIZE_TOP);//SW
        vertex[5] = new Vector3(Mathf.Cos(ang30 * 5)* innerRadius, hValue, Mathf.Sin(ang30*5)* innerRadius);// med E y SE
        vertex[6] = new Vector3(Mathf.Cos(ang30 * 6)* SIZE_TOP, hValue, Mathf.Sin(ang30*6)* SIZE_TOP);//W
        vertex[7] = new Vector3(Mathf.Cos(ang30 * 7)* innerRadius, hValue, Mathf.Sin(ang30*7)* innerRadius);// med E y SE
        vertex[8] = new Vector3(Mathf.Cos(ang30 * 8)* SIZE_TOP, hValue, Mathf.Sin(ang30*8)* SIZE_TOP);//NW
        vertex[9] = new Vector3(Mathf.Cos(ang30 * 9)* innerRadius, hValue, Mathf.Sin(ang30*9)* innerRadius);// med E y SE
        vertex[10] = new Vector3(Mathf.Cos(ang30 * 10)* SIZE_TOP, hValue, Mathf.Sin(ang30*10)* SIZE_TOP);//NE
        vertex[11] = new Vector3(Mathf.Cos(ang30 * 11)* innerRadius, hValue, Mathf.Sin(ang30*11)* innerRadius);// med E y SE
        vertex[12] = new Vector3(Mathf.Cos(ang30 * 12)* SIZE_TOP, hValue, Mathf.Sin(ang30*12)* SIZE_TOP);//E
        
    }

    //create All
    public void Create(Map map){
        if (hexData == null) hexData = new HexaData(0,0);
        this.map = map;
        
        ShaderMaterial mat = (ShaderMaterial) MaterialOverride;
        //mat.VertexColorUseAsAlbedo= true;
        MaterialOverride = mat;

        SurfaceTool st = new SurfaceTool();
        st.SetMaterial(mat);
        st.Begin(Mesh.PrimitiveType.Triangles);

        createHexagon(st);  // Main, Unions and holes
        
        //Finally commit mesh
        st.GenerateNormals(); 
        st.GenerateTangents(); 
        Mesh = st.Commit(); 

        //PHYSICS COLLIDERS
        foreach (Node child in GetChildren()){
            if (child == geo) continue; 
            child.QueueFree(); //delete old physics
        }
        CreateTrimeshCollision(); //new physics

        //WATERS
        CreateRivers(st); //Rivers
        CreateSea(st); //Sea

        //DETAILS
        calculateValidSpawnDetailZones();
        CreateRocks(st); // rock On
        CreateGrass(st); // grass
        CreateTrees(st); // trees 

        //GAMEOBJECTS
        PlaceGO();

    }

    // Hexagon 
    private void createHexagon(SurfaceTool st){

        //cosas
        float dist =  innerRadius * 2/3;//distancia del puente
        float height = Hexagon.getRealHeight(hexData.height);

        //colors
        Color color = colors[hexData.colorIndex];//color del indexColor
        Color cOtherNE = color;
        Color cOtherSE = color;
        Color cOtherS = color;

        //TOP TRIS, NOW!
        GeoAux.createTri(st,vertex[0],vertex[12],vertex[2],color);//SE 
        GeoAux.createTri(st,vertex[0],vertex[2],vertex[4],color);//S  
        GeoAux.createTri(st,vertex[0],vertex[4],vertex[6],color);//SW 
        GeoAux.createTri(st,vertex[0],vertex[6],vertex[8],color);//NW 
        GeoAux.createTri(st,vertex[0],vertex[8],vertex[10],color);//N  
        GeoAux.createTri(st,vertex[0],vertex[10],vertex[12],color);//NE 

        float deltaH_NE = 0;
        float deltaH_SE = 0;
        float deltaH_S = 0;
        
        //NE
        HexaData hdNE = hexData.neighbours[5];
        if (hdNE != null){
            cOtherNE = colors[hdNE.colorIndex];
            pNEv1 = vertex[10];
            pNEv4 = vertex[12];
            float ang = Mathf.Pi/6; //30º
            float h = Hexagon.getRealHeight(hdNE.height);
            deltaH_NE = (h - height);
            Vector3 offset = new Vector3(Mathf.Cos(ang)*dist, deltaH_NE ,-Mathf.Sin(ang)*dist);
            pNEv2 = pNEv1 + offset;
            pNEv3 = pNEv4 + offset;
            if (deltaH_NE < 0) {
                GeoAux.createQuad(st,pNEv1,pNEv2,pNEv3,pNEv4,color);
            }else{
                GeoAux.createQuad(st,pNEv1,pNEv2,pNEv3,pNEv4,cOtherNE);
            }
            
        }
        
        //SE
        HexaData hdSE = hexData.neighbours[0];
        if (hdSE != null){
            cOtherSE = colors[hdSE.colorIndex];
            pSEv1 = vertex[12];
            pSEv4 = vertex[2];
            float ang = -Mathf.Pi/6; //-30º
            float h = Hexagon.getRealHeight(hdSE.height);
            deltaH_SE = (h- height);
            Vector3 offset = new Vector3(Mathf.Cos(ang)*dist,deltaH_SE,-Mathf.Sin(ang)*dist);
            pSEv2 = pSEv1 + offset;
            pSEv3 = pSEv4 + offset;
            if (deltaH_SE < 0) {
                GeoAux.createQuad(st,pSEv1,pSEv2,pSEv3,pSEv4,color);
            }else{
                GeoAux.createQuad(st,pSEv1,pSEv2,pSEv3,pSEv4,cOtherSE);
            }
        }
        
        //S
        HexaData hdS = hexData.neighbours[1];
        if (hdS != null){
            cOtherS = colors[hdS.colorIndex];
            pSv1 = vertex[2];
            pSv4 = vertex[4];
            float ang = -Mathf.Pi/2; //-90º
            float h = Hexagon.getRealHeight(hdS.height);
            deltaH_S = (h- height);
            Vector3 offset = new Vector3(Mathf.Cos(ang)*dist,deltaH_S,-Mathf.Sin(ang)*dist);
            pSv2 = pSv1 + offset;
            pSv3 = pSv4 + offset;
            if (deltaH_S < 0){ GeoAux.createQuad(st,pSv1,pSv2,pSv3,pSv4,color);
            }else{
                GeoAux.createQuad(st,pSv1,pSv2,pSv3,pSv4,cOtherS);
            }
        }
        
        //TRI Entre puentes NE-SE
        if (hdNE != null && hdSE != null){
            //GeoAux.createColorTri(st,pNEv4,pNEv3,pSEv2,color,cOtherNE,cOtherSE);
            if (deltaH_NE<=0 && deltaH_SE<=0){ 
                GeoAux.createTri(st,pNEv4,pNEv3,pSEv2,color);
            }else{
                if (deltaH_NE<deltaH_SE){
                    GeoAux.createTri(st,pNEv4,pNEv3,pSEv2,cOtherSE);
                }else{
                    GeoAux.createTri(st,pNEv4,pNEv3,pSEv2,cOtherNE);
                }
            }
            
        }
        //TRI entre puentes SE-S
        if (hdSE != null && hdS != null){
            //GeoAux.createColorTri(st,pSv1,pSEv3,pSv2,color,cOtherSE,cOtherS);
            if (deltaH_SE<=0 && deltaH_S <=0) {
                GeoAux.createTri(st,pSv1,pSEv3,pSv2,color);
            }else{
                if (deltaH_SE<deltaH_S){
                    GeoAux.createTri(st,pSv1,pSEv3,pSv2,cOtherS); 
                }else{
                    GeoAux.createTri(st,pSv1,pSEv3,pSv2,cOtherSE);
                }
            }
        }

        //FALDAS: si es borde de mapa(sin vecino) hay que tapar la cara
        int[][] pares = {
            new int[]{12,2},new int[]{2,4}, new int[]{4,6},
            new int[]{6,8},new int[]{8,10}, new int[]{10,12}
        };
        for (int i = 0;i<6;i++){
            if (hexData.neighbours[i] == null){
                int[] par = pares[i];
                int der = par[0]; int izq = par[1];
                Vector3 altoDer = vertex[der];
                Vector3 bajoDer = new Vector3(vertex[der].x,0,vertex[der].z) * (4/3f);
                Vector3 altoIzq = vertex[izq];
                Vector3 bajoIzq = new Vector3(vertex[izq].x,0,vertex[izq].z) * (4/3f);
                //Quad que baja recto en el hueco en donde no hay vecino
                GeoAux.createQuad(st,altoDer,bajoDer,bajoIzq,altoIzq,color);

                //en un sentido
                int anterior = i-1; if (anterior<0) anterior=5;
                if(hexData.neighbours[anterior] != null){
                    //si anterior es no es null -> hay un hueco triangular
                    Vector3 upDer = altoDer;
                    Vector3 downDer = bajoDer;
                    //el punto que falta es del vector del puente contrario a upDer, sabiendo la direccion del vecino conozco el punto
                    Vector3 upDerOtro = upDer; 
                    Color otroC = color;
                    if (par[0] == 12 && par[1]==2){ 
                        upDerOtro = pNEv3;
                        if (deltaH_NE>0){
                            otroC = cOtherNE;
                        }else{
                            otroC = color;
                        }
                    }
                    if (par[0] == 2 && par[1]==4){ 
                        upDerOtro = pSEv3; 
                        if (deltaH_SE>0){
                            otroC = cOtherSE;
                        }else{
                            otroC = color;
                        }
                    }
                    if (par[0] == 4 && par[1]==6){ 
                        upDerOtro = pSv3; 
                        
                        if (deltaH_S<0){
                            otroC = color;
                        }else{
                            otroC = cOtherS;
                        }
                    }
                    //tapa con el tri
                    //GeoAux.createColorTri(st,upDer,upDerOtro,downDer,color,otroC,color);
                    GeoAux.createTri(st,upDer,upDerOtro,downDer,otroC);
                }

                //en el otro sentido hay que tapar tambien los triangulos:
                int siguiente = i+1; if (siguiente>5) siguiente=0;
                if(hexData.neighbours[siguiente] != null){
                    Vector3 upIzq = altoIzq;
                    Vector3 downIzq = bajoIzq;
                    //el punto que falta es del puente contrario a UpIzq, sabiendo la direccion del vecino se conoce el punto
                    Vector3 upIzqOtro = upIzq; 
                    Color otroC = color;
                    if (par[0] == 8 && par[1]==10) {
                        upIzqOtro = pNEv2; 
                        if (deltaH_NE>0){
                            otroC = cOtherNE;
                        }else{
                            otroC = color;
                        }
                    }
                    if (par[0] == 10 && par[1]==12) {
                        upIzqOtro = pSEv2; 
                        if (deltaH_SE>0){ 
                            otroC = cOtherSE;
                        }else{
                            otroC = color;
                        }
                    }
                    if (par[0] == 12 && par[1]==2) {
                        upIzqOtro = pSv2; 
                        if (deltaH_S<0){
                            otroC = color;
                        }else{
                            otroC = cOtherS;
                        }
                    }
                    //tapa con el tri
                    //GeoAux.createColorTri(st,downIzq,upIzqOtro,upIzq,color,otroC,color);
                    GeoAux.createTri(st,downIzq,upIzqOtro,upIzq,otroC);
                }
            }
        }
    }

    // Rivers
    private void CreateRivers(SurfaceTool st){
        //Rivers vertex
        Vector3[] riverVertex = new Vector3[13];
        //River links Top
        Vector3 rpNEv1 = new Vector3(), rpNEv2 = new Vector3(), rpNEv3 = new Vector3(), rpNEv4 = new Vector3(),
        rpSEv1 = new Vector3(), rpSEv2 = new Vector3(), rpSEv3 = new Vector3(), rpSEv4 = new Vector3(),
        rpSv1 = new Vector3(), rpSv2 = new Vector3(), rpSv3 = new Vector3(), rpSv4 = new Vector3(),
        rpSWv1 = new Vector3(), rpSWv2 = new Vector3(), rpSWv3 = new Vector3(), rpSWv4 = new Vector3(),
        rpNWv1 = new Vector3(), rpNWv2 = new Vector3(), rpNWv3 = new Vector3(), rpNWv4 = new Vector3(),
        rpNv1 = new Vector3(), rpNv2 = new Vector3(), rpNv3 = new Vector3(), rpNv4 = new Vector3();
        //River links largues
        Vector3 pNEv1Link = new Vector3(), pNEv2Link = new Vector3(),pNEv3Link = new Vector3(),pNEv4Link = new Vector3(),
        pSEv1Link = new Vector3(), pSEv2Link = new Vector3(), pSEv3Link = new Vector3(), pSEv4Link = new Vector3(),
        pSv1Link = new Vector3(), pSv2Link = new Vector3(), pSv3Link = new Vector3(), pSv4Link = new Vector3();

        //RIVERS vertices top
        float h = Hexagon.getRealHeight(hexData.height);
        float hValue = h + HEIGHT_RIBER_OFFSET;
        float innerRadiusRiver = (Mathf.Sqrt(3)/2)*SIZE_RIBER; //med
        
        riverVertex[0] = new Vector3(0,hValue,0);//CENTRO
        riverVertex[1] = new Vector3(Mathf.Cos(ang30 * 1)* innerRadiusRiver, hValue, Mathf.Sin(ang30*1)* innerRadiusRiver);// med E y SE
        riverVertex[2] = new Vector3(Mathf.Cos(ang30 * 2)* SIZE_RIBER, hValue, Mathf.Sin(ang30*2)* SIZE_RIBER);//SE
        riverVertex[3] = new Vector3(Mathf.Cos(ang30 * 3)* innerRadiusRiver, hValue, Mathf.Sin(ang30*3)* innerRadiusRiver);//med 
        riverVertex[4] = new Vector3(Mathf.Cos(ang30 * 4)* SIZE_RIBER, hValue, Mathf.Sin(ang30*4)* SIZE_RIBER);//SW
        riverVertex[5] = new Vector3(Mathf.Cos(ang30 * 5)* innerRadiusRiver, hValue, Mathf.Sin(ang30*5)* innerRadiusRiver);//med
        riverVertex[6] = new Vector3(Mathf.Cos(ang30 * 6)* SIZE_RIBER, hValue, Mathf.Sin(ang30*6)* SIZE_RIBER);//W
        riverVertex[7] = new Vector3(Mathf.Cos(ang30 * 7)* innerRadiusRiver, hValue, Mathf.Sin(ang30*7)* innerRadiusRiver) ;//med
        riverVertex[8] = new Vector3(Mathf.Cos(ang30 * 8)* SIZE_RIBER, hValue, Mathf.Sin(ang30*8)* SIZE_RIBER);//NW
        riverVertex[9] = new Vector3(Mathf.Cos(ang30 * 9)* innerRadiusRiver, hValue, Mathf.Sin(ang30*9)* innerRadiusRiver);//med
        riverVertex[10] = new Vector3(Mathf.Cos(ang30 * 10)* SIZE_RIBER, hValue, Mathf.Sin(ang30*10)* SIZE_RIBER);//NE
        riverVertex[11] = new Vector3(Mathf.Cos(ang30 * 11)* innerRadiusRiver, hValue, Mathf.Sin(ang30*11)* innerRadiusRiver);//med
        riverVertex[12] = new Vector3(Mathf.Cos(ang30 * 12)* SIZE_RIBER, hValue, Mathf.Sin(ang30*12)* SIZE_RIBER);//E
            
        //ShaderMaterial matRiver = (ShaderMaterial)river.MaterialOverride;
        st.SetMaterial(map.matRiber);
        st.Begin(Mesh.PrimitiveType.Triangles);

        // Top links and inter links
        if (!hexData.river){
            st.GenerateNormals(); 
            river.Mesh = st.Commit(); 
            return;
        } 

        // Things 
        float distRiverTop = innerRadius - innerRadiusRiver;
        float distLink = innerRadius * 2/3;//distancia del puente
        Color color = colorRiver;

        // pintar centros? 
        bool linkSE = false, linkSE_in = false, linkSE_out = false;
        bool linkS = false, linkS_in = false, linkS_out = false;
        bool linkSW= false, linkSW_in = false, linkSW_out = false;
        bool linkNW = false, linkNW_in = false, linkNW_out = false;
        bool linkN = false, linkN_in = false, linkN_out = false;
        bool linkNE = false, linkNE_in = false, linkNE_out = false;
        
        int countRiverNeibours = 0;
        
        //metrics unions top links NE 
        rpNEv1 = riverVertex[10];
        rpNEv4 = riverVertex[12];
        float ang = Mathf.Pi/6; //30º 
        Vector3 offset = new Vector3(Mathf.Cos(ang)*distRiverTop,0,-Mathf.Sin(ang)*distRiverTop);
        rpNEv2 = rpNEv1 + offset;
        rpNEv3 = rpNEv4 + offset;

        //metrics unions top links SE 
        rpSEv1 = riverVertex[12];
        rpSEv4 = riverVertex[2];
        ang = -Mathf.Pi/6; //-30º 
        offset = new Vector3(Mathf.Cos(ang)*distRiverTop,0,-Mathf.Sin(ang)*distRiverTop);
        rpSEv2 = rpSEv1 + offset;
        rpSEv3 = rpSEv4 + offset;

        //metrics unions top links S 
        rpSv1 = riverVertex[2];
        rpSv4 = riverVertex[4];
        ang = -Mathf.Pi/2; //-90º 
        offset = new Vector3(Mathf.Cos(ang)*distRiverTop,0,-Mathf.Sin(ang)*distRiverTop);
        rpSv2 = rpSv1 + offset;
        rpSv3 = rpSv4 + offset;

        //metrics unions top links SW 
        rpSWv1 = riverVertex[4];
        rpSWv4 = riverVertex[6];
        ang = -Mathf.Pi*5/6; //-90º 
        offset = new Vector3(Mathf.Cos(ang)*distRiverTop,0,-Mathf.Sin(ang)*distRiverTop);
        rpSWv2 = rpSWv1 + offset;
        rpSWv3 = rpSWv4 + offset;

        //metrics unions top links NW 
        rpNWv1 = riverVertex[6];
        rpNWv4 = riverVertex[8];
        ang = Mathf.Pi*5/6; //150º 
        offset = new Vector3(Mathf.Cos(ang)*distRiverTop,0,-Mathf.Sin(ang)*distRiverTop);
        rpNWv2 = rpNWv1 + offset;
        rpNWv3 = rpNWv4 + offset;

        //metrics unions top links N 
        rpNv1 = riverVertex[8];
        rpNv4 = riverVertex[10];
        ang = Mathf.Pi*3/6; //90º 
        offset = new Vector3(Mathf.Cos(ang)*distRiverTop,0,-Mathf.Sin(ang)*distRiverTop);
        rpNv2 = rpNv1 + offset;
        rpNv3 = rpNv4 + offset;

        // Rivers IN or OUT?
        HexaData hdNE = hexData.neighbours[5];
        if (hdNE != null){
            linkNE_out = hexData.riversOut[5] == hdNE;
            linkNE_in = hdNE.riversOut[2] == hexData;
            if (hdNE.river && (linkNE_out|| linkNE_in)){
                linkNE = true;
                countRiverNeibours++;
            }
        }
        HexaData hdSE = hexData.neighbours[0];
        if (hdSE != null){
            linkSE_out = hexData.riversOut[0] == hdSE;
            linkSE_in = hdSE.riversOut[3] == hexData;
            if (hdSE.river && (linkSE_out ||linkSE_in)){
                linkSE = true;
                countRiverNeibours++;
            }
        }
        HexaData hdS = hexData.neighbours[1];
        if (hdS != null){
            linkS_out = hexData.riversOut[1] == hdS;
            linkS_in = hdS.riversOut[4] == hexData;
            if (hdS.river && (linkS_out || linkS_in)){
                linkS = true;
                countRiverNeibours++;
            }
        }
        HexaData hdSW = hexData.neighbours[2];
        if (hdSW != null){
            linkSW_out = hexData.riversOut[2] == hdSW;
            linkSW_in = hdSW.riversOut[5] == hexData;
            if (hdSW.river && (linkSW_out || linkSW_in)){
                linkSW = true;
                countRiverNeibours++;
            }
        }
        HexaData hdNW = hexData.neighbours[3];
        if (hdNW != null){
            linkNW_out = hexData.riversOut[3] == hdNW;
            linkNW_in = hdNW.riversOut[0] == hexData;
            if (hdNW.river && (linkNW_out || linkNW_in)){
                linkNW = true;
                countRiverNeibours++;       
            }
        }
        HexaData hdN = hexData.neighbours[4];
        if (hdN != null){
            linkN_out = hexData.riversOut[4] == hdN;
            linkN_in = hdN.riversOut[1] == hexData;
            if (hdN.river && (linkN_out || linkN_in)){
                linkN = true;
                countRiverNeibours++;
            }
        }
        //LINKS TOP (bajo agua no)
        if (!hexData.water){
            if (linkNE) GeoAux.createQuad(st,rpNEv1,rpNEv2,rpNEv3,rpNEv4,color);
            if (linkSE) GeoAux.createQuad(st,rpSEv1,rpSEv2,rpSEv3,rpSEv4,color);
            if (linkS)  GeoAux.createQuad(st,rpSv1,rpSv2,rpSv3,rpSv4,color);
            if (linkSW) GeoAux.createQuad(st,rpSWv1,rpSWv2,rpSWv3,rpSWv4,color);
            if (linkNW) GeoAux.createQuad(st,rpNWv1,rpNWv2,rpNWv3,rpNWv4,color);
            if (linkN) GeoAux.createQuad(st,rpNv1,rpNv2,rpNv3,rpNv4,color);
        }

        //LARGUES LINKS 
        if (linkNE){
            h = Hexagon.getRealHeight(hdNE.height);
            float otherH = h + HEIGHT_RIBER_OFFSET;
            float deltaH = otherH - hValue;
            pNEv1Link = rpNEv2;
            pNEv4Link = rpNEv3;
            ang = Mathf.Pi*1/6; //30º 
            offset = new Vector3(Mathf.Cos(ang) * distLink, deltaH, -Mathf.Sin(ang) * distLink);
            pNEv2Link = pNEv1Link + offset;
            pNEv3Link = pNEv4Link + offset;
            if (deltaH != 0){
                //drop water -> regular link
                GeoAux.createQuad(st,pNEv1Link,pNEv2Link,pNEv3Link,pNEv4Link,color);
            }else{
                float displacement =  GeoAux.FloatRange(map.random,-1,1) * SIZE_RIBER / 5;
                Vector3 dirnormalRandom = (pNEv2Link - pNEv3Link).Normalized() * displacement;
                Vector3 pmedNEv1v2 = pNEv1Link + (pNEv2Link - pNEv1Link)/2;
                Vector3 pmedNEv4v3 = pNEv4Link + (pNEv3Link - pNEv4Link)/2;
                Vector3 dV1 = pmedNEv1v2 + dirnormalRandom;
                Vector3 dV2 = pmedNEv4v3 + dirnormalRandom;
                GeoAux.createQuad(st,pNEv1Link,dV1,dV2,pNEv4Link,color);
                GeoAux.createQuad(st,dV1,pNEv2Link,pNEv3Link,dV2,color);
            }
        }

        if (linkSE){
            h = Hexagon.getRealHeight(hdSE.height);
            float otherH = h + HEIGHT_RIBER_OFFSET;
            float deltaH = otherH - hValue;
            pSEv1Link = rpSEv2;
            pSEv4Link = rpSEv3;

            ang = Mathf.Pi*-1/6; //-30º 
            offset = new Vector3(Mathf.Cos(ang) * distLink, 0, -Mathf.Sin(ang) * distLink);
            offset.y = deltaH;
            
            pSEv2Link = pSEv1Link + offset;
            pSEv3Link = pSEv4Link + offset;
            
            if (deltaH != 0){
                //drop water -> regular link
                GeoAux.createQuad(st,pSEv1Link,pSEv2Link,pSEv3Link,pSEv4Link,color);
            }else{
                float displacement =  GeoAux.FloatRange(map.random,-1,1) * SIZE_RIBER / 5;
                Vector3 dirnormalRandom = (pSEv2Link - pSEv3Link).Normalized() * displacement;
                Vector3 pmedSEv1v2 = pSEv1Link + (pSEv2Link - pSEv1Link)/2;
                Vector3 pmedSEv4v3 = pSEv4Link + (pSEv3Link - pSEv4Link)/2;
                Vector3 dV1 = pmedSEv1v2 + dirnormalRandom;
                Vector3 dV2 = pmedSEv4v3 + dirnormalRandom;
                GeoAux.createQuad(st,pSEv1Link,dV1,dV2,pSEv4Link,color);
                GeoAux.createQuad(st,dV1,pSEv2Link,pSEv3Link,dV2,color);
            }
        }

        if (linkS){
            h = Hexagon.getRealHeight(hdS.height);
            float otherH = h + HEIGHT_RIBER_OFFSET;
            float deltaH = otherH - hValue;
            pSv1Link = rpSv2;
            pSv4Link = rpSv3;
            ang = -Mathf.Pi*3/6; //-90º 
            offset = new Vector3(Mathf.Cos(ang) * distLink, deltaH, -Mathf.Sin(ang) * distLink);
            pSv2Link = pSv1Link + offset;
            pSv3Link = pSv4Link + offset;
            if (deltaH != 0){
                //drop water -> regular link
                GeoAux.createQuad(st,pSv1Link,pSv2Link,pSv3Link,pSv4Link,color);
            }else{
                float displacement =  GeoAux.FloatRange(map.random,-1,1) * SIZE_RIBER / 5;
                Vector3 dirnormalRandom = (pSv2Link - pSv3Link).Normalized() * displacement;
                Vector3 pmedSv1v2 = pSv1Link + (pSv2Link - pSv1Link)/2;
                Vector3 pmedSv4v3 = pSv4Link + (pSv3Link - pSv4Link)/2;
                Vector3 dV1 = pmedSv1v2 + dirnormalRandom;
                Vector3 dV2 = pmedSv4v3 + dirnormalRandom;
                GeoAux.createQuad(st,pSv1Link,dV1,dV2,pSv4Link,color);
                GeoAux.createQuad(st,dV1,pSv2Link,pSv3Link,dV2,color);
            }
        }

        //CENTRO PERO CON 12 TRIS (bajo agua no)
        if (!hexData.water){
            if (countRiverNeibours < 2  || countRiverNeibours > 3){
                linkSE = linkS = linkSW = linkNW = linkN = linkNE = true;// un lago 
            }
            if (linkSE){
                GeoAux.createTri(st,riverVertex[0],riverVertex[12],riverVertex[2],color);//SE 0 
            } 
            if (linkS){
                GeoAux.createTri(st,riverVertex[0],riverVertex[2],riverVertex[4],color);//S  1 
            } 
            if (linkSW){
                GeoAux.createTri(st,riverVertex[0],riverVertex[4],riverVertex[6],color);//SW 2 
            } 
            if (linkNW){
                GeoAux.createTri(st,riverVertex[0],riverVertex[6],riverVertex[8],color);//NW 3 
            }
            if (linkN){
                GeoAux.createTri(st,riverVertex[0],riverVertex[8],riverVertex[10],color);//N  4 
            } 
            if (linkNE){
                GeoAux.createTri(st,riverVertex[0],riverVertex[10],riverVertex[12],color);//NE 5
            } 

            //Tapar huecos feos del rio
            if (countRiverNeibours>1){
                if (linkSE && linkSW){
                    GeoAux.createTri(st,riverVertex[0],riverVertex[2],riverVertex[4],color);
                }
                if (linkSE && linkNW){
                    GeoAux.createTri(st,riverVertex[0],riverVertex[2],riverVertex[6],color);
                    GeoAux.createTri(st,riverVertex[0],riverVertex[8],riverVertex[12],color);
                }
                if (linkSE && linkN){
                    GeoAux.createTri(st,riverVertex[0],riverVertex[10],riverVertex[12],color);
                }
                if (linkS && linkNW){
                    GeoAux.createTri(st,riverVertex[0],riverVertex[4],riverVertex[6],color);
                }
                if (linkS && linkN){
                    GeoAux.createTri(st,riverVertex[0],riverVertex[10],riverVertex[2],color);
                    GeoAux.createTri(st,riverVertex[0],riverVertex[4],riverVertex[8],color);
                }
                if (linkS && linkNE){
                    GeoAux.createTri(st,riverVertex[0],riverVertex[12],riverVertex[2],color);
                }
                if (linkSW && linkN){
                    GeoAux.createTri(st,riverVertex[0],riverVertex[6],riverVertex[8],color);
                }
                if (linkSW && linkNE){
                    GeoAux.createTri(st,riverVertex[0],riverVertex[6],riverVertex[10],color);
                    GeoAux.createTri(st,riverVertex[0],riverVertex[12],riverVertex[4],color);
                }
                if (linkNE && linkNW){
                    GeoAux.createTri(st,riverVertex[0],riverVertex[8],riverVertex[10],color);
                }
            }
        }


        //finaly
        st.GenerateNormals(); 
        river.Mesh = st.Commit(); 
    }

    //water
    private void CreateSea(SurfaceTool st){
        //ShaderMaterial matSea = (ShaderMaterial)sea.MaterialOverride;
        st.SetMaterial(map.matSea);
        st.Begin(Mesh.PrimitiveType.Triangles);

        // Top links and inter links
        if (hexData.height>1) {
            st.GenerateNormals(); 
            sea.Mesh = st.Commit(); 
            return;
        }
         float hValue = HEIGHT_REAL_SEA;
        float distwater = innerRadius * 2/3;//water radius
        float ang30 = (Mathf.Pi/6);// 30º slides
        Color color = colorRiver;

        //el top pero con mas altura
        Vector3[] watervertex = new Vector3[13];

        for (int i = 0; i<vertex.Length; i++){
            Vector3 v = vertex[i];
            watervertex[i] = new Vector3(v.x,HEIGHT_REAL_SEA,v.z);
        }

        //top water
        GeoAux.createTri(st,watervertex[0],watervertex[12],watervertex[2],color);//SE 
        GeoAux.createTri(st,watervertex[0],watervertex[2],watervertex[4],color);//S  
        GeoAux.createTri(st,watervertex[0],watervertex[4],watervertex[6],color);//SW 
        GeoAux.createTri(st,watervertex[0],watervertex[6],watervertex[8],color);//NW 
        GeoAux.createTri(st,watervertex[0],watervertex[8],watervertex[10],color);//N 
        GeoAux.createTri(st,watervertex[0],watervertex[10],watervertex[12],color);//NE 

        // links with other
        HexaData hdNE = hexData.neighbours[5];
        Vector3 pwNEv1 = new Vector3(pNEv1.x,HEIGHT_REAL_SEA,pNEv1.z);
        Vector3 pwNEv2 = new Vector3(pNEv2.x,HEIGHT_REAL_SEA,pNEv2.z);
        Vector3 pwNEv3 = new Vector3(pNEv3.x,HEIGHT_REAL_SEA,pNEv3.z);
        Vector3 pwNEv4 = new Vector3(pNEv4.x,HEIGHT_REAL_SEA,pNEv4.z);

        HexaData hdSE = hexData.neighbours[0];
        Vector3 pwSEv1 = new Vector3(pSEv1.x,HEIGHT_REAL_SEA,pSEv1.z);
        Vector3 pwSEv2 = new Vector3(pSEv2.x,HEIGHT_REAL_SEA,pSEv2.z);
        Vector3 pwSEv3 = new Vector3(pSEv3.x,HEIGHT_REAL_SEA,pSEv3.z);
        Vector3 pwSEv4 = new Vector3(pSEv4.x,HEIGHT_REAL_SEA,pSEv4.z);

        HexaData hdS = hexData.neighbours[1];
        Vector3 pwSv1 = new Vector3(pSv1.x,HEIGHT_REAL_SEA,pSv1.z);
        Vector3 pwSv2 = new Vector3(pSv2.x,HEIGHT_REAL_SEA,pSv2.z);
        Vector3 pwSv3 = new Vector3(pSv3.x,HEIGHT_REAL_SEA,pSv3.z);
        Vector3 pwSv4 = new Vector3(pSv4.x,HEIGHT_REAL_SEA,pSv4.z);

        //siempre se construyen estes:
        GeoAux.createQuad(st,pwNEv1,pwNEv2,pwNEv3,pwNEv4,color); //NE
        GeoAux.createQuad(st,pwSEv1,pwSEv2,pwSEv3,pwSEv4,color); //SE
        GeoAux.createQuad(st,pwSv1,pwSv2,pwSv3,pwSv4,color); //S
        
        //tri holes links
        GeoAux.createTri(st,pwNEv4,pwNEv3,pwSEv2,color);//NE-SE
        GeoAux.createTri(st,pwSv1,pwSEv3,pwSv2,color); //SE-S

        // links with ground (no water)
        HexaData hdSW = hexData.neighbours[2];
        Vector3 pwSWv1 = watervertex[4];
        Vector3 pwSWv4 = watervertex[6];
        float ang = ang30*7;
        Vector3 offset = new Vector3(Mathf.Cos(ang) * distwater, 0, -Mathf.Sin(ang) * distwater);
        Vector3 pwSWv2 = pwSWv1 + offset;
        Vector3 pwSWv3 = pwSWv4 + offset;
        
        HexaData hdNW = hexData.neighbours[3];
        ang = ang30*5; 
        offset = new Vector3(Mathf.Cos(ang) * distwater, 0, -Mathf.Sin(ang) * distwater);
        Vector3 pwNWv1 = watervertex[6];
        Vector3 pwNWv4= watervertex[8];
        Vector3 pwNWv2 = pwNWv1 + offset;
        Vector3 pwNWv3= pwNWv4 + offset;

        HexaData hdN = hexData.neighbours[4];
        ang = ang30*3; 
        offset = new Vector3(Mathf.Cos(ang) * distwater, 0, -Mathf.Sin(ang) * distwater);
        Vector3 pwNv1 = watervertex[8];
        Vector3 pwNv4 = watervertex[10];
        Vector3 pwNv2 = pwNv1 + offset;
        Vector3 pwNv3 = pwNv4 + offset;

        if (hdSW != null && !hdSW.water){
            GeoAux.createQuad(st,pwSWv1,pwSWv2,pwSWv3,pwSWv4,color);
            if (hdS!= null) 
                GeoAux.createTri(st,pwSv4,pwSv3,pwSWv2,color);//S-SW
        }
        
        if (hdNW != null && !hdNW.water){
            GeoAux.createQuad(st,pwNWv1,pwNWv2,pwNWv3,pwNWv4,color);
            if (hdSW!= null && !hdSW.water) 
                GeoAux.createTri(st,pwSWv4,pwSWv3,pwNWv2,color);//SW-NW
        }
        
        if (hdN != null && !hdN.water){
            GeoAux.createQuad(st,pwNv1,pwNv2,pwNv3,pwNv4,color);
            if (hdNW!= null && !hdNW.water)
                GeoAux.createTri(st,pwNWv4,pwNWv3,pwNv2,color);//NW-N
        }

        //ultimo tri superior
        if (hdN != null && !hdN.water){
            if (hdNE!= null )
                GeoAux.createTri(st,pwNv4,pwNv3,pwNEv2,color);//N-NE
        }
        
        //finaly
        st.GenerateNormals(); 
        sea.Mesh = st.Commit(); 
    }

    //Detail: Rocks
    private void CreateRocks(SurfaceTool st){
        //Rocks
        MeshInstance rock = geo.GetNodeOrNull("rocks") as MeshInstance;
        if (rock != null)
            rock.Free();
        rock = new MeshInstance();
        rock.Name = ("rocks");
        rock.MaterialOverride = map.matRock;

        //surface tool
        st.SetMaterial(map.matRock);
        st.Begin(Mesh.PrimitiveType.Triangles);

        // no rocks when:
        if (hexData.getHeight()<3){
            rock.Mesh = st.Commit();
            geo.AddChild(rock);
            return;
        }
        
        //poblate
        int numRocks = 1; // max rocks
        int h = hexData.getHeight();
        int[] rockcolors = new int[]{7,8,8};
        
        switch(h){
            case 10:
                rockcolors = new int[]{9,10,10};
                break;
            case 9:
                rockcolors = new int[]{8,9,9};
                break;
            case 8:
                rockcolors = new int[]{7,8,8};
                numRocks = 2;
                break;
            case 7:
                numRocks = 2;
                break;
            case 6:
                numRocks = 2;
                break;
        }

        float poblate = GeoAux.FloatRange(map.random, 0, 1.0f);
        if (h==3 && poblate < 0.75f) numRocks = 0;
        if (h==4 && poblate < 0.50f) numRocks = 0;
        if (h==5 && poblate < 0.30f) numRocks = 0;

        int rndi = map.random.Next(0,rockcolors.Length);
        Color color = colors[rockcolors[rndi]];

        float variation = innerRadius * 0.01f;
        float sizeDetail = 0.2f;

        for (int i = 0;i<numRocks;i++){
            float scale = GeoAux.FloatRange(map.random, 0.1f, 0.5f);
            Vector3 pos = getValidFreeDetailPosition(variation,sizeDetail);
            if (pos != Vector3.Zero){
                Rock.createVertex(st,map.random,pos,scale,color);
                //detaillsPositions.Add(new DetaillPlazed(pos,sizeDetail)); //free rocks
            }            
        }

        //finaly
        st.GenerateNormals(); 
        rock.Mesh = st.Commit();
        geo.AddChild(rock);
    }

    //Detail: Grass
    private void CreateGrass(SurfaceTool st) {
         //Rocks
        MeshInstance grass = geo.GetNodeOrNull("grass") as MeshInstance;
        if (grass != null)
            grass.Free();
        grass = new MeshInstance();
        grass.Name = ("grass");
        grass.MaterialOverride = map.matGrass;

        //surface tool
        st.SetMaterial(map.matGrass);
        st.Begin(Mesh.PrimitiveType.Triangles);

        //no grass when:
        int h = hexData.getHeight();
        if (hexData.river || h<3 || h>5){
            grass.Mesh = st.Commit();
            geo.AddChild(grass);
            return;
        }

        //place positions:
        int count = 4;
        int subinstances = 1;
        float variation = innerRadius * 0.01f;
        float subvariation = 0.2f;
        float sizeDetail = 0.1f;

        for (int i = 0;i<count;i++){
            float scale = GeoAux.FloatRange(map.random, 0.4f, 0.5f);
            Vector3 pos = getValidFreeDetailPosition(variation,sizeDetail);

            if (pos != Vector3.Zero){
                //detaillsPositions.Add(new DetaillPlazed(pos,sizeDetail)); // free grass

                //add random subvariation on plane XZ to zone position
                for (int j = 0;j<subinstances;j++){
                    float x = GeoAux.FloatRange(map.random,-subvariation,subvariation); 
                    float y = 0; //height 
                    float z = GeoAux.FloatRange(map.random,-subvariation,subvariation); 
                    Vector3 subpos = pos + new Vector3(x,y,z); // add subvariation
                    Grass.createVertex(st,map.random,subpos,scale);
                }
            }            
        }

        //finaly
        st.GenerateNormals(); 
        grass.Mesh = st.Commit();
        geo.AddChild(grass);
    }

    //Detail: Tree
    private void CreateTrees(SurfaceTool st){
        MeshInstance trees = geo.GetNodeOrNull("trees") as MeshInstance;
        if (trees != null) trees.Free();
        trees = new MeshInstance();
        trees.Name = ("trees");
        trees.MaterialOverride = map.matTree;

        //surface tool
        st.SetMaterial(map.matTree);
        st.Begin(Mesh.PrimitiveType.Triangles);

        //height on object child
        float height = vertex[0].y;
        trees.Translation = new Vector3(trees.Translation.x,height,trees.Translation.z);

        //colors trees
        Color cWood =  Tree.COLOR_WOOD;
        Color cFoliage =  Tree.COLOR_FOLIAGE;

        //exit when:
        int h = hexData.getHeight();
        if (hexData.river || h<3 || h>5){
            trees.Mesh = st.Commit();
            geo.AddChild(trees);
            return;
        }

        //place trees on top hex
        int count = 2; // max trees
        float poblate = GeoAux.FloatRange(map.random, 0, 1.0f);
        if (h==3 && poblate < 0.75f) count = 0;
        if (h==4 && poblate < 0.50f) count = 0;
        
        float variation = innerRadius * 0.01f;
        float sizeDetail = 0.8f;

        for (int i = 0;i<count;i++){
            float scale = GeoAux.FloatRange(map.random, 0.3f, 0.4f);
            Vector3 pos = getValidFreeDetailPosition(variation,sizeDetail);
            pos.y = 0; // position y on transform

            if (pos != Vector3.Zero){
                Tree.createVertex(st,map.random,pos,scale,cWood,cFoliage);
                detaillsPositions.Add(new DetaillPlazed(pos,sizeDetail));
            }            
        }

        //finaly
        st.GenerateNormals(); 
        trees.Mesh = st.Commit();
        geo.AddChild(trees);
    }

    //AUXs terrain details positions
    private void calculateValidSpawnDetailZones(){

        //zones: 0-center, 1-SE, 2-S,...
        //linkzones: no yet
        detailZonesFree = new bool[]{true,true,true,true,true,true,true};
        detaillsPositions = new List<DetaillPlazed>();
    
        //if exist riber, road or bigdetail invalide zone
        int countRiverNeibours = 0;

        if (hexData.river) {
            detailZonesFree[0] = false;
        }

        // Rivers IN or OUT?
        HexaData hdNE = hexData.neighbours[5];
        if (hdNE != null){
            bool linkNE_out = hexData.riversOut[5] == hdNE;
            bool linkNE_in = hdNE.riversOut[2] == hexData;
            if (hdNE.river && (linkNE_out|| linkNE_in)){
                detailZonesFree[6] = false; // has riber, no details here
                countRiverNeibours++;
            }
        }
        HexaData hdSE = hexData.neighbours[0];
        if (hdSE != null){
            bool linkSE_out = hexData.riversOut[0] == hdSE;
            bool linkSE_in = hdSE.riversOut[3] == hexData;
            if (hdSE.river && (linkSE_out ||linkSE_in)){
                detailZonesFree[1] = false;
                countRiverNeibours++;            
            }
        }
        HexaData hdS = hexData.neighbours[1];
        if (hdS != null){
            bool linkS_out = hexData.riversOut[1] == hdS;
            bool linkS_in = hdS.riversOut[4] == hexData;
            if (hdS.river && (linkS_out || linkS_in)){
                detailZonesFree[2] = false;
                countRiverNeibours++;
            }
        }
        HexaData hdSW = hexData.neighbours[2];
        if (hdSW != null){
            bool linkSW_out = hexData.riversOut[2] == hdSW;
            bool linkSW_in = hdSW.riversOut[5] == hexData;
            if (hdSW.river && (linkSW_out || linkSW_in)){
                detailZonesFree[3] = false;
                countRiverNeibours++;
            }
        }
        HexaData hdNW = hexData.neighbours[3];
        if (hdNW != null){
            bool linkNW_out = hexData.riversOut[3] == hdNW;
            bool linkNW_in = hdNW.riversOut[0] == hexData;
            if (hdNW.river && (linkNW_out || linkNW_in)){
                detailZonesFree[4] = false;
                countRiverNeibours++;  
            }
        }
        HexaData hdN = hexData.neighbours[4];
        if (hdN != null){
            bool linkN_out = hexData.riversOut[4] == hdN;
            bool linkN_in = hdN.riversOut[1] == hexData;
            if (hdN.river && (linkN_out || linkN_in)){
                detailZonesFree[5] = false;
                countRiverNeibours++;
            }
        }

        //small espaces? ->  no valid spawn zone:
        if (!detailZonesFree[1] && !detailZonesFree[3]) detailZonesFree[2] = false;
        if (!detailZonesFree[2] && !detailZonesFree[4]) detailZonesFree[3] = false;
        if (!detailZonesFree[3] && !detailZonesFree[5]) detailZonesFree[4] = false;
        if (!detailZonesFree[4] && !detailZonesFree[6]) detailZonesFree[5] = false;
        if (!detailZonesFree[5] && !detailZonesFree[1]) detailZonesFree[6] = false;
        if (!detailZonesFree[6] && !detailZonesFree[2]) detailZonesFree[1] = false;


        //center lake?
        if (hexData.river && (countRiverNeibours < 2  || countRiverNeibours > 3)){
            detailZonesFree[0] = false;
            detailZonesFree[1] = false;
            detailZonesFree[2] = false;
            detailZonesFree[3] = false;
            detailZonesFree[4] = false;
            detailZonesFree[5] = false;
            detailZonesFree[6] = false;
        }
    }
    private Vector3 getValidFreeDetailPosition(float variationRadio, float sizeDetail){
        Vector3 zonePosition = Vector3.Zero;
        List<int> validzones = new List<int>();

        for (int i = 0;i<detailZonesFree.Length;i++){
            if (detailZonesFree[i]) validzones.Add(i);
        }
        
        if (validzones.Count== 0) return zonePosition; //0, no free zones

        //random valid zone
        int index = map.random.Next(0,validzones.Count);
        int zone = validzones[index];

        switch(zone){
            case 0: 
            zonePosition = vertex[0]; break;
            case 1: 
            zonePosition = vertex[0] + ((vertex[1] - vertex[0])/2); break;
            case 2: 
            zonePosition = vertex[0] + ((vertex[3] - vertex[0])/2); break;
            case 3: 
            zonePosition = vertex[0] + ((vertex[5] - vertex[0])/2); break;
            case 4: 
            zonePosition = vertex[0] + ((vertex[7] - vertex[0])/2); break;
            case 5: 
            zonePosition = vertex[0] + ((vertex[9] - vertex[0])/2); break;
            case 6: 
            zonePosition = vertex[0] + ((vertex[11] - vertex[0])/2); break;
        }

        // ckeck free radius
        int tries = 10;
        for (int i=0;i<tries;i++){ 
            
            //add random variation on plane XZ to zone position
            float x = GeoAux.FloatRange(map.random,-variationRadio,variationRadio); 
            float y = 0; //height 
            float z = GeoAux.FloatRange(map.random,-variationRadio,variationRadio); 
            Vector3 detailPosition = zonePosition + new Vector3(x,y,z); // add variation

            bool isFree = true;//check

            foreach (DetaillPlazed d in detaillsPositions){
                float lenght = (d.position - detailPosition).Length();
                if (lenght < (sizeDetail + d.sizeDetail)){
                    isFree = false;
                    break;
                }
            }

            //return position if correct
            if (isFree){
                return detailPosition;
            }
        }

        //zero if incorrect position
        return Vector3.Zero;
    }

    //Place GameObjects
    private void PlaceGO() {
        Spatial go = geo.GetNodeOrNull("go") as Spatial;
        if (go != null) go.Free();
        
        if (hexData.indexGO < 0){
            return;
        }

        //instancing..
        PackedScene psGO = map.resGOs[hexData.indexGO];
        go = (Spatial)psGO.Instance(); 
        go.Name = "go";

        //Translate and add child
        geo.AddChild(go); 
        float height = vertex[0].y;
        go.Translation = new Vector3(go.Translation.x,height,go.Translation.z);
    }

}

class DetaillPlazed{
    public readonly Vector3 position = Vector3.Zero;
    public readonly float sizeDetail = 0f;

    public DetaillPlazed(Vector3 position, float sizeDetail){
        this.position = position;
        this.sizeDetail = sizeDetail;
    }
}