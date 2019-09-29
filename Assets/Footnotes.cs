using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using KModkit;


public class Footnotes : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;
	public KMColorblindMode colorblindMode;

    private static int _moduleIdCounter = 1;
    private int _moduleId;

    public MeshRenderer IDBox;
    public MeshRenderer colorText;
	
    public Texture[] colors;

	public KMSelectable[] button;

    public MeshRenderer[] buttonText;
	
    public MeshRenderer[] colorThing;


	string[] footnoteNames = new string[9]
	{
		"*", "†", "‡", "§", "¶", "||", "**", "††", "‡‡"
	};

	string[] locationNames = new string[4]
	{
		"top left", "top right", "bottom left", "bottom right"
	};
	
	string[] colorNames = new string[4]
	{
		"red", "yellow", "green", "blue"
	};
	
	string[] letterNames = new string[4]
	{
		"A", "B", "C", "D"
	};
	
	int[] numberNames = new int[4]
	{
		1, 2, 3, 4
	};
	
    string[] alphabet = new string[26]
    { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
      "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};


    int[,] extraValues = new int[,]
    {
        {9, 2, -2, 4, 2, 4 },
        {5, 4, 8, 5, 4, 5 },
        {4, 5, 2, 4, 7, 2 },
        {2, 4, -5, 2, 5, 10 },
        {5, 4, 2, 5, 11, 5 },
        {5, 2, 4, 12, 2, -4 }
    };

/*

If button -A* is -vertically adjacent to* button -B*
If button -A*'s number is -greater than* button -B*'s number
If the -first digit of the bomb's serial number* is -greater than or equal to* the -yellow* button's number
If the button -diagonally adjacent to* button -A* is button -B*

a) vertically adjacent to, horizontally adjacent to, diagonally adjacent to, orthagonally adjacent to, the same as, different from, above, below, left of, right of
b) is greater than, is less than, is equal to, is less than or equal to, is greater than or equal to, is within one of, is within two of, is more than two away from
c) number of port plates, number of ports, number of battery holders, number of batteries, number of indicators, last digit of the bomb's serial number, first digit of the bomb's serial number, second digit of the bomb's serial number

a) is up or down from, is left or right from, is diagonal from, is exactly, is next in reading order from, is previous in reading order from
*/

	string[] introConditions0 = new string[]
	{
		"vertically adjacent to", "horizontally adjacent to", "diagonally adjacent to", "orthagonally adjacent to", "the same as", "different from", "above", "below", "left of", "right of"
		
	};
	string[] introConditions1 = new string[]
	{
		"is greater than", "is less than", "is equal to", "is less than or equal to", "is greater than or equal to", "is within one of", "is within two of", "is more than two away from"
		
	};
	string[] finalInstructions = new string[]
	{
		"is up or down from", "is left or right from", "is diagonal from", "is exactly", "is next in reading order from", "is previous in reading order from"
	};
	string[] numberValues = new string[]
	{
		"number of port plates", "number of ports", "number of battery holders", "number of batteries", "number of indicators", "last digit of the bomb's serial number", "first digit of the bomb's serial number", "second digit of the bomb's serial number"
	};

    bool pressedAllowed = false;

    // TWITCH PLAYS SUPPORT
    //int tpStages; This one is not needed for this module
    // TWITCH PLAYS SUPPORT


    bool isSolved = false;
    bool tpActive = false;
	
	bool colorblindModeEnabled = false;

	int introCondition;
	/*
0 = If button -A* is -vertically adjacent to* button -B*
1 = If button -A*'s number is -greater than* button -B*'s number
2 = If the -first digit of the bomb's serial number* is -greater than or equal to* the -yellow* button's number
	*/
	int introInstruction;
	int trueInstruction;
	int falseInstruction;

	int introButton1;
	int introButton2;
	int trueButton;
	int falseButton;
	
	int introButton1type;
	int introButton2type;
	int trueButtonType;
	int falseButtonType;
	
	bool sumGreater = false;

    int numberValue = -1;
    int curIndex = -1;

	/*
Color type: rygb
Letter type: abcd
Number type: 1234
Position type: tl tr bl br (only used in intro conditions 1 and 2) 

	*/
	
	int solutionButton; //0 = tl, 1 = tr, 2 = bl, 3 = br
	
    int[,] buttonValues = new int[,]
    {
        {-1, -1, -1},
        {-1, -1, -1},
        {-1, -1, -1},
        {-1, -1, -1}
    };
	//button location (0-3 as in solutionButton), button value type (as in xButtonType) = value
	//if the top left button were a yellow D1, buttonValues[0, 0] would be 1, [0, 1] would be 3, and [0, 2] would be 0.
	
	int[] footnoteValues = new int[]
	{
		-1, -1, -1, -1
	};

    int[,,] footnoteButtonPos = new int[,,]
    { //"*", "†", "‡", "§", "¶", "||", "**", "††", "‡‡"
        { // * (single asterisk)
			{3, 0, 2, 1},
			{0, 1, 3, 2},
			{0, 1, 2, 3},
			{1, 2, 0, 3}
		},
        { // † (dagger)
			{3, 2, 0, 1},
			{3, 1, 0, 2},
			{2, 1, 0, 3},
			{2, 3, 0, 1}
		},
        { // ‡ (double dagger)
			{0, 3, 1, 2},
			{1, 0, 3, 2},
			{3, 1, 2, 0},
			{1, 0, 3, 2}
		},
        { // § (section symbol)
			{1, 3, 2, 0},
			{2, 0, 1, 3},
			{0, 2, 3, 1},
			{0, 3, 2, 1}
		},
        { // ¶ (paragraph)
			{0, 1, 3, 2},
			{0, 2, 3, 1},
			{1, 3, 0, 2},
			{1, 2, 3, 0}
		},
        { // || (parallel lines)
			{3, 0, 1, 2},
			{1, 2, 0, 3},
			{3, 1, 2, 0},
			{1, 3, 0, 2}
		},
        { // ** (double asterisk)
			{2, 1, 3, 0},
			{3, 1, 0, 2},
			{3, 1, 2, 0},
			{0, 2, 3, 1}
		},
        { // †† (two daggers)
			{1, 2, 0, 3},
			{0, 2, 3, 1},
			{3, 1, 2, 0},
			{2, 3, 1, 0}
		},
        { // ‡‡ (two double daggers)
			{2, 0, 3, 1},
			{1, 2, 3, 0},
			{3, 0, 2, 1},
			{0, 3, 2, 1}
		},
    };
	//footnote number, list number, position number = value
	//list order is Color (4), Letter (4), Number (4), Position (4)
	//Direction 1 (10), Inequality (8), Edgework (8), Direction 2 (6)
	
	int[,] footnoteDirection1 = new int[,]
	{
		{5, 6, 9, 8, 7, 2, 3, 0, 1, 4},
		{9, 0, 5, 8, 1, 7, 6, 2, 4, 3},
		{3, 8, 4, 1, 6, 7, 0, 9, 2, 5},
		{4, 3, 2, 8, 1, 9, 0, 5, 6, 7},
		{2, 4, 6, 9, 8, 5, 7, 3, 0, 1},
		{4, 2, 3, 0, 7, 9, 5, 8, 6, 1},
		{1, 2, 6, 8, 9, 7, 0, 5, 4, 3},
		{7, 1, 9, 5, 0, 2, 4, 3, 6, 8},
		{3, 8, 1, 5, 6, 4, 2, 0, 7, 9}
	};
	
	int[,] footnoteInequality = new int[,]
	{
			{1, 0, 2, 6, 3, 4, 5, 7},
			{6, 4, 0, 2, 3, 5, 7, 1},
			{0, 4, 3, 1, 7, 2, 5, 6},
			{3, 2, 5, 0, 6, 1, 7, 4},
			{2, 3, 4, 0, 6, 7, 5, 1},
			{2, 0, 1, 6, 3, 4, 5, 7},
			{1, 6, 0, 4, 7, 5, 2, 3},
			{3, 0, 7, 4, 2, 5, 6, 1},
			{7, 0, 1, 3, 2, 4, 5, 6}
	};
	
	int[,] footnoteEdgework = new int[,]
	{
			{0, 1, 4, 2, 5, 3, 7, 6},
			{7, 1, 2, 5, 3, 6, 0, 4},
			{3, 2, 5, 4, 0, 6, 7, 1},
			{5, 4, 3, 1, 6, 7, 2, 0},
			{4, 0, 2, 1, 3, 7, 5, 6},
			{3, 2, 5, 6, 4, 1, 0, 7},
			{0, 3, 7, 4, 6, 1, 5, 2},
			{4, 0, 7, 3, 6, 5, 2, 1},
			{0, 7, 4, 5, 2, 3, 1, 6}
	};
	
	
	int[,] footnoteDirection2 = new int[,]
	{
			{0, 1, 4, 3, 2, 5},
			{2, 5, 1, 4, 3, 0},
			{5, 3, 2, 0, 4, 1},
			{4, 2, 5, 3, 0, 1},
			{1, 0, 3, 4, 5, 2},
			{0, 5, 2, 3, 1, 4},
			{4, 1, 3, 2, 0, 5},
			{3, 1, 4, 0, 5, 2},
			{3, 4, 0, 2, 5, 1}
	};	
	
	int introFootnoteOmitted;
	bool trueFootnoteFirst;
	bool falseFootnoteFirst;
	
	string realInstruction;
	string fakeInstruction;
	string displayedInstruction;	
	string realInstructionT;
	string fakeInstructionT;
	string displayedInstructionT;

    int possibleTrue;
    int possibleFalse;

    int correctButton;
	
	bool isIntroTrue;
    int latestPosition = -1;
    void Start()
    {
        _moduleId = _moduleIdCounter++;
        colorblindModeEnabled = colorblindMode.ColorblindModeActive;
        for (int snPos = 0; snPos < 6; snPos++)
        {
            
            if (!(Bomb.GetSerialNumber().Substring(snPos, 1).TryParseInt() >= 0 && Bomb.GetSerialNumber().Substring(snPos, 1).TryParseInt() <= 9))
            {
                if (Array.IndexOf(alphabet, Bomb.GetSerialNumber().Substring(snPos, 1)) + 1 > latestPosition)
                {
                    latestPosition = Array.IndexOf(alphabet, Bomb.GetSerialNumber().Substring(snPos, 1)) + 1;
                }
            }
        }
        delegationZone();
        Init();
        pressedAllowed = true;
    }

    void Init()
    {
		if (Bomb.GetSerialNumberNumbers().Sum() > latestPosition)
		{
			sumGreater = true;
		}
		introFootnoteOmitted = UnityEngine.Random.Range(0, 3);
		trueFootnoteFirst = true;
		falseFootnoteFirst = true;
		if (UnityEngine.Random.Range(0, 2) == 0)
		{
			trueFootnoteFirst = false;
		}
		if (UnityEngine.Random.Range(0, 2) == 0)
		{
			falseFootnoteFirst = false;
		}
		buttonValues[0, 0] = UnityEngine.Random.Range(0, 4);
		buttonValues[0, 1] = UnityEngine.Random.Range(0, 4);
		buttonValues[0, 2] = UnityEngine.Random.Range(0, 4);
		for (int bN = 1; bN < 4; bN++)
		{
			for (int vN = 0; vN < 3; vN++)
			{
				buttonValues[bN, vN] = buttonValues[0, vN];
				var badStuff = true;
				while (badStuff)
				{
					badStuff = false;
					buttonValues[bN, vN] = UnityEngine.Random.Range(0, 4);
					for (int xyz = 0; xyz < bN; xyz++)
					{
						if (buttonValues[bN, vN] == buttonValues[xyz, vN])
						{
							badStuff = true;
							xyz = bN;
						}
					}
				}
				
			}
		}
        
        footnoteValues[0] = UnityEngine.Random.Range(0, 9);
        for (int fN = 1; fN < 4; fN++)
		{
			footnoteValues[fN] = footnoteValues[0];
			var badStuff = true;
			while (badStuff)
			{
				badStuff = false;
				footnoteValues[fN] = UnityEngine.Random.Range(0, 9);
				for (int xyz = 0; xyz < fN; xyz++)
				{
					if (footnoteValues[fN] == footnoteValues[xyz])
					{
						badStuff = true;
						xyz = fN;
					}
				}
			}
            
        }
		/* currentAccessory = UnityEngine.Random.Range(0, 6); */
		for (int bN = 0; bN < 4; bN++)
		{		
			Debug.LogFormat("[Footnotes #{0}] The {1} button is a {2} {3}.", _moduleId, locationNames[bN], 
				colorNames[buttonValues[bN, 0]], (letterNames[buttonValues[bN, 1]] + numberNames[buttonValues[bN, 2]]));
			colorThing[bN].material.mainTexture = colors[buttonValues[bN, 0]];
			buttonText[bN].GetComponentInChildren<TextMesh>().text = (letterNames[buttonValues[bN, 1]] + numberNames[buttonValues[bN, 2]]);
		}
        
		introCondition = UnityEngine.Random.Range(0, 3);

        trueInstruction = UnityEngine.Random.Range(0, 6);
		falseInstruction = (UnityEngine.Random.Range(1, 6) + trueInstruction) % 6;


        trueButton = UnityEngine.Random.Range(0, 4);
		falseButton = (UnityEngine.Random.Range(1, 4) + trueButton) % 4;

        trueButtonType = UnityEngine.Random.Range(0, 4);
		falseButtonType = (UnityEngine.Random.Range(1, 4) + trueButtonType) % 4;


        introButton1 = UnityEngine.Random.Range(0, 4);
        introButton2 = (UnityEngine.Random.Range(1, 4) + introButton1) % 4;
        realInstruction = "";
        fakeInstruction = "";
        displayedInstruction = "";
        //introCondition = 1;
		if (introCondition == 0)
		{
            introInstruction = UnityEngine.Random.Range(0, 10);
            introButton1type = UnityEngine.Random.Range(0, 3);
            introButton2type = (UnityEngine.Random.Range(1, 3) + introButton1type) % 3;
            //If button -A* is -vertically adjacent to* button -B*
            realInstruction = "If the ";
            fakeInstruction = "If the ";
            displayedInstruction = "If the ";
            switch (introButton1type)
            {
                case 0:
                    realInstruction = realInstruction + colorNames[buttonValues[introButton1, 0]];
                    if (introFootnoteOmitted != 0)
                    {
                        curIndex = 0;
                        while (Array.IndexOf(colorNames, colorNames[buttonValues[introButton1, 0]]) != footnoteButtonPos[footnoteValues[0], 0, curIndex])
                        {
                            curIndex++;
                        }
                        if (sumGreater)
                        {
                            fakeInstruction = fakeInstruction + colorNames[footnoteButtonPos[footnoteValues[0], 0, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[0]];
                            displayedInstruction = displayedInstruction + colorNames[footnoteButtonPos[footnoteValues[0], 0, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[0]];
                        }
                        else
                        {
                            fakeInstruction = fakeInstruction + colorNames[footnoteButtonPos[footnoteValues[0], 0, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[0]];
                            displayedInstruction = displayedInstruction + colorNames[footnoteButtonPos[footnoteValues[0], 0, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[0]];
                        }
                    }
                    else
                    {
                        fakeInstruction = fakeInstruction + colorNames[buttonValues[introButton1, 0]];
                        displayedInstruction = displayedInstruction + colorNames[buttonValues[introButton1, 0]];
                    }
                    break;

                case 1:
                    realInstruction = realInstruction + letterNames[buttonValues[introButton1, 1]];
                    if (introFootnoteOmitted != 0)
                    {
                        curIndex = 0;
                        while (Array.IndexOf(letterNames, letterNames[buttonValues[introButton1, 1]]) != footnoteButtonPos[footnoteValues[0], 1, curIndex])
                        {
                            curIndex++;
                        }
                        if (sumGreater)
                        {
                            fakeInstruction = fakeInstruction + letterNames[footnoteButtonPos[footnoteValues[0], 1, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[0]];
                            displayedInstruction = displayedInstruction + letterNames[footnoteButtonPos[footnoteValues[0], 1, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[0]];
                        }
                        else
                        {
                            fakeInstruction = fakeInstruction + letterNames[footnoteButtonPos[footnoteValues[0], 1, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[0]];
                            displayedInstruction = displayedInstruction + letterNames[footnoteButtonPos[footnoteValues[0], 1, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[0]];
                        }
                    }
                    else
                    {
                        fakeInstruction = fakeInstruction + letterNames[buttonValues[introButton1, 1]];
                        displayedInstruction = displayedInstruction + letterNames[buttonValues[introButton1, 1]];
                    }
                    break;

                default:
                    realInstruction = realInstruction + "" + numberNames[buttonValues[introButton1, 2]];
                    if (introFootnoteOmitted != 0)
                    {
                        curIndex = 0;
                        while (Array.IndexOf(numberNames, numberNames[buttonValues[introButton1, 2]]) != footnoteButtonPos[footnoteValues[0], 2, curIndex])
                        {
                            curIndex++;
                        }
                        if (sumGreater)
                        {
                            fakeInstruction = fakeInstruction + numberNames[footnoteButtonPos[footnoteValues[0], 2, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[0]];
                            displayedInstruction = displayedInstruction + numberNames[footnoteButtonPos[footnoteValues[0], 2, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[0]];
                        }
                        else
                        {
                            fakeInstruction = fakeInstruction + numberNames[footnoteButtonPos[footnoteValues[0], 2, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[0]];
                            displayedInstruction = displayedInstruction + numberNames[footnoteButtonPos[footnoteValues[0], 2, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[0]];
                        }
                    }
                    else
                    {
                        fakeInstruction = fakeInstruction + numberNames[buttonValues[introButton1, 2]];
                        displayedInstruction = displayedInstruction + numberNames[buttonValues[introButton1, 2]];
                    }
                    break;

            }
            realInstruction = realInstruction + " button is " + introConditions0[introInstruction] + " the ";
            if (introFootnoteOmitted == 0)
            {
                curIndex = 0;
                while (Array.IndexOf(introConditions0, introConditions0[introInstruction]) != footnoteDirection1[footnoteValues[0], curIndex])
                {
                    curIndex++;
                }
                if (sumGreater)
                {
                    fakeInstruction = fakeInstruction + " button is " + introConditions0[footnoteDirection1[footnoteValues[0], (curIndex + 9) % 10]] + footnoteNames[footnoteValues[0]] + " the ";
                    displayedInstruction = displayedInstruction + " button\nis " + introConditions0[footnoteDirection1[footnoteValues[0], (curIndex + 9) % 10]] + footnoteNames[footnoteValues[0]] + "the\n";
                }
                else
                {
                    fakeInstruction = fakeInstruction + " button is " + introConditions0[footnoteDirection1[footnoteValues[0], (curIndex + 1) % 10]] + footnoteNames[footnoteValues[0]] + " the ";
                    displayedInstruction = displayedInstruction + " button\nis " + introConditions0[footnoteDirection1[footnoteValues[0], (curIndex + 1) % 10]] + footnoteNames[footnoteValues[0]] + "the\n";
                }
            }
            else if (introFootnoteOmitted == 1)
            {
                fakeInstruction = fakeInstruction + " button is " + introConditions0[introInstruction] + " the ";
                displayedInstruction = displayedInstruction + " button is\n" + introConditions0[introInstruction] + "the\n";
            }
            else
            {
                curIndex = 0;
                while (Array.IndexOf(introConditions0, introConditions0[introInstruction]) != footnoteDirection1[footnoteValues[1], curIndex])
                {
                    curIndex++;
                }
                if (sumGreater)
                {
                    fakeInstruction = fakeInstruction + " button is " + introConditions0[footnoteDirection1[footnoteValues[1], (curIndex + 9) % 10]] + footnoteNames[footnoteValues[1]] + " the ";
                    displayedInstruction = displayedInstruction + " button\nis " + introConditions0[footnoteDirection1[footnoteValues[1], (curIndex + 9) % 10]] + footnoteNames[footnoteValues[1]] + "the\n";
                }
                else
                {
                    fakeInstruction = fakeInstruction + " button is " + introConditions0[footnoteDirection1[footnoteValues[1], (curIndex + 1) % 10]] + footnoteNames[footnoteValues[1]] + " the ";
                    displayedInstruction = displayedInstruction + " button\nis " + introConditions0[footnoteDirection1[footnoteValues[1], (curIndex + 1) % 10]] + footnoteNames[footnoteValues[1]] + "the\n";
                }
            }
            

            switch (introButton2type)
            {
                case 0:
                    realInstruction = realInstruction + colorNames[buttonValues[introButton2, 0]];
                    if (introFootnoteOmitted != 2)
                    {
                        curIndex = 0;
                        while (Array.IndexOf(colorNames, colorNames[buttonValues[introButton2, 0]]) != footnoteButtonPos[footnoteValues[1], 0, curIndex])
                        {
                            curIndex++;
                        }
                        if (sumGreater)
                        {
                            fakeInstruction = fakeInstruction + colorNames[footnoteButtonPos[footnoteValues[1], 0, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[1]];
                            displayedInstruction = displayedInstruction + colorNames[footnoteButtonPos[footnoteValues[1], 0, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[1]];
                        }
                        else
                        {
                            fakeInstruction = fakeInstruction + colorNames[footnoteButtonPos[footnoteValues[1], 0, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[1]];
                            displayedInstruction = displayedInstruction + colorNames[footnoteButtonPos[footnoteValues[1], 0, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[1]];
                        }
                    }
                    else
                    {
                        fakeInstruction = fakeInstruction + colorNames[buttonValues[introButton2, 0]];
                        displayedInstruction = displayedInstruction + colorNames[buttonValues[introButton2, 0]];
                    }
                    break;
                case 1:
                    realInstruction = realInstruction + letterNames[buttonValues[introButton2, 1]];
                    if (introFootnoteOmitted != 2)
                    {
                        curIndex = 0;
                        while (Array.IndexOf(letterNames, letterNames[buttonValues[introButton2, 1]]) != footnoteButtonPos[footnoteValues[1], 1, curIndex])
                        {
                            curIndex++;
                        }
                        if (sumGreater)
                        {
                            fakeInstruction = fakeInstruction + letterNames[footnoteButtonPos[footnoteValues[1], 1, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[1]];
                            displayedInstruction = displayedInstruction + letterNames[footnoteButtonPos[footnoteValues[1], 1, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[1]];
                        }
                        else
                        {
                            fakeInstruction = fakeInstruction + letterNames[footnoteButtonPos[footnoteValues[1], 1, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[1]];
                            displayedInstruction = displayedInstruction + letterNames[footnoteButtonPos[footnoteValues[1], 1, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[1]];
                        }
                    }
                    else
                    {
                        fakeInstruction = fakeInstruction + letterNames[buttonValues[introButton2, 1]];
                        displayedInstruction = displayedInstruction + letterNames[buttonValues[introButton2, 1]];
                    }
                    break;
                default:
                    realInstruction = realInstruction + "" + numberNames[buttonValues[introButton2, 2]];
                    if (introFootnoteOmitted != 2)
                    {
                        curIndex = 0;
                        while (Array.IndexOf(numberNames, numberNames[buttonValues[introButton2, 2]]) != footnoteButtonPos[footnoteValues[1], 2, curIndex])
                        {
                            curIndex++;
                        }
                        if (sumGreater)
                        {
                            fakeInstruction = fakeInstruction + numberNames[footnoteButtonPos[footnoteValues[1], 2, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[1]];
                            displayedInstruction = displayedInstruction + numberNames[footnoteButtonPos[footnoteValues[1], 2, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[1]];
                        }
                        else
                        {
                            fakeInstruction = fakeInstruction + numberNames[footnoteButtonPos[footnoteValues[1], 2, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[1]];
                            displayedInstruction = displayedInstruction + numberNames[footnoteButtonPos[footnoteValues[1], 2, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[1]];
                        }
                    }
                    else
                    {
                        fakeInstruction = fakeInstruction + numberNames[buttonValues[introButton2, 2]];
                        displayedInstruction = displayedInstruction + numberNames[buttonValues[introButton2, 2]];
                    }
                    break;
            }

        }
		else if (introCondition == 1)
		{
            introInstruction = UnityEngine.Random.Range(0, 8);
            introButton1type = UnityEngine.Random.Range(0, 3);
            if (introButton1type == 2)
            {
                introButton1type = 3;
            }
            introButton2type = introButton1type;
            while (introButton2type == introButton1type || introButton2type == 2)
            {
                introButton2type = UnityEngine.Random.Range(0, 4);
            }
            
            //If button -A* is -vertically adjacent to* button -B*
            realInstruction = "If the ";
            fakeInstruction = "If the ";
            displayedInstruction = "If the ";
            switch (introButton1type)
            {
                case 0:
                    realInstruction = realInstruction + colorNames[buttonValues[introButton1, 0]];
                    if (introFootnoteOmitted != 0)
                    {
                        curIndex = 0;
                        while (Array.IndexOf(colorNames, colorNames[buttonValues[introButton1, 0]]) != footnoteButtonPos[footnoteValues[0], 0, curIndex])
                        {
                            curIndex++;
                        }
                        if (sumGreater)
                        {
                            fakeInstruction = fakeInstruction + colorNames[footnoteButtonPos[footnoteValues[0], 0, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[0]];
                            displayedInstruction = displayedInstruction + colorNames[footnoteButtonPos[footnoteValues[0], 0, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[0]];
                        }
                        else
                        {
                            fakeInstruction = fakeInstruction + colorNames[footnoteButtonPos[footnoteValues[0], 0, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[0]];
                            displayedInstruction = displayedInstruction + colorNames[footnoteButtonPos[footnoteValues[0], 0, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[0]];
                        }
                    }
                    else
                    {
                        fakeInstruction = fakeInstruction + colorNames[buttonValues[introButton1, 0]];
                        displayedInstruction = displayedInstruction + colorNames[buttonValues[introButton1, 0]];
                    }
                    break;

                case 1:
                    realInstruction = realInstruction + letterNames[buttonValues[introButton1, 1]];
                    if (introFootnoteOmitted != 0)
                    {
                        curIndex = 0;
                        while (Array.IndexOf(letterNames, letterNames[buttonValues[introButton1, 1]]) != footnoteButtonPos[footnoteValues[0], 1, curIndex])
                        {
                            curIndex++;
                        }
                        if (sumGreater)
                        {
                            fakeInstruction = fakeInstruction + letterNames[footnoteButtonPos[footnoteValues[0], 1, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[0]];
                            displayedInstruction = displayedInstruction + letterNames[footnoteButtonPos[footnoteValues[0], 1, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[0]];
                        }
                        else
                        {
                            fakeInstruction = fakeInstruction + letterNames[footnoteButtonPos[footnoteValues[0], 1, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[0]];
                            displayedInstruction = displayedInstruction + letterNames[footnoteButtonPos[footnoteValues[0], 1, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[0]];
                        }
                    }
                    else
                    {
                        fakeInstruction = fakeInstruction + letterNames[buttonValues[introButton1, 1]];
                        displayedInstruction = displayedInstruction + letterNames[buttonValues[introButton1, 1]];
                    }
                    break;

                default:
                    realInstruction = realInstruction + "" + locationNames[introButton1];
                    if (introFootnoteOmitted != 0)
                    {
                        curIndex = 0;
                        while (Array.IndexOf(locationNames, locationNames[introButton1]) != footnoteButtonPos[footnoteValues[0], 3, curIndex])
                        {
                            curIndex++;
                        }
                        if (sumGreater)
                        {
                            fakeInstruction = fakeInstruction + locationNames[footnoteButtonPos[footnoteValues[0], 3, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[0]];
                            displayedInstruction = displayedInstruction + locationNames[footnoteButtonPos[footnoteValues[0], 3, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[0]];
                        }
                        else
                        {
                            fakeInstruction = fakeInstruction + locationNames[footnoteButtonPos[footnoteValues[0], 3, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[0]];
                            displayedInstruction = displayedInstruction + locationNames[footnoteButtonPos[footnoteValues[0], 3, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[0]];
                        }
                    }
                    else
                    {
                        fakeInstruction = fakeInstruction + locationNames[introButton1];
                        displayedInstruction = displayedInstruction + locationNames[introButton1];
                    }
                    break;

            }
            realInstruction = realInstruction + " button's number " + introConditions1[introInstruction] + " the ";
            if (introFootnoteOmitted == 0)
            {/*
                if (sumGreater)
                {
                    fakeInstruction = fakeInstruction + " button " + introConditions0[(introInstruction + 9) % 10] + " the ";
                    displayedInstruction = displayedInstruction + " button " + introConditions0[(introInstruction + 9) % 10] + " the ";
                }
                else
                {
                    fakeInstruction = fakeInstruction + " button " + introConditions0[(introInstruction + 1) % 10] + " the ";
                    displayedInstruction = displayedInstruction + " button " + introConditions0[(introInstruction + 1) % 10] + " the ";
                } */
                curIndex = 0;
                while (Array.IndexOf(introConditions1, introConditions1[introInstruction]) != footnoteInequality[footnoteValues[0], curIndex])
                {
                    curIndex++;
                }
                if (sumGreater)
                {
                    fakeInstruction = fakeInstruction + " button's number " + introConditions1[footnoteInequality[footnoteValues[0], (curIndex + 7) % 8]] + footnoteNames[footnoteValues[0]] + " the ";
                    displayedInstruction = displayedInstruction + " button's number\n" + introConditions1[footnoteInequality[footnoteValues[0], (curIndex + 7) % 8]] + footnoteNames[footnoteValues[0]] + "the\n";
                }
                else
                {
                    fakeInstruction = fakeInstruction + " button's number " + introConditions1[footnoteInequality[footnoteValues[0], (curIndex + 1) % 8]] + footnoteNames[footnoteValues[0]] + " the ";
                    displayedInstruction = displayedInstruction + " button's number\n" + introConditions1[footnoteInequality[footnoteValues[0], (curIndex + 1) % 8]] + footnoteNames[footnoteValues[0]] + "the\n";
                }
            }
            else if (introFootnoteOmitted == 1)
            {
                fakeInstruction = fakeInstruction + " button's number " + introConditions1[introInstruction] + " the ";
                displayedInstruction = displayedInstruction + " button's number\n" + introConditions1[introInstruction] + "the\n";
            }
            else
            { /*
                if (sumGreater)
                {
                    fakeInstruction = fakeInstruction + " button " + introConditions0[(introInstruction + 9) % 10] + " the ";
                    displayedInstruction = displayedInstruction + " button " + introConditions0[(introInstruction + 9) % 10] + " the ";
                }
                else
                {
                    fakeInstruction = fakeInstruction + " button " + introConditions0[(introInstruction + 1) % 10] + " the ";
                    displayedInstruction = displayedInstruction + " button " + introConditions0[(introInstruction + 1) % 10] + " the ";
                } */
                curIndex = 0;
                while (Array.IndexOf(introConditions1, introConditions1[introInstruction]) != footnoteInequality[footnoteValues[1], curIndex])
                {
                    curIndex++;
                }
                if (sumGreater)
                {
                    fakeInstruction = fakeInstruction + " button's number " + introConditions1[footnoteInequality[footnoteValues[1], (curIndex + 7) % 8]] + footnoteNames[footnoteValues[1]] + " the ";
                    displayedInstruction = displayedInstruction + " button's number\n" + introConditions1[footnoteInequality[footnoteValues[1], (curIndex + 7) % 8]] + footnoteNames[footnoteValues[1]] + "the\n";
                }
                else
                {
                    fakeInstruction = fakeInstruction + " button's number " + introConditions1[footnoteInequality[footnoteValues[1], (curIndex + 1) % 8]] + footnoteNames[footnoteValues[1]] + " the ";
                    displayedInstruction = displayedInstruction + " button's number\n" + introConditions1[footnoteInequality[footnoteValues[1], (curIndex + 1) % 8]] + footnoteNames[footnoteValues[1]] + "the\n";
                }
            }
            

            switch (introButton2type)
            {
                case 0:
                    realInstruction = realInstruction + colorNames[buttonValues[introButton2, 0]];
                    if (introFootnoteOmitted != 2)
                    {
                        curIndex = 0;
                        while (Array.IndexOf(colorNames, colorNames[buttonValues[introButton2, 0]]) != footnoteButtonPos[footnoteValues[1], 0, curIndex])
                        {
                            curIndex++;
                        }
                        if (sumGreater)
                        {
                            fakeInstruction = fakeInstruction + colorNames[footnoteButtonPos[footnoteValues[1], 0, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[1]];
                            displayedInstruction = displayedInstruction + colorNames[footnoteButtonPos[footnoteValues[1], 0, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[1]];
                        }
                        else
                        {
                            fakeInstruction = fakeInstruction + colorNames[footnoteButtonPos[footnoteValues[1], 0, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[1]];
                            displayedInstruction = displayedInstruction + colorNames[footnoteButtonPos[footnoteValues[1], 0, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[1]];
                        }
                    }
                    else
                    {
                        fakeInstruction = fakeInstruction + colorNames[buttonValues[introButton2, 0]];
                        displayedInstruction = displayedInstruction + colorNames[buttonValues[introButton2, 0]];
                    }
                    break;
                case 1:
                    realInstruction = realInstruction + letterNames[buttonValues[introButton2, 1]];
                    if (introFootnoteOmitted != 2)
                    {
                        curIndex = 0;
                        while (Array.IndexOf(letterNames, letterNames[buttonValues[introButton2, 1]]) != footnoteButtonPos[footnoteValues[1], 1, curIndex])
                        {
                            curIndex++;
                        }
                        if (sumGreater)
                        {
                            fakeInstruction = fakeInstruction + letterNames[footnoteButtonPos[footnoteValues[1], 1, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[1]];
                            displayedInstruction = displayedInstruction + letterNames[footnoteButtonPos[footnoteValues[1], 1, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[1]];
                        }
                        else
                        {
                            fakeInstruction = fakeInstruction + letterNames[footnoteButtonPos[footnoteValues[1], 1, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[1]];
                            displayedInstruction = displayedInstruction + letterNames[footnoteButtonPos[footnoteValues[1], 1, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[1]];
                        }
                    }
                    else
                    {
                        fakeInstruction = fakeInstruction + letterNames[buttonValues[introButton2, 1]];
                        displayedInstruction = displayedInstruction + letterNames[buttonValues[introButton2, 1]];
                    }
                    break;
                default:
                    realInstruction = realInstruction + "" + locationNames[introButton2];
                    if (introFootnoteOmitted != 2)
                    {
                        curIndex = 0;
                        while (Array.IndexOf(locationNames, locationNames[introButton2]) != footnoteButtonPos[footnoteValues[1], 3, curIndex])
                        {
                            curIndex++;
                        }
                        if (sumGreater)
                        {
                            fakeInstruction = fakeInstruction + locationNames[footnoteButtonPos[footnoteValues[1], 3, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[1]];
                            displayedInstruction = displayedInstruction + locationNames[footnoteButtonPos[footnoteValues[1], 3, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[1]];
                        }
                        else
                        {
                            fakeInstruction = fakeInstruction + locationNames[footnoteButtonPos[footnoteValues[1], 3, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[1]];
                            displayedInstruction = displayedInstruction + locationNames[footnoteButtonPos[footnoteValues[1], 3, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[1]];
                        }
                    }
                    else
                    {
                        fakeInstruction = fakeInstruction + locationNames[introButton2];
                        displayedInstruction = displayedInstruction + locationNames[introButton2];
                    }
                    break;
            }
        }
        else
        {
            introInstruction = UnityEngine.Random.Range(0, 8);
            introButton1 = UnityEngine.Random.Range(0, 8);
            introButton2type = UnityEngine.Random.Range(0, 3);
            if (introButton2type == 2)
            {
                introButton2type = 3;
            }
            
            //If button -A* is -vertically adjacent to* button -B*
            realInstruction = "If the ";
            fakeInstruction = "If the ";
            displayedInstruction = "If the ";
            realInstruction = realInstruction + numberValues[introButton1];
            if (introFootnoteOmitted != 0)
            {
                curIndex = 0;
                while (Array.IndexOf(numberValues, numberValues[introButton1]) != footnoteEdgework[footnoteValues[0], curIndex])
                {
                    curIndex++;
                }
                if (sumGreater)
                {
                    fakeInstruction = fakeInstruction + numberValues[footnoteEdgework[footnoteValues[0], (curIndex + 7) % 8]] + footnoteNames[footnoteValues[0]];
                    displayedInstruction = displayedInstruction + numberValues[footnoteEdgework[footnoteValues[0], (curIndex + 7) % 8]] + footnoteNames[footnoteValues[0]];
                }
                else
                {
                    fakeInstruction = fakeInstruction + numberValues[footnoteEdgework[footnoteValues[0], (curIndex + 1) % 8]] + footnoteNames[footnoteValues[0]];
                    displayedInstruction = displayedInstruction + numberValues[footnoteEdgework[footnoteValues[0], (curIndex + 1) % 8]] + footnoteNames[footnoteValues[0]];
                }
            }
            else
            {
                fakeInstruction = fakeInstruction + numberValues[introButton1];
                displayedInstruction = displayedInstruction + numberValues[introButton1];
            }
            realInstruction = realInstruction + " " + introConditions1[introInstruction] + " the ";
            if (introFootnoteOmitted == 0)
            {
                curIndex = 0;
                while (Array.IndexOf(introConditions1, introConditions1[introInstruction]) != footnoteInequality[footnoteValues[0], curIndex])
                {
                    curIndex++;
                }
                if (sumGreater)
                {
                    fakeInstruction = fakeInstruction + " " + introConditions1[footnoteInequality[footnoteValues[0], (curIndex + 7) % 8]] + footnoteNames[footnoteValues[0]] + " the ";
                    displayedInstruction = displayedInstruction + "\n" + introConditions1[footnoteInequality[footnoteValues[0], (curIndex + 7) % 8]] + footnoteNames[footnoteValues[0]] + "the\n";
                }
                else
                {
                    fakeInstruction = fakeInstruction + " " + introConditions1[footnoteInequality[footnoteValues[0], (curIndex + 1) % 8]] + footnoteNames[footnoteValues[0]] + " the ";
                    displayedInstruction = displayedInstruction + "\n" + introConditions1[footnoteInequality[footnoteValues[0], (curIndex + 1) % 8]] + footnoteNames[footnoteValues[0]] + "the\n";
                }
            }
            else if (introFootnoteOmitted == 1)
            {
                fakeInstruction = fakeInstruction + " " + introConditions1[introInstruction] + " the ";
                displayedInstruction = displayedInstruction + "\n" + introConditions1[introInstruction] + "the\n";
            }
            else
            {
                curIndex = 0;
                while (Array.IndexOf(introConditions1, introConditions1[introInstruction]) != footnoteInequality[footnoteValues[1], curIndex])
                {
                    curIndex++;
                }
                if (sumGreater)
                {
                    fakeInstruction = fakeInstruction + " " + introConditions1[footnoteInequality[footnoteValues[1], (curIndex + 7) % 8]] + footnoteNames[footnoteValues[1]] + " the ";
                    displayedInstruction = displayedInstruction + "\n" + introConditions1[footnoteInequality[footnoteValues[1], (curIndex + 7) % 8]] + footnoteNames[footnoteValues[1]] + "the\n";
                }
                else
                {
                    fakeInstruction = fakeInstruction + " " + introConditions1[footnoteInequality[footnoteValues[1], (curIndex + 1) % 8]] + footnoteNames[footnoteValues[1]] + " the ";
                    displayedInstruction = displayedInstruction + "\n" + introConditions1[footnoteInequality[footnoteValues[1], (curIndex + 1) % 8]] + footnoteNames[footnoteValues[1]] + "the\n";
                }
            }
            

            switch (introButton2type)
            {
                case 0:
                    realInstruction = realInstruction + colorNames[buttonValues[introButton2, 0]];
                    if (introFootnoteOmitted != 2)
                    {
                        curIndex = 0;
                        while (Array.IndexOf(colorNames, colorNames[buttonValues[introButton2, 0]]) != footnoteButtonPos[footnoteValues[1], 0, curIndex])
                        {
                            curIndex++;
                        }
                        if (sumGreater)
                        {
                            fakeInstruction = fakeInstruction + colorNames[footnoteButtonPos[footnoteValues[1], 0, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[1]];
                            displayedInstruction = displayedInstruction + colorNames[footnoteButtonPos[footnoteValues[1], 0, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[1]];
                        }
                        else
                        {
                            fakeInstruction = fakeInstruction + colorNames[footnoteButtonPos[footnoteValues[1], 0, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[1]];
                            displayedInstruction = displayedInstruction + colorNames[footnoteButtonPos[footnoteValues[1], 0, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[1]];
                        }
                    }
                    else
                    {
                        fakeInstruction = fakeInstruction + colorNames[buttonValues[introButton2, 0]];
                        displayedInstruction = displayedInstruction + colorNames[buttonValues[introButton2, 0]];
                    }
                    break;
                case 1:
                    realInstruction = realInstruction + letterNames[buttonValues[introButton2, 1]];
                    if (introFootnoteOmitted != 2)
                    {
                        curIndex = 0;
                        while (Array.IndexOf(letterNames, letterNames[buttonValues[introButton2, 1]]) != footnoteButtonPos[footnoteValues[1], 1, curIndex])
                        {
                            curIndex++;
                        }
                        if (sumGreater)
                        {
                            fakeInstruction = fakeInstruction + letterNames[footnoteButtonPos[footnoteValues[1], 1, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[1]];
                            displayedInstruction = displayedInstruction + letterNames[footnoteButtonPos[footnoteValues[1], 1, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[1]];
                        }
                        else
                        {
                            fakeInstruction = fakeInstruction + letterNames[footnoteButtonPos[footnoteValues[1], 1, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[1]];
                            displayedInstruction = displayedInstruction + letterNames[footnoteButtonPos[footnoteValues[1], 1, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[1]];
                        }
                    }
                    else
                    {
                        fakeInstruction = fakeInstruction + letterNames[buttonValues[introButton2, 1]];
                        displayedInstruction = displayedInstruction + letterNames[buttonValues[introButton2, 1]];
                    }
                    break;
                default:
                    realInstruction = realInstruction + "" + locationNames[introButton2];
                    if (introFootnoteOmitted != 2)
                    {
                        curIndex = 0;
                        while (Array.IndexOf(locationNames, locationNames[introButton2]) != footnoteButtonPos[footnoteValues[1], 2, curIndex])
                        {
                            curIndex++;
                        }
                        if (sumGreater)
                        {
                            fakeInstruction = fakeInstruction + locationNames[footnoteButtonPos[footnoteValues[1], 2, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[1]];
                            displayedInstruction = displayedInstruction + locationNames[footnoteButtonPos[footnoteValues[1], 2, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[1]];
                        }
                        else
                        {
                            fakeInstruction = fakeInstruction + locationNames[footnoteButtonPos[footnoteValues[1], 2, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[1]];
                            displayedInstruction = displayedInstruction + locationNames[footnoteButtonPos[footnoteValues[1], 2, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[1]];
                        }
                    }
                    else
                    {
                        fakeInstruction = fakeInstruction + locationNames[introButton2];
                        displayedInstruction = displayedInstruction + locationNames[introButton2];
                    }
                    break;
            }
        }
        realInstruction = realInstruction + " button";
        fakeInstruction = fakeInstruction + " button";
        displayedInstruction = displayedInstruction + " button";
        if (introCondition != 0)
        {
            realInstruction = realInstruction + "'s number";
            fakeInstruction = fakeInstruction + "'s number";
            displayedInstruction = displayedInstruction + "'s number";
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        var isGood = false;
        while (!isGood)
        {
            isGood = true;
            realInstructionT = realInstructionT + ", press the button that " + finalInstructions[trueInstruction] + " the ";
            fakeInstructionT = fakeInstructionT + ", press the button that ";
            displayedInstructionT = displayedInstructionT + ", press the button that\n";
            if (trueFootnoteFirst)
            {
                curIndex = 0;
                while (Array.IndexOf(finalInstructions, finalInstructions[trueInstruction]) != footnoteDirection2[footnoteValues[2], curIndex])
                {
                    curIndex++;
                }
                //fakeInstructionT = fakeInstructionT + ", press the button that " //+ finalInstructions[trueInstruction] + " the ";
                //finalInstructions
                if (sumGreater)
                {
                    fakeInstructionT = fakeInstructionT + finalInstructions[footnoteDirection2[footnoteValues[2], (curIndex + 5) % 6]] + footnoteNames[footnoteValues[2]] + " the "; 
                    displayedInstructionT = displayedInstructionT + finalInstructions[footnoteDirection2[footnoteValues[2], (curIndex + 5) % 6]] + footnoteNames[footnoteValues[2]] + "the\n";
                }
                else
                {
                    fakeInstructionT = fakeInstructionT + finalInstructions[footnoteDirection2[footnoteValues[2], (curIndex + 1) % 6]] + footnoteNames[footnoteValues[2]] + " the ";
                    displayedInstructionT = displayedInstructionT + finalInstructions[footnoteDirection2[footnoteValues[2], (curIndex + 1) % 6]] + footnoteNames[footnoteValues[2]] + "the\n";
                }
            }
            else
            {
                fakeInstructionT = fakeInstructionT + finalInstructions[trueInstruction] + " the ";
                displayedInstructionT = displayedInstructionT + finalInstructions[trueInstruction] + "the\n";
            }
            switch (trueButtonType)
            {
                case 0:
                    realInstructionT = realInstructionT + colorNames[buttonValues[trueButton, 0]];
                    if (trueFootnoteFirst)
                    {
                        fakeInstructionT = fakeInstructionT + colorNames[buttonValues[trueButton, 0]];
                        displayedInstructionT = displayedInstructionT + colorNames[buttonValues[trueButton, 0]];
                    }
                    else
                    {
                        curIndex = 0;
                        while (Array.IndexOf(colorNames, colorNames[buttonValues[trueButton, 0]]) != footnoteButtonPos[footnoteValues[2], 0, curIndex])
                        {
                            curIndex++;
                        }
                        if (sumGreater)
                        {
                            fakeInstructionT = fakeInstructionT + colorNames[footnoteButtonPos[footnoteValues[2], 0, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[2]];
                            displayedInstructionT = displayedInstructionT + colorNames[footnoteButtonPos[footnoteValues[2], 0, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[2]];
                        }
                        else
                        {
                            fakeInstructionT = fakeInstructionT + colorNames[footnoteButtonPos[footnoteValues[2], 0, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[2]];
                            displayedInstructionT = displayedInstructionT + colorNames[footnoteButtonPos[footnoteValues[2], 0, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[2]];
                        }
                    }
                    break;

                case 1:
                    realInstructionT = realInstructionT + letterNames[buttonValues[trueButton, 1]];
                    if (trueFootnoteFirst)
                    {
                        fakeInstructionT = fakeInstructionT + letterNames[buttonValues[trueButton, 1]];
                        displayedInstructionT = displayedInstructionT + letterNames[buttonValues[trueButton, 1]];
                    }
                    else
                    {
                        curIndex = 0;
                        while (Array.IndexOf(letterNames, letterNames[buttonValues[trueButton, 1]]) != footnoteButtonPos[footnoteValues[2], 1, curIndex])
                        {
                            curIndex++;
                        }
                        if (sumGreater)
                        {
                            fakeInstructionT = fakeInstructionT + letterNames[footnoteButtonPos[footnoteValues[2], 1, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[2]];
                            displayedInstructionT = displayedInstructionT + letterNames[footnoteButtonPos[footnoteValues[2], 1, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[2]];
                        }
                        else
                        {
                            fakeInstructionT = fakeInstructionT + letterNames[footnoteButtonPos[footnoteValues[2], 1, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[2]];
                            displayedInstructionT = displayedInstructionT + letterNames[footnoteButtonPos[footnoteValues[2], 1, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[2]];
                        }
                    }
                    break;

                case 2:
                    realInstructionT = realInstructionT + "" + numberNames[buttonValues[trueButton, 2]];
                    if (trueFootnoteFirst)
                    {
                        fakeInstructionT = fakeInstructionT + numberNames[buttonValues[trueButton, 2]];
                        displayedInstructionT = displayedInstructionT + numberNames[buttonValues[trueButton, 2]];
                    }
                    else
                    {
                        curIndex = 0;
                        while (Array.IndexOf(numberNames, numberNames[buttonValues[trueButton, 2]]) != footnoteButtonPos[footnoteValues[2], 2, curIndex])
                        {
                            curIndex++;
                        }
                        if (sumGreater)
                        {
                            fakeInstructionT = fakeInstructionT + numberNames[footnoteButtonPos[footnoteValues[2], 2, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[2]];
                            displayedInstructionT = displayedInstructionT + numberNames[footnoteButtonPos[footnoteValues[2], 2, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[2]];
                        }
                        else
                        {
                            fakeInstructionT = fakeInstructionT + numberNames[footnoteButtonPos[footnoteValues[2], 2, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[2]];
                            displayedInstructionT = displayedInstructionT + numberNames[footnoteButtonPos[footnoteValues[2], 2, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[2]];
                        }
                    }
                    break;

                default:
                    realInstructionT = realInstructionT + "" + locationNames[trueButton];
                    if (trueFootnoteFirst)
                    {
                        fakeInstructionT = fakeInstructionT + locationNames[trueButton];
                        displayedInstructionT = displayedInstructionT + locationNames[trueButton];
                    }
                    else
                    {
                        curIndex = 0;
                        while (Array.IndexOf(locationNames, locationNames[trueButton]) != footnoteButtonPos[footnoteValues[2], 3, curIndex])
                        {
                            curIndex++;
                        }
                        if (sumGreater)
                        {
                            fakeInstructionT = fakeInstructionT + locationNames[footnoteButtonPos[footnoteValues[2], 3, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[2]];
                            displayedInstructionT = displayedInstructionT + locationNames[footnoteButtonPos[footnoteValues[2], 3, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[2]];
                        }
                        else
                        {
                            fakeInstructionT = fakeInstructionT + locationNames[footnoteButtonPos[footnoteValues[2], 3, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[2]];
                            displayedInstructionT = displayedInstructionT + locationNames[footnoteButtonPos[footnoteValues[2], 3, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[2]];
                        }
                    }
                    break;

            }
            realInstructionT = realInstructionT + " button. Otherwise, press the button that " + finalInstructions[falseInstruction] + " the ";
            fakeInstructionT = fakeInstructionT + " button. Otherwise, press the button that ";
            displayedInstructionT = displayedInstructionT + " button.\nOtherwise, press the button that\n";
            if (falseFootnoteFirst)
            {
                curIndex = 0;
                while (Array.IndexOf(finalInstructions, finalInstructions[falseInstruction]) != footnoteDirection2[footnoteValues[3], curIndex])
                {
                    curIndex++;
                }
                if (sumGreater)
                {
                    fakeInstructionT = fakeInstructionT + finalInstructions[footnoteDirection2[footnoteValues[3], (curIndex + 5) % 6]] + footnoteNames[footnoteValues[3]] + " the ";
                    displayedInstructionT = displayedInstructionT + finalInstructions[footnoteDirection2[footnoteValues[3], (curIndex + 5) % 6]] + footnoteNames[footnoteValues[3]] + "the\n";
                }
                else
                {
                    fakeInstructionT = fakeInstructionT + finalInstructions[footnoteDirection2[footnoteValues[3], (curIndex + 1) % 6]] + footnoteNames[footnoteValues[3]] + " the ";
                    displayedInstructionT = displayedInstructionT + finalInstructions[footnoteDirection2[footnoteValues[3], (curIndex + 1) % 6]] + footnoteNames[footnoteValues[3]] + "the\n";
                }
            }
            else
            {
                fakeInstructionT = fakeInstructionT + finalInstructions[falseInstruction] + " the ";
                displayedInstructionT = displayedInstructionT + finalInstructions[falseInstruction] + "the\n";
            }

            switch (falseButtonType)
            {
                case 0:
                    realInstructionT = realInstructionT + colorNames[buttonValues[falseButton, 0]];
                    if (falseFootnoteFirst)
                    {
                        fakeInstructionT = fakeInstructionT + colorNames[buttonValues[falseButton, 0]];
                        displayedInstructionT = displayedInstructionT + colorNames[buttonValues[falseButton, 0]];
                    }
                    else
                    {
                        curIndex = 0;
                        while (Array.IndexOf(colorNames, colorNames[buttonValues[falseButton, 0]]) != footnoteButtonPos[footnoteValues[3], 0, curIndex])
                        {
                            curIndex++;
                        }
                        if (sumGreater)
                        {
                            fakeInstructionT = fakeInstructionT + colorNames[footnoteButtonPos[footnoteValues[3], 0, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[3]];
                            displayedInstructionT = displayedInstructionT + colorNames[footnoteButtonPos[footnoteValues[3], 0, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[3]];
                        }
                        else
                        {
                            fakeInstructionT = fakeInstructionT + colorNames[footnoteButtonPos[footnoteValues[3], 0, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[3]];
                            displayedInstructionT = displayedInstructionT + colorNames[footnoteButtonPos[footnoteValues[3], 0, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[3]];
                        }
                    }
                    break;

                case 1:
                    realInstructionT = realInstructionT + letterNames[buttonValues[falseButton, 1]];
                    if (falseFootnoteFirst)
                    {
                        fakeInstructionT = fakeInstructionT + letterNames[buttonValues[falseButton, 1]];
                        displayedInstructionT = displayedInstructionT + letterNames[buttonValues[falseButton, 1]];
                    }
                    else
                    {
                        curIndex = 0;
                        while (Array.IndexOf(letterNames, letterNames[buttonValues[falseButton, 1]]) != footnoteButtonPos[footnoteValues[3], 1, curIndex])
                        {
                            curIndex++;
                        }
                        if (sumGreater)
                        {
                            fakeInstructionT = fakeInstructionT + letterNames[footnoteButtonPos[footnoteValues[3], 1, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[3]];
                            displayedInstructionT = displayedInstructionT + letterNames[footnoteButtonPos[footnoteValues[3], 1, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[3]];
                        }
                        else
                        {
                            fakeInstructionT = fakeInstructionT + letterNames[footnoteButtonPos[footnoteValues[3], 1, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[3]];
                            displayedInstructionT = displayedInstructionT + letterNames[footnoteButtonPos[footnoteValues[3], 1, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[3]];
                        }
                    }
                    break;

                case 2:
                    realInstructionT = realInstructionT + "" + numberNames[buttonValues[falseButton, 2]];
                    if (falseFootnoteFirst)
                    {
                        fakeInstructionT = fakeInstructionT + numberNames[buttonValues[falseButton, 2]];
                        displayedInstructionT = displayedInstructionT + numberNames[buttonValues[falseButton, 2]];
                    }
                    else
                    {
                        curIndex = 0;
                        while (Array.IndexOf(numberNames, numberNames[buttonValues[falseButton, 2]]) != footnoteButtonPos[footnoteValues[3], 2, curIndex])
                        {
                            curIndex++;
                        }
                        if (sumGreater)
                        {
                            fakeInstructionT = fakeInstructionT + numberNames[footnoteButtonPos[footnoteValues[3], 2, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[3]];
                            displayedInstructionT = displayedInstructionT + numberNames[footnoteButtonPos[footnoteValues[3], 2, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[3]];
                        }
                        else
                        {
                            fakeInstructionT = fakeInstructionT + numberNames[footnoteButtonPos[footnoteValues[3], 2, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[3]];
                            displayedInstructionT = displayedInstructionT + numberNames[footnoteButtonPos[footnoteValues[3], 2, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[3]];
                        }
                    }
                    break;

                default:
                    realInstructionT = realInstructionT + "" + locationNames[falseButton];
                    if (falseFootnoteFirst)
                    {
                        fakeInstructionT = fakeInstructionT + locationNames[falseButton];
                        displayedInstructionT = displayedInstructionT + locationNames[falseButton];
                    }
                    else
                    {
                        curIndex = 0;
                        while (Array.IndexOf(locationNames, locationNames[falseButton]) != footnoteButtonPos[footnoteValues[3], 3, curIndex])
                        {
                            curIndex++;
                        }
                        if (sumGreater)
                        {
                            fakeInstructionT = fakeInstructionT + locationNames[footnoteButtonPos[footnoteValues[3], 3, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[3]];
                            displayedInstructionT = displayedInstructionT + locationNames[footnoteButtonPos[footnoteValues[3], 3, (curIndex + 3) % 4]] + footnoteNames[footnoteValues[3]];
                        }
                        else
                        {
                            fakeInstructionT = fakeInstructionT + locationNames[footnoteButtonPos[footnoteValues[3], 3, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[3]];
                            displayedInstructionT = displayedInstructionT + locationNames[footnoteButtonPos[footnoteValues[3], 3, (curIndex + 1) % 4]] + footnoteNames[footnoteValues[3]];
                        }
                    }
                    break;

            }
            realInstructionT = realInstructionT + " button.";
            fakeInstructionT = fakeInstructionT + " button.";
            displayedInstructionT = displayedInstructionT + " button.";
            possibleFalse = findButton(falseInstruction, falseButton);
            possibleTrue = findButton(trueInstruction, trueButton);
            if (possibleTrue == possibleFalse)
            {
                isGood = false;
                trueButton = UnityEngine.Random.Range(0, 4);
                falseButton = (UnityEngine.Random.Range(1, 4) + trueButton) % 4;
                realInstructionT = "";
                fakeInstructionT = "";
                displayedInstructionT = "";
            }
        }
        realInstruction = realInstruction + realInstructionT;
        fakeInstruction = fakeInstruction + fakeInstructionT;
        displayedInstruction = displayedInstruction + displayedInstructionT;
        Debug.LogFormat("[Footnotes #{0}] The displayed instructions are: '{1}'.", _moduleId, fakeInstruction);
        Debug.LogFormat("[Footnotes #{0}] The real instructions are: '{1}'.", _moduleId, realInstruction);
        IDBox.GetComponentInChildren<TextMesh>().text = displayedInstruction;
        isIntroTrue = findSolution();
        if (isIntroTrue)
        {
            correctButton = possibleTrue;
        }
        else
        {
            correctButton = possibleFalse;
        }
        Debug.LogFormat("[Footnotes #{0}] You should press the {1} {2} button.", _moduleId, colorNames[buttonValues[correctButton, 0]], 
            (letterNames[buttonValues[correctButton, 1]] + numberNames[buttonValues[correctButton, 2]]));
        if (colorblindModeEnabled)
        {
            colorText.GetComponentInChildren<TextMesh>().text =
                colorNames[buttonValues[0, 0]].Substring(0, 1).ToUpperInvariant() + " " + colorNames[buttonValues[1, 0]].Substring(0, 1).ToUpperInvariant() + "\n" +
                colorNames[buttonValues[2, 0]].Substring(0, 1).ToUpperInvariant() + " " + colorNames[buttonValues[3, 0]].Substring(0, 1).ToUpperInvariant(); 
        }
        else
        {
            colorText.GetComponentInChildren<TextMesh>().text = "";
        }
        pressedAllowed = true;
    }


    void OnHold()
    {
		
    }

    void OnRelease()
    {
    }

    int findButton(int instructionNumber, int buttonNumber)
    {
        //"is up or down from", "is left or right from", "is diagonal from", "is exactly", "is next in reading order from", "is previous in reading order from"
        switch (instructionNumber)
        {
            case 0:
                return (buttonNumber + 2) % 4;
            case 1:
                switch (buttonNumber)
                {
                    case 0:
                        return 1;
                    case 1:
                        return 0;
                    case 2:
                        return 3;
                    case 3:
                        return 2;
                    default:
                        return -1;
                }
            case 2:
                return 3 - buttonNumber;
            case 3:
                return buttonNumber;
            case 4:
                return (buttonNumber + 1) % 4;
            case 5:
                return (buttonNumber + 3) % 4;
            default:
                break;
        }
        return 0;
    }

    bool findSolution()
    {
        switch (introCondition)
        {
            case 0:
                switch (introInstruction)
                {
                    //"vertically adjacent to", "horizontally adjacent to", "diagonally adjacent to", "orthagonally adjacent to", "the same as", "different from", "above", "below", "left of", "right of"
                    case 0:
                        if (introButton1 % 2 == introButton2 % 2)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case 1:
                        if (introButton1 + introButton2 == 1 || introButton1 + introButton2 == 5)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case 2:
                        if (introButton1 + introButton2 == 3)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case 3:
                        if (introButton1 + introButton2 != 3)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case 4:
                        if (introButton1 == introButton2)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case 5:
                        if (introButton1 != introButton2)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case 6:
                        if (introButton1 <= 1 && introButton2 >= 2)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case 7:
                        if (introButton2 <= 1 && introButton1 >= 2)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case 8:
                        if (introButton1 % 2 == 0 && introButton2 % 2 == 1)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case 9:
                        if (introButton1 % 2 == 1 && introButton2 % 2 == 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    default:
                        return false;
                }
            case 1:
                switch (introInstruction)
                {
                    //"is greater than", "is less than", "is equal to", "is less than or equal to", "is greater than or equal to", "is within one of", "is within two of", "is more than two away from"
                    case 0:
                        if (numberNames[buttonValues[introButton1, 2]] > numberNames[buttonValues[introButton2, 2]])
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case 1:
                        if (numberNames[buttonValues[introButton1, 2]] < numberNames[buttonValues[introButton2, 2]])
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case 2:
                        if (numberNames[buttonValues[introButton1, 2]] == numberNames[buttonValues[introButton2, 2]])
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case 3:
                        if (numberNames[buttonValues[introButton1, 2]] <= numberNames[buttonValues[introButton2, 2]])
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case 4:
                        if (numberNames[buttonValues[introButton1, 2]] >= numberNames[buttonValues[introButton2, 2]])
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case 5:
                        if (numberNames[buttonValues[introButton1, 2]] - numberNames[buttonValues[introButton2, 2]] >= -1 && 
                            numberNames[buttonValues[introButton1, 2]] - numberNames[buttonValues[introButton2, 2]] <= 1)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case 6:
                        if (numberNames[buttonValues[introButton1, 2]] - numberNames[buttonValues[introButton2, 2]] >= -2 &&
                            numberNames[buttonValues[introButton1, 2]] - numberNames[buttonValues[introButton2, 2]] <= 2)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case 7:
                        if (numberNames[buttonValues[introButton1, 2]] - numberNames[buttonValues[introButton2, 2]] >= 3 ||
                            numberNames[buttonValues[introButton1, 2]] - numberNames[buttonValues[introButton2, 2]] <= -3)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    default:
                        return false;
                }
            default:
                var edgeworkNumber = -1;
                //"number of port plates", "number of ports", "number of battery holders", "number of batteries", "number of indicators", "last digit of the bomb's serial number", "first digit of the bomb's serial number", "second digit of the bomb's serial number"
                switch (introButton1)
                {
                    case 0:
                        edgeworkNumber = Bomb.GetPortPlateCount();
                        break;
                    case 1:
                        edgeworkNumber = Bomb.GetPortCount();
                        break;
                    case 2:
                        edgeworkNumber = Bomb.GetBatteryHolderCount();
                        break;
                    case 3:
                        edgeworkNumber = Bomb.GetBatteryCount();
                        break;
                    case 4:
                        edgeworkNumber = Bomb.GetIndicators().Count();
                        break;
                    case 5:
                        edgeworkNumber = Bomb.GetSerialNumberNumbers().Last();
                        break;
                    case 6:
                        edgeworkNumber = Bomb.GetSerialNumberNumbers().First();
                        break;
                    case 7:
                        var digitsSN = 0;
                        var snPosition = -1;
                        while (digitsSN != 2)
                        {
                            snPosition++;
                            if (Bomb.GetSerialNumber().Substring(snPosition, 1) == "0" || Bomb.GetSerialNumber().Substring(snPosition, 1) == "1" ||
                                Bomb.GetSerialNumber().Substring(snPosition, 1) == "2" || Bomb.GetSerialNumber().Substring(snPosition, 1) == "3" ||
                                Bomb.GetSerialNumber().Substring(snPosition, 1) == "4" || Bomb.GetSerialNumber().Substring(snPosition, 1) == "5" ||
                                Bomb.GetSerialNumber().Substring(snPosition, 1) == "6" || Bomb.GetSerialNumber().Substring(snPosition, 1) == "7" ||
                                Bomb.GetSerialNumber().Substring(snPosition, 1) == "8" || Bomb.GetSerialNumber().Substring(snPosition, 1) == "9")
                            {
                                digitsSN++;
                            }
                        }
                        edgeworkNumber = Int16.Parse(Bomb.GetSerialNumber().Substring(snPosition, 1));
                        break;
                    default:
                        break;
                }
                switch (introInstruction)
                {
                    //"is greater than", "is less than", "is equal to", "is less than or equal to", "is greater than or equal to", "is within one of", "is within two of", "is more than two away from"
                    case 0:
                        if (edgeworkNumber > numberNames[buttonValues[introButton2, 2]])
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case 1:
                        if (edgeworkNumber < numberNames[buttonValues[introButton2, 2]])
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case 2:
                        if (edgeworkNumber == numberNames[buttonValues[introButton2, 2]])
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case 3:
                        if (edgeworkNumber <= numberNames[buttonValues[introButton2, 2]])
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case 4:
                        if (edgeworkNumber >= numberNames[buttonValues[introButton2, 2]])
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case 5:
                        if (edgeworkNumber - numberNames[buttonValues[introButton2, 2]] >= -1 &&
                            edgeworkNumber - numberNames[buttonValues[introButton2, 2]] <= 1)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case 6:
                        if (edgeworkNumber - numberNames[buttonValues[introButton2, 2]] >= -2 &&
                            edgeworkNumber - numberNames[buttonValues[introButton2, 2]] <= 2)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case 7:
                        if (edgeworkNumber - numberNames[buttonValues[introButton2, 2]] >= 3 ||
                            edgeworkNumber - numberNames[buttonValues[introButton2, 2]] <= -3)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    default:
                        return false;
                }
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} (submit/s/answer/a/press/p) (letter/number/color) to press a button. A color will only be submitted if its full name (red/yellow/green/blue) is given; using 'B' submits bravo, not blue.";
    private readonly bool TwitchShouldCancelCommand = false;
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        tpActive = true;
        var piecesRaw = command.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        string theError;
        theError = "";
        yield return null;
		
        if (piecesRaw.Count() == 0)
        {
            theError = "sendtochaterror No arguments! You need to use (submit/s/answer/a/press/p), then a letter, number, or color.";
            yield return theError;
        }
        else if (piecesRaw[0] == "colorblind")
        {
            colorblindModeEnabled = true;
            colorText.GetComponentInChildren<TextMesh>().text =
                colorNames[buttonValues[0, 0]].Substring(0, 1).ToUpperInvariant() + " " + colorNames[buttonValues[1, 0]].Substring(0, 1).ToUpperInvariant() + "\n" +
                colorNames[buttonValues[2, 0]].Substring(0, 1).ToUpperInvariant() + " " + colorNames[buttonValues[3, 0]].Substring(0, 1).ToUpperInvariant();
            yield return null;
        }
        else if (piecesRaw.Count() < 2)
        {
            theError = "sendtochaterror Not enough arguments! You need to use (submit/s/answer/a/press/p), then a letter, number, or color.";
            yield return theError;
        }
        else if (piecesRaw[0] == "submit" || piecesRaw[0] == "answer" || piecesRaw[0] == "press" ||
            piecesRaw[0] == "s" || piecesRaw[0] == "a" || piecesRaw[0] == "p")
        {
            //Debug.Log("Answer in");
            var listPosNum = -1;
            if (piecesRaw[1] == "red" || piecesRaw[1] == "yellow" || piecesRaw[1] == "green" || piecesRaw[1] == "blue")
            {
                //Debug.Log("Color chosen");
                listPosNum = Array.IndexOf(colorNames, piecesRaw[1]);
                for (int buttonNum = 0; buttonNum < 4; buttonNum++)
                {
                    if (buttonValues[buttonNum, 0] == listPosNum)
                    {
                        yield return new WaitForSeconds(.1f);
                        yield return null;
                        doSubmit(buttonNum);
                    }
                }
            }
            else if (piecesRaw[1] == "1" || piecesRaw[1] == "2" || piecesRaw[1] == "3" || piecesRaw[1] == "4")
            {
                //Debug.Log("Number chosen: " + piecesRaw[1]);
                //Debug.Log("Numbers: " + numberNames[0] + ", " + numberNames[1] + ", " + numberNames[2] + ", " +numberNames[3] + ".");
                listPosNum = Array.IndexOf(numberNames, Int16.Parse(piecesRaw[1]));
                //Debug.Log("listPosNum = " + listPosNum);
                for (int buttonNum = 0; buttonNum < 4; buttonNum++)
                {
                    //Debug.Log("buttonNum = " + buttonNum + " and buttonValues[buttonNum, 2] is " + buttonValues[buttonNum, 2]);
                    if (buttonValues[buttonNum, 2] == listPosNum)
                    {
                        yield return new WaitForSeconds(.1f);
                        yield return null;
                        doSubmit(buttonNum);
                    }
                }
            }
            else if (piecesRaw[1] == "a" || piecesRaw[1] == "b" || piecesRaw[1] == "c" || piecesRaw[1] == "d")
            {
                //Debug.Log("Letter chosen");
                listPosNum = Array.IndexOf(letterNames, piecesRaw[1].ToUpperInvariant());
                for (int buttonNum = 0; buttonNum < 4; buttonNum++)
                {
                    if (buttonValues[buttonNum, 1] == listPosNum)
                    {
                        yield return new WaitForSeconds(.1f);
                        yield return null;
                        doSubmit(buttonNum);
                    }
                }
                //submit.OnInteract();

            }
            else
            {
                theError = "sendtochaterror Sorry, I did not recognize the argument: '" + piecesRaw[1] + "'. Valid colors are red, yellow, green, blue. Valid letters are A, B, C, D. Valid numbers are 1, 2, 3, 4.";
                yield return theError;
            }
        }
        else
        {
                theError = "sendtochaterror Sorry, I did not recognize the command: '" + piecesRaw[0] + "'. You must use submit, answer, press, s, a, or p, as well as a color, letter, or number.";
                yield return theError;
        }
     }

    void doSubmit(int index)
    {
        if (pressedAllowed)
        {
            Debug.LogFormat("[Footnotes #{0}] You pressed the {1} {2} button.", _moduleId, colorNames[buttonValues[index, 0]],
                (letterNames[buttonValues[index, 1]] + numberNames[buttonValues[index, 2]]));
            if (index == correctButton)
            {
                Debug.LogFormat("[Footnotes #{0}] This is correct, module disarmed!", _moduleId);
                //Debug.LogFormat("[Footnotes #{0}] GetFormattedTime is {1}, GetTime is {2}!", _moduleId, Bomb.GetFormattedTime(), Bomb.GetTime());
                pressedAllowed = false;
                isSolved = true;
                Module.HandlePass();
            }
            else
            {
                Debug.LogFormat("[Footnotes #{0}] Wrong, strike given.", _moduleId);
                realInstruction = "";
                realInstructionT = "";
                fakeInstruction = "";
                fakeInstructionT = "";
                displayedInstruction = "";
                displayedInstructionT = "";
                Module.HandleStrike();
                Init();
            }
        }
    }

    void delegationZone()
    {
        button[0].OnInteract += delegate () { doSubmit(0); button[0].AddInteractionPunch(0.2f); return false; };
        button[1].OnInteract += delegate () { doSubmit(1); button[1].AddInteractionPunch(0.2f); return false; };
        button[2].OnInteract += delegate () { doSubmit(2); button[2].AddInteractionPunch(0.2f); return false; };
        button[3].OnInteract += delegate () { doSubmit(3); button[3].AddInteractionPunch(0.2f); return false; };

        button[0].OnInteractEnded += delegate () { OnRelease(); };
        button[1].OnInteractEnded += delegate () { OnRelease(); };
        button[2].OnInteractEnded += delegate () { OnRelease(); };
        button[3].OnInteractEnded += delegate () { OnRelease(); };
    }

    

}
