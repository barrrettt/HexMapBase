using Godot;
using System;
using System.Threading.Tasks;

public class Editor : Spatial{
    private Camara camara;
    private WorldEnvironment centro;
    private Map map;

    private GenerationAlgoritm generationAlgoritm = new GenerationAlgoritm();

    //GUI REFERENCES
    private Label lblMousePos1, lblCameraPos1, lblSelectedPos1;
    private Label lblMousePos2, lblCameraPos2, lblSelectedPos2;

    private Button buElevations, buStyles, buGeneration, buOptions;

    private Control pElevations, pStyles, pGeneration, pOptions;
    private Control pTools;
    private Control[] panels;
    private String actualToolSelected = "";
    private HexaData lastOrigin = null;
    private Label lblActualTool;

    //gui generation
    private Label lblSeedMap,lblSizeMap,lblStyleMap;
    private Label lblAlgoParam0, lblAlgoParam1, lblAlgoParam2, lblAlgoParam3;
    private Label lblGenParam0,lblGenParam1,lblGenParam2,lblGenParam3,lblGenParam4,lblGenParam5,lblGenParam6,lblGenParam7,lblGenParam8,lblGenParam9;
    private GuiModals modals;

    //DEBUG 
    int ballnumber = 0;
    private RigidBody[] balls; 
    //GENERATION
    private Random random = new Random(); 
    public override void _EnterTree(){ 
        //Things
        centro = GetNode<WorldEnvironment>("center"); 
        map = centro.GetNode<Map>("Map"); 
        camara = centro.GetNode<Camara>("Camara"); 
        spaceState = GetWorld().DirectSpaceState; //physic ray neededs 

        //labels
        lblActualTool = GetNode<Label>("GUI/UpPanel/HB/HB/lblTool"); //GUI panel top
        lblMousePos1 = GetNode<Label>("GUI/BottomPanel/HB/LPanel/HB/VB/HB1/Label"); //labels of debug positions
        lblMousePos2 = GetNode<Label>("GUI/BottomPanel/HB/LPanel/HB/VB/HB1/Label2");
        lblCameraPos1 = GetNode<Label>("GUI/BottomPanel/HB/LPanel/HB/VB/HB2/Label");
        lblCameraPos2 = GetNode<Label>("GUI/BottomPanel/HB/LPanel/HB/VB/HB2/Label2");
        lblSelectedPos1 = GetNode<Label>("GUI/BottomPanel/HB/LPanel/HB/VB/HB3/Label");
        lblSelectedPos2 = GetNode<Label>("GUI/BottomPanel/HB/LPanel/HB/VB/HB3/Label2");

        //subpanels 
        pTools = GetNode<Control>("GUI/RightPanel/Right/PTools"); 
        pElevations = GetNode<Control>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/PElevations"); 
        pStyles = GetNode<Control>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBStyles"); 
        pGeneration = GetNode<Control>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration"); 
        pOptions = GetNode<Control>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBOptions"); 

        panels = new Control[] { 
            pElevations,
            pStyles,
            pGeneration,
            pOptions
        }; 

        // PROCEDURAL GEN PANEL
        lblSeedMap = GetNode<Label>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/hbSeed/lblSeedMap");
        lblSizeMap = GetNode<Label>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/hbSize/lblSizeMap");
        lblStyleMap = GetNode<Label>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/hbStyle/lblStyleMap");
        
        lblAlgoParam0 = GetNode<Label>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/hbAlgorit0/lblGenParam");
        lblAlgoParam1 = GetNode<Label>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/hbAlgorit1/lblGenParam");
        lblAlgoParam2 = GetNode<Label>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/hbAlgorit2/lblGenParam");
        lblAlgoParam3 = GetNode<Label>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/hbAlgorit3/lblGenParam");

        lblGenParam0 = GetNode<Label>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/HB0/lblGenParam");
        lblGenParam1 = GetNode<Label>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/HB1/lblGenParam");
        lblGenParam2 = GetNode<Label>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/HB2/lblGenParam");
        lblGenParam3 = GetNode<Label>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/HB3/lblGenParam");
        lblGenParam4 = GetNode<Label>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/HB4/lblGenParam");
        lblGenParam5 = GetNode<Label>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/HB5/lblGenParam");
        lblGenParam6 = GetNode<Label>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/HB6/lblGenParam");
        lblGenParam7 = GetNode<Label>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/HB7/lblGenParam");
        lblGenParam8 = GetNode<Label>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/HB8/lblGenParam");
        lblGenParam9 = GetNode<Label>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/HB9/lblGenParam");

        buttonSetGenerationParam(""); //init labels with values

        //modals
        modals = GetNode<GuiModals>("GUI/Modals");
        
        // translate text GUI 
        locateTexts();

    }
    private void locateTexts(){
        lblMousePos1.Text = "Mouse:";
        lblCameraPos1.Text = "Camera:";
        lblSelectedPos1.Text = "Selected:";
        lblMousePos2.Text = "";
        lblCameraPos2.Text = "";
        lblSelectedPos2.Text = "";
    }

    public override void _Ready(){
        
        //init mapdata
        generationAlgoritm.sizeMap = 20;
        generationAlgoritm.generateMapData(map);

        //physics balls debug
        balls = new RigidBody[ballnumber];
        
        for (int i = 0;i<balls.Length;i++){
            RigidBody ball = new RigidBody();
            CollisionShape cs = new CollisionShape();
            SphereShape sphere = new SphereShape();
            sphere.Radius = 0.1f;
            cs.Shape = sphere;
            CSGSphere csgs = new CSGSphere();
            csgs.Radius = 0.1f; csgs.RadialSegments = 12; csgs.Rings = 6;
            ball.AddChild(cs);
            ball.AddChild(csgs);
            centro.AddChild(ball);
            balls[i] = ball;
        }

        map.instanceAllMap(); //Show all map

        initcamera();//camera to center

        GD.Print("Editor ready");
    }

    public override void _Process(float delta) {
        //debug
        physicDebug();
    }

    public override void _PhysicsProcess(float delta){
        if (camara.isPC) mousePosOver(); //on mobile no over indicator
        cameraPosOver();
    }

    public override void _UnhandledInput(InputEvent @event){
        if (@event is InputEventMouseButton ){
            InputEventMouse btn = (InputEventMouse) @event;
            if (btn.IsPressed()){
                
                if (btn.ButtonMask == (int)ButtonList.Left){
                    rayoMouseClick();
                }
                if (btn.ButtonMask == (int)ButtonList.Right){
                    buttonToolSelect("");
                }
            }
        }

    }

    // RAYS CAMEREA AND MOUSE
    private PhysicsDirectSpaceState spaceState;

    private Hexagon ray(Vector3 origin, Vector3 target){
        Hexagon hx = null;
        if (origin == null || target == null) return hx;

        Vector3 dir = target - origin;
        Vector3 ray = origin + (dir*5);// ray

        var result = spaceState.IntersectRay(origin,ray,null,2147483647,true,false);
        if (result.Count > 0){
            Node coll = result["collider"] as Node;
            Node parent = coll.GetParent();
            if (parent is Hexagon){
                hx = parent as Hexagon;
            }
        }
        return hx;
    }
    private void cameraPosOver(){
        //Control a donde apunta la cámara para saber el punto central del mapa
        Vector3 camTarget = ((Spatial)camara.GetChild(0)).GlobalTransform.origin;
        Vector3 camOrigen = camara.GlobalTransform.origin;
        Hexagon hx = ray(camOrigen,camTarget);
        if (hx != null && hx.hexData != null){
            int r = hx.hexData.row; 
            int c = hx.hexData.col;
            lblCameraPos2.Text = String.Format("({0},{1})",r,c);
            map.cameraRayPosition = map.mapData.getHexPosition(r,c);
        }else{
            lblCameraPos2.Text ="-";
        }
    }

    private void mousePosOver() { 
        //Control a donde apunta la cámara para saber el punto central del mapa
        Vector3[] mRay = camara.getMouseRay(); 
        Hexagon hx = ray(mRay[0],mRay[1]);
        if (hx != null && hx.hexData!= null){
            lblMousePos2.Text = String.Format("({0},{1})",hx.hexData.row,hx.hexData.col);
            map.moveOver(hx.hexData.row, hx.hexData.col);
        }else{
            lblMousePos2.Text ="";
            map.moveOver(-1,-1);
        }
    }
    
    private void rayoMouseClick(){
        //Control a donde apunta la cámara para saber el punto central del mapa
        Vector3[] mRay = camara.getMouseRay(); 
        Hexagon hx = ray(mRay[0],mRay[1]);
        if (hx != null && hx.hexData != null){
            lblSelectedPos2.Text = String.Format("({0},{1})",hx.hexData.row,hx.hexData.col); 
            
            if (actualToolSelected != ""){
                exeTool(hx.hexData);
            }

            map.moveSelector(hx.hexData.row, hx.hexData.col);

        }else{
            lblSelectedPos2.Text ="";
        }
    }

    //camera init: limits and center
    private void initcamera(){
        //calculate camera limits
        int size = map.mapData.getSize();
        Vector3 pos = map.mapData.getHexPosition(size-1,size-1);
        camara.movelimitsTopLeft = new Vector2 (0,0);
        camara.movelimitsDownRight = new Vector2 (pos.x,pos.z);

        //center point
        pos = map.mapData.getHexPosition(size/2,size/2);
        camara.move(new Vector2(pos.x,pos.z));
    }

    //DEBUG PHYSICS
    private void physicDebug(){
        foreach (RigidBody ball in balls){
            if (ball.Translation.y<-10f || ball.Sleeping){
                int row = random.Next(map.mapData.getSize());
                int col = random.Next(map.mapData.getSize());
                
                Vector3 vPos = map.mapData.getHexPosition(row,col);
                vPos = vPos + new Vector3(0,20f,0);
                ball.Translation = vPos;
                ball.Sleeping = false;
                ball.AddTorque(new Vector3(
                    (float)random.NextDouble(),
                    (float)random.NextDouble(),
                    (float)random.NextDouble()));
            }
        }
    }

    //GUI CONTROLS
    private void buttonPanelclick(int buttonIndex){

        Boolean allHides = true;
        for (int i = 0; i< panels.Length;i++){
            if (i==buttonIndex){
                panels[i].Visible = !panels[i].Visible;
            } else{
                panels[i].Visible = false;
            }
            allHides &= !panels[i].Visible;
        }
                
        //panel base se muestra si alguien visible
        pTools.Visible = !allHides;

        //no tools
        actualToolSelected = "";
        lblActualTool.Text = "";

        //si algun panel activo, camara no responde a acciones
        camara.playerControl = allHides;
    }

    private void buttonToolSelect(String toolname){
        buttonPanelclick(-1);//hide panels
        actualToolSelected = toolname;
        lblActualTool.Text = toolname;
    }

    private async void buttonSetGenerationParam(String nameparam){
        
        switch (nameparam){
            case "seed":
                //show modal
                modals.showModalInputString("Map seed", generationAlgoritm.seed, 20);
                //waiting for modal state READY 
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } );
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show
                generationAlgoritm.seed = modals.value_str; //set param value
                break;

            case "size":
                //show modal
                modals.showModalInputInteger("Map size", generationAlgoritm.sizeMap,0,50);
                //waiting for modal state READY 
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } );
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show
                generationAlgoritm.sizeMap = modals.value_int; //set param value
                break;
            
            case "style":
                //show modal
                modals.showModalMessage("No map styles yet");
                //waiting for modal state READY 
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } );
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show
                generationAlgoritm.styleMap = 0;
                break;

            case "param_passes": 
                modals.showModalInputInteger("Algoritm passes", generationAlgoritm.passes, 1, 50);//show modal 
                //waiting for modal state READY 
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } ); 
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show 
                generationAlgoritm.passes = modals.value_int; //set param value 
                break; 
            
            case "param_period": 
                modals.showModalInputFloat("Algoritm period", generationAlgoritm.period , 0, 100); //show modal 
                //waiting for modal state READY 
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } );  
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show 
                generationAlgoritm.period = modals.value_float; //set param value 
                break; 
            
            case "param_periodMulti":
                modals.showModalInputFloat("Algoritm period multiply", generationAlgoritm.periodMulti, 0, 100); //show modal 
                //waiting for modal state READY  
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } ); 
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show 
                generationAlgoritm.periodMulti = modals.value_float; //set param value 
                break; 
            
            case "parma_heightSteep": 
                modals.showModalInputFloat("Algoritm heightSteep", generationAlgoritm.heightSteep, 0, 100); //show modal 
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } ); //waiting for modal state READY 
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show 
                generationAlgoritm.heightSteep = modals.value_float; //set param value 
                break; 
            
            case "param0": 
                modals.showModalInputInteger("Deep sea level", generationAlgoritm.heightParams[0] , 0, 20); //show modal 
                //waiting for modal state READY 
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } ); 
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show 
                generationAlgoritm.heightParams[0] = modals.value_int; //set param value 
                break; 

            case "param1": 
                modals.showModalInputInteger("Sea level", generationAlgoritm.heightParams[1] , 0, 20);//show modal 
                //waiting for modal state READY 
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } ); 
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show 
                generationAlgoritm.heightParams[1]  = modals.value_int; //set param value 
                break;

            case "param2":
                modals.showModalInputInteger("Beach level", generationAlgoritm.heightParams[2] , 0, 20);//show modal
                //waiting for modal state READY 
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } ); 
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show 
                generationAlgoritm.heightParams[2]  = modals.value_int; //set param value 
                break; 

            case "param3":
                modals.showModalInputInteger("Grass level", generationAlgoritm.heightParams[3], 0, 20);//show modal 
                //waiting for modal state READY 
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } ); 
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show 
                generationAlgoritm.heightParams[3] = modals.value_int; //set param value 
                break; 
            
            case "param4": 
                modals.showModalInputInteger("Deep Grass level", generationAlgoritm.heightParams[4], 0, 20);//show modal 
                //waiting for modal state READY  
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } ); 
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show 
                generationAlgoritm.heightParams[4] = modals.value_int; //set param value 
                break; 

            case "param5": 
                modals.showModalInputInteger("Forest level", generationAlgoritm.heightParams[5], 0, 20);//show modal 
                //waiting for modal state READY 
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } ); 
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show 
                generationAlgoritm.heightParams[5] = modals.value_int; //set param value 
                break; 

            case "param6": 
                modals.showModalInputInteger("Hill level", generationAlgoritm.heightParams[6], 0, 20);//show modal 
                //waiting for modal state READY 
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } ); 
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show 
                generationAlgoritm.heightParams[6] = modals.value_int; //set param value 
                break; 

            case "param7":
                modals.showModalInputInteger("High hill level", generationAlgoritm.heightParams[7], 0, 20);//show modal 
                //waiting for modal state READY 
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } ); 
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show 
                generationAlgoritm.heightParams[7] = modals.value_int; //set param value 
                break; 

            case "param8": 
                modals.showModalInputInteger("Mountain level", generationAlgoritm.heightParams[8], 0, 20);//show modal 
                //waiting for modal state READY 
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } ); 
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show 
                generationAlgoritm.heightParams[8] = modals.value_int; //set param value 
                break; 

            case "param9": 
                modals.showModalInputInteger("Top level", generationAlgoritm.heightParams[9], 0, 20);//show modal 
                //waiting for modal state READY  
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } ); 
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show 
                generationAlgoritm.heightParams[9] = modals.value_int; //set param value 
                break; 
        }

        lblSeedMap.Text = generationAlgoritm.seed;
        lblSizeMap.Text =  generationAlgoritm.sizeMap.ToString();
        lblStyleMap.Text = "not yet";
        lblAlgoParam0.Text = generationAlgoritm.passes.ToString();
        lblAlgoParam1.Text = generationAlgoritm.period.ToString();
        lblAlgoParam2.Text = generationAlgoritm.periodMulti.ToString();
        lblAlgoParam3.Text = generationAlgoritm.heightSteep.ToString();
        lblGenParam0.Text = generationAlgoritm.heightParams[0].ToString();
        lblGenParam1.Text = generationAlgoritm.heightParams[1].ToString();
        lblGenParam2.Text = generationAlgoritm.heightParams[2].ToString();
        lblGenParam3.Text = generationAlgoritm.heightParams[3].ToString();
        lblGenParam4.Text = generationAlgoritm.heightParams[4].ToString();
        lblGenParam5.Text = generationAlgoritm.heightParams[5].ToString();
        lblGenParam6.Text = generationAlgoritm.heightParams[6].ToString();
        lblGenParam7.Text = generationAlgoritm.heightParams[7].ToString();
        lblGenParam8.Text = generationAlgoritm.heightParams[8].ToString();
        lblGenParam9.Text = generationAlgoritm.heightParams[9].ToString();
    }
    
    private void buttonGenerateTerrain(){

        //exe algoritm to generate data terrain
        generationAlgoritm.generateMapData(map);

        //Instancing all map
        map.instanceAllMap();

        // init camera position and zoom
        initcamera();
    }

    //MANUAL EDITION
    public void exeTool(HexaData hxd){
        
        switch(actualToolSelected){
            case "up": map.upTerrain(hxd); break;
            case "up2": map.up2Terrain(hxd); break;
            case "down":map.downTerrain(hxd); break;
            case "down2":map.down2Terrain(hxd); break;
            case "river":
                map.createRiver(lastOrigin,hxd); 
                lastOrigin = hxd;
                break;
            case "riverclear": 
                map.cleanRivers(hxd); 
                lastOrigin = null;
                break;
            case "detail_0":map.placeGO(hxd,0); break;
            case "detailClear":map.placeGO(hxd,-1); break;
        }
    }

}

class GenerationAlgoritm {

    //main param
    public String seed;
    public int sizeMap = 20;
    public int styleMap = 0;

    // algoritm param
    public int passes = 10;
    public float period = 1f;
    public float periodMulti = 2f;
    
    public float heightSteep = 10;

    //min height
    public int[] heightParams = new int[]{
        0,0,0,0,0,0,0,0,0,11
    };

    private int tointSeed(){
        if (seed == null || seed.Length == 0) return 0;
        int ihash = seed.GetHashCode();
        return ihash;
    }

    public void generateMapData(Map map){
        
        //Ramdom
        int seed = tointSeed();
        if (seed == 0){
            map.random = new Random();
            seed = map.random.Next();
        }else{
            map.random = new Random(seed);
        }

        // New map
        map.mapData = new MapData(sizeMap);
        
        //Noise
        OpenSimplexNoise noise = new OpenSimplexNoise();
        noise.Seed = seed;
        noise.Octaves = 8;
        noise.Lacunarity = 1.5f;
        noise.Persistence = 0.02f;
        noise.Period = period;

        int nOffSetX = map.random.Next(-sizeMap,sizeMap);
        int nOffSetY = map.random.Next(-sizeMap,sizeMap);
        
        float[,] dataTerrain = new float[sizeMap,sizeMap];
        float min = float.MaxValue; 
        float max = float.MinValue;
        
        for(int p = 0; p < passes; p++){
            noise.Period = p * periodMulti;

            for (int i = 0; i<sizeMap; i++){
                for (int j = 0; j<sizeMap; j++){
                    float h = dataTerrain[j,i] + Mathf.Abs(noise.GetNoise2d(j+nOffSetX,i+nOffSetY) * heightSteep);
                    dataTerrain[j,i] = h;
                    if (h<min) min = h;
                    if (h>max) max = h;
                }
            }
        }

        // To map data
        for (int i = 0; i<sizeMap; i++){
            for (int j = 0; j<sizeMap; j++){
                float h =  dataTerrain[j,i];
                float value = Mathf.InverseLerp(min,max,h);
                if (value.Equals(float.NaN)) {value = 0;}
                float lerp = Mathf.Lerp(0,heightParams[9],value);
                
                int height =  Mathf.RoundToInt(lerp);
                //min levels
                if (height <= heightParams[0]) height = 0; 
                else if (height <= heightParams[1]) height = 1;
                else if (height <= heightParams[2]) height = 2;
                else if (height <= heightParams[3]) height = 3;
                else if (height <= heightParams[4]) height = 4;
                else if (height <= heightParams[5]) height = 5;
                else if (height <= heightParams[6]) height = 6;
                else if (height <= heightParams[7]) height = 7;
                else if (height <= heightParams[8]) height = 8;
                //else if (height <= heightParams[9]) height = 9;

                //limit
                if (height>10) height = 10;

                HexaData hexaData = map.mapData.GetHexaData(i,j);
                hexaData.setHeight(height);
            }
        }

    }

}
