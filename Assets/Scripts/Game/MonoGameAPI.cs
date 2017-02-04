using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TimeSpan = System.TimeSpan;
using Exception = System.Exception;
using StackTrace = System.Diagnostics.StackTrace;
using ScottGarland;

public enum UnitType { Space, Metric, Small, Inter }

public partial class MonoGame
{

    /*public static void Buy(MiningMachine machine) 
    {
        bool buy = false;
        switch (machine.currCost)
        {
            case Currency.Hashes:
                if (me.MinedHashes > machine.cost)
                {
                    me.MinedHashes -= machine.cost + machine.cost / 2 * machine.qnt;
                    buy = true;
                }
                break;
            
            case Currency.BTC:
                if (me.Bitcoins > machine.cost)
                {
                    me.Bitcoins -= machine.cost + machine.cost / 2 * machine.qnt;
                    buy = true;
                }
                break;
            
            case Currency.Dollars:
                if (me.Dollars > machine.cost)
                {
                    me.Dollars -= machine.cost + machine.cost / 2 * machine.qnt;
                    buy = true;
                }
                break;
        }
        if (buy)
        {
            me.HashRate += machine.hashRate+machine.hashRate/3*machine.qnt;
            machine.qnt++;
        }
    }*/

    public static void Buy(Transistors t)
    {
        if (me.Bitcoins >= t.unitCost)
        {
            //int index = me.transGroups.IndexOf(t);
            me.Bitcoins -= t.unitCost;
            //int u = t.unitsAdquired;
            ++t.unitsAdquired;
            //Debug.Log("CurIndex: "+curIndex+"; Count: "+me.transGroups.Count);
            t.unitCost = FloatToBInt(t.baseCost, Mathf.Pow(1.15f, t.unitsAdquired));
            if(!t.unlocked)
            {
                if(me.transGroups.IndexOf(t) < me.transGroups.Count-2)
                    me.unblockedTrans.Add(me.transGroups[me.transGroups.IndexOf(t)+1]);
                t.unlocked = true;
                //++curIndex;
            }
            //t.updateConst();
            //Debug.Log(t.myConst);
        }
    }

    public static int getNext(int u, int reducer)
    {
        return Mathf.RoundToInt(1 + u * Mathf.Pow(u + 1, (1 + u / Mathf.Pow(10, u.ToString().Length + 3))) / reducer);
    }

    //Lo voy a dejar como upgrade
    /*public static void BuyMouse()
    {

    }*/

    public static string GetPrefix(BigInteger num, UnitType t = UnitType.Space, string format = "{0} {1}H", int decimals = 2)
    {
        string[] ordinals = null;

        switch (t)
        {
            case UnitType.Metric:
                ordinals = new[] { "", "k", "M", "B", "T", "q", "Q", "s", "S", "O", "N", "D" };
                break;
            case UnitType.Space:
                ordinals = new[] { "", "K", "M", "G", "T", "P", "E", "Z", "Y" };
                break;
            case UnitType.Small:
                ordinals = new[] { "p", "n", "µ", "m" };
                break;
            case UnitType.Inter:
                ordinals = new[] { "µ", "m", "", "k", "M" };
                break;
        }

        int ordinal = 0;

        BigInteger or = num;

        while (num >= 1000)
        {
            num /= 1000;
            ++ordinal;
        }

        if (ordinal >= ordinals.Length)
            ordinal = ordinals.Length - 1;

        string stror = or.ToString(), strnum = num.ToString();

        int d = 1;
        string c = "0";

        if(ordinal > 0)
        {
            d = stror.Length - strnum.Length;
            c = stror.Substring(strnum.Length, ((d < decimals) ? d : decimals));
        }
        
        return System.String.Format(format,
            strnum + ((d > 0 && int.Parse(c) == 0) ? "" : "," + c),
           ordinals[ordinal]);
    }

	public string GetTimeFormat(float secs) 
	{
		TimeSpan t = TimeSpan.FromSeconds((double)secs);
        return string.Format((t.Hours > 0 ? "{0:D" + (t.Hours < 10 ? "1" : "2") + "} horas " : "") + (t.Minutes > 0 ? "{1:D" + (t.Minutes < 10 ? "1" : "2") + "} minutos " : "")+(t.Seconds > 0 ? "{2:D" + (t.Seconds < 10 ? "1" : "2") + "} segundos" : ""), //"{2:D"+(t.Seconds < 10 ? "1" : "2")+"} segundos", 
			t.Hours, 
			t.Minutes, 
			t.Seconds);
	}

    public BigInteger GetDifficulty(ulong time)
    {
        return time * HashRate / uint.MaxValue;
    }

    public static BigInteger FloatToBInt(BigInteger b, float f)
    {
        return (b * new BigInteger(Mathf.RoundToInt(f * floatMaxDecs))) / floatMaxDecs;
    }

    public static BigInteger BIntLerp(BigInteger v1, BigInteger v2, float t)
    {
        return v1 + FloatToBInt(v2, t) - FloatToBInt(v1, t);
    }

}

public class TextureFade
{
    public Rect pos;
    public Texture2D tex;
    public float time, alpha = 1;
}

public class ClickInfo
{
    public float cTime;
}

public class Transistors
{

    const int constant = 1000;

    public static float rightScroll = 0;

    public int myConst = constant, curLvl = 0, unitsAdquired = 0, mmSize = 100;
    public BigInteger baseCost, unitCost = 0, minedHashes = 0, l_minedHashes = 0;
    public float lAccum;
    public bool unlocked;
    //unitCost: El precio del grupo viene dado en µBTCs
    //mmSize: El valor minimo de esta variable es 100 puesto que en 1 mm caben 100 transistores de 10 µm, y el valor máx: 20,000

    public Transistors(BigInteger c)
    {
        unitCost = c;
        baseCost = c;
    }

    public string getName()
    {
        return string.Format("Transistor de {0} [(mm²) {1} x {2} = {3} u. a {4}/s]",
           getSize(),
           MonoGame.GetPrefix(new BigInteger(mmSize) * new BigInteger(mmSize), UnitType.Metric, "{0}{1}"),
           unitsAdquired,
           MonoGame.GetPrefix(getTotal(), UnitType.Metric, "{0}{1}"),
           MonoGame.GetPrefix(getUHRate()));
    }

    public void updateConst()
    {
        if (curLvl > 0)
        {
            myConst /= curLvl * 2;
            if (curLvl > 5)
            {
                myConst /= curLvl * 5;
                if (curLvl > 9 && curLvl <= 12)
                    myConst /= curLvl * 10;
            }
        }
    }

    public BigInteger getHRate()
    {
        return (new BigInteger(unitsAdquired) * ((new BigInteger(mmSize) * new BigInteger(mmSize)) / myConst));
    }

    public BigInteger getUHRate()
    {
        return (new BigInteger(mmSize) * new BigInteger(mmSize)) / myConst;
    }

    public BigInteger getTotal()
    {
        return (new BigInteger(unitsAdquired) * new BigInteger(mmSize) * new BigInteger(mmSize));
    }

    public string getSize()
    {
        int pSize = (int)(1000000000f / mmSize);
        int fSize = pSize;
        int l = pSize.ToString().Length;
        if (l > 5)
        {
            int rounder = (int)(Mathf.Pow(10, l - 1) / 2);
            fSize = (int)(Mathf.Round(pSize / rounder) * rounder);
        }
        return MonoGame.GetPrefix(fSize, UnitType.Small, "{0} {1}m", 1);
    }

    public static void Draw(Rect pos, List<Transistors> transistors)
    {
        if((Screen.height - pos.yMin)/pos.height < transistors.Count)
            rightScroll = GUI.VerticalSlider(new Rect(pos.xMin + pos.width, pos.yMin, 10, Screen.height - pos.yMin), rightScroll, 0, pos.height * transistors.Count - (Screen.height - pos.yMin));
        GUI.BeginGroup(new Rect(pos.xMin, pos.yMin, pos.width+10, pos.height * transistors.Count));
        GUI.BeginGroup(new Rect(0, -rightScroll, pos.width+10, pos.height * transistors.Count));
        for (int i = 0; i < transistors.Count; ++i)
        {
            Rect gRect = new Rect(0, i * pos.height, pos.width + ((Screen.height - pos.yMin) / pos.height < transistors.Count ? 0 : 10), pos.height);
            try
            {
                if (GUI.Button(gRect, string.Format("{0}\n[{1}, coste: {2}, mining rig: {3}/s, h.minadas: {4}]",
                    transistors[i].getName(),
                    transistors[i].unitsAdquired,
                    MonoGame.GetPrefix(transistors[i].unitCost, UnitType.Inter, "{0} {1}BTCs"),
                    MonoGame.GetPrefix(transistors[i].getHRate()),
                    MonoGame.GetPrefix(transistors[i].l_minedHashes)),
                    new GUIStyle("box") { wordWrap = MonoGame.me.screenPerc < 1, fontSize = Mathf.RoundToInt(13 * MonoGame.me.screenPerc + (MonoGame.me.screenPerc < 1 ? .1f : 0)), alignment = TextAnchor.MiddleLeft, padding = new RectOffset(Mathf.RoundToInt(30 * MonoGame.me.screenPerc), 0, 0, 0), normal = new GUIStyleState() { background = GUI.skin.box.normal.background, textColor = ((transistors[i].unitCost > MonoGame.me.Bitcoins) ? Color.gray : Color.white) } }))
                    MonoGame.Buy(transistors[i]);
            }
            catch (Exception ex)
            {
                Debug.LogError("Script generated " + ex.GetType() + "\n" + ex.Message);
            }
        }
        GUI.EndGroup();
        GUI.EndGroup();
    }

}

public class HoveringElement
{
    public Rect position;
    public Rect box;
    public string caption;
    public Menu condition;

    public HoveringElement(Rect p, Rect b, string c, Menu cond)
    {
        position = p;
        box = b;
        caption = c;
        condition = cond;
    }
}

/*public enum Currency { Hashes, BTC, Dollars }
public class MiningMachine
{
    public Currency currCost = Currency.Hashes;
    public int cost, qnt;
    public BigInteger hashRate = new BigInteger(); //Hashes mined per second
    public BigInteger minedHashes = new BigInteger();
    public string name;
}

public class MachineList
{
    private static bool collapsed = true;
    public static void Draw(Rect pos, string caption, List<MiningMachine> machines)
    {
        //Event e = Event.current;
        if (GUI.Button(new Rect(pos.xMin, pos.yMin, pos.width, pos.height), caption+" "+((collapsed) ? "(+)" : "(-)"), new GUIStyle("box") { alignment = TextAnchor.MiddleLeft, padding = new RectOffset(15, 0, 0, 0) }))
            collapsed = !collapsed;
        if(!collapsed)
            for (int i = 0; i < machines.Count; ++i)
            {
                Rect gRect = new Rect(pos.xMin, pos.yMin+(i+1)*25, pos.width, pos.height); //Bytes minados & accumulative hashrate
                if (GUI.Button(gRect, machines[i].name + " [" + machines[i].qnt + ", coste: " + (machines[i].cost + machines[i].cost / 2 * machines[i].qnt) + ", mining rig: " + MonoGame.GetMetricString(machines[i].hashRate + machines[i].hashRate / 3 * machines[i].qnt) + "/s, h. minadas: "+MonoGame.GetMetricString(machines[i].minedHashes)+"]", new GUIStyle("box") { alignment = TextAnchor.MiddleLeft, padding = new RectOffset(30, 0, 0, 0) }))
                    MonoGame.Buy(machines[i]);
            }
    }
}

[System.Serializable]
public class ListWrapper : List<MiningMachine>
{
    public List<MiningMachine> list;
}*/