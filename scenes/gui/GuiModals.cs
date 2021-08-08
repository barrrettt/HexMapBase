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
        NONE,STRING,INT,FLOAT,BOOLEAN
    }

    public MODAL_TYPE type = MODAL_TYPE.STRING;

    // exit values
    public String value_str = "";
    public int value_int = 0;
    public float value_float = 0.0f;
    public bool value_bool = false;

    //ref to controls
    private Label lblTitle;
    private Control subpaneString,subpaneInt,subpaneFloat,subpaneBoolean;
    private Control[] subpanels;

    private Button buOk, buCancel;

    public override void _EnterTree(){
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

        //hide me
        this.Visible = false;
    }

    private void showModal(String title){
        if (this.estado != MODAL_ENUM.HIDE) return;
        this.estado = MODAL_ENUM.SHOW;
        this.Visible = true;
        lblTitle.Text = title;
        foreach (Control pane in subpanels){ pane.Visible = false;}

        buOk.Visible = true;
        buCancel.Visible = true;
    }   

    public void showModalInputString(String title,String value, int maxLeght){
        showModal(title);
        this.type = MODAL_TYPE.STRING;
        subpaneString.Visible = true;

        LineEdit lineIn = (LineEdit) subpaneString.GetChild(0);
        lineIn.Text = value;
        lineIn.MaxLength = maxLeght;
        value_str = value;
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
    }

    public void showModalInputBoolean(String title, Boolean value){
        this.type = MODAL_TYPE.BOOLEAN;
        showModal(title);
        subpaneBoolean.Visible = true;

        CheckButton ckb = (CheckButton) subpaneFloat.GetChild(0);
        ckb.Pressed = value;
        value_bool = value;
    }

    public void showModalMessage(String title){
        this.type = MODAL_TYPE.NONE;
        showModal(title);
        buCancel.Visible = false;
    }

    //SET READY
    public void inputReady(bool isValid){ 
        if (estado != MODAL_ENUM.SHOW)return; 
        estado = MODAL_ENUM.READY; 
        this.Visible = false; 
        if (!isValid) return; //cancel do nothing 

        switch (type){
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

}
