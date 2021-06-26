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

    public override void _Ready() {
        Create(); // generate mesh, colors, heights
        //debug
        iGeo = new ImmediateGeometry();
        AddChild(iGeo);
    }
    
    public override void _Process(float delta){
        //drawDebug();
    }

    //DEBUG GEOMETRY
    private ImmediateGeometry iGeo;
    private void drawDebug(){
        iGeo.Clear();
        iGeo.Begin(Mesh.PrimitiveType.Lines);
        iGeo.SetColor(new Color(1,1,1));
        iGeo.AddSphere(2,50,1); 
        iGeo.End();
    }

    //constante para la altura
    public static float HEIGHT_VALUE =  0.5f;
    public static float SIZE_TOP = 0.75f;//0.75f;

    //Actualizar geometrias según los datos
    public Vector3[] vertex = new Vector3[7];

    //RIBERS
    public static float HEIGHT_RIBER_OFFSET = 0.05f;
    public static float SIZE_RIBER = 0.1f;
    public Vector3[] riberVertex = new Vector3[7];
    public Vector3[] riberVertexMed = new Vector3[7];

    public void Create(){
        if (hexData == null) hexData = new HexaData(0,0);

        SpatialMaterial mat = (SpatialMaterial) MaterialOverride;
        SurfaceTool st = new SurfaceTool();
        st.SetMaterial(mat);
        st.Begin(Mesh.PrimitiveType.Triangles);
        
        CreateHexMetrics(st); // Main hexagon and metrics
        CreateHexUnions(st);  //Unions and base hex
        
        // FINALLY 
        st.GenerateNormals(); 
        st.GenerateTangents(); 
        Mesh = st.Commit(); 

        //Physic mesh 
        CreateTrimeshCollision(); 

        //Rivers 
        CreateRivers(st);
    }

     //Crea los 6 Tris con los vertex 
    private void CreateHexMetrics(SurfaceTool st){
        float sexto =  Mathf.Pi/3;
        float hValue = HEIGHT_VALUE * hexData.height;

        //vertices
        vertex[0] = new Vector3(0,hValue,0);//CENTRO
        vertex[1] = new Vector3(Mathf.Cos(sexto * 1)* SIZE_TOP, hValue, Mathf.Sin(sexto*1)* SIZE_TOP) ;//SE
        vertex[2] = new Vector3(Mathf.Cos(sexto * 2)* SIZE_TOP, hValue, Mathf.Sin(sexto*2)* SIZE_TOP);//SW
        vertex[3] = new Vector3(Mathf.Cos(sexto * 3)* SIZE_TOP, hValue, Mathf.Sin(sexto*3)* SIZE_TOP);//W
        vertex[4] = new Vector3(Mathf.Cos(sexto * 4)* SIZE_TOP, hValue, Mathf.Sin(sexto*4)* SIZE_TOP);//NW
        vertex[5] = new Vector3(Mathf.Cos(sexto * 5)* SIZE_TOP, hValue, Mathf.Sin(sexto*5)* SIZE_TOP);//NE
        vertex[6] = new Vector3(Mathf.Cos(sexto * 6)* SIZE_TOP, hValue, Mathf.Sin(sexto*6)* SIZE_TOP);//E
        
        Color color = colors[hexData.colorIndex];//color del indexColor
        createTri(st,vertex[0],vertex[6],vertex[1],color);//SE 0 
        createTri(st,vertex[0],vertex[1],vertex[2],color);//S  1 
        createTri(st,vertex[0],vertex[2],vertex[3],color);//SW 2 
        createTri(st,vertex[0],vertex[3],vertex[4],color);//NW 3 
        createTri(st,vertex[0],vertex[4],vertex[5],color);//N  4 
        createTri(st,vertex[0],vertex[5],vertex[6],color);//NE 5 
    }

    private void CreateHexUnions(SurfaceTool st){

        //cosas
        float innerRadius = 1f * (Mathf.Sqrt(3)/2)*SIZE_TOP; 
        float dist = 2*innerRadius/3;//distancia del puente

        //colors
        Color color = colors[hexData.colorIndex];//color del indexColor
        Color cOtherNE = colors[hexData.colorIndex];
        Color cOtherE = colors[hexData.colorIndex];
        Color cOtherS = colors[hexData.colorIndex];
        
        //puntos de los 3 quads de los puentes 
        Vector3 pNEv1 = new Vector3(); Vector3 pNEv2 = new Vector3(); Vector3 pNEv3 = new Vector3(); Vector3 pNEv4 = new Vector3();
        Vector3 pSEv1 = new Vector3(); Vector3 pSEv2= new Vector3(); Vector3 pSEv3 = new Vector3();Vector3 pSEv4= new Vector3();
        Vector3 pSv1= new Vector3(); Vector3 pSv2 = new Vector3();Vector3 pSv3= new Vector3();Vector3 pSv4= new Vector3();

        //NE
        HexaData hdNE = hexData.neighbours[5];
        if (hdNE != null){
            cOtherNE = colors[hdNE.colorIndex];
            pNEv1 = vertex[5];
            pNEv4 = vertex[6];
            float ang30 = Mathf.Pi/6; //30º
            float deltaH = (hdNE.height-hexData.height) * HEIGHT_VALUE;
            Vector3 offset = new Vector3(Mathf.Cos(ang30)*dist,deltaH,-Mathf.Sin(ang30)*dist);
            pNEv2 = pNEv1 + offset;
            pNEv3 = pNEv4 + offset;
            createColorQuad(st,pNEv1,pNEv2,pNEv3,pNEv4,cOtherNE,color);
        }
        
        //SE
        HexaData hdSE = hexData.neighbours[0];
        if (hdSE != null){
            cOtherE = colors[hdSE.colorIndex];
            pSEv1 = vertex[6];
            pSEv4 = vertex[1];
            float ang = -Mathf.Pi/6; //-30º
            float hValue = (hdSE.height-hexData.height) * HEIGHT_VALUE;//dif altura
            Vector3 offset = new Vector3(Mathf.Cos(ang)*dist,hValue,-Mathf.Sin(ang)*dist);
            pSEv2 = pSEv1 + offset;
            pSEv3 = pSEv4 + offset;
            createColorQuad(st,pSEv1,pSEv2,pSEv3,pSEv4,cOtherE,color);
        }
        
        //S
        HexaData hdS = hexData.neighbours[1];
        if (hdS != null){
            cOtherS = colors[hdS.colorIndex];
            pSv1 = vertex[1];
            pSv4 = vertex[2];
            float ang = -Mathf.Pi/2; //-90º
            float hValue = (hdS.height-hexData.height) * HEIGHT_VALUE;//dif altura
            Vector3 offset = new Vector3(Mathf.Cos(ang)*dist,hValue,-Mathf.Sin(ang)*dist);
            pSv2 = pSv1 + offset;
            pSv3 = pSv4 + offset;
            createColorQuad(st,pSv1,pSv2,pSv3,pSv4,cOtherS,color);
        }
        
        //TRI Entre puentes NE-SE
        if (hdNE != null && hdSE != null){
            createColorTri(st,pNEv4,pNEv3,pSEv2,color,cOtherNE,cOtherE);
        }
        //TRI entre puentes SE-S
        if (hdSE != null && hdS != null){
            createColorTri(st,pSv1,pSEv3,pSv2,color,cOtherE,cOtherS);
        }

        //FALDAS: si es borde de mapa(sin vecino) hay que tapar la cara
        int[][] pares = {
            new int[]{6,1},new int[]{1,2}, new int[]{2,3},
            new int[]{3,4},new int[]{4,5}, new int[]{5,6}
        };
        for (int i = 0;i<6;i++){
            if (hexData.neighbours[i] == null){
                int[] par = pares[i];
                int der = par[0]; int izq = par[1];
                Vector3 altoDer = vertex[der];
                Vector3 bajoDer = new Vector3(vertex[der].x,0,vertex[der].z) * (4/3f);
                Vector3 altoIzq = vertex[izq];
                Vector3 bajoIzq = new Vector3(vertex[izq].x,0,vertex[izq].z) * (4/3f);
                createColorQuad(st,altoDer,bajoDer,bajoIzq,altoIzq,color,color);

                //en un sentido
                int anterior = i-1; if (anterior<0) anterior=5;
                if(hexData.neighbours[anterior] != null){
                    //si anterior es no es null -> hay un hueco triangular
                    Vector3 upDer = altoDer;
                    Vector3 downDer = bajoDer;
                    //el punto que falta es del vector del puente contrario a upDer, sabiendo la direccion del vecino conozco el punto
                    Vector3 upDerOtro = upDer; Color otroC = color;
                    if (par[0] == 6 && par[1]==1){ upDerOtro = pNEv3; otroC = cOtherNE;}
                    if (par[0] == 1 && par[1]==2){ upDerOtro = pSEv3; otroC = cOtherE;}
                    if (par[0] == 2 && par[1]==3){ upDerOtro = pSv3; otroC = cOtherS;}
                    //tapa con el tri
                    createColorTri(st,upDer,upDerOtro,downDer,color,otroC,color);
                }

                //en el otro sentido hay que tapar tambien los triangulos:
                int siguiente = i+1; if (siguiente>5) siguiente=0;
                if(hexData.neighbours[siguiente] != null){
                    Vector3 upIzq = altoIzq;
                    Vector3 downIzq = bajoIzq;
                    //el punto que falta es del puente contrario a UpIzq, sabiendo la direccion del vecino se conoce el punto
                    Vector3 upIzqOtro = upIzq; Color otroC = color;
                    if (par[0] == 4 && par[1]==5) {upIzqOtro = pNEv2; otroC = cOtherNE;}
                    if (par[0] == 5 && par[1]==6) {upIzqOtro = pSEv2; otroC = cOtherE;}
                    if (par[0] == 6 && par[1]==1) {upIzqOtro = pSv2; otroC = cOtherS;}
                    //tapa con el tri
                    createColorTri(st,downIzq,upIzqOtro,upIzq,color,otroC,color);
                }
            }
        }
        
    }

    // Crear rios 
    private void CreateRivers(SurfaceTool st){
        if (!hexData.riber) return;
        SpatialMaterial matRiver = new SpatialMaterial();
        st.SetMaterial(matRiver);

        //riber center hex
        float trozitos =  Mathf.Pi/3;
        float hValue = (HEIGHT_VALUE * hexData.height) + HEIGHT_RIBER_OFFSET;

        //vertices normales
        riberVertex[0] = new Vector3(0,hValue,0);//CENTRO
        riberVertex[1] = new Vector3(Mathf.Cos(trozitos * 1)* SIZE_RIBER, hValue, Mathf.Sin(trozitos*1)* SIZE_RIBER) ;//SE
        riberVertex[2] = new Vector3(Mathf.Cos(trozitos * 2)* SIZE_RIBER, hValue, Mathf.Sin(trozitos*2)* SIZE_RIBER);//SW
        riberVertex[3] = new Vector3(Mathf.Cos(trozitos * 3)* SIZE_RIBER, hValue, Mathf.Sin(trozitos*3)* SIZE_RIBER);//W
        riberVertex[4] = new Vector3(Mathf.Cos(trozitos * 4)* SIZE_RIBER, hValue, Mathf.Sin(trozitos*4)* SIZE_RIBER);//NW
        riberVertex[5] = new Vector3(Mathf.Cos(trozitos * 5)* SIZE_RIBER, hValue, Mathf.Sin(trozitos*5)* SIZE_RIBER);//NE
        riberVertex[6] = new Vector3(Mathf.Cos(trozitos * 6)* SIZE_RIBER, hValue, Mathf.Sin(trozitos*6)* SIZE_RIBER);//E

        //vertices medios 
        float innerRadiusRiber = (Mathf.Sqrt(3)/2)*SIZE_RIBER; 

        trozitos =  (Mathf.Pi/6);//doceavos de circunferencias
        riberVertexMed[0] = new Vector3(0,hValue,0);//CENTRO
        riberVertexMed[1] = new Vector3(Mathf.Cos(trozitos * 1)* innerRadiusRiber, hValue, Mathf.Sin(trozitos*1)* innerRadiusRiber) ;//1-2
        riberVertexMed[2] = new Vector3(Mathf.Cos(trozitos * 2)* innerRadiusRiber, hValue, Mathf.Sin(trozitos*2)* innerRadiusRiber);//2-3
        riberVertexMed[3] = new Vector3(Mathf.Cos(trozitos * 3)* innerRadiusRiber, hValue, Mathf.Sin(trozitos*3)* innerRadiusRiber);//3-4
        riberVertexMed[4] = new Vector3(Mathf.Cos(trozitos * 4)* innerRadiusRiber, hValue, Mathf.Sin(trozitos*4)* innerRadiusRiber);//4-5
        riberVertexMed[5] = new Vector3(Mathf.Cos(trozitos * 5)* innerRadiusRiber, hValue, Mathf.Sin(trozitos*5)* innerRadiusRiber);//5-6
        riberVertexMed[6] = new Vector3(Mathf.Cos(trozitos * 6)* innerRadiusRiber, hValue, Mathf.Sin(trozitos*6)* innerRadiusRiber);//6-1


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
        float hValue = (HEIGHT_VALUE * hexData.height) + HEIGHT_RIBER_OFFSET;
        float distLink = 2*innerRadius/3;//distancia del puente
        Color color = colorRiber;

        // pintar centros?
        bool linkSE = false;
        bool linkS = false;
        bool linkSW= false;
        bool linkNW = false;
        bool linkN = false;
        bool linkNE = false;
        int countRiberNeibours = 0;
        
        
        //metrics unions top links NE
        Vector3 pNEv1Top = new Vector3(); 
        Vector3 pNEv2Top = new Vector3(); 
        Vector3 pNEv3Top = new Vector3(); 
        Vector3 pNEv4Top = new Vector3();
        
        pNEv1Top = riberVertex[5];
        pNEv4Top = riberVertex[6];
        float ang = Mathf.Pi/6; //30º
        Vector3 offset = new Vector3(Mathf.Cos(ang)*distRiberTop,0,-Mathf.Sin(ang)*distRiberTop);
        pNEv2Top = pNEv1Top + offset;
        pNEv3Top = pNEv4Top + offset;

        HexaData hdNE = hexData.neighbours[5];
        if (hdNE != null){
            if (hdNE.riber){
                linkNE = true;
                countRiberNeibours++;
                createColorQuad(st,pNEv1Top,pNEv2Top,pNEv3Top,pNEv4Top,color,color);
            }
        }

        //metrics unions top links SE
        Vector3 pSEv1Top = new Vector3(); 
        Vector3 pSEv2Top = new Vector3(); 
        Vector3 pSEv3Top = new Vector3();
        Vector3 pSEv4Top = new Vector3();

        pSEv1Top = riberVertex[6];
        pSEv4Top = riberVertex[1];
        ang = -Mathf.Pi/6; //-30º
        offset = new Vector3(Mathf.Cos(ang)*distRiberTop,0,-Mathf.Sin(ang)*distRiberTop);
        pSEv2Top = pSEv1Top + offset;
        pSEv3Top = pSEv4Top + offset;

        HexaData hdSE = hexData.neighbours[0];
        if (hdSE != null){
            if (hdSE.riber){
                linkSE = true;
                countRiberNeibours++;
                createColorQuad(st,pSEv1Top,pSEv2Top,pSEv3Top,pSEv4Top,color,color);
            }
        }

        //metrics unions top links S
        Vector3 pSv1Top = new Vector3(); 
        Vector3 pSv2Top = new Vector3(); 
        Vector3 pSv3Top = new Vector3();
        Vector3 pSv4Top = new Vector3();

        pSv1Top = riberVertex[1];
        pSv4Top = riberVertex[2];
        ang = -Mathf.Pi/2; //-90º
        offset = new Vector3(Mathf.Cos(ang)*distRiberTop,0,-Mathf.Sin(ang)*distRiberTop);
        pSv2Top = pSv1Top + offset;
        pSv3Top = pSv4Top + offset;

        HexaData hdS = hexData.neighbours[1];
        if (hdS != null){
            if (hdS.riber){
                linkS = true;
                countRiberNeibours++;
                createColorQuad(st,pSv1Top,pSv2Top,pSv3Top,pSv4Top,color,color);
            }
        }

        //metrics unions top links SW
        Vector3 pSWv1Top = new Vector3(); 
        Vector3 pSWv2Top = new Vector3(); 
        Vector3 pSWv3Top = new Vector3();
        Vector3 pSWv4Top = new Vector3();
        pSWv1Top = riberVertex[2];
        pSWv4Top = riberVertex[3];
        ang = -Mathf.Pi*5/6; //-90º
        offset = new Vector3(Mathf.Cos(ang)*distRiberTop,0,-Mathf.Sin(ang)*distRiberTop);
        pSWv2Top = pSWv1Top + offset;
        pSWv3Top = pSWv4Top + offset;

        HexaData hdSW = hexData.neighbours[2];
        if (hdSW != null){
            if (hdSW.riber){
                linkSW = true;
                countRiberNeibours++;
                createColorQuad(st,pSWv1Top,pSWv2Top,pSWv3Top,pSWv4Top,color,color);
            }
        }

        //metrics unions top links NW
        Vector3 pNWv1Top = new Vector3(); 
        Vector3 pNWv2Top = new Vector3(); 
        Vector3 pNWv3Top = new Vector3();
        Vector3 pNWv4Top = new Vector3();
        pNWv1Top = riberVertex[3];
        pNWv4Top = riberVertex[4];
        ang = Mathf.Pi*5/6; //150º
        offset = new Vector3(Mathf.Cos(ang)*distRiberTop,0,-Mathf.Sin(ang)*distRiberTop);
        pNWv2Top = pNWv1Top + offset;
        pNWv3Top = pNWv4Top + offset;

        HexaData hdNW = hexData.neighbours[3];
        if (hdNW != null){
            if (hdNW.riber){
                linkNW = true;
                countRiberNeibours++;
                createColorQuad(st,pNWv1Top,pNWv2Top,pNWv3Top,pNWv4Top,color,color);
            }
        }

        //metrics unions top links N
        Vector3 pNv1Top = new Vector3(); 
        Vector3 pNv2Top = new Vector3(); 
        Vector3 pNv3Top = new Vector3();
        Vector3 pNv4Top = new Vector3();

        pNv1Top = riberVertex[4];
        pNv4Top = riberVertex[5];
        ang = Mathf.Pi*3/6; //90º
        offset = new Vector3(Mathf.Cos(ang)*distRiberTop,0,-Mathf.Sin(ang)*distRiberTop);
        pNv2Top = pNv1Top + offset;
        pNv3Top = pNv4Top + offset;

        HexaData hdN = hexData.neighbours[4];
        if (hdN != null){
            if (hdN.riber){
                linkN = true;
                countRiberNeibours++;
                createColorQuad(st,pNv1Top,pNv2Top,pNv3Top,pNv4Top,color,color);
            }
        }

        //LARGUES LINKS
        if (linkNE){
            float otherHvalue = (HEIGHT_VALUE * hdNE.height) + HEIGHT_RIBER_OFFSET;
            float deltaH = otherHvalue - hValue;
            Vector3 pNEv1Link = pNEv2Top;
            Vector3 pNEv4Link = pNEv3Top;
            ang = Mathf.Pi/6; //30º
            offset = new Vector3(Mathf.Cos(ang) * distLink, deltaH, -Mathf.Sin(ang) * distLink);
            Vector3 pNEv2Link = pNEv1Link + offset;
            Vector3 pNEv3Link = pNEv4Link + offset;
            createColorQuad(st,pNEv1Link,pNEv2Link,pNEv3Link,pNEv4Link,color,color);
        }

        if (linkSE){
            float otherHvalue = (HEIGHT_VALUE * hdSE.height) + HEIGHT_RIBER_OFFSET;
            float deltaH = otherHvalue - hValue;
            Vector3 pSEv1Link = pSEv2Top;
            Vector3 pSEv4Link = pSEv3Top;
            ang = -Mathf.Pi/6; //-30º
            offset = new Vector3(Mathf.Cos(ang) * distLink, deltaH, -Mathf.Sin(ang) * distLink);
            Vector3 pSEv2Link = pSEv1Link + offset;
            Vector3 pSEv3Link = pSEv4Link + offset;
            createColorQuad(st,pSEv1Link,pSEv2Link,pSEv3Link,pSEv4Link,color,color);
        }

        if (linkS){
            float otherHvalue = (HEIGHT_VALUE * hdS.height) + HEIGHT_RIBER_OFFSET;
            float deltaH = otherHvalue - hValue;
            Vector3 pSv1Link = pSv2Top;
            Vector3 pSv4Link = pSv3Top;
            ang = -Mathf.Pi*3/6; //-90º
            offset = new Vector3(Mathf.Cos(ang) * distLink, deltaH, -Mathf.Sin(ang) * distLink);
            Vector3 pSv2Link = pSv1Link + offset;
            Vector3 pSv3Link = pSv4Link + offset;
            createColorQuad(st,pSv1Link,pSv2Link,pSv3Link,pSv4Link,color,color);
        }


        //CENTRO TRIS PERO CON 12 TRIS
        if (countRiberNeibours == 0 || countRiberNeibours == 1){
            linkSE = linkS = linkSW = linkNW = linkN = linkNE = true;
        }
        if (linkSE){
            //createTri(st,riberVertex[0],riberVertex[6],riberVertex[1],color);//SE 0 
            createTri(st,riberVertex[0],riberVertex[6],riberVertexMed[1],color);
            createTri(st,riberVertex[0],riberVertexMed[1],riberVertex[1],color);
        } 
        if (linkS){
            //createTri(st,riberVertex[0],riberVertex[1],riberVertex[2],color);//S  1 
            createTri(st,riberVertex[0],riberVertex[1],riberVertex[2],color);//S  1 
            createTri(st,riberVertex[0],riberVertex[1],riberVertex[2],color);//S  1 
        } 
        if (linkSW) createTri(st,riberVertex[0],riberVertex[2],riberVertex[3],color);//SW 2 
        if (linkNW) createTri(st,riberVertex[0],riberVertex[3],riberVertex[4],color);//NW 3 
        if (linkN) createTri(st,riberVertex[0],riberVertex[4],riberVertex[5],color);//N  4 
        if (linkNE) createTri(st,riberVertex[0],riberVertex[5],riberVertex[6],color);//NE 5
            
    }

    // AUX 
    private void createColorQuad(SurfaceTool st, Vector3 v1,Vector3 v2,Vector3 v3,Vector3 v4,Color cOt, Color cMe){
        //2 tris:
        createColorTri(st,v1,v2,v3,cMe,cOt,cOt);
        createColorTri(st,v3,v4,v1,cOt,cMe,cMe);
    }

    // AUX 
    private void createColorTri(SurfaceTool st, Vector3 v1, Vector3 v2, Vector3 v3, Color c1, Color c2, Color c3){
        st.AddColor(c1);
        st.AddUv(new Vector2(0,0));
        st.AddVertex(v1);
        st.AddColor(c2);
        st.AddUv(new Vector2(0,0));
        st.AddVertex(v2);
        st.AddColor(c3);
        st.AddUv(new Vector2(0,0));
        st.AddVertex(v3);
    }

    // AUX 
    private void createTri(SurfaceTool st, Vector3 v1,Vector3 v2,Vector3 v3,Color color){
        createColorTri(st,v1,v2,v3,color,color,color);
    }
    // AUX 
    private void createQuad(SurfaceTool st, Vector3 v1,Vector3 v2,Vector3 v3,Vector3 v4,Color color){
        //2 tris:
        createTri(st,v1,v2,v3,color);
        createTri(st,v3,v4,v1,color);
    }

}
