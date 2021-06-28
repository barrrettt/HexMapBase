using Godot;
using System;

public class Hexagon : MeshInstance{
    public HexaData hexData;
    
    // 10 colors
    private Color[] colors = new Color[]{
        new Color("#022340"),//blue dark
        new Color("#115999"),//blue
        new Color("#859911"),//yellow
        new Color("#518513"),//y-green
        new Color("#1f5404"),//g
        new Color("#0c2b02"), //green d
        new Color("#2e2a0a"), //broown
        new Color("#2e210a"), //b dark
        new Color("#5e4e38"), //b light
        new Color("#ccc3b8"), //white
        new Color("#ffffff"), //white w
    };

    private Color colorRiber = new Color("#115999");//blue
    
    //STATICS METRICS
    public static float SIZE_TOP = 0.75f;//0.75f; //radius top hex
    public static float HEIGHT_RIBER_OFFSET = 0.05f;
    public static float SIZE_RIBER = 0.50f;
    public float getRealHeight(){
        float heightValue = 0.4f;
        int height = hexData.getHeight();
        if (height>5) heightValue = 0.5f;
        if (height>8) heightValue = 0.6f;
        return heightValue * height;
    }
    
    // Main vertex
    public Vector3[] vertex = new Vector3[13];
    //Main Links vertex
    public Vector3 pNEv1 = new Vector3(), pNEv2 = new Vector3(), pNEv3 = new Vector3(), pNEv4 = new Vector3(), 
    pSEv1 = new Vector3(), pSEv2= new Vector3(), pSEv3 = new Vector3(), pSEv4= new Vector3(), 
    pSv1= new Vector3(), pSv2 = new Vector3(), pSv3= new Vector3(), pSv4= new Vector3();
    //Rivers vertex
    public Vector3[] riberVertex = new Vector3[13];
    //River links Top
    public Vector3 rpNEv1 = new Vector3(), rpNEv2 = new Vector3(), rpNEv3 = new Vector3(), rpNEv4 = new Vector3(),
    rpSEv1 = new Vector3(), rpSEv2 = new Vector3(), rpSEv3 = new Vector3(), rpSEv4 = new Vector3(),
    rpSv1 = new Vector3(), rpSv2 = new Vector3(), rpSv3 = new Vector3(), rpSv4 = new Vector3(),
    rpSWv1 = new Vector3(), rpSWv2 = new Vector3(), rpSWv3 = new Vector3(), rpSWv4 = new Vector3(),
    rpNWv1 = new Vector3(), rpNWv2 = new Vector3(), rpNWv3 = new Vector3(), rpNWv4 = new Vector3(),
    rpNv1 = new Vector3(), rpNv2 = new Vector3(), rpNv3 = new Vector3(), rpNv4 = new Vector3();
    //River links largues
    public Vector3 pNEv1Link = new Vector3(), pNEv2Link = new Vector3(),pNEv3Link = new Vector3(),pNEv4Link = new Vector3(),
    pSEv1Link = new Vector3(), pSEv2Link = new Vector3(), pSEv3Link = new Vector3(), pSEv4Link = new Vector3(),
    pSv1Link = new Vector3(), pSv2Link = new Vector3(), pSv3Link = new Vector3(), pSv4Link = new Vector3();

    // CREATE HEXAGON:
    private Random random;
    public void Create(Random random){
        if (hexData == null) hexData = new HexaData(0,0);
        this.random = random;
        
        SpatialMaterial mat = (SpatialMaterial) MaterialOverride;
        SurfaceTool st = new SurfaceTool();
        st.SetMaterial(mat);
        st.Begin(Mesh.PrimitiveType.Triangles);
        
        CreateHexMetrics(st); // Main hexagon and metrics
        CreateHexUnions(st);  //Unions and holes
        
        // FINALLY 
        st.GenerateNormals(); 
        st.GenerateTangents(); 
        Mesh = st.Commit(); 

        //Physic mesh 
        foreach(Node child in GetChildren()){child.QueueFree();}
        CreateTrimeshCollision();

        //Rivers 
        CreateRivers(st);
    }

     //Basic terrain
    private void CreateHexMetrics(SurfaceTool st){
        //metics
        float angle = (Mathf.Pi/6);// 30º slides
        float hValue = getRealHeight();
        float innerRadius = (Mathf.Sqrt(3)/2)*SIZE_TOP; //med

        //vertices
        vertex[0] = new Vector3(0,hValue,0);//CENTRO
        vertex[1] = new Vector3(Mathf.Cos(angle * 1)* innerRadius, hValue, Mathf.Sin(angle*1)* innerRadius);// med E y SE
        vertex[2] = new Vector3(Mathf.Cos(angle * 2)* SIZE_TOP, hValue, Mathf.Sin(angle*2)* SIZE_TOP) ;//SE
        vertex[3] = new Vector3(Mathf.Cos(angle * 3)* innerRadius, hValue, Mathf.Sin(angle*3)* innerRadius);// med
        vertex[4] = new Vector3(Mathf.Cos(angle * 4)* SIZE_TOP, hValue, Mathf.Sin(angle*4)* SIZE_TOP);//SW
        vertex[5] = new Vector3(Mathf.Cos(angle * 5)* innerRadius, hValue, Mathf.Sin(angle*5)* innerRadius);// med E y SE
        vertex[6] = new Vector3(Mathf.Cos(angle * 6)* SIZE_TOP, hValue, Mathf.Sin(angle*6)* SIZE_TOP);//W
        vertex[7] = new Vector3(Mathf.Cos(angle * 7)* innerRadius, hValue, Mathf.Sin(angle*7)* innerRadius);// med E y SE
        vertex[8] = new Vector3(Mathf.Cos(angle * 8)* SIZE_TOP, hValue, Mathf.Sin(angle*8)* SIZE_TOP);//NW
        vertex[9] = new Vector3(Mathf.Cos(angle * 9)* innerRadius, hValue, Mathf.Sin(angle*9)* innerRadius);// med E y SE
        vertex[10] = new Vector3(Mathf.Cos(angle * 10)* SIZE_TOP, hValue, Mathf.Sin(angle*10)* SIZE_TOP);//NE
        vertex[11] = new Vector3(Mathf.Cos(angle * 11)* innerRadius, hValue, Mathf.Sin(angle*11)* innerRadius);// med E y SE
        vertex[12] = new Vector3(Mathf.Cos(angle * 12)* SIZE_TOP, hValue, Mathf.Sin(angle*12)* SIZE_TOP);//E
        
        Color color = colors[hexData.colorIndex];//color del indexColor
        GeoAux.createTri(st,vertex[0],vertex[12],vertex[2],color);//SE 
        GeoAux.createTri(st,vertex[0],vertex[2],vertex[4],color);//S  
        GeoAux.createTri(st,vertex[0],vertex[4],vertex[6],color);//SW 
        GeoAux.createTri(st,vertex[0],vertex[6],vertex[8],color);//NW 
        GeoAux.createTri(st,vertex[0],vertex[8],vertex[10],color);//N  
        GeoAux.createTri(st,vertex[0],vertex[10],vertex[12],color);//NE 
    }

    private void CreateHexUnions(SurfaceTool st){

        //cosas
        float innerRadius = 1f * (Mathf.Sqrt(3)/2)*SIZE_TOP; 
        float dist = 2*innerRadius/3;//distancia del puente
        float height = getRealHeight();

        //colors
        Color color = colors[hexData.colorIndex];//color del indexColor
        Color cOtherNE = colors[hexData.colorIndex];
        Color cOtherE = colors[hexData.colorIndex];
        Color cOtherS = colors[hexData.colorIndex];
        
        //NE
        HexaData hdNE = hexData.neighbours[5];
        if (hdNE != null){
            cOtherNE = colors[hdNE.colorIndex];
            pNEv1 = vertex[10];
            pNEv4 = vertex[12];
            float ang30 = Mathf.Pi/6; //30º
            float deltaH = (hdNE.hexagon.getRealHeight()- height);
            Vector3 offset = new Vector3(Mathf.Cos(ang30)*dist,deltaH,-Mathf.Sin(ang30)*dist);
            pNEv2 = pNEv1 + offset;
            pNEv3 = pNEv4 + offset;
            GeoAux.createColorQuad(st,pNEv1,pNEv2,pNEv3,pNEv4,cOtherNE,color);
        }
        
        //SE
        HexaData hdSE = hexData.neighbours[0];
        if (hdSE != null){
            cOtherE = colors[hdSE.colorIndex];
            pSEv1 = vertex[12];
            pSEv4 = vertex[2];
            float ang = -Mathf.Pi/6; //-30º
            float deltaH = (hdSE.hexagon.getRealHeight()- height);
            Vector3 offset = new Vector3(Mathf.Cos(ang)*dist,deltaH,-Mathf.Sin(ang)*dist);
            pSEv2 = pSEv1 + offset;
            pSEv3 = pSEv4 + offset;
            GeoAux.createColorQuad(st,pSEv1,pSEv2,pSEv3,pSEv4,cOtherE,color);
        }
        
        //S
        HexaData hdS = hexData.neighbours[1];
        if (hdS != null){
            cOtherS = colors[hdS.colorIndex];
            pSv1 = vertex[2];
            pSv4 = vertex[4];
            float ang = -Mathf.Pi/2; //-90º
            float deltaH = (hdS.hexagon.getRealHeight()- height);
            Vector3 offset = new Vector3(Mathf.Cos(ang)*dist,deltaH,-Mathf.Sin(ang)*dist);
            pSv2 = pSv1 + offset;
            pSv3 = pSv4 + offset;
            GeoAux.createColorQuad(st,pSv1,pSv2,pSv3,pSv4,cOtherS,color);
        }
        
        //TRI Entre puentes NE-SE
        if (hdNE != null && hdSE != null){
            GeoAux.createColorTri(st,pNEv4,pNEv3,pSEv2,color,cOtherNE,cOtherE);
        }
        //TRI entre puentes SE-S
        if (hdSE != null && hdS != null){
            GeoAux.createColorTri(st,pSv1,pSEv3,pSv2,color,cOtherE,cOtherS);
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
                GeoAux.createColorQuad(st,altoDer,bajoDer,bajoIzq,altoIzq,color,color);

                //en un sentido
                int anterior = i-1; if (anterior<0) anterior=5;
                if(hexData.neighbours[anterior] != null){
                    //si anterior es no es null -> hay un hueco triangular
                    Vector3 upDer = altoDer;
                    Vector3 downDer = bajoDer;
                    //el punto que falta es del vector del puente contrario a upDer, sabiendo la direccion del vecino conozco el punto
                    Vector3 upDerOtro = upDer; Color otroC = color;
                    if (par[0] == 12 && par[1]==2){ upDerOtro = pNEv3; otroC = cOtherNE;}
                    if (par[0] == 2 && par[1]==4){ upDerOtro = pSEv3; otroC = cOtherE;}
                    if (par[0] == 4 && par[1]==6){ upDerOtro = pSv3; otroC = cOtherS;}
                    //tapa con el tri
                    GeoAux.createColorTri(st,upDer,upDerOtro,downDer,color,otroC,color);
                }

                //en el otro sentido hay que tapar tambien los triangulos:
                int siguiente = i+1; if (siguiente>5) siguiente=0;
                if(hexData.neighbours[siguiente] != null){
                    Vector3 upIzq = altoIzq;
                    Vector3 downIzq = bajoIzq;
                    //el punto que falta es del puente contrario a UpIzq, sabiendo la direccion del vecino se conoce el punto
                    Vector3 upIzqOtro = upIzq; Color otroC = color;
                    if (par[0] == 8 && par[1]==10) {upIzqOtro = pNEv2; otroC = cOtherNE;}
                    if (par[0] == 10 && par[1]==12) {upIzqOtro = pSEv2; otroC = cOtherE;}
                    if (par[0] == 12 && par[1]==2) {upIzqOtro = pSv2; otroC = cOtherS;}
                    //tapa con el tri
                    GeoAux.createColorTri(st,downIzq,upIzqOtro,upIzq,color,otroC,color);
                }
            }
        }
        
    }

    // Ribers
    private void CreateRivers(SurfaceTool st){
        if (!hexData.riber) return;
        SpatialMaterial matRiver = new SpatialMaterial();
        st.SetMaterial(matRiver);

        //metics
        float angle =  (Mathf.Pi/6);//doceavos de circunferencias
        float hValue = getRealHeight() + HEIGHT_RIBER_OFFSET;
        float innerRadius = (Mathf.Sqrt(3)/2)*SIZE_RIBER; //med
        
        riberVertex[0] = new Vector3(0,hValue,0);//CENTRO
        riberVertex[1] = new Vector3(Mathf.Cos(angle * 1)* innerRadius, hValue, Mathf.Sin(angle*1)* innerRadius);// med E y SE
        riberVertex[2] = new Vector3(Mathf.Cos(angle * 2)* SIZE_RIBER, hValue, Mathf.Sin(angle*2)* SIZE_RIBER);//SE
        riberVertex[3] = new Vector3(Mathf.Cos(angle * 3)* innerRadius, hValue, Mathf.Sin(angle*3)* innerRadius);//med 
        riberVertex[4] = new Vector3(Mathf.Cos(angle * 4)* SIZE_RIBER, hValue, Mathf.Sin(angle*4)* SIZE_RIBER);//SW
        riberVertex[5] = new Vector3(Mathf.Cos(angle * 5)* innerRadius, hValue, Mathf.Sin(angle*5)* innerRadius);//med
        riberVertex[6] = new Vector3(Mathf.Cos(angle * 6)* SIZE_RIBER, hValue, Mathf.Sin(angle*6)* SIZE_RIBER);//W
        riberVertex[7] = new Vector3(Mathf.Cos(angle * 7)* innerRadius, hValue, Mathf.Sin(angle*7)* innerRadius) ;//med
        riberVertex[8] = new Vector3(Mathf.Cos(angle * 8)* SIZE_RIBER, hValue, Mathf.Sin(angle*8)* SIZE_RIBER);//NW
        riberVertex[9] = new Vector3(Mathf.Cos(angle * 9)* innerRadius, hValue, Mathf.Sin(angle*9)* innerRadius);//med
        riberVertex[10] = new Vector3(Mathf.Cos(angle * 10)* SIZE_RIBER, hValue, Mathf.Sin(angle*10)* SIZE_RIBER);//NE
        riberVertex[11] = new Vector3(Mathf.Cos(angle * 11)* innerRadius, hValue, Mathf.Sin(angle*11)* innerRadius);//med
        riberVertex[12] = new Vector3(Mathf.Cos(angle * 12)* SIZE_RIBER, hValue, Mathf.Sin(angle*12)* SIZE_RIBER);//E


        // Top links and inter links
        CreateRiverUnions(st);

        //finaly
        st.GenerateNormals(); 
        st.GenerateTangents(); 
        Mesh = st.Commit(); 
    }
    
    private void CreateRiverUnions(SurfaceTool st){
        // Things 
        float innerRadius = (Mathf.Sqrt(3)/2)*SIZE_TOP; 
        float innerRadiusRiber = (Mathf.Sqrt(3)/2)*SIZE_RIBER; 
        float distRiberTop = innerRadius - innerRadiusRiber;
        float hValue = getRealHeight() + HEIGHT_RIBER_OFFSET;
        float distLink = 2*innerRadius/3;//distancia del puente
        Color color = colorRiber;

        // pintar centros? 
        bool linkSE = false, linkSE_in = false, linkSE_out = false;
        bool linkS = false, linkS_in = false, linkS_out = false;
        bool linkSW= false, linkSW_in = false, linkSW_out = false;
        bool linkNW = false, linkNW_in = false, linkNW_out = false;
        bool linkN = false, linkN_in = false, linkN_out = false;
        bool linkNE = false, linkNE_in = false, linkNE_out = false;
        
        int countRiberNeibours = 0;
        
        //metrics unions top links NE 
        rpNEv1 = riberVertex[10];
        rpNEv4 = riberVertex[12];
        float ang = Mathf.Pi/6; //30º 
        Vector3 offset = new Vector3(Mathf.Cos(ang)*distRiberTop,0,-Mathf.Sin(ang)*distRiberTop);
        rpNEv2 = rpNEv1 + offset;
        rpNEv3 = rpNEv4 + offset;

        //metrics unions top links SE 
        rpSEv1 = riberVertex[12];
        rpSEv4 = riberVertex[2];
        ang = -Mathf.Pi/6; //-30º 
        offset = new Vector3(Mathf.Cos(ang)*distRiberTop,0,-Mathf.Sin(ang)*distRiberTop);
        rpSEv2 = rpSEv1 + offset;
        rpSEv3 = rpSEv4 + offset;

        //metrics unions top links S 
        rpSv1 = riberVertex[2];
        rpSv4 = riberVertex[4];
        ang = -Mathf.Pi/2; //-90º 
        offset = new Vector3(Mathf.Cos(ang)*distRiberTop,0,-Mathf.Sin(ang)*distRiberTop);
        rpSv2 = rpSv1 + offset;
        rpSv3 = rpSv4 + offset;

        //metrics unions top links SW 
        rpSWv1 = riberVertex[4];
        rpSWv4 = riberVertex[6];
        ang = -Mathf.Pi*5/6; //-90º 
        offset = new Vector3(Mathf.Cos(ang)*distRiberTop,0,-Mathf.Sin(ang)*distRiberTop);
        rpSWv2 = rpSWv1 + offset;
        rpSWv3 = rpSWv4 + offset;

        //metrics unions top links NW 
        rpNWv1 = riberVertex[6];
        rpNWv4 = riberVertex[8];
        ang = Mathf.Pi*5/6; //150º 
        offset = new Vector3(Mathf.Cos(ang)*distRiberTop,0,-Mathf.Sin(ang)*distRiberTop);
        rpNWv2 = rpNWv1 + offset;
        rpNWv3 = rpNWv4 + offset;

        //metrics unions top links N 
        rpNv1 = riberVertex[8];
        rpNv4 = riberVertex[10];
        ang = Mathf.Pi*3/6; //90º 
        offset = new Vector3(Mathf.Cos(ang)*distRiberTop,0,-Mathf.Sin(ang)*distRiberTop);
        rpNv2 = rpNv1 + offset;
        rpNv3 = rpNv4 + offset;

        // Ribers IN or OUT?
        HexaData hdNE = hexData.neighbours[5];
        if (hdNE != null){
            linkNE_out = hexData.ribersOut[5] == hdNE;
            linkNE_in = hdNE.ribersOut[2] == hexData;
            if (hdNE.riber && (linkNE_out|| linkNE_in)){
                linkNE = true;
                countRiberNeibours++;
            }
        }
        HexaData hdSE = hexData.neighbours[0];
        if (hdSE != null){
            linkSE_out = hexData.ribersOut[0] == hdSE;
            linkSE_in = hdSE.ribersOut[3] == hexData;
            if (hdSE.riber && (linkSE_out ||linkSE_in)){
                linkSE = true;
                countRiberNeibours++;
            }
        }

        HexaData hdS = hexData.neighbours[1];
        if (hdS != null){
            linkS_out = hexData.ribersOut[1] == hdS;
            linkS_in = hdS.ribersOut[4] == hexData;
            if (hdS.riber && (linkS_out || linkS_in)){
                linkS = true;
                countRiberNeibours++;
            }
        }

       
        HexaData hdSW = hexData.neighbours[2];
        if (hdSW != null){
            linkSW_out = hexData.ribersOut[2] == hdSW;
            linkSW_in = hdSW.ribersOut[5] == hexData;
            if (hdSW.riber && (linkSW_out || linkSW_in)){
                linkSW = true;
                countRiberNeibours++;
            }
        }
        HexaData hdNW = hexData.neighbours[3];
        if (hdNW != null){
            linkNW_out = hexData.ribersOut[3] == hdNW;
            linkNW_in = hdNW.ribersOut[0] == hexData;
            if (hdNW.riber && (linkNW_out || linkNW_in)){
                linkNW = true;
                countRiberNeibours++;       
            }
        }
        HexaData hdN = hexData.neighbours[4];
        if (hdN != null){
            linkN_out = hexData.ribersOut[4] == hdN;
            linkN_in = hdN.ribersOut[1] == hexData;
            if (hdN.riber && (linkN_out || linkN_in)){
                linkN = true;
                countRiberNeibours++;
            }
        }

        //LINKS TOP (bajo agua no)
        if (!hexData.water){
            if (linkNE) GeoAux.createColorQuad(st,rpNEv1,rpNEv2,rpNEv3,rpNEv4,color,color);
            if (linkSE) GeoAux.createColorQuad(st,rpSEv1,rpSEv2,rpSEv3,rpSEv4,color,color);
            if (linkS)  GeoAux.createColorQuad(st,rpSv1,rpSv2,rpSv3,rpSv4,color,color);
            if (linkSW) GeoAux.createColorQuad(st,rpSWv1,rpSWv2,rpSWv3,rpSWv4,color,color);
            if (linkNW) GeoAux.createColorQuad(st,rpNWv1,rpNWv2,rpNWv3,rpNWv4,color,color);
            if (linkN) GeoAux.createColorQuad(st,rpNv1,rpNv2,rpNv3,rpNv4,color,color);
        }


        //LARGUES LINKS 
        if (linkNE){
            float otherH = hdNE.hexagon.getRealHeight() + HEIGHT_RIBER_OFFSET;
            float deltaH = otherH - hValue;
            pNEv1Link = rpNEv2;
            pNEv4Link = rpNEv3;
            ang = Mathf.Pi/6; //30º 
            offset = new Vector3(Mathf.Cos(ang) * distLink, deltaH, -Mathf.Sin(ang) * distLink);
            pNEv2Link = pNEv1Link + offset;
            pNEv3Link = pNEv4Link + offset;
            GeoAux.createColorQuad(st,pNEv1Link,pNEv2Link,pNEv3Link,pNEv4Link,color,color);
        }

        if (linkSE){
            float otherH = hdSE.hexagon.getRealHeight() + HEIGHT_RIBER_OFFSET;
            float deltaH = otherH - hValue;
            pSEv1Link = rpSEv2;
            pSEv4Link = rpSEv3;
            ang = -Mathf.Pi/6; //-30º 
            offset = new Vector3(Mathf.Cos(ang) * distLink, deltaH, -Mathf.Sin(ang) * distLink);
            pSEv2Link = pSEv1Link + offset;
            pSEv3Link = pSEv4Link + offset;
            GeoAux.createColorQuad(st,pSEv1Link,pSEv2Link,pSEv3Link,pSEv4Link,color,color);
        }

        if (linkS){
            float otherH = hdS.hexagon.getRealHeight() + HEIGHT_RIBER_OFFSET;
            float deltaH = otherH - hValue;
            pSv1Link = rpSv2;
            pSv4Link = rpSv3;
            ang = -Mathf.Pi*3/6; //-90º 
            offset = new Vector3(Mathf.Cos(ang) * distLink, deltaH, -Mathf.Sin(ang) * distLink);
            pSv2Link = pSv1Link + offset;
            pSv3Link = pSv4Link + offset;
            GeoAux.createColorQuad(st,pSv1Link,pSv2Link,pSv3Link,pSv4Link,color,color);
        }


        //CENTRO PERO CON 12 TRIS (bajo agua no)
        if (!hexData.water){
            if (countRiberNeibours < 2  || countRiberNeibours > 3){
                linkSE = linkS = linkSW = linkNW = linkN = linkNE = true;// un lago 
            }
            if (linkSE){
                GeoAux.createTri(st,riberVertex[0],riberVertex[12],riberVertex[2],color);//SE 0 
            } 
            if (linkS){
                GeoAux.createTri(st,riberVertex[0],riberVertex[2],riberVertex[4],color);//S  1 
            } 
            if (linkSW){
                GeoAux.createTri(st,riberVertex[0],riberVertex[4],riberVertex[6],color);//SW 2 
            } 
            if (linkNW){
                GeoAux.createTri(st,riberVertex[0],riberVertex[6],riberVertex[8],color);//NW 3 
            }
            if (linkN){
                GeoAux.createTri(st,riberVertex[0],riberVertex[8],riberVertex[10],color);//N  4 
            } 
            if (linkNE){
                GeoAux.createTri(st,riberVertex[0],riberVertex[10],riberVertex[12],color);//NE 5
            } 

            //Tapar huecos feos del rio
            if (countRiberNeibours>1){
                if (linkSE && linkSW){
                    GeoAux.createTri(st,riberVertex[0],riberVertex[2],riberVertex[4],color);
                }
                if (linkSE && linkNW){
                    GeoAux.createTri(st,riberVertex[0],riberVertex[2],riberVertex[6],color);
                    GeoAux.createTri(st,riberVertex[0],riberVertex[8],riberVertex[12],color);
                }
                if (linkSE && linkN){
                    GeoAux.createTri(st,riberVertex[0],riberVertex[10],riberVertex[12],color);
                }
                if (linkS && linkNW){
                    GeoAux.createTri(st,riberVertex[0],riberVertex[4],riberVertex[6],color);
                }
                if (linkS && linkN){
                    GeoAux.createTri(st,riberVertex[0],riberVertex[10],riberVertex[2],color);
                    GeoAux.createTri(st,riberVertex[0],riberVertex[4],riberVertex[8],color);
                }
                if (linkS && linkNE){
                    GeoAux.createTri(st,riberVertex[0],riberVertex[12],riberVertex[2],color);
                }
                if (linkSW && linkN){
                    GeoAux.createTri(st,riberVertex[0],riberVertex[6],riberVertex[8],color);
                }
                if (linkSW && linkNE){
                    GeoAux.createTri(st,riberVertex[0],riberVertex[6],riberVertex[10],color);
                    GeoAux.createTri(st,riberVertex[0],riberVertex[12],riberVertex[4],color);
                }
                if (linkNE && linkNW){
                    GeoAux.createTri(st,riberVertex[0],riberVertex[8],riberVertex[10],color);
                }
            }
        }
    }

   
}
