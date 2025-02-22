using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;


namespace StargateNetwork
{
    class Program
    {
        public class Echo : WebSocketBehavior
        {
            protected override async void OnMessage(MessageEventArgs wibi)
            {
                Console.WriteLine("Received message from client :" + wibi.Data);
                
                //check for IDC
                try
                {
                    if (wibi.Data.Contains("IDC:"))
                    {
                        using (var db = new StargateContext())
                        {
                            Console.WriteLine("IDC SENT: " + wibi.Data.Substring(4));
                            Stargate gate = await StargateTools.FindGateById(ID, db);
                            Sessions.SendTo(wibi.Data, gate.dialed_gate_id);
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception caught during IDC: " + e.Message);
                }


                //Deserialize the incoming message
                string type;
                dynamic? message = JsonConvert.DeserializeObject(wibi.Data);
                if (message != null)
                {
                    type = message.type;
                }
                else
                {
                    Console.WriteLine("Failed to deserialize message. ignoring...");
                    return;
                }


                Console.WriteLine("Received: " + type + " from client");
                Console.WriteLine("Client id = " + ID);

                //message handler
                switch (type)
                {
                    //used when gate requests initial address during setup
                    case "requestAddress":
                    {
                        try
                        {
                            string requestedAddress =
                                message.gate_address; //i need to do this because cs is being funny
                            Console.WriteLine("New address request: '" + requestedAddress + "'");

                            //check db if any gates already have the address
                            using (var db = new StargateContext())
                            {
                                var gate = await StargateTools.FindGateByAddress(requestedAddress, db);

                                if (gate.id != "NULL")
                                {
                                    bool overRide = false;

                                    if (UnixTimestamp() - gate.update_date > 60)
                                    {
                                        Console.WriteLine("database entry stale, overriding...");
                                        db.Remove(gate);
                                        overRide = true;
                                    }
                                    else if (gate.id == ID)
                                    {
                                        Console.WriteLine("Gate already exists in database. Skipping...");
                                        break;
                                    }

                                    if (!overRide)
                                    {
                                        Console.WriteLine("Address in use");
                                        Send("403");
                                        break;
                                    }
                                }

                                db.Add(new Stargate()
                                {
                                    id = ID,
                                    gate_address = message.gate_address,
                                    gate_code = message.gate_code,
                                    is_headless = message.is_headless,
                                    session_url = message.session_id,
                                    active_users = message.current_users,
                                    max_users = message.max_users,
                                    gate_status = "IDLE",
                                    session_name = message.gate_name,
                                    owner_name = message.host_id,
                                    iris_state = "false",
                                    creation_date = UnixTimestamp(),
                                    update_date = UnixTimestamp(),
                                    dialed_gate_id = "",
                                    is_persistent = false,
                                    world_record = "",
                                    public_gate = message.@public
                                });
                                await db.SaveChangesAsync();
                                Send("{code: 200, message: \"Address accepted\" }");
                                Console.WriteLine("Stargate added to database");
                            }

                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Exception caught during address request: " + e.Message);
                            Send("403");
                            break;
                        }
                    }

                    //used to make sure the dialed address is valid
                    case "validateAddress":
                    {
                        try
                        {
                            Console.WriteLine("Address validation Requested");

                            string requestedAddress_full = message.gate_address;
                            if (requestedAddress_full.Length < 6)
                            {
                                Console.WriteLine("Address is too short");
                                Send("CSDialCheck:404");
                                break;
                            }

                            string requestedAddress = requestedAddress_full.Substring(0, 6);

                            //query database for requested gate
                            using (var db = new StargateContext())
                            {
                                var requestedGate = await StargateTools.FindGateByAddress(requestedAddress, db);

                                var currentGate = await StargateTools.FindGateById(ID, db);

                                //check if requested gate exists
                                if (requestedGate.id == "NULL")
                                {
                                    Console.WriteLine("No stargate found");
                                    Send("CSValidCheck:404");
                                    break;
                                }

                                //check if requested address is of valid length (WHYYYYY IS THIS A SEPRORATE FUNCTION FOR UNIVERSE GATES OTHER GATES DO THIS IN GAME!!!!!!
                                if (requestedAddress.Length < 6)
                                {
                                    Console.WriteLine("Address is too short");
                                    Send("CSValidCheck:400");
                                }

                                //check if gate is trying to dial itself
                                if (requestedGate.gate_address == currentGate.gate_address)
                                {
                                    Console.WriteLine("Gate is trying to dial itself!!!");
                                    Send("CSValidCheck:403");
                                    break;
                                }

                                //check if destination gate is busy
                                if (requestedGate.gate_status != "IDLE")
                                {
                                    Console.WriteLine("Gate is busy");
                                    Console.WriteLine("Gate status: " + requestedGate.gate_status);
                                    Send("CSValidCheck:403");
                                    break;
                                }

                                //find chev count to send to requested gate
                                string currentGateCode = currentGate.gate_code;
                                string requestedGateCode = requestedGate.gate_code;
                                int chevCount = 0;

                                switch (requestedAddress_full.Length)
                                {
                                    case 6:
                                    {
                                        if (requestedGateCode == currentGateCode)
                                        {
                                            chevCount = 6;
                                        }
                                        else
                                        {
                                            chevCount = -1;
                                        }

                                        break;
                                    }

                                    case 7:
                                    {
                                        if (requestedGateCode.Substring(0, 1) ==
                                            requestedAddress_full.Substring(6, 1) &&
                                            requestedGateCode.Substring(0, 1) != "U" &&
                                            currentGateCode.Substring(0, 1) != "U")
                                        {
                                            chevCount = 7;
                                        }
                                        else
                                        {
                                            chevCount = -1;
                                        }

                                        break;
                                    }

                                    case 8:
                                    {
                                        if (requestedGate.gate_code == requestedAddress_full.Substring(6, 2))
                                        {
                                            chevCount = 8;
                                        }
                                        else
                                        {
                                            chevCount = -1;
                                        }

                                        break;
                                    }
                                }

                                if (chevCount == -1)
                                {
                                    Console.WriteLine("Invalid gate code!");
                                    Send("CSValidCheck:302");
                                    break;
                                }

                                //check if destination is full
                                if (requestedGate.active_users >= requestedGate.max_users)
                                {
                                    Console.WriteLine("Max users reached on requested session!");
                                    Send("CSValidCheck:403");
                                    break;
                                }

                                //dial gate
                                Send("CSValidCheck:200");
                                Console.Write("Address validated!");

                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Exception caught during address validation: " + e.Message);
                            Send("CSValidCheck:403");
                            break;
                        }
                    }

                    //used to make a request to the server to dial a remote gate
                    case "dialRequest":
                    {
                        try
                        {
                            Console.WriteLine("Dial Requested");

                            string requestedAddress_full = message.gate_address;
                            if (requestedAddress_full.Length < 6)
                            {
                                Console.WriteLine("Address is too short");
                                Send("CSDialCheck:404");
                                break;
                            }

                            string requestedAddress = requestedAddress_full.Substring(0, 6);

                            //query database for requested gate
                            using (var db = new StargateContext())
                            {
                                var requestedGate = await StargateTools.FindGateByAddress(requestedAddress, db);

                                var currentGate = await StargateTools.FindGateById(ID, db);

                                //check if requested gate exists
                                if (requestedGate.id == "NULL")
                                {
                                    Console.WriteLine("No stargate found");
                                    Send("CSDialCheck:404");
                                    break;
                                }

                                //check if gate is trying to dial itself
                                if (requestedGate.gate_address == currentGate.gate_address)
                                {
                                    Console.WriteLine("Gate is trying to dial itself!!!");
                                    Send("CSDialCheck:403");
                                    break;
                                }

                                //check if destination gate is busy
                                if (requestedGate.gate_status != "IDLE")
                                {
                                    Console.WriteLine("Gate is busy");
                                    Console.WriteLine("Gate status: " + requestedGate.gate_status);
                                    Send("CSValidCheck:403");
                                    break;
                                }

                                //if gate is persistent make sure the world is up and if not start it and wait for it to fully load //TODO
                                if (requestedGate.is_persistent)
                                {
                                    /*
                                    if (!(requestedGate.world_record == //function that returns true if the world is already up))
                                    {
                                        Console.WriteLine("Requested gate is in closed world. starting...")
                                        //function that starts world
                                        //waits for world to start

                                        //updates gate info
                                        requestedGate = await StargateTools.FindGateByAddress(requestedAddress, db);
                                    }
                                    */
                                }


                                //find chev count to send to requested gate
                                string currentGateCode = currentGate.gate_code;
                                string requestedGateCode = requestedGate.gate_code;
                                int chevCount = 0;

                                switch (requestedAddress_full.Length)
                                {
                                    case 6:
                                    {
                                        if (requestedGateCode == currentGateCode)
                                        {
                                            chevCount = 6;
                                        }
                                        else
                                        {
                                            chevCount = -1;
                                        }

                                        break;
                                    }

                                    case 7:
                                    {
                                        if (requestedGateCode.Substring(0, 1) ==
                                            requestedAddress_full.Substring(6, 1) &&
                                            requestedGateCode.Substring(0, 1) != "U" &&
                                            currentGateCode.Substring(0, 1) != "U")
                                        {
                                            chevCount = 7;
                                        }
                                        else
                                        {
                                            chevCount = -1;
                                        }

                                        break;
                                    }

                                    case 8:
                                    {
                                        if (requestedGate.gate_code == requestedAddress_full.Substring(6, 2))
                                        {
                                            chevCount = 8;
                                        }
                                        else
                                        {
                                            chevCount = -1;
                                        }

                                        break;
                                    }
                                }

                                if (chevCount == -1)
                                {
                                    Console.WriteLine("Invalid gate code!");
                                    Send("CSDialCheck:302");
                                    break;
                                }

                                //check if destination is full
                                if (requestedGate.active_users >= requestedGate.max_users)
                                {
                                    Console.WriteLine("Max users reached on requested session!");
                                    Send("CSDialCheck:403");
                                    break;
                                }

                                //update gate states on database
                                requestedGate.gate_status = "INCOMING";

                                currentGate.gate_status = "OPEN";
                                currentGate.dialed_gate_id = requestedGate.id;

                                await db.SaveChangesAsync();

                                //dial gate
                                Send("CSDialCheck:200");
                                Send("CSDialedSessionURL:" + requestedGate.session_url);

                                switch (chevCount)
                                {
                                    case 6:
                                    {
                                        Sessions.SendTo("Impulse:OpenIncoming:7", requestedGate.id);
                                        break;
                                    }

                                    case 7:
                                    {
                                        Sessions.SendTo("Impulse:OpenIncoming:8", requestedGate.id);
                                        break;
                                    }

                                    case 8:
                                    {
                                        Sessions.SendTo("Impulse:OpenIncoming:9", requestedGate.id);
                                        break;
                                    }
                                }

                                Console.Write("Stargate open!");
                            }

                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Exception caught during dial request: " + e.Message);
                            Send("CSDialCheck:403");
                            break;
                        }
                    }

                    //used to close wormhole on both gates
                    case "closeWormhole":
                    {
                        try
                        {
                            //query database for current gate
                            using (var db = new StargateContext())
                            {
                                var currentGate = await StargateTools.FindGateById(ID, db);

                                //close remote gate
                                Console.WriteLine("Closing wormhole: " + currentGate.dialed_gate_id);
                                Sessions.SendTo("Impulse:CloseWormhole", currentGate.dialed_gate_id);

                                currentGate.dialed_gate_id = "";
                                currentGate.gate_status = "IDLE";
                                await db.SaveChangesAsync();
                            }

                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Exception caught during close wormhole: " + e.Message);
                            break;
                        }
                    }

                    //used to update info about the gate on the database
                    case "updateData":
                    {
                        try
                        {
                            Console.WriteLine("Updated requested");

                            //find gate and update record
                            using (var db = new StargateContext())
                            {
                                var gate = await StargateTools.FindGateById(ID, db);

                                if (gate.id == "NULL")
                                {
                                    Console.WriteLine("No stargate found for update. Is it registered?");
                                    break;
                                }

                                gate.active_users = message.currentUsers;
                                gate.max_users = message.maxUsers;
                                gate.gate_status = message.gate_status;
                                gate.update_date = UnixTimestamp();
                                await db.SaveChangesAsync();

                                Console.WriteLine("Updated record");
                            }

                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Exception caught during update data: " + e.Message);
                            break;
                        }
                    }

                    //used to update iris state info on the server
                    case "updateIris":
                    {
                        try
                        {
                            Console.WriteLine("Iris state: " + message.iris_state);

                            using (var db = new StargateContext())
                            {
                                //set iris state in database // 
                                var gate = await StargateTools.FindGateById(ID, db);
                                gate.iris_state = message.iris_state;
                                await db.SaveChangesAsync();


                                Console.WriteLine("Sending iris state to dialing gate");
                                Stargate incomingGate = await StargateTools.FindGateByDialedId(gate.id, db);
                                Sessions.SendTo("IrisUpdate:" + gate.iris_state, incomingGate.id);
                            }

                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Exception caught during update iris: " + e.Message);
                            break;
                        }
                    }

                    //keepalive
                    case "keepAlive":
                    {
                        try
                        {
                            using (var db = new StargateContext())
                            {
                                Stargate gate = await StargateTools.FindGateById(ID, db);
                                gate.update_date = UnixTimestamp();
                                await db.SaveChangesAsync();
                            }

                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Exception caught during keep alive: " + e.Message);
                            break;
                        }
                    }
                }
                
            }
        }

        public static int UnixTimestamp()
        {
            return (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        static void CleanStaleDb()
        {
            try
            {
                while (true)
                {
                    using (var db = new StargateContext())
                    {
                        var gates = StargateTools.FindAllGates(db, true);

                        foreach (var gate in gates)
                        {
                            if (UnixTimestamp() - gate.update_date > 60)
                            {
                                StargateTools.RemoveGate(gate, db);

                                Console.WriteLine("Cleaned stale stargate from database");
                            }
                        }

                        db.SaveChanges();
                    }

                    Thread.Sleep(60000);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception caught when cleaning state gates: " + e.Message);
            }
        }


        static void Main(string[] args)
        {
            //get env vars
            string? WS_URI = Environment.GetEnvironmentVariable("WS_URI");
            if (string.IsNullOrEmpty(WS_URI))
            {
                WS_URI = "ws://192.168.1.14:27015";
            }

            //start websocket server
            WebSocketServer wssv = new WebSocketServer(WS_URI);
            wssv.AddWebSocketService<Echo>("/Echo");
            wssv.Start();
            Console.WriteLine("server started on: " + WS_URI);

            //start bunkum http server
            BunKum.StartBunKum();

            //start database cleaner thread
            Thread dbCleanThread = new Thread(CleanStaleDb);
            dbCleanThread.Start();
            Console.WriteLine("Db cleaner started");

            Console.ReadKey();
            wssv.Stop();
        }
    }
}