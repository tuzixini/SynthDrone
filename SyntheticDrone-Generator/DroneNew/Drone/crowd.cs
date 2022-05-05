using System;
using GTA.Math;
using GTA;
using System.IO;
using System.Text;
using GTA.Native;
using System.Collections.Generic;
using Drone;

namespace Crowd
{
    public class Create
    {
        public static uint[] pedHashes = { 3716251309, 4209271110, 3886638041, 664399832, 1498487404, 2680389410, 2064532783, 3100414644, 2869588309, 846439045, 1746653202, 3756278757, 2992445106, 623927022, 2563194959, 2515474659, 2775713665, 1330042375, 2240226444, 452351020, 1768677545, 588969535, 103106535, 2206530719, 3609190705, 1809430156, 390939205, 3579522037, 4049719826, 1334976110, 1146800212, 51789996, 115168927, 2255803900, 999748158, 653210662, 2111372120, 3284966005, 2688103263, 2374966032, 4206136267, 1684083350, 549978415, 3072929548, 4079145784, 2359345766, 3394697810, 3290105390, 1750583735, 2459507570, 1099825042, 1546450936, 2549481101, 2276611093, 1567728751, 2255894993, 949295643, 1191403201, 3887273010, 3365863812, 793439294, 1561705728, 2217749257, 3271294718, 435429221, 2651349821, 2756120947, 534725268, 3835149295, 2638072698, 2633130371, 1206185632, 1312913862, 3990661997, 2952446692, 3349113128, 1032073858, 117698822, 3669401835, 1423699487, 516505552, 2346291386, 3767780806, 951767867, 1446741360, 3519864886, 3247667175, 469792763, 4033578141, 429425116, 2842568196, 2766184958, 4246489531, 3812756443, 1382414087, 3523131524, 3065114024, 3008586398, 1519319503, 4198014287, 1240094341, 1482427218, 2047212121, 3877027275, 599294057, 3938633710, 3529955798, 2512875213, 321657486, 1890499016, 1055701597, 3367442045, 2896414922, 1090617681, 941695432, 826475330, 4096714883, 411102470, 70821038, 62440720, 3640249671, 3972697109, 2557996913, 2218630415, 3502104854, 1650288984, 503621995, 2674735073, 355916122, 1426880966, 3988550982, 3684436375, 797459875, 2602752943, 891398354, 3881519900, 3374523516, 2928082356, 365775923, 696250687, 3189832196, 3083210802, 2494442380, 663522487, 3881194279, 4121954205, 2705543429, 261586155, 32417469, 1720428295, 331645324, 1640504453, 579932932, 479578891, 131961260, 2988916046, 1459905209, 2363277399, 2780469782, 4255728232, 3014915558, 1226102803, 3064628686, 1674107025, 3499148112, 3896218551, 2231547570, 3512565361, 2423691919, 2962707003, 1846684678, 1039800368, 1830688247, 2120901815, 330231874, 225514697, 3621428889, 3613962792, 3454621138, 767028979, 373000027, 744758650, 2923947184, 919005580, 1264851357, 1641152947, 1466037421, 2185745201, 1863555924, 3681718840, 3367706194, 3321821918, 835315305, 2435054400, 3782053633, 1644266841, 3482496489, 2608926626, 1165780219, 2021631368, 1371553700, 2908022696, 920595805, 2681481517, 3513928062, 1767892582, 832784782, 1068876755, 815693290, 1530648845, 238213328, 3265820418, 3250873975, 4030826507, 587703123, 894928436, 1951946145, 3680420864, 600300561, 1082572151, 228715206, 1982350912, 3882958867, 803106487, 377976310, 788622594, 349680864, 1204772502, 3728026165, 1347814329, 808859815, 1581098148, 2318861297, 193817059, 1752208920, 4131252449, 988062523, 466359675, 1416254276, 1388848350, 3343476521, 349505262, 1380197501, 2114544056, 2340239206, 1520708641, 766375082, 68070371, 2981205682, 1699403886, 42647445, 1404403376, 101298480, 2422005962, 3019107892, 3654768780, 2597531625, 824925120, 2659242702, 810804565, 2124742566, 933092024, 1142162924, 3188223741, 4058522530 };
        public struct Animation
        {
            public int shortcutIndex;
            public string strShortcutIndex;
            public string animLibrary;
            public string animName;
            public int duration;

            string toString()
            {
                return strShortcutIndex + " " + animLibrary + " " + animName + " " + duration;
            }
        };
        public static List<Animation> allAnimation = new List<Animation>();
        public static int animLen;

        public static bool initAnimation(string fileName)
        {
            StreamReader sr = new StreamReader(File.OpenRead(fileName), Encoding.Default);
            string line = sr.ReadLine();
            int index = 1;
            while (line != null)
            {
                line = line.Trim();
                //srtshortcutindex animlibrary animname duration
                string[] split = line.Split(new char[] { ' ' });
                Animation temp;
                temp.shortcutIndex = index;
                temp.strShortcutIndex = split[0];
                temp.animLibrary = split[1];
                temp.animName = split[2];
                temp.duration = (int)Convert.ToInt32(split[3]);
                allAnimation.Add(temp);
                //next line
                index += 1;
                line = sr.ReadLine();
            }
            animLen = index;
            return true;
        }

        public bool Put(int num, Vector3[] areas)
        {
            int areasNum = areas.Length % 4;
            if (areasNum != 0) { return false; }
            areasNum =(int)areas.Length / 4;
            int areaId = 0;
            Random rd = new Random();
            Ped myPed = Function.Call<Ped>(Hash.PLAYER_PED_ID);
            Vector3 loc = Function.Call<Vector3>(Hash.GET_ENTITY_COORDS, myPed, true);
            float avgz = loc.Z+0.5f;
            float x = 0, y = 0;
            for (int i = 0; i < num; i++)
            {
                areaId = rd.Next(1,areasNum)-1;
                Vector3[] area = { areas[areaId],areas[areaId+1],areas[areaId+2],areas[areaId+3]};
                float sx=0, sy=0, dx=0, dy=0;
                getBoard(area,ref sx, ref sy, ref dx, ref dy);
                x = rd.Next((int)sx, (int)dx);
                y = rd.Next((int)sy, (int)dy);
                uint pedHash = pedHashes[rd.Next(0, pedHashes.GetLength(0))];
                float heading = 360.0f * rd.Next(0, 10000) / 10000;
                Animation anim = allAnimation[rd.Next(0, animLen)];
                Function.Call(Hash.REQUEST_MODEL, pedHash);
                while (!Function.Call<bool>(Hash.HAS_MODEL_LOADED, pedHash)) { Wait(0); }
                Ped tempPed = Function.Call<Ped>(Hash.CREATE_PED, 4, pedHash, x, y, avgz, heading, false, true);
                Function.Call(Hash.SET_MODEL_AS_NO_LONGER_NEEDED, pedHash);
                bool ex = Function.Call<bool>(Hash.DOES_ANIM_DICT_EXIST, anim.animLibrary);
                if (ex)
                {
                    Function.Call(Hash.REQUEST_ANIM_DICT, anim.animLibrary);
                    while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, anim.animLibrary)) { int temp = 0; }
                }
                

            }
            return true;
        }

        public static void getBoard(Vector3[] area,ref float sx,ref float sy,ref float dx,ref float dy)
        {
            //float sx, sy, dx, dy;
            sx = getMin(new float[] { area[0].X, area[1].X, area[2].X, area[3].X });
            sy = getMin(new float[] { area[0].Y, area[1].Y, area[2].Y, area[3].Y });
            dx = getMax(new float[] { area[0].X, area[1].X, area[2].X, area[3].X });// - sx;
            dy = getMax(new float[] { area[0].Y, area[1].Y, area[2].Y, area[3].Y });// - sy;
            //return sx,sy,dx,dy;
        }
        public static float getMax(float[] array)
        {
            float max = array[0];
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] >= max)
                {
                    max = array[i];
                }
            }
            return max;
        }
        public static float getMin(float[] array)
        {
            float min = array[0];
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] <= min)
                {
                    min = array[i];
                }
            }
            return min;
        }

    }
}
