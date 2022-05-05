//基础需要的命名空间
using fileIO;
using GTA;
using GTA.Math;
using GTA.Native;
//Self define namespace
using GTAVisionUtils;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
    

namespace Drone
{
    public class Drone : Script   //must use the “Script” class
    {
        //struct to save the location
        public struct Location
        {
            public Vector3 coo; //coordinate x,y,z
            public Vector3 ang; //cam angle x,y,z
            public float fov;   //field of view fov
            public int id;      //the id of current location
        }
        //Some common used variables
        //public string dataDir = "DroneData";         //dir to save data
        public string dataDir = "F:\\DroneData";         //dir to save data
        public string logger = "DroneLog.txt";  //logfile
        public string locFile = "camLoc.txt";   //the file save the locations need collection
        public Location[] allLocation = new Location[1000];//save all the locatioons
        public Location currentLoc;             //the current processing location 
        public int locNum=0;                      //the procussing number of locations
        public int locTotalNum = 0;                 //total number of locations
        public int interTime = 500;             // the time interval between each capture (ms)
        public int frameNum = 300;               // the frame number of each video
        public bool isInit = false;
        public Weather curWeather;              //wather for current location
        public DateTime curDateTime;            //date and time for current location
        public Weather[] allWeather = { Weather.ExtraSunny, Weather.Clear, Weather.Foggy, Weather.Raining, Weather.ThunderStorm };
        public int[] allTimeInter = { 1,2,3,4,5,6 };//time interval 5-9-13-17-21-0
        Random ra = new Random(996);
        public int sindw = 0;
        public int sindt = 0;
        public int sloc = 0;
        public bool isFirstLoop = true;
        public Camera capCam;

        // varibales for location record
        public string scriptStatus;             // the current script status
        public string DroneLocCol="_DroneLocCol.txt"; // txt file to save collected drone locations
        public bool CameraMode = false;         // camera mode
        public Vector3 camDelta;
        public bool IsMovement = false;
        public float nfov = 0;
        public Camera cCam;
        public float cameraSpeedFactor = 5f;
        public bool viewNext = false;

        //variables for dense scense create
        public List<Vector3> areas = new List<Vector3>();
        public List<Ped> allPed = new List<Ped>();
        //public Vector3[] areas = new Vector3[100];
        public int pointNum = 0;
        public int NUM = 50;
        public float avgz = 0;

        //the function is the Constructor function must use the same name as the main class
        //this function will be run after this class loaded
        public Drone()
        {
            Tick += OnTick; //reflash event 刷新事件
            KeyDown += OnKeyDown; //key down event 按键按下事件
            Interval = 10; //the reflash time (ms)刷新事件的间隔时间，单位毫秒
            Game.Pause(false);
        }
        //reflash event 刷新事件
        void OnTick(object sender, EventArgs e)
        {
            //pass
            if (CameraMode)
            {
                AdjustCam();
            }
        }  

        //key down event 快捷键事件
        void OnKeyDown(object sender, KeyEventArgs e)
        {
            //key B
            if (e.KeyCode == Keys.B)
            {
                if (! isInit )
                { Init(); }
                Log.WriteLog(logger, "Start caputer work!!!");
                for (int indw = sindw; indw < allWeather.GetLength(0); indw++)
                {
                    if (!isFirstLoop) { sindt = 0; }
                    for (int indt = sindt; indt < allTimeInter.GetLength(0); indt++)
                    {
                        locNum = 0;
                        if (isFirstLoop) { locNum = sloc - 1; }  
                        while (locNum < locTotalNum)//for each location*******
                        {
                            isFirstLoop = false;
                            currentLoc = allLocation[locNum];
                            // pro precess the camera rotation info 
                            currentLoc.ang.X = -90; currentLoc.ang.Y = 0; currentLoc.fov = 50;
                            currentLoc.coo.Z = allLocation[locNum].coo.Z + ra.Next(-30, 50);// random change the height
                            if (((currentLoc.id < 44) && (currentLoc.id!=17)) && (currentLoc.coo.Z > 280))
                            {  currentLoc.coo.Z = 280; }
                            //Log.WriteLog(logger, currentLoc.coo.Z.ToString() + allLocation[locNum].coo.Z.ToString());
                            // reset weather and time index
                            curWeather =allWeather[indw];
                            curDateTime = GTA.World.CurrentDate;
                            if (indt == 5)
                            {
                                curDateTime = curDateTime.AddHours(ra.Next(0,5) - curDateTime.Hour);
                            }
                            curDateTime = curDateTime.AddHours(ra.Next(5+indt*4,5+indt*4+4)-curDateTime.Hour);
                            curDateTime = curDateTime.AddMinutes(ra.Next(0, 59) - curDateTime.Minute);
                            curDateTime = curDateTime.AddSeconds(ra.Next(0, 59) - curDateTime.Second);
                            Log.WriteLog(logger, "Start for new weather-" + indw.ToString() +"-"+ curWeather.ToString() + " and time" + indt.ToString() +"-"+ curDateTime.ToString());
                            //make save dir
                            string savePath = curDateTime.ToString("HH_mm");
                            savePath = savePath.Replace(' ', '-').Replace(':', '-').Replace('/', '-');
                            savePath = "LOC-" + currentLoc.id.ToString() + "-" + savePath + "-" + curWeather.ToString();
                            savePath = dataDir + "/" + savePath;
                            if (!Directory.Exists(savePath))
                            {
                                Directory.CreateDirectory(savePath);    // create data save dir
                                Log.WriteLog(logger, "Dir Created: " + savePath + "for location:" + currentLoc.id.ToString());
                                string info = "";
                                info = "LOC: " + currentLoc.coo.X.ToString() + " " + currentLoc.coo.Y.ToString() + " " + currentLoc.coo.Z.ToString() + " ";
                                info += currentLoc.ang.X.ToString() + " " + currentLoc.ang.Y.ToString() + " " + currentLoc.ang.Z.ToString() + " " + currentLoc.fov.ToString() + " " + currentLoc.id.ToString();
                                info += "\nWeather: " + curWeather.ToString() + "\nGameDateTime: " + curDateTime.ToString();
                                info += "\nRecordingTime: " + DateTime.Now.ToString("MM-dd-hh-mm-ss");
                                Log.Write(savePath + "/info.txt", info + "\n");
                                CaptureCurrentLocation(savePath);
                                Log.WriteLog(logger, "Finish loc " + currentLoc.id.ToString() +" with weather "+curWeather.ToString()+" and time "+curDateTime.ToString());
                            }
                            else
                            {
                                Log.WriteLog(logger, "Dir Create Failed!!! Please Check: " + savePath);
                            }
                            Log.WriteLog(logger, "Caputure work for loc " + currentLoc.id.ToString() + " has done!!");
                            Log.WriteLog(logger, "locNum : " + locNum.ToString() + ", locTotalNum :" + locTotalNum.ToString());
                            locNum += 1;//next location
                        }
                    }
                    
                }
            }
            //key r to recover the game view
            if (e.KeyCode == Keys.R)
            {
                Log.WriteLog(logger, "Game reset!!!");
                ResetCam();
                if (scriptStatus == "denseScenseCreate")
                {
                    Ped myPed = Function.Call<Ped>(Hash.PLAYER_PED_ID);
                    //Vector3 playerLoc = Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_IN_WORLD_COORDS, myPed, 0.0, 0.0, 0.0);
                    Function.Call(Hash.SET_ENTITY_COORDS_NO_OFFSET, myPed, currentLoc.coo.X, currentLoc.coo.Y, avgz, 0, 0, 1);
                }
            }
            //key F10 to capture drone locationf
            if (e.KeyCode == Keys.F10)
            {
                scriptStatus = "colDroneLoc";
                if (CameraMode==false)
                {
                    StartNewCamera("_DroneLocCol_Figure.txt");
                    GTA.UI.Notification.Show("Now you can set the camera location. Push I to save current camera infomation");
                }
                else
                {
                    AdjustCam();
                }
            }
            if (e.KeyCode==Keys.F11)
            {
                if (scriptStatus!= "denseScenseCreate")
                {
                    // start dense scense create mode;
                    scriptStatus = "denseScenseCreate";
                    locFile = "createDenseLoc.txt";
                    if (!isInit)
                        { Init(); }
                    initAnimation(dataDir + "/SceneDirectorAnim.txt");
                    goToLoc();
                    // weather and time
                    sindw = 0;
                }
                else
                {
                    if (!CameraMode)
                    {
                        GTA.UI.Notification.Show("push F12 enter camera mode, adujst camera,then start captuer.");
                    }

                    else
                    {
                        startCapDenseScense();
                    }
                        
                }

            }
            if (e.KeyCode == Keys.F6)
            {
                if ((scriptStatus == "denseScenseCreate") & (allPed.ToArray().GetLength(0) > 0))
                {
                    for (int i = 0; i < allPed.ToArray().GetLength(0); i++)
                    {
                        allPed[i].Task.WanderAround();
                    }
                }
            }
            if (e.KeyCode == Keys.F7)
            {
                //areas.Clear();
                if (areas.Count > 0)
                {
                    areas.RemoveAt(areas.Count - 1);
                }
            }
            if (e.KeyCode == Keys.F8)
            {
                if (!putCrowd(NUM))
                {
                    string info = "Error!!! The length of area points is: "+areas.ToArray().GetLength(0).ToString();
                    GTA.UI.Notification.Show(info);
                }
            }
            if (e.KeyCode == Keys.F9)
            {
                deleteAllPed();
            }
            if (e.KeyCode == Keys.F12)
            {
                if (scriptStatus == "denseScenseCreate")
                {
                    if (CameraMode == false)
                    {
                        //StartNewCamera("createDenseLoc.txt");
                        ResetCam();
                        World.DestroyAllCameras();
                        cCam = World.CreateCamera(currentLoc.coo, currentLoc.ang, currentLoc.fov);
                        Wait(100);
                        Function.Call(Hash.STOP_CAM_POINTING, cCam);
                        Function.Call(Hash.RENDER_SCRIPT_CAMS, true, 0, 3000, 1, 0);
                        GTA.UI.Notification.Show("Now you can move the camera up and down use Q and E. Push R to reset camera. F11 to start capture.");
                        Ped myPed = Function.Call<Ped>(Hash.PLAYER_PED_ID);
                        Function.Call(Hash.SET_ENTITY_COORDS_NO_OFFSET, myPed, currentLoc.coo.X + 50, currentLoc.coo.Y + 50, 500, 0, 0, 1);
                        CameraMode = true;
                    }
                    //else
                    //{
                    //    AdjustCam();
                    //}
                }
                else
                {
                    // set camera face to the ground
                    Function.Call(Hash.SET_CAM_ROT, cCam, 90, 0, 90, 2);
                }
                    
            }
            if (e.KeyCode == Keys.I)
            {
                if (scriptStatus == "denseScenseCreate")
                {
                    Ped myPed = Function.Call<Ped>(Hash.PLAYER_PED_ID);
                    Vector3 loc = Function.Call<Vector3>(Hash.GET_ENTITY_COORDS, myPed, true);
                    //areas[pointNum] = loc;
                    areas.Add(loc);
                    //pointNum += 1;
                    string info = "Number " + (areas.ToArray().GetLength(0)).ToString() + " point get!";
                    GTA.UI.Notification.Show(info);
                    avgz = loc.Z + 0.5f;
                    //GTA.UI.Notification.Show(areas.ToArray().GetLength(0).ToString());
                }
                else
                {
                    if (scriptStatus == "colDroneLoc")
                    {
                        Vector3 loc = Function.Call<Vector3>(Hash.GET_CAM_COORD, cCam);
                        Vector3 rot = Function.Call<Vector3>(Hash.GET_CAM_ROT, cCam);
                        float fov = cCam.FieldOfView;
                        string info = "";
                        info = loc.X.ToString() + " " + loc.Y.ToString() + " " + loc.Z.ToString() + " " + rot.X.ToString() + " " + rot.Y.ToString() + " " + rot.Z.ToString() + " " + fov.ToString();
                        Log.Write(dataDir + "/" + DroneLocCol, info);
                        GTA.UI.Notification.Show("One Camera Info Saved!");
                    }
                    else
                    {
                        Ped myPed = Function.Call<Ped>(Hash.PLAYER_PED_ID);
                        Vector3 loc = Function.Call<Vector3>(Hash.GET_ENTITY_COORDS, myPed, true);
                        string info = "";
                        info = loc.X.ToString() + " " + loc.Y.ToString() + " " + loc.Z.ToString() + " -90 -90 0 40";
                        Log.Write(dataDir + "/" + DroneLocCol, info);
                        GTA.UI.Notification.Show("One Camera Info Saved! with person location");

                    }
                }
            }
            #region weathertime scense control
            if (e.KeyCode == Keys.Y)
            {
                if (scriptStatus == "denseScenseCreate")
                {
                    sindw = sindw + 1;
                    if (sindw > (allWeather.GetLength(0) - 1)) { sindw = 0; }
                    GTA.World.Weather = allWeather[sindw];
                    Log.WriteLog(logger, "Weather: "+allWeather[sindw].ToString());
                }
            }
            if (e.KeyCode == Keys.G)
            {
                if (scriptStatus == "denseScenseCreate")
                {
                    curDateTime = GTA.World.CurrentDate;
                    curDateTime = curDateTime.AddMinutes(-60);
                    Function.Call(Hash.SET_CLOCK_TIME, curDateTime.Hour, curDateTime.Minute, curDateTime.Second);
                    Log.WriteLog(logger, "DateTime: " + curDateTime.ToString());
                }
            }
            if (e.KeyCode == Keys.H)
            {
                if (scriptStatus == "denseScenseCreate")
                {
                    sindw = sindw - 1;
                    if (sindw < 1) { sindw = allWeather.GetLength(0) - 1; }
                    GTA.World.Weather = allWeather[sindw];
                    Log.WriteLog(logger, "Weather: " + allWeather[sindw].ToString());
                }
            }
            if (e.KeyCode == Keys.J)
            {
                if (scriptStatus == "denseScenseCreate")
                {
                    curDateTime = GTA.World.CurrentDate;
                    curDateTime = curDateTime.AddMinutes(60);
                    Function.Call(Hash.SET_CLOCK_TIME, curDateTime.Hour, curDateTime.Minute, curDateTime.Second);
                    Log.WriteLog(logger, "DateTime: " + curDateTime.ToString());
                }
            }
            //key T  make a test now is log
            if (e.KeyCode == Keys.U)
            {
                deleteAllPed();
                areas.Clear();
                locNum += 1;
                if (locNum <= allLocation.GetLength(0)-1) { goToLoc(); }
                else { locNum = allLocation.GetLength(0) - 1; GTA.UI.Notification.Show("Location End!!"); }
            }
            if (e.KeyCode == Keys.T)
            {
                deleteAllPed();
                areas.Clear();
                locNum -= 1;
                if (locNum >=0) { goToLoc(); }
                else { locNum = 0; GTA.UI.Notification.Show("Location End!!"); }
            }
            #endregion
            #region camera control
            if (e.KeyCode==Keys.W)
            {
                if ((scriptStatus== "colDroneLoc")&CameraMode)
                {
                    camDelta.X = 1;
                    IsMovement = true;
                }
            }
            if (e.KeyCode == Keys.A)
            {
                if ((scriptStatus == "colDroneLoc") & CameraMode)
                {
                    camDelta.Y = -1;
                    IsMovement = true;
                }
                
            }
            if (e.KeyCode == Keys.S)
            {
                if ((scriptStatus == "colDroneLoc") & CameraMode)
                {
                    camDelta.X = -1;
                    IsMovement = true;
                }
            }
            if (e.KeyCode == Keys.D)
            {
                if ((scriptStatus == "colDroneLoc") & CameraMode)
                {
                    camDelta.Y = 1;
                    IsMovement = true;
                }
            }
            if (e.KeyCode == Keys.Q)
            {
                if (((scriptStatus == "colDroneLoc")||(scriptStatus == "denseScenseCreate"))& CameraMode)
                {
                    camDelta.Z = 0.3F;
                    IsMovement = true;
                }
            }
            if (e.KeyCode == Keys.E)
            {
                if (((scriptStatus == "colDroneLoc") || (scriptStatus == "denseScenseCreate")) & CameraMode)
                {
                    camDelta.Z = -0.3F;
                    IsMovement = true;
                }
            }
            if (e.KeyCode == Keys.Oemplus)
            {
                if ((scriptStatus == "colDroneLoc") & CameraMode)
                {
                    nfov = 1.0F;
                    IsMovement = true;
                }
            }
            if (e.KeyCode == Keys.OemMinus)
            {
                if ((scriptStatus == "colDroneLoc") & CameraMode)
                {
                    nfov = -1.0F;
                    IsMovement = true;
                }
            }
            if (e.KeyCode == Keys.Z)
            {
                if ((scriptStatus == "colDroneLoc") || (scriptStatus == "denseScenseCreate")) 
                {
                    cameraSpeedFactor += 0.1f;
                }
            }
            if (e.KeyCode == Keys.X)
            {
                if ((scriptStatus == "colDroneLoc") || (scriptStatus == "denseScenseCreate"))
                {
                    cameraSpeedFactor -= 0.1f;
                    if (cameraSpeedFactor <=0)
                    { cameraSpeedFactor = 0.1f; }
                }
            }
            if (e.KeyCode==Keys.P)
            {
                cameraSpeedFactor += 0.1f;
            }
            if (e.KeyCode == Keys.N)
            {
                //indx += 1;
                //if (indx >= 24)//allWeather.GetLength(0))
                //    { indx = 0; }

                Function.Call(Hash.SET_CLOCK_TIME, 5, 0, 0);
                GTA.World.Weather = Weather.Clear;//allWeather[indx];
                //GTA.UI.Notification.Show(indx.ToString());

            }
            #endregion
        }
        #region capture scense
        public void Init()
        {   //some init operations
            if (!Directory.Exists(dataDir))
            {Directory.CreateDirectory(dataDir);} // create data dir
             logger = dataDir + "/" + logger;
            Log.WriteLog(logger, "\n\n ---NewTry---");
            Log.WriteLog(logger, "Logfile created!!!"); // create log file
            //load capture locations
            locNum = 0; 
            locFile = dataDir + "/" + locFile;
            Log.WriteLog(logger, "Location file is: "+locFile);
            StreamReader sr = new StreamReader(File.OpenRead(locFile), Encoding.Default);
            string line = sr.ReadLine();
            while (line != null)
            {
                line = line.Trim();
                //x,y,z,angx,angy,angz,fov,id
                string[] split = line.Split(new char[] {' '});
                allLocation[locNum].coo.X = (float)Convert.ToDouble(split[0]);
                allLocation[locNum].coo.Y = (float)Convert.ToDouble(split[1]);
                allLocation[locNum].coo.Z = (float)Convert.ToDouble(split[2]);
                allLocation[locNum].ang.X = (float)Convert.ToDouble(split[3]);
                allLocation[locNum].ang.Y = (float)Convert.ToDouble(split[4]);
                allLocation[locNum].ang.Z = (float)Convert.ToDouble(split[5]);
                allLocation[locNum].fov = (float)Convert.ToDouble(split[6]);
                allLocation[locNum].id = Convert.ToInt32(split[7]);
                //next line
                line = sr.ReadLine();
                locNum += 1;
            }
           locTotalNum = locNum;
            Log.WriteLog(logger, "Location Total Number is: " + locTotalNum.ToString());
            locNum = 0;
            // load start location, start sindw and sindt
            sr = new StreamReader(File.OpenRead(dataDir + "/" + "startind.txt"), Encoding.Default);
            line = sr.ReadLine();
            sr.Close();
            line = line.Trim();
            Log.WriteLog(logger, line);
            string[] split1 = line.Split(new char[] { ' ' });
            sloc = (int)Convert.ToInt16(split1[0]);
            sindw = (int)Convert.ToInt16(split1[1]);
            sindt = (int)Convert.ToInt16(split1[2]);
            Log.WriteLog(logger, "sloc: " + sloc.ToString() + "sindw: " + sindw.ToString() + "sindt: " + sindt.ToString());
            Log.WriteLog(logger, "**************Init Finish!**************");
            isInit = true;
        }
        public void CaptureCurrentLocation(string savePath)
        {
            //capture the complete video frame with one location
            int counter = 0;
            string saveName = "";
            SetCam();
            //set weather and datetime
            curDateTime = curDateTime.AddMinutes(-30);
            GTA.World.Weather = curWeather;//allWeather[indx];
            Function.Call(Hash.SET_CLOCK_TIME, curDateTime.Hour, curDateTime.Minute, curDateTime.Second);
            Wait(60000);// wait for weather and time  one minut in the real world is half hour in the game
            GTA.World.Weather = curWeather;//allWeather[indx];
            Log.WriteLog(logger, "Capture started with game time: "+GTA.World.CurrentDate.ToString());
            while (counter<frameNum)
            {
                saveName = savePath + "/"+currentLoc.id.ToString()+"-"+counter.ToString();
                Game.Pause(true);
                CaptureInfos(saveName);
                CaptureFigures(saveName);
                Game.Pause(false);
                counter += 1;
                Wait(interTime);
            }
            
        }
        public void SetCam()
        {
            ResetCam();
            // place the palyer
            Ped myPed = Function.Call<Ped>(Hash.PLAYER_PED_ID);
            //Vector3 playerLoc = Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_IN_WORLD_COORDS, myPed, 0.0, 0.0, 0.0);
            Function.Call(Hash.SET_ENTITY_COORDS_NO_OFFSET, myPed, currentLoc.coo.X, currentLoc.coo.Y, currentLoc.coo.Z, 0, 0, 1);// outdoor
            Wait(20000);
            //hide the player PED????
            Function.Call(Hash.SET_ENTITY_COORDS_NO_OFFSET, myPed, currentLoc.coo.X+50, currentLoc.coo.Y+50, 500, 0, 0, 1);// outdoor
            Wait(10000);

            // create camera
            World.DestroyAllCameras();
            capCam = World.CreateCamera(currentLoc.coo, currentLoc.ang, currentLoc.fov);
            Wait(100);
            Function.Call(Hash.STOP_CAM_POINTING, capCam);
            Function.Call(Hash.RENDER_SCRIPT_CAMS, true, 0, 3000, 1, 0);
            Wait(1000);
            //Log.WriteLog(logger, "Set Camera for current finished!!");

        }
        public void ResetCam()
        {   // funciton to reset the camera
            World.DestroyAllCameras();
            Function.Call(Hash.RENDER_SCRIPT_CAMS, false, 1, 2000, 1, 0);
            CameraMode = false;
            //Log.WriteLog(logger, "Camera Reset!!!");
            Wait(2000);
            
        }

        public void CaptureInfos(string saveName)
        {
            saveName = saveName + ".xml";
            //******************* Step 1 Process all Peds *******************
            //Log.WriteLog(logger, "Start PED info collection into file" + saveName);
            // capture the target location and other information for current scense
            Ped[] allPed = World.GetNearbyPeds(currentLoc.coo, 2000);
            int totalNum = allPed.GetLength(0);
            for (int i =0;i<totalNum; i++)
            {
                Log.WriteXml(saveName, GetPedInfo(allPed[i],i));

            }
            //******************* Step2  Process all VEHICLEs *******************
            //Log.WriteLog(logger, "Start VEHICLEs info collection into file" + saveName);
            Vehicle curVehicle;
            Vehicle[] allVehicle = World.GetNearbyVehicles(currentLoc.coo, 2000);
            totalNum = allVehicle.GetLength(0);
            for (int i =0;i<totalNum;i++)
            {
                curVehicle = allVehicle[i];
                // catch vehicle info
                Log.WriteXml(saveName, GetVehicleInfo(curVehicle,i));
                // catch all occupants inside the vehicle
                Ped[] Occupants = curVehicle.Occupants;
                for (int j = 0; j < Occupants.GetLength(0); j++)
                {
                    Log.WriteXml(saveName, GetPedInfo(Occupants[j], j));
                }
            }
        }

        public string Vector3ToString(Vector3 vec)
        {
            PointF position = GTA.UI.Screen.WorldToScreen(vec);
            return "x = " + position.X.ToString() + " y = " + position.Y.ToString();
        }
        public string GetVehicleInfo(Vehicle curVehicle,int index)
        {
            string vehicleInfo = "";
            //PointF position = GTA.UI.Screen.WorldToScreen(curVehicle.Position);

            vehicleInfo = "<vehicle Num=" + index.ToString() + ">\n";
            vehicleInfo += "\t<MemAddr " + curVehicle.MemoryAddress.ToString() + " />\n";
            vehicleInfo += "\t<Position " + Vector3ToString(curVehicle.Position) + " />\n";
            vehicleInfo += "\t<IsVisible " + Function.Call<bool>(Hash.IS_ENTITY_VISIBLE, curVehicle).ToString() + "/>\n";
            vehicleInfo += "\t<IsOccluded " + Function.Call<bool>(Hash.IS_ENTITY_OCCLUDED, curVehicle).ToString() + "/>\n";
            vehicleInfo += "\t<IsOnScreen " + Function.Call<bool>(Hash.IS_ENTITY_ON_SCREEN, curVehicle).ToString() + "/>\n";
            vehicleInfo += "\t<Class " + curVehicle.ClassType.ToString() + " />\n";
            vehicleInfo += "\t<DisplayName " + curVehicle.DisplayName + " />\n";
            vehicleInfo += "\t<LocalizedName " + curVehicle.LocalizedName + " />\n";
            vehicleInfo += "\t<ClassDisplayName " + curVehicle.ClassDisplayName + " />\n";
            vehicleInfo += "\t<ClassLocalizedName " + curVehicle.ClassLocalizedName + " />\n";
            vehicleInfo += "\t<LeftPosition " + Vector3ToString(curVehicle.LeftPosition) + " />\n";
            vehicleInfo += "\t<RightPosition " + Vector3ToString(curVehicle.RightPosition) + " />\n";
            vehicleInfo += "\t<RearPosition " + Vector3ToString(curVehicle.RearPosition) + " />\n";
            vehicleInfo += "\t<FrontPosition " + Vector3ToString(curVehicle.FrontPosition) + " />\n";
            vehicleInfo += "</vehicle>\n";

            return vehicleInfo;
        }

        public string GetPedInfo(Ped curPed, int index,string note="")
        {
            string pedInfo = "";
            const int headCode = 0x796E;
            Vector3 boneLoc = Function.Call<Vector3>(Hash.GET_PED_BONE_COORDS, curPed, headCode, 0, 0, 0);
            //如何判断是否是同一个ped？？ ped = ped ？  或者使用 ped.MemoryAddress??  ped.euqals ????
            PointF head;
            head = GTA.UI.Screen.WorldToScreen(boneLoc);

            pedInfo = "<ped Num=" + index.ToString() + ">\n";
            pedInfo += "\t<MemAddr " + curPed.MemoryAddress.ToString() + " />\n";
            pedInfo += "\t<Head x=" + head.X.ToString() + " y=" + head.Y.ToString() + " />\n";
            pedInfo += "\t<IsInVehicle " + Function.Call<bool>(Hash.IS_PED_IN_ANY_VEHICLE, curPed) + "/>\n";
            pedInfo += "\t<IsVisible " + Function.Call<bool>(Hash.IS_ENTITY_VISIBLE, curPed).ToString() + "/>\n";
            pedInfo += "\t<IsOccluded " + Function.Call<bool>(Hash.IS_ENTITY_OCCLUDED, curPed).ToString() + "/>\n";
            pedInfo += "\t<GroupIndex " + Function.Call<int>(Hash.GET_PED_GROUP_INDEX, curPed, false).ToString() + "/>\n";
            pedInfo += "\t<IsOnScreen " + Function.Call<bool>(Hash.IS_ENTITY_ON_SCREEN, curPed).ToString() + "/>\n";
            string vecaddr ="";
            if (Function.Call<bool>(Hash.IS_ENTITY_ATTACHED_TO_ANY_VEHICLE, curPed))
            { Vehicle veh = Function.Call<Vehicle>(Hash._FIND_VEHICLE_CARRYING_THIS_ENTITY, curPed);
                vecaddr= veh.MemoryAddress.ToString(); }
            else { vecaddr = "None"; }
            pedInfo += "\t<VehicleIsOn " + vecaddr + " />\n";
            //pedInfo += "\t<IsInVehicle " + curPed.IsInVehicle.ToString() + "/>\n";
            //pedInfo += "\t<Group "+ curPed.PedGroup.ToString() +"/>\n";
            //pedInfo += "\t<IsVisible " + curPed.IsVisible.ToString() + "/>\n";
            //pedInfo += "\t<IsOccluded " + curPed.IsOccluded.ToString() + "/>\n";
            pedInfo += "\t<LeftPosition " + Vector3ToString(curPed.LeftPosition) + " />\n";
            pedInfo += "\t<RightPosition " + Vector3ToString(curPed.RightPosition) + " />\n";
            pedInfo += "\t<RearPosition " + Vector3ToString(curPed.RearPosition) + " />\n";
            pedInfo += "\t<FrontPosition " + Vector3ToString(curPed.FrontPosition) + " />\n";
            pedInfo += "\t<Note "+note+"/>\n";
            pedInfo += "</ped>\n";

            return pedInfo;
        }
        // capture current frame include scnense
        public void CaptureFigures(string saveName)
        {
            Wait(100);
            var depth = VisionNative.GetDepthBuffer();
            var stencil = VisionNative.GetStencilBuffer();
            var screen = VisionNative.GetColorBuffer();
            while ((depth==null)||(stencil==null)||(screen ==null))
            {
                depth = VisionNative.GetDepthBuffer();
                stencil = VisionNative.GetStencilBuffer();
                screen = VisionNative.GetColorBuffer();
            }
            //var res = Game.ScreenResolution;
            string tempPath = saveName + "-depth.raw";
            fileIO.Image.Write(tempPath, depth);
            tempPath = saveName + "-stencil.raw";
            fileIO.Image.Write(tempPath, stencil);
            tempPath = saveName + "-screen.raw";
            fileIO.Image.Write(tempPath, screen);
            //Log.WriteLog(logger, "Figures captured!!!");
        }
        public void StartNewCamera(string locFile)
        {
            //locFile //= "_DroneLocCol_Figure.txt";
            if (!isInit)
            { Init(); }
            Log.WriteLog(logger, "Start Collect Drone Location!!");
            ResetCam();
            Ped myPed = Function.Call<Ped>(Hash.PLAYER_PED_ID);
            Vector3 startLocation = Function.Call<Vector3>(Hash.GET_ENTITY_COORDS, myPed, true);
            Vector3 startAng = Function.Call<Vector3>(Hash.GET_ENTITY_COORDS, myPed, true);
            startAng.X = -90; startAng.Y = -90; startAng.Z = 0;
            cCam = World.CreateCamera(startLocation, startAng, 50);
            Wait(100);
            Function.Call(Hash.STOP_CAM_POINTING, cCam);
            Function.Call(Hash.RENDER_SCRIPT_CAMS, true, 0, 3000, 1, 0);
            CameraMode = true;
        }
        public void AdjustCam()
        {
            if (viewNext)
            {
                if (locNum<locTotalNum)
                {
                    currentLoc = allLocation[locNum];
                    cCam = World.CreateCamera(currentLoc.coo, currentLoc.ang, currentLoc.fov);
                    Function.Call(Hash.STOP_CAM_POINTING, cCam);
                    Function.Call(Hash.RENDER_SCRIPT_CAMS, true, 0, 3000, 1, 0);
                    viewNext = false;
                    locNum += 1;
                }
                else { GTA.UI.Notification.Show("Final Location!"); viewNext = false; }
            }
            // disable controls
            int[] disabledControls =  {0,2,3,4,5,6,16,17,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,44,45,71,72,75,140,141,142,143,156,243,257,261,262,263,264,267,268,269,270,271,272,273};
            for (int i=0;i<disabledControls.GetLength(0);i++)
            {
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, disabledControls[i], 1);
            }
            // move the camera
            Vector3 camNewPos = cCam.Position;
            float fov = cCam.FieldOfView;
            Vector3 camRot = cCam.Rotation;
            Vector3 camDir = RotationToDirection(camRot);
            
            if (camDelta.X!=0.0f)
            {
                camNewPos.X += camDir.X * camDelta.X * cameraSpeedFactor;
                camNewPos.Y += camDir.Y * camDelta.X * cameraSpeedFactor;
                camNewPos.Z += camDir.Z * camDelta.X * cameraSpeedFactor;
            }
            if (camDelta.Y != 0.0f)
            {
                Vector3 b = new Vector3(0, 0, 0);
                b.Z = 1.0f;
                Vector3 sideWays = crossProduct(camDir, b);
                camNewPos.X += camDir.X * camDelta.X * cameraSpeedFactor;
                camNewPos.Y += camDir.Y * camDelta.X * cameraSpeedFactor;
            }
            if(camDelta.Z!=0.0f)
            {
                camNewPos.Z += camDelta.Z * cameraSpeedFactor;
            }
            if(nfov!=0.0)
            {
                fov += nfov;
            }
            Function.Call(Hash.SET_CAM_COORD, cCam, camNewPos.X, camNewPos.Y, camNewPos.Z);
            Function.Call(Hash.SET_CAM_FOV, cCam, fov);
            //rotation
            float rightAxisX = Function.Call<float>(Hash.GET_DISABLED_CONTROL_NORMAL, 0, 220);
            float rightAxisY = Function.Call<float>(Hash.GET_DISABLED_CONTROL_NORMAL, 0, 221);
            if (rightAxisX != 0.0 || rightAxisY != 0.0)
            {
                //Rotate camera - Multiply by sensitivity settings
                camRot = cCam.Rotation;
                camRot.X += rightAxisY * -5.0f;
                camRot.Z += rightAxisX * -10.0f;
                Function.Call(Hash.SET_CAM_ROT, cCam, camRot.X, camRot.Y, camRot.Z,2);
            }
            // clear move information
            camDelta = new Vector3(0, 0, 0);
            nfov = 0;
        }

        public Vector3 RotationToDirection(Vector3 rotation)
        {
            double retZ = rotation.Z * 0.01745329f;
            double retX = rotation.X * 0.01745329f;
            double absX = Math.Abs(Math.Cos(retX));
            return new Vector3((float)-(Math.Sin(retZ) * absX), (float)(Math.Cos(retZ) * absX), (float)Math.Sin(retX));
        }
        public Vector3 crossProduct(Vector3 a, Vector3 b)
        {
            Vector3 retVector= new Vector3(0, 0, 0);
            retVector.X = 0.0f;retVector.Y = 0.0f;retVector.Z = 0.0f;
            retVector.X = a.Y * b.Z - a.Z * b.Y;
            retVector.Y = a.Z * b.X - a.X * b.Z;
            retVector.Z = a.X * b.Y - a.Y * b.X;
            return retVector;
        }
        #endregion

        #region denseScenseCreate
        public void goToLoc()
        {
            ResetCam();
            currentLoc = allLocation[locNum];
            Ped myPed = Function.Call<Ped>(Hash.PLAYER_PED_ID);
            Function.Call(Hash.SET_ENTITY_COORDS_NO_OFFSET, myPed, currentLoc.coo.X, currentLoc.coo.Y, currentLoc.coo.Z, 0, 0, 1);// outdoor
        }

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
        public bool initAnimation(string fileName)
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
            sr.Close();
            Log.WriteLog(logger, "Finish load anim");
            return true;
        }

        public bool putCrowd(int num)
        {
            Game.Pause(true);
            int areasNum = areas.ToArray().GetLength(0) % 4;
            if (areasNum != 0) { return false; }
            areasNum = (int)areas.ToArray().GetLength(0) / 4;
            //Log.WriteLog(logger, "areasNum: "+areasNum.ToString()+ ",areas.Length: "+ areas.ToArray().Length.ToString());
            int areaId = 0;
            Random rd = new Random();
            Ped myPed = Function.Call<Ped>(Hash.PLAYER_PED_ID);
            Vector3 loc = Function.Call<Vector3>(Hash.GET_ENTITY_COORDS, myPed, true);
            //float avgz = loc.Z + 0.5f;
            float x = 0, y = 0;
            //Log.WriteLog(logger, "Start Put Crowd!!");
            //GTA.UI.Notification.Show(World.PedCapacity.ToString());
            for (int i = 0; i < num; i++)
            {
                areaId = rd.Next(0, areasNum) * 4;
                Vector3[] area = { areas[areaId], areas[areaId + 1], areas[areaId + 2], areas[areaId + 3] };
                float sx = 0, sy = 0, dx = 0, dy = 0;
                getBoard(area, ref sx, ref sy, ref dx, ref dy);
                //string info = "areaId: " + areaId.ToString() + "sx: " + sx.ToString() + "sy: " + sy.ToString() + "dx: " + dx.ToString() + "dy: " + dy.ToString();
                //Log.WriteLog(logger, info);
                x = rd.Next((int)sx, (int)dx);
                y = rd.Next((int)sy, (int)dy);
                uint pedHash = pedHashes[rd.Next(0, pedHashes.GetLength(0))];
                float heading = 360.0f * rd.Next(0, 10000) / 10000;
                Animation anim = allAnimation[rd.Next(0, animLen)];
                //info = "x: " + x.ToString() + ",y: " + y.ToString()+",pedHash: "+pedHash.ToString()+",avgz: "+avgz.ToString()+",heading: "+heading.ToString();
                //Log.WriteLog(logger, info);
                // put crowd
                Ped tempPed = World.CreateRandomPed(new Vector3 { X = x, Y = y, Z = avgz });
                //Function.Call(Hash.REQUEST_MODEL, pedHash);
                //while (!Function.Call<bool>(Hash.HAS_MODEL_LOADED, pedHash)) { Wait(0); }
                //Ped tempPed = Function.Call<Ped>(Hash.CREATE_PED, 4, pedHash, x, y, avgz, heading, false, true);
                //Function.Call(Hash.SET_MODEL_AS_NO_LONGER_NEEDED, pedHash);
                //bool ex = Function.Call<bool>(Hash.DOES_ANIM_DICT_EXIST, anim.animLibrary);
                //Log.WriteLog(logger, ex.ToString());
                //if (ex)
                //{
                //    Function.Call(Hash.REQUEST_ANIM_DICT, anim.animLibrary);
                //    while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, anim.animLibrary)) { Wait(0); }
                //}
                ////Wait(50);
                if (tempPed != null)
                { //tempPed.Task.WanderAround();
                    allPed.Add(tempPed);
                }
                //Function.Call(Hash.TASK_PLAY_ANIM, tempPed, anim.animLibrary, anim.animName, 8.0f, -8.0f, anim.duration, true, 8.0f, 0, 0, 0);
                //Wait(50);
            }

            Log.WriteLog(logger, "Finish Put Crowd!!");
            Game.Pause(false);
            return true;
        }
        public void getBoard(Vector3[] area, ref float sx, ref float sy, ref float dx, ref float dy)
        {
            //float sx, sy, dx, dy;
            sx = getMin(new float[] { area[0].X, area[1].X, area[2].X, area[3].X });
            sy = getMin(new float[] { area[0].Y, area[1].Y, area[2].Y, area[3].Y });
            dx = getMax(new float[] { area[0].X, area[1].X, area[2].X, area[3].X });// - sx;
            dy = getMax(new float[] { area[0].Y, area[1].Y, area[2].Y, area[3].Y });// - sy;
            //return sx,sy,dx,dy;
        }
        public float getMax(float[] array)
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
        public float getMin(float[] array)
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

        public void startCapDenseScense()
        {
            //make save dir
            string savePath = GTA.World.CurrentDate.ToString("HH_mm");
            savePath = savePath.Replace(' ', '-').Replace(':', '-').Replace('/', '-');
            savePath = "LOC-" + currentLoc.id.ToString() + "-" + savePath + "-" + World.Weather.ToString()+"-Dense";
            savePath = dataDir + "/" + savePath;
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
                //capture the complete video frame with current location
                int counter = 0;
                string saveName = "";
                Log.WriteLog(logger, "Capture started with game time: " + GTA.World.CurrentDate.ToString());
                while (counter < frameNum)
                {
                    saveName = savePath + "/" + currentLoc.id.ToString() + "-" + counter.ToString();
                    Game.Pause(true);
                    CaptureInfos(saveName);
                    CaptureFigures(saveName);
                    Game.Pause(false);
                    counter += 1;
                    Wait(interTime);
                }
            }
            Log.WriteLog(logger, "Finish Capture!!!");
        }
        public void deleteAllPed()
        {
            if ((scriptStatus == "denseScenseCreate")&(allPed.ToArray().GetLength(0)>0))
            {
                for (int i = 0; i < allPed.ToArray().GetLength(0); i++)
                {
                    allPed[i].Delete();
                }
            }
            Ped[] alllPed = World.GetNearbyPeds(currentLoc.coo, 2000);
            for (int i = 0; i < alllPed.GetLength(0); i++)
            {
                alllPed[i].Delete();
            }
        }
        #endregion
    }

}