using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ScottGarland;

public enum Menu { Menu, Stats, Settings, Updates }
public partial class MonoGame : MonoBehaviour 
{

    public static MonoGame me;

    public const int floatMaxDecs = 10000000;
    private const int itemTransHeight = 50;

    //public static int curIndex = 0;

    public Texture2D metalTex, stoneTex, background, clickTex;
    public float fadeTime = 2, accTime = 1;

    public float screenPerc
    {
        get
        {
            return Screen.width / authorScreenWidth;
        }
    }

    /*
    
        stonePadd = Screen.width * .03f;
        stoneWidth = LeftBlockWidth - stonePadd * 2;
        stoneHeight = stoneTex.height * stoneWidth / stoneTex.width;
        
    */

    private float stonePadd
    {
        get
        {
            return Screen.width * .03f;
        }
    }

    private float stoneWidth
    {
        get
        {
            return LeftBlockWidth - stonePadd * 2;
        }
    }

    private float stoneHeight
    {
        get
        {
            int sw = stoneTex == null ? 175 : stoneTex.width;
            return stoneTex.height * stoneWidth / sw;
        }
    }

    private const float authorScreenWidth = 1366f;

    private AnimUtil animBtc;
    private Texture2D separator1, separator2, separator3;
	private Menu curMenu;
    private Rect LeftBlock
    {
        get
        {
            return new Rect(0, 0, LeftBlockWidth, Screen.height);
        }
    }

    private Rect RightBlock
    {
        get
        {
            int s3 = separator3 == null ? 17 : separator3.height;
            return new Rect(Screen.width - RightBlockWidth, 50 + s3, RightBlockWidth, Screen.height);
        }
    }

    /*
    
        LeftBlock = new Rect(0, 0, LeftBlockWidth, Screen.height);
        RightBlock = new Rect(Screen.width - RightBlockWidth, 50 + separator3.height, RightBlockWidth, Screen.height);

    */

    //Settings
    private int s_quality;
    private bool s_runInBack;
    private Rect sr_quality, sr_runInBack, sr_padding;

    private List<HoveringElement> h_Elems = new List<HoveringElement>();

    //Misc GUI
	private Texture2D sep;

    //Hashes related variables
    private BigInteger MinedHashes = 0, HashesPerClick = 1, AccHashRate = 0, lastHashes = 0, HashRate = 0, BTCRate = 0, lastBTC = 0, allBTCs = 0, clickedHashes = 0, l_MinedHashes = 0, lastHR = 0; //l_HashRate = 0, l_lastHashes = 0;
    public BigInteger Bitcoins = 0;

    private float btcTimer = 0;

    /*
    
        LeftBlockWidth = Mathf.Round((Screen.width - separator1.width * 2) * .3378f); //100 + stoneTex.width * 2;
        CentralBlockWidth = Mathf.Round((Screen.width - separator1.width * 2) * .3371f);
        RightBlockWidth = Mathf.Round((Screen.width - separator1.width * 2) * .3251f);

    */

    //Sizes
    private float LeftBlockWidth
    {
        get
        {
            int s1 = separator1 == null ? 17 : separator1.width;
            return Mathf.Round((Screen.width - s1 * 2) * .3378f);
        }
    }

    private float CentralBlockWidth
    {
        get
        {
            int s1 = separator1 == null ? 17 : separator1.width;
            return Mathf.Round((Screen.width - s1 * 2) * .3371f);
        }
    }

    private float RightBlockWidth
    {
        get
        {
            int s1 = separator1 == null ? 17 : separator1.width;
            return Mathf.Round((Screen.width - s1 * 2) * .3251f);
        }
    }

    private float BackgroundWidth
    {
        get
        {
            int s1 = separator1 == null ? 17 : separator1.width;
            return CentralBlockWidth + s1 * 2;
        }
    }

    //Pieces of boulder utility
    private List<TextureFade> texFade = new List<TextureFade>();
    private int quickCount;
        
    //Clicks acumulative
    private List<ClickInfo> clicks = new List<ClickInfo>();
    private int accClicks;

    //Machines
    //public List<ListWrapper> Machines = new List<ListWrapper>();
    public List<Transistors> transGroups = new List<Transistors>();
    public List<Transistors> unblockedTrans = new List<Transistors>();

    //Block related variables
    private int currentBlock;

    //Exchange related variables
    private float ExchangeFactor;
    /*
     
     (+)
     0.1 per transacction
     0.25 per save (every 10 mins)
     0.5 every resolved block
     200-2000% (faded) * bitcoin
     5.0 each new diff (2,016 blocks)
     30 each reward change (210,000 blocks)
     
     (-)
     ...
     
     */

    public static Vector2 fixedMousePos
    {
        get
        {
            return new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
        }
    }

	// Use this for initialization
	void Start() 
    {

        //Init
        me = this;

        //Needs to be here because of the next request to it in 'CentralBlockWidth' var
        separator1 = (new TextureBorder(new TextureCrop(metalTex, new Rect(0, 0, 15, metalTex.height)).GetTexture()) { leftBorder = 1, rightBorder = 1, topBorder = 0, bottomBorder = 0 }).GetTexture();

        //Set the blocks' sizes
        //LeftBlockWidth = Mathf.Round((Screen.width - separator1.width * 2) * .3378f); //100 + stoneTex.width * 2;
        //CentralBlockWidth = Mathf.Round((Screen.width - separator1.width * 2) * .3371f);
        //RightBlockWidth = Mathf.Round((Screen.width - separator1.width * 2) * .3251f);

        //CentralBlockWidth = Screen.width - (LeftBlockWidth * 2 + separator1.width);
        //BackgroundWidth = CentralBlockWidth + separator1.width * 2;

        //Load textures
        animBtc = new AnimUtil("Textures/coin");

        //Separators
        separator2 = (new TextureBorder(new TextureCrop(metalTex, new Rect(0, 0, CentralBlockWidth+2, 15)).GetTexture()) { leftBorder = 0, rightBorder = 0, topBorder = 1, bottomBorder = 1 }).GetTexture();
        separator3 = (new TextureBorder(new TextureCrop(metalTex, new Rect(0, 0, LeftBlockWidth+2, 15)).GetTexture()) { leftBorder = 0, rightBorder = 0, topBorder = 1, bottomBorder = 1 }).GetTexture();

		sep = new Texture2D(1, 1);
		sep.SetPixel(1, 1, new Color(.8f, .8f, .8f));
		sep.Apply();

        //Some rects...
        //LeftBlock = new Rect(0, 0, LeftBlockWidth, Screen.height);
        //RightBlock = new Rect(Screen.width - RightBlockWidth, 50 + separator3.height, RightBlockWidth, Screen.height);

        //Set stone props
        //stonePadd = Screen.width * .03f;
        //stoneWidth = LeftBlockWidth - stonePadd * 2;
        //stoneHeight = stoneTex.height * stoneWidth / stoneTex.width;

        //Load settings
        s_quality = QualitySettings.GetQualityLevel();
        sr_quality = new Rect(5, 80, BackgroundWidth - 45, 20);
        s_runInBack = Application.runInBackground;
        sr_runInBack = new Rect(5, 100, BackgroundWidth - 45, 20);
        sr_padding = new Rect(LeftBlockWidth + separator1.width, 100 + separator2.height, 0, 0);

        h_Elems.Add(new HoveringElement(sr_quality.Change(-20, RectSides.Top).Change(20, RectSides.Bottom).Add(sr_padding), new Rect(0, 0, 500, 40), "Cambiar la calidad te dará unos FPS más, cámbia esta opción únicamente si tu ordenador es una verdadera patata.", Menu.Settings));
        h_Elems.Add(new HoveringElement(sr_runInBack.Add(sr_padding), new Rect(0, 0, 500, 120), "Esta opción te permitirá seguir ejecutando el juego en segundo plano, es decir, si está opción está marcada y la aplicación del juego pierde el foco el juego seguirá ejecutándose.\nCompruébalo desactivando esta opción y haciendo click en algún lado de la página fuera del cuadro del juego.\nSi tu microprocesador no es muy potente le vendría bien un descanso de vez en cuando, entonces es cuando deberías activar esta opción.", Menu.Settings));

        //Init machines
        transGroups.Add(new Transistors(5) { mmSize = 100 });
        transGroups.Add(new Transistors(35) { mmSize = 166 });
        transGroups.Add(new Transistors(225) { mmSize = 333 });
        transGroups.Add(new Transistors(1180) { mmSize = 666 });
        transGroups.Add(new Transistors(3333) { mmSize = 1000 });
        transGroups.Add(new Transistors(6250) { mmSize = 1250 });
        transGroups.Add(new Transistors(13000) { mmSize = 1666 });
        transGroups.Add(new Transistors(43500) { mmSize = 2857 });
        transGroups.Add(new Transistors(96000) { mmSize = 4000 });
        transGroups.Add(new Transistors(205000) { mmSize = 5555 });
        transGroups.Add(new Transistors(435000) { mmSize = 7692 });
        transGroups.Add(new Transistors(990000) { mmSize = 11111 });
        transGroups.Add(new Transistors(2050000) { mmSize = 15384 });
        transGroups.Add(new Transistors(4600000) { mmSize = 22222 });
        transGroups.Add(new Transistors(9765000) { mmSize = 31250 });
        transGroups.Add(new Transistors(22000000) { mmSize = 45454 });
        transGroups.Add(new Transistors(57500000) { mmSize = 71428 });
        transGroups.Add(new Transistors(120000000) { mmSize = 100000 });
        transGroups.Add(new Transistors(260000000) { mmSize = 142857 });
        transGroups.Add(new Transistors(533333333) { mmSize = 200000 });
        transGroups.Add(new Transistors(2240000000) { mmSize = 400000 });
        transGroups.Add(new Transistors(3666666667) { mmSize = 500000 });
        transGroups.Add(new Transistors(15333333333) { mmSize = 1000000 });
        transGroups.Add(new Transistors(21160000000) { mmSize = 1150000 });
        transGroups.Add(new Transistors(26050000000) { mmSize = 1250000 });
        transGroups.Add(new Transistors(30815000000) { mmSize = 1333333 });
        transGroups.Add(new Transistors(40500000000) { mmSize = 1500000 });
        transGroups.Add(new Transistors(64000000000) { mmSize = 1850000 });
        transGroups.Add(new Transistors(10240000000) { mmSize = 2300000 });
        transGroups.Add(new Transistors(14600000000) { mmSize = 2700000 });
        transGroups.Add(new Transistors(18600000000) { mmSize = 3000000 });
        transGroups.Add(new Transistors(43200000000) { mmSize = 4500000 });
        transGroups.Add(new Transistors(79200000000) { mmSize = 6000000 });
        transGroups.Add(new Transistors(145000000000) { mmSize = 8000000 });
        transGroups.Add(new Transistors(233333333333) { mmSize = 10000000 });
        transGroups.Add(new Transistors(-1) { mmSize = -1 });

        unblockedTrans.Add(transGroups[0]);

    }
	
	// Update is called once per frame
	void Update() 
    {
        if (Input.GetMouseButtonDown(0) && LeftBlock.Contains(fixedMousePos))
        {
            texFade.Add(new TextureFade() { pos = new Rect(fixedMousePos.x-16, fixedMousePos.y-16, 32, 32), tex = clickTex });
            quickCount++;
            clicks.Add(new ClickInfo() { cTime = Time.time });
            accClicks++;
            MinedHashes += HashesPerClick;
			clickedHashes++;
        }
        if (quickCount > 0)
            for (int i = 0; i < quickCount; ++i)
            {
                texFade[i].time += Time.deltaTime;
                if (texFade[i].time > fadeTime)
                {
                    texFade.RemoveAt(i);
                    --quickCount;
                }
            }
        if (accClicks > 0) 
            for(int i = 0; i < accClicks; ++i) 
                if(Time.time - clicks[i].cTime > accTime) 
                {
                    clicks.RemoveAt(i);
                    --accClicks;
                }
        AccHashRate = accClicks * HashesPerClick;
        if (RightBlock.Contains(fixedMousePos) && Input.GetAxis("Mouse ScrollWheel") != 0 && Transistors.rightScroll >= 0 && Transistors.rightScroll <= unblockedTrans.Count*itemTransHeight-(RightBlock.height-RightBlock.yMin))
            Transistors.rightScroll -= Input.GetAxis("Mouse ScrollWheel")/Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.F11) && !Screen.fullScreen)
        {
            Screen.SetResolution(Screen.resolutions[0].width, Screen.resolutions[0].height, false);
            Debug.Log("Going to fullscreen...");
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && Screen.fullScreen)
            Screen.fullScreen = false;
    }

    void FixedUpdate() 
    {
        //Lerping...
        l_MinedHashes = MinedHashes + BIntLerp(0, BigInteger.ToUInt64(lastHR), btcTimer);

        foreach (Transistors t in transGroups)
            if (t.unitsAdquired > 0)
                t.l_minedHashes = t.minedHashes + BIntLerp(0, BigInteger.ToUInt64(t.getHRate()), btcTimer);

        //l_HashRate = l_MinedHashes - l_lastHashes;
        //l_lastHashes = l_MinedHashes;

        if (btcTimer >= 1)
        { //Esto se llama a cada segundo para obtener una lectura precisa
            HashRate = MinedHashes - lastHashes;
            lastHashes = MinedHashes;

			BTCRate = Bitcoins - lastBTC;
			lastBTC = Bitcoins;

            if (HashRate > 0 && allBTCs <= 21000000000000)
            {
                BigInteger deltaHT = FloatToBInt(HashRate, 1 - Random.value) / (int)(1 / Time.deltaTime);
                if (deltaHT > 1) {
                    Bitcoins += deltaHT;
					allBTCs += deltaHT;
				} else if (Random.value < .5f) {
                    ++Bitcoins;
					++allBTCs;
				}
            }

            lastHR = 0;

            foreach (Transistors t in transGroups)
                if (t.unitsAdquired > 0)
                {
                    BigInteger hr = t.getHRate();
                    lastHR += hr;
                    if (hr > 0)
                    {
                        t.minedHashes += hr;
                        MinedHashes += hr;
                    }
                }

            btcTimer = 0;
        }

        btcTimer += Time.deltaTime;
    }

    void OnGUI()
    {

        //Bloque de la derecha
        GUI.DrawTexture(new Rect(0, 0, BackgroundWidth, Screen.height), background);

        //Falling coins goes here

        GUI.DrawTexture(new Rect(LeftBlockWidth/2-stoneWidth/2, Screen.height/2-stoneHeight/2, stoneWidth, stoneHeight), stoneTex);
        animBtc.Draw(LeftBlockWidth / 2 - 32, Screen.height / 2 - 16);

        //Pieces of rocks goes here

        Color c = GUI.color;
        foreach (TextureFade t in texFade)
        {
            t.alpha -= Time.deltaTime/fadeTime;
            c.a = t.alpha;
            GUI.color = c;
            GUI.DrawTexture(t.pos, t.tex);
        }

        c.a = 1;
        GUI.color = c;

        //Debug...
        //if(GUI.Button(new Rect(5, 5, 100, 20), "Debug bought items!"))
        //    foreach(Transistors t in transGroups)
        //        Debug.Log("Transistor de "+ t.getSize() + ": x"+t.unitsAdquired);

        GUI.Box(new Rect(-5, 30 * (screenPerc - .2f), LeftBlockWidth + 10, 50), "");
        GUI.Label(new Rect(0, 32.5f * (screenPerc - .2f), LeftBlockWidth, 50), "Hashes minadas: " + GetPrefix(l_MinedHashes), new GUIStyle("label") { fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.UpperCenter, padding = new RectOffset(-50, 0, 3, 3) });
        //GUI.Label(new Rect(285 * (screenPerc + .07f), 22 * (screenPerc - .4f), 300, 50), GetPrefix(l_MinedHashes), new GUIStyle("label") { fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft });
        GUI.Label(new Rect(0, 35 * (screenPerc - .2f), LeftBlockWidth, 40), "por segundo: " + GetPrefix(HashRate + AccHashRate) +"/s", new GUIStyle("label") { fontSize = 12, alignment = TextAnchor.LowerCenter });

        GUI.Box(new Rect(10, Screen.height-30, 200, 25), GetPrefix(Bitcoins, UnitType.Inter, "{0} {1}BTCs"));

        //Left separator
        GUI.DrawTexture(new Rect(LeftBlockWidth, -2, separator1.width, separator1.height), separator1);

        //Right sepatator
        GUI.DrawTexture(new Rect(Screen.width - RightBlockWidth - separator1.width, -2, separator1.width, separator1.height), separator1);

        //Next feature: resize texture on click & mouse hand texture around the rock

        //Areas

        //Bloque central
        GUI.BeginGroup(new Rect(LeftBlockWidth + separator1.width, 0, CentralBlockWidth, Screen.height));
        GUI.DrawTexture(new Rect(0, 0, BackgroundWidth, Screen.height), background);
		if(GUI.Button(new Rect(0, 0, 150 * screenPerc, 45), "Estadísticas")) curMenu = Menu.Stats;
		if(GUI.Button(new Rect(0, 55, 150 * screenPerc, 45), "Ajustes")) curMenu = Menu.Settings;
		if(GUI.Button(new Rect(BackgroundWidth-150 * screenPerc-34, 0, 150 * screenPerc, 45), "Info")) curMenu = Menu.Menu;
		GUI.BeginGroup(new Rect(0, 100+separator2.height, BackgroundWidth, Screen.height - 100));
		GUI.Box(new Rect(-1, -1, BackgroundWidth-30, Screen.height - 100 - separator2.height + 5), "");
		if(curMenu == Menu.Menu) 
			GUI.Label(new Rect(5, 5, BackgroundWidth-44, Screen.height - 100 - separator2.height), "<size="+Mathf.Round(20*screenPerc+.1f)+"><b>¡Bienvenido a Bitcoin Mining Game!</b></size>\n\nEste juego trata de basarse en realidad, el minado de Bitcoins es algo que está hace no mucho estaba en auge, pero que poco a poco está decayendo debido a la relación coste/beneficios que es demasiado deficiente y seguirá siendo mayor a lo largo del tiempo.\nEl propósito de este juego es enseñar a todos los publicos como de compleja resuelta esta función. Aunque ahora mismo no tenga mucho contenido, en el futuro el juego intentará ser un fiel retrato a la realidad vigente, hasta entonces debéis aguardar. ;)\n\nHaz click sobre la roca de la derecha para conseguir tus primeras Hashes que te harán ganar unas cuantas microBitcoins las cuales te servirán para comprar tus primeros diez mil transistores que te otorgarán una capacidad de minado de 10 Hashes/segundo.\nPoco a poco ve consiguiendo más Bitcoins para ir pudiendo comprar transistores con un proceso de fabricación más eficiente.\n\nArriba tienes dos menús: Estadísticas y Ajustes.\n- Estadísticas: Te mostrará información detallada del proceso de minado.\n- Ajustes: Podrás cambiar la configuración del juego.\n\n<b>Nota:</b> Juega este juego en pantalla completa para disfrutar de una mejor experiencia.\n\nEl equipo de Lerp2Dev espera que tu experiencia jugando sea de tu agrado, en especial Ikillnukes. ;)\nUn saludo.", new GUIStyle("label") { richText = true, fontSize = Mathf.RoundToInt(13*(screenPerc+.2f)) }); //\n\n<b>Nota:</b> Ni el coste de los transistores, ni el Hashrate del mismo es lineal, es decir, cada 5 items de transistor comprados el Hashrate y el coste cambian torno a una función exponencial.
		else if(curMenu == Menu.Stats) //En el futuro poner un scroll...
		{
			GUI.Label(new Rect(5, 5, BackgroundWidth-44, 25), "Estadísticas", new GUIStyle("label") { alignment = TextAnchor.MiddleCenter, fontSize = 24, fontStyle = FontStyle.Bold });
			GUI.Label(new Rect(5, 30, BackgroundWidth-44, 25), "General", new GUIStyle("label") { fontSize = 20 });
			GUI.DrawTexture(new Rect(5, 55, BackgroundWidth-44, 2), sep);
			GUI.Label(new Rect(5, 65, BackgroundWidth-44, 20), "<b>Hashes minadas:</b> "+GetPrefix(l_MinedHashes), new GUIStyle("label") { richText = true });
			GUI.Label(new Rect(5, 85, BackgroundWidth-44, 20), "<b>Hashes/s:</b> "+GetPrefix(HashRate + AccHashRate) +"/s", new GUIStyle("label") { richText = true });
			GUI.Label(new Rect(5, 105, BackgroundWidth-44, 20), "<b>Hash minadas a clicks:</b> "+(clickedHashes)+" Hashes", new GUIStyle("label") { richText = true });
			GUI.Label(new Rect(5, 125, BackgroundWidth-44, 20), "<b>BTCs disponibles:</b> "+GetPrefix(Bitcoins, UnitType.Inter, "{0} {1}BTCs"), new GUIStyle("label") { richText = true });
			GUI.Label(new Rect(5, 145, BackgroundWidth-44, 20), "<b>BTCs totales:</b> "+GetPrefix(allBTCs, UnitType.Inter, "{0} {1}BTCs"), new GUIStyle("label") { richText = true });
			GUI.Label(new Rect(5, 165, BackgroundWidth-44, 20), "<b>BTCs gastadas:</b> "+(allBTCs-Bitcoins)+" BTCs", new GUIStyle("label") { richText = true });
			GUI.Label(new Rect(5, 185, BackgroundWidth-44, 20), "<b>BTCs/s:</b> "+GetPrefix(BTCRate, UnitType.Inter, "{0} {1}BTCs/s"), new GUIStyle("label") { richText = true });
			GUI.Label(new Rect(5, 205, BackgroundWidth-44, 20), "<b>BTCs/Hash (prob):</b> "+(l_MinedHashes > 0 ? (((float)BigInteger.ToUInt64(allBTCs)/(float)BigInteger.ToUInt64(l_MinedHashes))*100).ToString("F5")+" %" : "0 %"), new GUIStyle("label") { richText = true });
			GUI.Label(new Rect(5, 245, BackgroundWidth-44, 40), "<b>Tiempo jugado desde esta sesión:</b> "+GetTimeFormat(Time.timeSinceLevelLoad), new GUIStyle("label") { richText = true, wordWrap = screenPerc < 1 }); //GetPrefix(Bitcoins/MinedHashes, UnitType.Inter, "{0} {1}BTCs/Hash")
		}
		else if(curMenu == Menu.Settings) 
		{
			GUI.Label(new Rect(5, 5, BackgroundWidth-44, 25), "Ajustes", new GUIStyle("label") { alignment = TextAnchor.MiddleCenter, fontSize = 24, fontStyle = FontStyle.Bold });
			GUI.Label(new Rect(5, 30, BackgroundWidth-44, 25), "General", new GUIStyle("label") { fontSize = 20 });
			GUI.DrawTexture(new Rect(5, 55, BackgroundWidth-44, 2), sep);
            GUI.Label(new Rect(5, 60, BackgroundWidth-44, 20), "Nivel de calidad ["+QualitySettings.names[s_quality]+"]:");
            s_quality = (int)GUI.HorizontalSlider(sr_quality, s_quality, 0, QualitySettings.names.Length-1);
            s_runInBack = GUI.Toggle(sr_runInBack, s_runInBack, " Seguir ejecutando en segundo plano");
            if(GUI.Button(new Rect((BackgroundWidth-44)/2-75, 125, 150, 45), "Guardar cambios"))
            {
                QualitySettings.SetQualityLevel(s_quality, false);
                Application.runInBackground = s_runInBack;
            }
		}
		GUI.EndGroup();
        GUI.EndGroup();

        //Bloque central (separador)
        GUI.BeginGroup(new Rect(LeftBlockWidth + separator1.width - 1, 0, CentralBlockWidth + 2, Screen.height));
        GUI.DrawTexture(new Rect(0, 100, separator2.width + 2, separator2.height), separator2);
        GUI.EndGroup();

        //Bloque de la izquierda
        GUI.BeginGroup(new Rect(Screen.width - RightBlockWidth, 0, RightBlockWidth, Screen.height));
        GUI.DrawTexture(new Rect(0, 0, BackgroundWidth, Screen.height), background);
        GUI.Label(new Rect(0, 0, RightBlockWidth, 20), "Tienda", new GUIStyle("label") { alignment = TextAnchor.UpperCenter });
        GUI.Box(new Rect(0, 20, RightBlockWidth, 30), "Work in progress...", new GUIStyle("box") { alignment = TextAnchor.MiddleCenter });
        Transistors.Draw(new Rect(0, 50+separator3.height, RightBlockWidth-10, itemTransHeight), unblockedTrans);
        GUI.EndGroup();

        //Bloque de la izquierda (separador)
        GUI.BeginGroup(new Rect(Screen.width - RightBlockWidth - 1, 0, RightBlockWidth + 1, Screen.height));
        GUI.DrawTexture(new Rect(0, 50, separator3.width + 1, separator3.height), separator3);
        GUI.EndGroup();

        if (h_Elems != null && h_Elems.Count > 0)
            foreach (HoveringElement h in h_Elems)
                if (h.position.Contains(fixedMousePos) && curMenu == h.condition)
                    GUI.Box(new Rect(h.box.xMin+fixedMousePos.x, h.box.yMin+fixedMousePos.y, h.box.width, h.box.height), h.caption, new GUIStyle("box") { wordWrap = true });

    }

}