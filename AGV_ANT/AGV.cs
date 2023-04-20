using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RestSharp;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Net;
using System.IO;
using System.Data;
using System.Web;

namespace ANT_DLL
{
    public class JH_AGV_DLL
    {

        #region "1. AGV Login"

        public String AGV_Login(String URL, String USER, String Pass)
        {
            RestClient client = new RestClient("http://"+ URL.ToString());

            RestRequest request = new RestRequest("/wms/rest/login", Method.POST);

            request.Method = Method.POST;

            var body = new
            {
                username = USER.ToString(),
                password = Pass.ToString(),
                apiVersion = new
                {
                    major = "0",
                    minor = "2"
                }
            };
            request.AddJsonBody(body);

            var response = client.Execute(request);

            Console.WriteLine(response.Content);

            var parseJson = JObject.Parse(response.Content);

            var VarId = parseJson["token"].ToString();      // 세션토큰

            return VarId.ToString();
        }
        #endregion

        #region "2. AGV Get Mission : 미션상태 정보 조회 QueryField  0 :전부, 1: 활성, 2: 실행중, 3: 종료됨, 4: 거절됨, 5: 취소됨, 6: 실종됨 "

        public DataTable AGV_GetMission(String URL, String Ver, String VarId,int version, String DataRange, String OrderFiled, String QDay, string QueryField)
        {
            const string quote = "\"";

            string _RETAPI = "http://" + URL.ToString() + "/wms/rest/" + Ver.ToString() + "/missions";

            string sDataRange = "";

            if (QueryField.ToString() == "")
            {
                sDataRange = "";
            }
            else
            {
                sDataRange = "&datarange=%5B0%2C" + DataRange.ToString() + "%5D";
            }



            string sQueryField = "";


            if (QueryField.ToString() == "")
            {
                sQueryField = "";
            }
            else
            {
                sQueryField = "&dataselection:%7B%22criteria%2%3A%5B%22navigationstate%3A%3Aint%20IN%3A" + QueryField.ToString() + "%22%2C%22age%3A%3Aint%20IN%3A0%7C1%22%5D%2C%22composition%22%3A%22AND%22%7D";
            }

            string test = "";

            test = _RETAPI;

            WebRequest request2 = WebRequest.Create(_RETAPI);

            request2.Headers.Add("Authorization", "Bearer " + VarId.ToString());

            string requestResult = "";

            using (var response = request2.GetResponse())
            {
                using (Stream dataStream = response.GetResponseStream())
                {
                    var reader = new StreamReader(dataStream);
                    requestResult = reader.ReadToEnd();
                }
            }

            agvMission.Root record = JsonConvert.DeserializeObject<agvMission.Root>(requestResult);

            DataTable dt = new DataTable();

            dt.Columns.Add("missionid", typeof(string));
            dt.Columns.Add("fromnode", typeof(string));
            dt.Columns.Add("tonode", typeof(string));
            dt.Columns.Add("dispatchtime", typeof(string));
            dt.Columns.Add("arrivingtime", typeof(string));
            dt.Columns.Add("state", typeof(string));

            foreach (var item in record.payload.missions)
            {
                List<string> arr = new List<string>();

                //   arr.Add(Convert.ToString(item.missionid));
                if (item.missionid != null)
                {
                    arr.Add(item.missionid);
                }
                else
                {
                    arr.Add("");
                }

                if (item.fromnode != null)
                {
                    arr.Add(item.fromnode);
                }
                else
                {
                    arr.Add("");
                }

                //arr.Add(item.fromnode);
                if (item.tonode != null)
                {
                    arr.Add(item.tonode);
                }
                else
                {
                    arr.Add("");
                }
                // arr.Add(item.tonode);

                // arr.Add(item.dispatchtime);

                if (item.dispatchtime != null)
                {
                    arr.Add(item.dispatchtime);
                }
                else
                {
                    arr.Add("");
                }


                //arr.Add(item.arrivingtime);

                if (item.arrivingtime != null)
                {
                    arr.Add(item.arrivingtime);
                }
                else
                {
                    arr.Add("");
                }

                // arr.Add(item.transportstate);

                string strstate = "";
                if (item.transportstate != null)
                {

                    switch (item.transportstate)
                    {
                        case 0:
                            strstate = "New.";
                            break;
                        case 1:
                            strstate = "Accepted.";
                            break;
                        case 2:
                            strstate = "Rejected.";
                            break;
                        case 3:
                            strstate = "Assigned.";
                            break;
                        case 4:
                            strstate = "Moving.";
                            break;
                        case 5:
                            strstate = "Transporting to selector.";
                            break;
                        case 6:
                            strstate = "Selecting delivery from start.";
                            break;
                        case 7:
                            strstate = "Delivering.";
                            break;
                        case 8:
                            strstate = "Terminated.";
                            break;
                        case 9:
                            strstate = "Cancelled.";
                            break;
                        case 10:
                            strstate = "Error.";
                            break;
                        case 11:
                            strstate = "Cancelling.";
                            break;
                        case 12:
                            strstate = "Selecting pick up node.";
                            break;
                        case 13:
                            strstate = "Selecting delivery from selector.";
                            break;
                        case 14:
                            strstate = "Moving to departure selector.";
                            break;
                    }

                }
                else
                {
                    arr.Add("");
                }

                arr.Add(strstate);

                dt.Rows.Add(arr[0], arr[1], arr[2], arr[3], arr[4], arr[5]);

            }

            return dt;

        }
        #endregion

        #region "3. AGV Mission"

        public String AGV_OnlyMission(String URL, String Ver, String VarId, String AGVNm, String UserID, String missiontype, String PayLoad, String toNode)
        {
            RestClient client = new RestClient("http://" + URL.ToString());

            RestRequest request = new RestRequest("/wms/rest/" + Ver.ToString() + "/missions", Method.POST);

            request.AddHeader("Authorization", "Bearer " + VarId.ToString());

            request.Method = Method.POST;


            var body = new
            {
                missionrequest = new
                {
                    requestor = UserID.ToString(),
                    missiontype = missiontype.ToString(),
                    fromnode = "",
                    tonode = toNode.ToString(),
                    cardinality = "1",
                    priority = 3,
                    parameters = new
                    {
                        value = new
                        {
                            payload = PayLoad.ToString(),
                            vehicle = AGVNm.ToString()
                        },
                        desc = "Mission extension",
                        type = "org.json.JSONObject",
                        name = "parameters"

                    }

                }
            };


            request.AddJsonBody(body);

            var response = client.Execute(request);

            Console.WriteLine(response.Content);

            var parseJson = JObject.Parse(response.Content);

            var retcode = parseJson["retcode"].ToString();

            String missioinId = string.Empty;

            if (retcode.ToString() == "0")
            {

                missioinId = parseJson["payload"]["acceptedmissions"].ToString();
                missioinId = missioinId.Replace("\"", "").Replace("[", "").Replace("]", "").Replace(" ", "");
            }
            else
            {
                missioinId = "NO";
            }

            return missioinId.ToString();
        }
        #endregion

        #region "4. AGV Mission Create (from-to)"

        public String AGV_Move_Mission(String URL, String Ver, String VarId, String AGVNm, String UserID, string missiontype, String PayLoad, String FromNode, String toNode)
        {
            RestClient client = new RestClient("http://" + URL.ToString());

            //RestRequest request = new RestRequest("/wms/rest/missions", Method.POST);
            RestRequest request = new RestRequest("/wms/rest/" + Ver.ToString() + "/missions", Method.POST);

            request.AddHeader("Authorization", "Bearer " + VarId.ToString());

            request.Method = Method.POST;


            var body = new
            {
                missionrequest = new
                {
                    requestor = UserID.ToString(),
                    missiontype = missiontype.ToString(),
                    fromnode = FromNode.ToString(),
                    tonode = toNode.ToString(),
                    cardinality = "1",
                    priority = "3",
                    parameters = new
                    {
                        value = new
                        {
                            //payload = PayLoad.ToString(),
                            vehicle = ""
                        },
                        desc = "Mission extension",
                        type = "org.json.JSONObject",
                        name = "parameters"

                    }

                }
            };

            //Serializes obj to JSON format and adds it to the request body.
            request.AddJsonBody(body);

            var response = client.Execute(request);

            var parseJson = JObject.Parse(response.Content);

            var retcode = parseJson["retcode"].ToString();

            String missioinId = string.Empty;

            if (retcode.ToString() == "0")
            {

                missioinId = parseJson["payload"]["acceptedmissions"].ToString();
                missioinId = missioinId.Replace("\"", "").Replace("[", "").Replace("]", "").Replace(" ", "");

            }
            else
            {
                missioinId = "NO";

            }

            return missioinId.ToString();
        }
        #endregion

        #region "5. AGV Get Mission id : 미션 id 상태 조회"

        public DataTable AGV_GetMissionId(String URL, String Ver, String VarId , String sID)
        {

            const string quote = "\"";

            string _RETAPI = "http://" + URL + "/wms/rest/" + Ver + "/missions/" + sID;
                        
            WebRequest request = WebRequest.Create(_RETAPI);

            request.Headers.Add("Authorization", "Bearer " + VarId);

            string requestResult = "";

            using (var response = request.GetResponse())
            {
                using (Stream dataStream = response.GetResponseStream())
                {
                    var reader = new StreamReader(dataStream);
                    requestResult = reader.ReadToEnd();
                }
            }

            agvMission.Root record = JsonConvert.DeserializeObject<agvMission.Root>(requestResult);

            DataTable dt = new DataTable();

            dt.Columns.Add("missionid", typeof(int));
            dt.Columns.Add("assignedto", typeof(string));
            dt.Columns.Add("fromnode", typeof(string));
            dt.Columns.Add("tonode", typeof(string));
            dt.Columns.Add("dispatchtime", typeof(string));
            dt.Columns.Add("arrivingtime", typeof(string));
            dt.Columns.Add("transportstate", typeof(string));
            dt.Columns.Add("state", typeof(string));


            foreach (var item in record.payload.missions)
            {
                List<string> arr = new List<string>();

                arr.Add(Convert.ToString(item.missionid));
                arr.Add(item.assignedto);
                arr.Add(item.fromnode);
                arr.Add(item.tonode);
                arr.Add(item.dispatchtime);
                arr.Add(item.arrivingtime);
                arr.Add(item.transportstate.ToString());

                string strstate = "";

                switch (item.transportstate)
                {
                    case 0:
                        strstate = "New.";
                        break;
                    case 1:
                        strstate = "Accepted.";
                        break;
                    case 2:
                        strstate = "Rejected.";
                        break;
                    case 3:
                        strstate = "Assigned.";
                        break;
                    case 4:
                        strstate = "Moving.";
                        break;
                    case 5:
                        strstate = "Transporting to selector.";
                        break;
                    case 6:
                        strstate = "Selecting delivery from start.";
                        break;
                    case 7:
                        strstate = "Delivering.";
                        break;
                    case 8:
                        strstate = "Terminated.";
                        break;
                    case 9:
                        strstate = "Cancelled.";
                        break;
                    case 10:
                        strstate = "Error.";
                        break;
                    case 11:
                        strstate = "Cancelling.";
                        break;
                    case 12:
                        strstate = "Selecting pick up node.";
                        break;
                    case 13:
                        strstate = "Selecting delivery from selector.";
                        break;
                    case 14:
                        strstate = "Moving to departure selector.";
                        break;
                }
                arr.Add(strstate);

                dt.Rows.Add(arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], arr[6], arr[7]);

            }

            return dt;

        }
        #endregion 

        #region "6. AGV STOP 0:성공, 1:실패"

        public Boolean AGV_STOP(String URL, String Ver, String VarId, String AGVnm)
        {
            RestClient client = new RestClient("http://" + URL.ToString());
            
            RestRequest request = new RestRequest("/wms/rest/" + Ver.ToString() + "/vehicles/" + AGVnm.ToString() + "/command", Method.POST);

            request.AddHeader("Authorization", "Bearer " + VarId.ToString());

            request.Method = Method.POST;

            var body = new
            {
                command = new
                {
                    name = "allowSendingMission",
                    args = new
                    {
                        allow = "false"
                    }

                }
            };

            request.AddJsonBody(body);

            var response = client.Execute(request);

            var parseJson = JObject.Parse(response.Content);

            var retcode = parseJson["retcode"].ToString();

            if (retcode.ToString() == "0")
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        #endregion

        #region "7. AGV RESUM 0:성공, 1:실패"

        public Boolean AGV_RESUM(String URL, String Ver, String VarId, String AGVnm)
        {
            RestClient client = new RestClient("http://" + URL.ToString());

            RestRequest request = new RestRequest("/wms/rest/" + Ver.ToString() + "/vehicles/" + AGVnm.ToString() + "/command", Method.POST);

            request.AddHeader("Authorization", "Bearer " + VarId.ToString());

            request.Method = Method.POST;

            var body = new
            {
                command = new
                {
                    name = "allowSendingMission",
                    args = new
                    {
                        allow = "ture"
                    }

                }
            };

            request.AddJsonBody(body);

            var response = client.Execute(request);

            var parseJson = JObject.Parse(response.Content);

            var retcode = parseJson["retcode"].ToString();

            if (retcode.ToString() == "0")
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        #endregion

        #region "8. AGV 추출 

        public Boolean AGV_Extract(String URL, String VarId, String AGVnm)
        {
            RestClient client = new RestClient("http://" + URL.ToString() + "/");

            RestRequest request = new RestRequest("wms/rest/vehicles/" + AGVnm.ToString() + "/command", Method.POST);

            request.AddHeader("Authorization", "Bearer " + VarId);

            request.Method = Method.POST;

            var body = new
            {
                command = new
                {
                    name = "extract",
                    args = new
                    {

                    }

                }
            };

            //Serializes obj to JSON format and adds it to the request body.
            request.AddJsonBody(body);

            var response = client.Execute(request);

            var parseJson = JObject.Parse(response.Content);

            var retcode = parseJson["retcode"].ToString();

            if (retcode.ToString() == "0")
            {
                return true;
            }
            else
            {
                return false;
            }

        }


        #endregion

        #region "9. AGV 배치 

        public Boolean AGV_Insert(String URL, String VarId, String AGVnm, String NodeId)
        {
            RestClient client = new RestClient("http://" + URL.ToString() + "/");

            RestRequest request = new RestRequest("wms/rest/vehicles/" + AGVnm.ToString() + "/command", Method.POST);

            request.AddHeader("Authorization", "Bearer " + VarId);

            request.Method = Method.POST;

            var body = new
            {
                command = new
                {
                    name = "insert",
                    args = new
                    {
                        nodeId = NodeId.ToString()
                    }

                }
            };

            request.AddJsonBody(body);

            var response = client.Execute(request);

            var parseJson = JObject.Parse(response.Content);

            var retcode = parseJson["retcode"].ToString();

            if (retcode.ToString() == "0")
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        #endregion

        #region "10. 미션 취소 "

        public Boolean AGV_DELETE(String URL, String Ver, String VarId, String MissionId)
        {
            RestClient client = new RestClient("http://" + URL.ToString());

            RestRequest request = new RestRequest("/wms/rest/" + Ver.ToString() +  "/missions/" + MissionId.ToString(), Method.DELETE);

            request.AddHeader("Authorization", "Bearer " + VarId);

            request.Method = Method.DELETE;

            var response = client.Execute(request);

            Console.WriteLine(response.Content);

            var parseJson = JObject.Parse(response.Content);

            var retcode = parseJson["retcode"].ToString();

            if (retcode.ToString() == "0")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region "11. Agv 상태 조회"
        public DataTable AGV_STATE(String URL, String Ver, String VarId, String AgvNm, int version)
        {
            const string quote = "\"";

            string _RETAPI = "http://" + URL.ToString() + "/wms/rest/"+ Ver.ToString() +"/vehicles";

            string AgvQ = "";
            if (AgvNm.ToString() == "")
            {
                AgvQ = "";
            }
            else
            {
                AgvQ = "&datasearchtoken=" + AgvNm.ToString();
            }

            WebRequest request2 = WebRequest.Create(_RETAPI);

            request2.Headers.Add("Authorization", "Bearer " + VarId);

            string requestResult2 = "";

            using (var response = request2.GetResponse())
            {
                using (Stream dataStream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(dataStream))
                    {
                        requestResult2 = reader.ReadToEnd();
                    }
                }
            }

            agvGetStateClass.Root record2 = JsonConvert.DeserializeObject<agvGetStateClass.Root>(requestResult2);


            DataTable dt = new DataTable();

            dt.Columns.Add("name", typeof(string));
            dt.Columns.Add("missionid", typeof(string));
            dt.Columns.Add("location", typeof(string));
            dt.Columns.Add("battery", typeof(string));
            dt.Columns.Add("state", typeof(string));


            foreach (var item in record2.payload.vehicles)
            {
                List<string> arr = new List<string>();

                arr.Add(Convert.ToString(item.name));
                //arr.Add(Convert.ToString(item.missionid));

                if (item.missionid != null)
                {
                    arr.Add(item.missionid);
                }
                else
                {
                    arr.Add("");
                }


                //arr.Add(Convert.ToString(item.location.currentnode.name));
                //arr.Add(item.state.BatteryInfo[0].ToString());
                //arr.Add(item.state.VehicleState[0].ToString());
                if (item.location != null)
                {
                    arr.Add(item.location.currentnode.name);
                }
                else
                {
                    arr.Add("");
                }

                if (item.state.BatteryInfo.Count != 0)
                {
                    arr.Add(item.state.BatteryInfo[0].ToString());
                }
                else
                {
                    arr.Add("");
                }

                if (item.state.VehicleState.Count != 0)
                {
                    arr.Add(item.state.VehicleState[0].ToString());
                }
                else
                {
                    arr.Add("");
                }

                dt.Rows.Add(arr[0], arr[1], arr[2], arr[3], arr[4]);

            }

            return dt;
        }

        #endregion

        #region "12. Agv 알람 조회 Order 필드 : createdat, "
        public DataTable AGV_GET_ALARMS(String URL, String Ver, String VarId, int version, String QDay, String DataRange,String DataSearchField, String DataSearchFieldType, String DataSearchLevel)
        {
            const string quote = "\"";

            string _RETAPI = "http://" + URL.ToString() + "/wms/rest/" + Ver + "/alarms?";

            string sDataRange = "";

            if (DataRange.ToString() == "")   // 정렬할 필드명
            {
                sDataRange = "";
            }
            else
            {
                sDataRange = DataRange.ToString();
            }

            string sDataSearchField = "";

            if (DataSearchField.ToString() == "")   // 정렬할 필드명
            {
                sDataSearchField = "";
            }
            else
            {
                sDataSearchField = "&dataorderby=%5B%5B%22" + DataSearchField.ToString();
            }

            string sDataSearchFieldType = "";

            if (DataSearchFieldType.ToString() == "")   // 정렬방식 desc, asc
            {
                sDataSearchFieldType = "asc";
            }
            else
            {
                sDataSearchFieldType = "desc" + sDataSearchFieldType.ToString();
            }

            string sDataSearchLevel = "";

            if (DataSearchLevel.ToString() == "")
            {
                sDataSearchLevel = "%22%3A%5B%5D%2C%22";
            }            
            else
            {
                sDataSearchLevel = "%22%3A%5B%22level%3A%3Atext+IN%3A" + DataSearchLevel.ToString() + "%22%2C%22sourcetype%3A%3Atext+IN%3Amission%7Cvehicle%7Cdevice%7Chardware%7Cstation%7Cserver%22%5D%2C%22";
            }

            string test = "";

            test = (_RETAPI + "&datarange=%5B0%2C26%5D" + sDataSearchField + "%22%2C%22" + sDataSearchFieldType + "%22%5D,%5B%22id%22%2C%22desc%22%5D%5D&" + "datasearchtoken=&dataselection=%7B%22criteria" + sDataSearchLevel + "composition%22%3A%22AND%22%7D");

            WebRequest request = WebRequest.Create(_RETAPI + "&datarange=%5B0%2C" + sDataRange + "%5D" + sDataSearchField + "%22%2C%22" + sDataSearchFieldType + "%22%5D,%5B%22id%22%2C%22desc%22%5D%5D&" + "datasearchtoken=&dataselection=%7B%22criteria" + sDataSearchLevel + "composition%22%3A%22AND%22%7D");             

            request.Headers.Add("Authorization", "Bearer " + VarId.ToString()) ;

            string requestResult = "";

            using (var response = request.GetResponse())
            {
                using (Stream dataStream = response.GetResponseStream())
                {
                    var reader = new StreamReader(dataStream);
                    requestResult = reader.ReadToEnd();
                }
            }

            alramClass.Root record = JsonConvert.DeserializeObject<alramClass.Root>(requestResult);

            DataTable dt = new DataTable();


            dt.Columns.Add("sourceid", typeof(string));
            dt.Columns.Add("eventname", typeof(string));
            dt.Columns.Add("timestamp", typeof(string));
            dt.Columns.Add("state", typeof(string));

            string err_name = "";

            foreach (var item in record.payload.alarms)
            {
                List<string> arr = new List<string>();

                if (item.sourceid != null)
                {
                    arr.Add(item.sourceid);
                }
                else
                {
                    arr.Add("");
                }

                if (item.eventname != null)
                {
                    arr.Add(item.eventname);
                }
                else
                {
                    arr.Add("");
                }
                if (item.timestamp != null)
                {
                    arr.Add(item.timestamp);
                }
                else
                {
                    arr.Add("");
                }
                if (item.state != null)
                {
                    switch (item.state)
                    {
                        case 0:
                            err_name = "에러활성중";
                            break;
                        case 1:
                            err_name = "승인됨(확인)";
                            break;
                        case 2:
                            err_name = "마감됨";
                            break;
                        case 3:
                            err_name = "해제됨";
                            break;

                    }

                    arr.Add(err_name);

                }
                else
                {

                    arr.Add("");

                }


                dt.Rows.Add(arr[0], arr[1], arr[2], arr[3]);

            }

            return dt;
        }
    }
    #endregion

    // class 

    class agvClass
    {

        public class Application2

        {
            public string ASversion { get; set; }
            public string ALversion { get; set; }
            public application3 application { get; set; }
            public string configuration { get; set; }
            public string name { get; set; }
            public string version { get; set; }

        }

        public class application3
        {
            public string name { get; set; }
            public string version { get; set; }
        }

        public class Role
        {
            public string name { get; set; }
            public string desc { get; set; }
        }

        public class Module
        {
            public List<string> urls { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public int state { get; set; }
        }

        public class Payload
        {
            public Application2 application { get; set; }
            public List<Role> roles { get; set; }
            public List<Module> modules { get; set; }
        }

        public class Root
        {
            public Payload payload { get; set; }
            public int retcode { get; set; }
        }
    }

    class agvGetStateClass
    {
        public class Args
        {
            public string nodeid { get; set; }
        }

        public class Action
        {
            public string sourceid { get; set; }
            public Args args { get; set; }
            public string name { get; set; }            
            public string sourcetype { get; set; }
        }

        public class State
        {
            [JsonProperty("body.shape", NullValueHandling = NullValueHandling.Ignore)]
            public List<string> BodyShape { get; set; }

            [JsonProperty("traffic.info", NullValueHandling = NullValueHandling.Ignore)]
            public List<string> TrafficInfo { get; set; }

            [JsonProperty("mission.progress", NullValueHandling = NullValueHandling.Ignore)]
            public List<object> MissionProgress { get; set; }

            [JsonProperty("connection.ok", NullValueHandling = NullValueHandling.Ignore)]
            public List<string> ConnectionOk { get; set; }

            [JsonProperty("error.bits")]
            public List<string> ErrorBits { get; set; }

            [JsonProperty("battery.info", NullValueHandling = NullValueHandling.Ignore)]
            public List<string> BatteryInfo { get; set; }

            [JsonProperty("vehicle.shape")]
            public List<string> VehicleShape { get; set; }

            [JsonProperty("vehicle.type")]
            public List<string> VehicleType { get; set; }

            [JsonProperty("lock.UUID")]
            public List<string> LockUUID { get; set; }

            [JsonProperty("vehicle.state", NullValueHandling = NullValueHandling.Include)]
            public List<string> VehicleState { get; set; }
            public List<object> messages { get; set; }

            [JsonProperty("lock.owner")]
            public List<string> LockOwner { get; set; }

            [JsonProperty("mission.info", NullValueHandling = NullValueHandling.Include)]
            public List<string> MissionInfo { get; set; }
            public List<object> errors { get; set; }

            [JsonProperty("battery.info.maxtemperature")]
            public List<string> BatteryInfoMaxtemperature { get; set; }

            [JsonProperty("sharedMemory.out")]
            public List<string> SharedMemoryOut { get; set; }

            [JsonProperty("sharedMemory.in")]
            public List<string> SharedMemoryIn { get; set; }
        }

        public class Currentnode
        {
            public string name { get; set; }
            public int id { get; set; }
        }

        public class Location
        {
            public List<string> coord { get; set; }
            public string course { get; set; }
            [JsonProperty("currentnode", NullValueHandling = NullValueHandling.Include)]
            public Currentnode currentnode { get; set; }
            public string map { get; set; }
            public string group { get; set; }
        }

        public class Vehicle
        {
            public bool coverage { get; set; }
            public string ipaddress { get; set; }
            public bool isloaded { get; set; }
            public string missionid { get; set; }
            public bool issimulated { get; set; }
            public string payload { get; set; }
            public int operatingstate { get; set; }
            [JsonProperty("name", Order = 1)]
            public string name { get; set; }
            public Action action { get; set; }
            public State state { get; set; }
            public DateTime timestamp { get; set; }
            public Location location { get; set; }
        }

        public class Payload
        {
            public bool antServerPause { get; set; }
            public List<string> resultinfo { get; set; }
            public List<Vehicle> vehicles { get; set; }
        }

        public class Root
        {
            public Payload payload { get; set; }
            public int retcode { get; set; }
        }
    }

    class agvMission
    {
        public class Missionrule
        {
        }

        public class Mission
        {
            [JsonProperty("missionid", Order = 1)]
            public string missionid { get; set; }
            public string state { get; set; }
            public int navigationstate { get; set; }
            public int transportstate { get; set; }
            public string fromnode { get; set; }
            public string tonode { get; set; }
            public bool isloaded { get; set; }
            public string payload { get; set; }
            public int priority { get; set; }
            public string assignedto { get; set; }
            public string payloadstatus { get; set; }
            public DateTime deadline { get; set; }
            public int missiontype { get; set; }
            [JsonProperty(Order = 2)]
            public string dispatchtime { get; set; }
            public int groupid { get; set; }
            public Missionrule missionrule { get; set; }
            public int timetodestination { get; set; }
            public string arrivingtime { get; set; }
            public string totalmissiontime { get; set; }
            public bool istoday { get; set; }
            public int schedulerstate { get; set; }
            public int stateinfo { get; set; }
            public bool askedforcancellation { get; set; }
            public string parameters { get; set; }
        }

        public class Payload
        {
            public List<Mission> missions { get; set; }
            public List<int> resultinfo { get; set; }
        }

        public class Root
        {
            public Payload payload { get; set; }
            public int retcode { get; set; }
        }

    }

    class alramClass
    {

        public class Sourcetypelocation
        {
            [JsonProperty("level")]
            public int tlevel { get; set; }
            [JsonProperty("x")]
            public string tx { get; set; }
            [JsonProperty("y")]
            public string ty { get; set; }
        }

        public class Acknowledgedby
        {
            [JsonProperty("name")]
            public string txtName { get; set; }
            [JsonProperty("id")]
            public string txtid { get; set; }
        }

        public class Alarm
        {
            public string sourceid { get; set; }
            public Sourcetypelocation sourcetypelocation { get; set; }
            public Acknowledgedby acknowledgedby { get; set; }
            public string uuid { get; set; }
            public string eventname { get; set; }
            public string acknowledgedat { get; set; }
            public string alarmargs { get; set; }
            public string alarmmessage { get; set; }
            public int eventcount { get; set; }
            public string firsteventat { get; set; }
            public string sourcetype { get; set; }
            public int state { get; set; }
            public string closedat { get; set; }
            public string lasteventat { get; set; }
            public string timestamp { get; set; }
            public string clearedat { get; set; }
        }

        public class Payload
        {
            public List<Alarm> alarms { get; set; }
            public List<int> resultinfo { get; set; }
        }

        public class Root
        {
            public Payload payload { get; set; }
            public int retcode { get; set; }
        }
    }
}
