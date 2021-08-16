using Godot;
using System;
using System.Threading.Tasks;

public class Editor : Spatial{
    private Camara camara;
    private WorldEnvironment centro;
    private String mapFileName = "";
    private Map map;

    private GenerationAlgoritm generationAlgoritm = new GenerationAlgoritm();

    //GUI REFERENCES
    private ColorRect CrSelected;
    private Label lblMousePos1, lblCameraPos1, lblSelectedPos1,lblMousePos2, lblCameraPos2, lblSelectedPos2;

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
    int ballnumber = 20;
    private RigidBody[] balls; 
    //GENERATION
    private Random random = new Random(); 

    private bool isPc = true;

    public override void _EnterTree(){ 
        //PC?
        // mobil or PC
        string osname = Godot.OS.GetName();
        if (osname=="Android" || osname=="iOS" ) {
            isPc = false;
        }

        //Things
        centro = GetNode<WorldEnvironment>("center"); 
        map = centro.GetNode<Map>("Map"); 
        camara = centro.GetNode<Camara>("Camara"); 
        spaceState = GetWorld().DirectSpaceState; //physic ray neededs 

        //colorrect selected
        CrSelected = GetNode<ColorRect>("GUI/BottomPanel/HB/LPanel/HB/CC/ColorRect");
        CrSelected.Color = new Color("#ffffff00");

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
        generationAlgoritm.sizeMap = 10;
        generationAlgoritm.generateMapData(map);
        map.instanceAllMap(); //Show all map
        initcamera();//camera to center
        buttonSetGenerationParam("");  //set algoritm params to gui

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
            CrSelected.Color = Hexagon.colors[hx.hexData.colorIndex];

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
                int maxsize = 100; if (!isPc) maxsize = 50;

                modals.showModalInputInteger("Map size", generationAlgoritm.sizeMap,0,maxsize);
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
                modals.showModalInputInteger("Deep sea level", generationAlgoritm.heightParams[0] , 0, 100); //show modal 
                //waiting for modal state READY 
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } ); 
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show 
                generationAlgoritm.heightParams[0] = modals.value_int; //set param value 
                break; 

            case "param1": 
                modals.showModalInputInteger("Sea level", generationAlgoritm.heightParams[1] , 0, 100);//show modal 
                //waiting for modal state READY 
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } ); 
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show 
                generationAlgoritm.heightParams[1]  = modals.value_int; //set param value 
                break;

            case "param2":
                modals.showModalInputInteger("Beach level", generationAlgoritm.heightParams[2] , 0, 100);//show modal
                //waiting for modal state READY 
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } ); 
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show 
                generationAlgoritm.heightParams[2]  = modals.value_int; //set param value 
                break; 

            case "param3":
                modals.showModalInputInteger("Grass level", generationAlgoritm.heightParams[3], 0, 100);//show modal 
                //waiting for modal state READY 
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } ); 
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show 
                generationAlgoritm.heightParams[3] = modals.value_int; //set param value 
                break; 
            
            case "param4": 
                modals.showModalInputInteger("Deep Grass level", generationAlgoritm.heightParams[4], 0, 100);//show modal 
                //waiting for modal state READY  
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } ); 
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show 
                generationAlgoritm.heightParams[4] = modals.value_int; //set param value 
                break; 

            case "param5": 
                modals.showModalInputInteger("Forest level", generationAlgoritm.heightParams[5], 0, 100);//show modal 
                //waiting for modal state READY 
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } ); 
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show 
                generationAlgoritm.heightParams[5] = modals.value_int; //set param value 
                break; 

            case "param6": 
                modals.showModalInputInteger("Hill level", generationAlgoritm.heightParams[6], 0, 100);//show modal 
                //waiting for modal state READY 
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } ); 
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show 
                generationAlgoritm.heightParams[6] = modals.value_int; //set param value 
                break; 

            case "param7":
                modals.showModalInputInteger("High hill level", generationAlgoritm.heightParams[7], 0, 100);//show modal 
                //waiting for modal state READY 
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } ); 
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show 
                generationAlgoritm.heightParams[7] = modals.value_int; //set param value 
                break; 

            case "param8": 
                modals.showModalInputInteger("Mountain level", generationAlgoritm.heightParams[8], 0, 100);//show modal 
                //waiting for modal state READY 
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } ); 
                modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show 
                generationAlgoritm.heightParams[8] = modals.value_int; //set param value 
                break; 

            case "param9": 
                modals.showModalInputInteger("Max level", generationAlgoritm.heightParams[9], 0, 100);//show modal 
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

    private async void buttonSave(){
        modals.showModalInputString("Map name", mapFileName, 20);
        await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } );
        modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show
        String outname = modals.value_str; //set param value
        if (outname == "") return; // cancel

        if (outname.Length < 8){
            modals.showModalMessage("Map name too small. Required lenght: 8-20");
            await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } );
            modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show
            return;
        }

        mapFileName = outname; //set new name
        await Task.Run(() => { 
            save();
        }); 

        buttonToolSelect("");//hide panel
    }

    private async void buttonLoad(){
        //modal maps
        modals.showMaps();
        await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } );
        modals.estado = GuiModals.MODAL_ENUM.HIDE; //mark ready for show
        mapFileName = modals.value_str; //set param value

        //load
        bool result = false;
        await Task.Run(() => { 
            result = load();
        }); 

        if (result){
            this.map.instanceAllMap();
            this.initcamera();
            buttonToolSelect("");//hide panel
        }
        
    }

    private async void buttonDelete(){
        if (mapFileName == "") return;
        modals.showConfirmationMessage("Delete map " + mapFileName + "?");
        await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } );
        modals.estado = GuiModals.MODAL_ENUM.HIDE;
        bool ok = modals.value_bool;

        if (ok){
            Godot.File file = new Godot.File(); 
            string path = "user://maps/" +mapFileName+".map"; 

            if (!file.FileExists(path)){ 
                modals.showModalMessage("Map not exist!");
                await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } );
                modals.estado = GuiModals.MODAL_ENUM.HIDE;
                return; 
            }

            Directory dir = new Directory();
            Error error = dir.Remove(path);
            if (error == Error.Ok){
                mapFileName = "";
                //new minimap
                generationAlgoritm.sizeMap = 5;
                generationAlgoritm.generateMapData(map);
                map.instanceAllMap(); //Show all map
                initcamera();//camera to center

                modals.showModalMessage("Map deleted!");
            }else{
                modals.showModalMessage("Error on delete file!");
            }

            //message wait
            await Task.Run(() => {do{}while(modals.estado != GuiModals.MODAL_ENUM.READY); } );
            modals.estado = GuiModals.MODAL_ENUM.HIDE;


        }

        buttonToolSelect("");//hide panel
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

    //SAVE / LOAD
    public void save(){
        GD.Print("Saving map " + mapFileName);
        Godot.File file = new Godot.File();
        String pathDir = "user://maps";
        String pathFile =  "/"+mapFileName+".map";
        Godot.Directory directory = new Godot.Directory();

        if (directory.Open(pathDir) != Error.Ok){
            directory.MakeDir(pathDir);
            GD.Print("Dir maps created");
        }
       
        //open
        Error error = file.Open(pathDir+pathFile , Godot.File.ModeFlags.Write);
        if (error!= Error.Ok) return;

        //Serialize xml
        GD.Print("File absolute: " + file.GetPathAbsolute());
        MapData m = map.mapData;
        String t = "\t"; //prettyPrint
        file.StoreLine("<map version=\"1.0\" size=\""+m.getSize()+"\" >");
            
            foreach(HexaData d in m.datas){
                file.StoreLine(t+"<hexdata r=\""+d.row+"\" c=\""+d.col+"\">");
                    file.StoreLine(t+t+"<color>"+d.colorIndex+"</color>");
                    file.StoreLine(t+t+"<height>"+d.height+"</height>");
                    file.StoreLine(t+t+"<GO>"+d.indexGO+"</GO>");
                    file.StoreLine(t+t+"<water>"+d.water+"</water>");
                    file.StoreLine(t+t+"<river>"+d.river+"</river>");
                    file.StoreLine(t+t+"<riversout>");
                        file.StoreLine(t+t+t+"<r0>" + ((d.riversOut[0] != null)?"true":"false") + "</r0>");
                        file.StoreLine(t+t+t+"<r1>" + ((d.riversOut[1] != null)?"true":"false") + "</r1>");
                        file.StoreLine(t+t+t+"<r2>" + ((d.riversOut[2] != null)?"true":"false") + "</r2>");
                        file.StoreLine(t+t+t+"<r3>" + ((d.riversOut[3] != null)?"true":"false") + "</r3>");
                        file.StoreLine(t+t+t+"<r4>" + ((d.riversOut[4] != null)?"true":"false") + "</r4>");
                        file.StoreLine(t+t+t+"<r5>" + ((d.riversOut[5] != null)?"true":"false") + "</r5>");
                    file.StoreLine(t+t+"</riversout>");
                file.StoreLine(t+"</hexdata>");
            }

        file.StoreLine("</map>");
        file.Close();
        GD.Print("Map saved!");
    }

    public bool load() { 
        
        Godot.File file = new Godot.File(); 
        string path = "user://maps/" +mapFileName+".map"; 

        if (!file.FileExists(path)){ 
            GD.Print("File no exist: " +path); 
            return false; 
        }

        //xml parser 
        file.Open(path,Godot.File.ModeFlags.Read);
        XMLParser xmlP = new XMLParser(); 
        Error err = xmlP.Open(path); 
        if (err != Error.Ok) {return false;} 
        GD.Print("Loading map: " + file.GetPathAbsolute()); 

        xmlP.Read(); 
        String version = xmlP.GetAttributeValue(0); 
        int sizeMap = int.Parse(xmlP.GetAttributeValue(1)); 
        MapData map = new MapData(sizeMap); 

        for (int j = 0; j<sizeMap; j++){ 
            for (int i = 0; i<sizeMap; i++){ 
                
                XMLParser.NodeType nodeType;
                String nodeNome = "";
                do{
                    xmlP.Read();
                    nodeType = xmlP.GetNodeType();
                    if (nodeType==XMLParser.NodeType.Element){
                        nodeNome = xmlP.GetNodeName();
                        if (nodeNome == "hexdata") break;
                    }
                }while(true);

                int r = int.Parse( xmlP.GetAttributeValue(0)); 
                int c = int.Parse(xmlP.GetAttributeValue(1));
                HexaData hd = map.GetHexaData(r,c);

                if (hd == null){GD.Print("Error: xml bad format!");}
                
                //fields
                hd.colorIndex = int.Parse(auxLoadData(xmlP,"color"));
                hd.height = int.Parse(auxLoadData(xmlP,"height"));
                hd.indexGO = int.Parse(auxLoadData(xmlP,"GO"));
                hd.water = bool.Parse(auxLoadData(xmlP,"water"));
                hd.river = bool.Parse(auxLoadData(xmlP,"river"));

                //riber out
                //String data = getNextTextNodeData(xmlP,"riversout");
                xmlP.Read();xmlP.Read();
                //6 neighbours
                bool r0 = bool.Parse(auxLoadData(xmlP,"r0"));
                bool r1 = bool.Parse(auxLoadData(xmlP,"r1"));
                bool r2 = bool.Parse(auxLoadData(xmlP,"r2"));
                bool r3 = bool.Parse(auxLoadData(xmlP,"r3"));
                bool r4 = bool.Parse(auxLoadData(xmlP,"r4"));
                bool r5 = bool.Parse(auxLoadData(xmlP,"r5"));
                
                //river bools to refs: SE,S,...
                if (r0) hd.riversOut[0] = hd.neighbours[0];
                if (r1) hd.riversOut[1] = hd.neighbours[1];
                if (r2) hd.riversOut[2] = hd.neighbours[2];
                if (r3) hd.riversOut[3] = hd.neighbours[3];
                if (r4) hd.riversOut[4] = hd.neighbours[4];
                if (r5) hd.riversOut[5] = hd.neighbours[5];

            }
        }
        file.Close();
        
        // data to map 
        this.map.mapData = map;
        return true;
    }

    private String auxLoadData(XMLParser xmlP, String nodename){
        int i = 0;
        int max = 50;
        String value = null;
        do{
            xmlP.Read();
            XMLParser.NodeType type = xmlP.GetNodeType();
            if (type== XMLParser.NodeType.Element){
                String name = xmlP.GetNodeName();
                if (name == nodename){
                    xmlP.Read();
                    value = xmlP.GetNodeData();
                    xmlP.Read();
                    break;
                }
            }
            i++;
        }while(i<max);

        return value;
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
