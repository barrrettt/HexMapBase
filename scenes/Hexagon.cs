using Godot;
using System;

public class Hexagon : MeshInstance{
    public HexaData hexData;
    
    // 10 colors
    private Color[] colors = new Color[]{
        new Color("#022340"),//blue dark
        new Color("#115999"),//b
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

    public override void _Ready() {
        CreateHexagon(); // generate mesh, colors, heights
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

    public void CreateHexagon(){
        if (hexData == null) hexData = new HexaData(0,0);

        SpatialMaterial mat = (SpatialMaterial) MaterialOverride;
        SurfaceTool st = new SurfaceTool();
        st.SetMaterial(mat);
        st.Begin(Mesh.PrimitiveType.Triangles);

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

        // Tapa superior 
        createHexTecho(st); 

        //Puentes NE,E,SE 
        crearPuentes(st); 

        // FINALLY 
        st.GenerateNormals(); 
        st.GenerateTangents(); 
        Mesh = st.Commit(); 

        //Physic poligon:
        createPhysic(hValue+0.1f);
    }

    private void crearPuentes(SurfaceTool st){

        //cosas
        float innerRadius = 1f * (Mathf.Sqrt(3)/2)*SIZE_TOP; 
        float dist = 2*innerRadius/3;//distancia del puente

        //colors
        Color color = colors[hexData.colorIndex];//color del indexColor
        Color cOtherNE = colors[hexData.colorIndex];
        Color cOtherE = colors[hexData.colorIndex];
        Color cOtherSE = colors[hexData.colorIndex];
        
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
            float ang = Mathf.Pi/6; //30º
            float deltaH = (hdNE.height-hexData.height) * HEIGHT_VALUE;
            pNEv2 = pNEv1 + (new Vector3(Mathf.Cos(ang)*dist,deltaH,-Mathf.Sin(ang)*dist));
            pNEv3 = pNEv4 + (new Vector3(Mathf.Cos(ang)*dist,deltaH,-Mathf.Sin(ang)*dist));
            createColorQuad(st,pNEv1,pNEv2,pNEv3,pNEv4,cOtherNE,color);
            //createColorQuad(st,pNEv1,pNEv2,pNEv3,pNEv4,new Color(1,0,0,1),new Color(1,0,0,1));
        }
        
        //SE
        HexaData hdSE = hexData.neighbours[0];
        if (hdSE != null){
            cOtherE = colors[hdSE.colorIndex];
            pSEv1 = vertex[6];
            pSEv4 = vertex[1];
            float ang = -Mathf.Pi/6; //-30º
            float hValue = (hdSE.height-hexData.height) * HEIGHT_VALUE;//dif altura
            pSEv2 = pSEv1 + (new Vector3(Mathf.Cos(ang)*dist,hValue,-Mathf.Sin(ang)*dist));
            pSEv3 = pSEv4 + (new Vector3(Mathf.Cos(ang)*dist,hValue,-Mathf.Sin(ang)*dist));
            createColorQuad(st,pSEv1,pSEv2,pSEv3,pSEv4,cOtherE,color);
            //createColorQuad(st,pEv1,pEv2,pEv3,pEv4,new Color(0,1,0,1),new Color(0,1,0,1));
        }
        
        //SE
        HexaData hdSW = hexData.neighbours[1];
        if (hdSW != null){
            cOtherSE = colors[hdSW.colorIndex];
            pSv1 = vertex[1];
            pSv4 = vertex[2];
            float ang = -Mathf.Pi/2; //-90º
            float hValue = (hdSW.height-hexData.height) * HEIGHT_VALUE;//dif altura
            pSv2 = pSv1 + (new Vector3(Mathf.Cos(ang)*dist,hValue,-Mathf.Sin(ang)*dist));
            pSv3 = pSv4 + (new Vector3(Mathf.Cos(ang)*dist,hValue,-Mathf.Sin(ang)*dist));
            createColorQuad(st,pSv1,pSv2,pSv3,pSv4,cOtherSE,color);
            //createColorQuad(st,pSEv1,pSEv2,pSEv3,pSEv4,new Color(0,0,1,1),new Color(0,0,1,1));
        }
        
        //TRI Entre puentes NE-SE
        if (hdNE != null && hdSE != null){
            createColorTri(st,pNEv4,pNEv3,pSEv2,color,cOtherNE,cOtherE);
        }
        //TRI entre puentes SE-SW
        if (hdSE != null && hdSW != null){
            createColorTri(st,pSv1,pSEv3,pSv2,color,cOtherE,cOtherSE);
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
                    if (par[0] == 2 && par[1]==3){ upDerOtro = pSv3; otroC = cOtherSE;}
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
                    if (par[0] == 6 && par[1]==1) {upIzqOtro = pSv2; otroC = cOtherSE;}
                    //tapa con el tri
                    createColorTri(st,downIzq,upIzqOtro,upIzq,color,otroC,color);
                }
            }
        }
        
    }

    //Crea los 6 Tris con los vertex
    private void createHexTecho(SurfaceTool st){
        Color color = colors[hexData.colorIndex];//color del indexColor
        createTri(st,vertex[0],vertex[6],vertex[1],color);//SE 0 
        createTri(st,vertex[0],vertex[1],vertex[2],color);//S  1 
        createTri(st,vertex[0],vertex[2],vertex[3],color);//SW 2 
        createTri(st,vertex[0],vertex[3],vertex[4],color);//NW 3 
        createTri(st,vertex[0],vertex[4],vertex[5],color);//N  4 
        createTri(st,vertex[0],vertex[5],vertex[6],color);//NE 5 
    }

    private void createColorQuad(SurfaceTool st, Vector3 v1,Vector3 v2,Vector3 v3,Vector3 v4,Color cOt, Color cMe){
        //2 tris:
        createColorTri(st,v1,v2,v3,cMe,cOt,cOt);
        createColorTri(st,v3,v4,v1,cOt,cMe,cMe);
    }

    //aux
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

    //aux
    private void createTri(SurfaceTool st, Vector3 v1,Vector3 v2,Vector3 v3,Color color){
        createColorTri(st,v1,v2,v3,color,color,color);
    }
    //aux
    private void createQuad(SurfaceTool st, Vector3 v1,Vector3 v2,Vector3 v3,Vector3 v4,Color color){
        //2 tris:
        createTri(st,v1,v2,v3,color);
        createTri(st,v3,v4,v1,color);
    }

    //PHYSICS POLIGON
    private void createPhysic(float height){
        CreateTrimeshCollision();
    }

}


