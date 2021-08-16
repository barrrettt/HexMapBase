using Godot;
using System;

public class GuiModals : Control{

    //modal states
    public enum MODAL_ENUM{
        HIDE,SHOW,READY
    }
    public MODAL_ENUM estado = MODAL_ENUM.HIDE;

    //modal types
    public enum MODAL_TYPE{
        NONE,CONFIRMATION,STRING,INT,FLOAT,BOOLEAN
    }

    public MODAL_TYPE type = MODAL_TYPE.STRING;

    // exit values
    public String value_str = "";
    public int value_int = 0;
    public float value_float = 0.0f;
    public bool value_bool = false;

    //ref to controls
    private Control panel;
    private Label lblTitle;
    private Control subpaneString,subpaneInt,subpaneFloat,subpaneBoolean;
    private Control[] subpanels;

    private Button buOk, buCancel;


    private Control panelMaps;
    private Control parentBuFiles;
    private Button buFile;

    public Boolean isPC = true; //pc-mobile

    public override void _EnterTree(){

        //panel de inputs y mensages
        panel = GetNode<Control>("panel");
        lblTitle = GetNode<Label>("panel/VB/HB/lblTitle");
        subpaneString = GetNode<Control>("panel/VB/HBContent/typeString");
        subpaneInt = GetNode<Control>("panel/VB/HBContent/typeInt");
        subpaneFloat = GetNode<Control>("panel/VB/HBContent/typeFloat");
        subpaneBoolean = GetNode<Control>("panel/VB/HBContent/typeBoolean");

        subpanels = new Control[]{
            subpaneString,subpaneInt,subpaneFloat,subpaneBoolean
        };

        buOk = GetNode<Button>("panel/VB/HBConfirm/buOk");
        buCancel = GetNode<Button>("panel/VB/HBConfirm/buCancel");

        //panel de files
        panelMaps = GetNode<Control>("panelmaps");
        parentBuFiles = GetNode<Control>("panelmaps/VB/HBContent/ScrollContainer/VB");
        buFile = GetNode<Button>("panelmaps/VB/HBContent/ScrollContainer/VB/buFile"); 

        //hide me
        this.Visible = false;

        // mobil or PC
        string osname = Godot.OS.GetName();
        if (osname=="Android" || osname=="iOS" ) {
            isPC = false;
        }

    }

    //show modals
    private void showModal(String title){
        if (this.estado != MODAL_ENUM.HIDE) return;
        this.estado = MODAL_ENUM.SHOW;
        this.Visible = true;
        panel.Visible = true;
        panelMaps.Visible = false;
        lblTitle.Text = title;
        foreach (Control pane in subpanels){ pane.Visible = false;}

        buOk.Visible = true;
        buCancel.Visible = true;

        if (!isPC && type != MODAL_TYPE.NONE){
            Vector2 size = GetViewport().GetVisibleRect().Size;
            float hight4 = size.y / 8;
            panel.RectPosition = new Vector2(panel.RectPosition.x, hight4);//up, no center
        }
    }   

    public void showModalInputString(String title,String value, int maxLeght){
        showModal(title);
        this.type = MODAL_TYPE.STRING;
        subpaneString.Visible = true;

        LineEdit lineEdit = (LineEdit) subpaneString.GetChild(0);
        lineEdit.Text = value;
        lineEdit.MaxLength = maxLeght;
        value_str = value;

        lineEdit.GrabFocus();
        lineEdit.CaretPosition = lineEdit.Text.Length();
    }

    public void showModalInputInteger(String title,int value, int minValue, int maxValue){
        if (minValue>maxValue)return;
        showModal(title);
        this.type = MODAL_TYPE.INT;
        subpaneInt.Visible = true;

        SpinBox sbIn = (SpinBox) subpaneInt.GetChild(0);
        sbIn.Value = value;
        sbIn.MinValue = minValue;
        sbIn.MaxValue = maxValue;
        value_int = value;

        LineEdit lineEdit = sbIn.GetLineEdit();
        lineEdit.GrabFocus();
        //lineEdit.CaretPosition = lineEdit.Text.Length();

    }

    public void showModalInputFloat(String title, float value, float minValue, float maxValue){
        if (minValue>maxValue)return;
        this.type = MODAL_TYPE.FLOAT;
        showModal(title);
        subpaneFloat.Visible = true;

        SpinBox sbIn = (SpinBox) subpaneFloat.GetChild(0);
        sbIn.Value = value;
        sbIn.MinValue = minValue;
        sbIn.MaxValue = maxValue;
        value_float = value;

        LineEdit lineEdit = sbIn.GetLineEdit();
        lineEdit.GrabFocus();
        //lineEdit.CaretPosition = lineEdit.Text.Length();
    }

    public void showModalInputBoolean(String title, Boolean value){
        this.type = MODAL_TYPE.BOOLEAN;
        showModal(title);
        subpaneBoolean.Visible = true;

        CheckButton ckb = (CheckButton) subpaneFloat.GetChild(0);
        ckb.Pressed = value;
        value_bool = value;

        ckb.GrabFocus();
    }

    public void showModalMessage(String title){
        this.type = MODAL_TYPE.NONE;
        showModal(title);
        buCancel.Visible = false;

        buOk.GrabFocus();
    }

    public void showConfirmationMessage(String title){
        this.type = MODAL_TYPE.CONFIRMATION;
        showModal(title);
        buCancel.Visible = true;
        buOk.GrabFocus();
    }

    //show maps
    public void showMaps(){
        if (this.estado != MODAL_ENUM.HIDE) return;
        this.estado = MODAL_ENUM.SHOW;
        this.Visible = true;
        panel.Visible = false;
        panelMaps.Visible = true;
        value_str ="";

        if (!isPC && type != MODAL_TYPE.NONE){
            Vector2 size = GetViewport().GetVisibleRect().Size;
            float hight4 = size.y / 8;
            panelMaps.RectPosition = new Vector2(panel.RectPosition.x, hight4);//up, no center
        }

        // limpia botones anteriores
        foreach (Node node in parentBuFiles.GetChildren()){
            node.Free();
        }

        Directory dir = new Directory();
        string path = "user://maps/"; 
        Error error = dir.Open(path);
        if (error!= Error.Ok){
            GD.Print("No hay directorio de mapas");
            return;
        }

        error = dir.ListDirBegin(false,false);
        while(true){
            String next = dir.GetNext();
            if (next == "")break;
            bool isDir = dir.CurrentIsDir();
            if (isDir)continue;

            Button buFile = new Button();
            buFile.Text=(next);
            buFile.RectMinSize = new Vector2(0,90);
            buFile.Connect("pressed",this,"mapClick",new Godot.Collections.Array{next});
            parentBuFiles.AddChild(buFile);
        }
    }   

    //SET READY
    public void inputReady(bool isValid){ 
        if (estado != MODAL_ENUM.SHOW)return; 
        estado = MODAL_ENUM.READY; 
        this.Visible = false; 

        //cancel do nothing 
        if (!isValid) {
            value_bool = false;
            value_str = "";
            return;
        }
        
        //get value
        switch (type){
            case MODAL_TYPE.CONFIRMATION:
            value_bool = true;
            break;

            case MODAL_TYPE.STRING: 
            LineEdit lineIn = (LineEdit) subpaneString.GetChild(0);
            value_str = lineIn.Text;
            break;

            case MODAL_TYPE.INT: 
            SpinBox sbIn = (SpinBox) subpaneInt.GetChild(0);
            value_int = (int)sbIn.Value;
            break;

            case MODAL_TYPE.FLOAT: 
            sbIn = (SpinBox) subpaneFloat.GetChild(0);
            value_float = (float)sbIn.Value;
            break;

            case MODAL_TYPE.BOOLEAN: 
            CheckButton ckb = (CheckButton) subpaneFloat.GetChild(0);
            value_bool = ckb.Pressed;
            break;
        }
    }

    public void mapClick(String filename){
        filename = filename.Substring(0,filename.Length-4);
        value_str = filename;
        estado = MODAL_ENUM.READY; 
        this.Visible = false; 
    }

}
