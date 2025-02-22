using Newtonsoft.Json;
using StargateNetwork.Types;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace StargateNetwork;

public class StargateClient : WebSocketBehavior, IDisposable
{
    private readonly StargateContext _db = new();

    protected override async void OnMessage(MessageEventArgs wibi)
    {
        Console.WriteLine("Received message from client :" + wibi.Data);

        //check for IDC
        try
        {
            if (wibi.Data.Contains("IDC:"))
            {
                Console.WriteLine("IDC SENT: " + wibi.Data[4..]);
                Stargate? gate = await this._db.FindGateById(ID);

                if (gate == null)
                {
                    Console.WriteLine("Current gate not found");
                    Send("CSValidCheck:404");
                    return;
                }

                Sessions.SendTo(wibi.Data, gate.DialedGateId);
                return;
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
                    Stargate? gate = await this._db.FindGateByAddress(requestedAddress);

                    if (gate != null)
                    {
                        bool overRide = false;

                        if (UnixTimestamp - gate.UpdateDate > 60)
                        {
                            Console.WriteLine("database entry stale, overriding...");
                            this._db.Remove(gate);
                            overRide = true;
                        }
                        else if (gate.Id == ID)
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

                    this._db.Add(new Stargate
                    {
                        Id = ID,
                        GateAddress = message.gate_address,
                        GateCode = message.gate_code,
                        IsHeadless = message.is_headless,
                        SessionUrl = message.session_id,
                        ActiveUsers = message.current_users,
                        MaxUsers = message.max_users,
                        GateStatus = "IDLE",
                        SessionName = message.gate_name,
                        OwnerName = message.host_id,
                        IrisState = "false",
                        CreationDate = UnixTimestamp,
                        UpdateDate = UnixTimestamp,
                        DialedGateId = "",
                        IsPersistent = false,
                        WorldRecord = "",
                        PublicGate = message.@public
                    });
                    await this._db.SaveChangesAsync();
                    Send("{code: 200, message: \"Address accepted\" }");
                    Console.WriteLine("Stargate added to database");

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

                    string requestedAddressFull = message.gate_address;
                    if (requestedAddressFull.Length < 6)
                    {
                        Console.WriteLine("Address is too short");
                        Send("CSDialCheck:404");
                        break;
                    }

                    string requestedAddress = requestedAddressFull[..6];

                    //query database for requested gate
                    Stargate? requestedGate = await this._db.FindGateByAddress(requestedAddress);
                    Stargate? currentGate = await this._db.FindGateById(ID);

                    if (requestedGate == null)
                    {
                        Console.WriteLine("Requested gate not found");
                        Send("CSValidCheck:404");
                        break;
                    }

                    if (currentGate == null)
                    {
                        Console.WriteLine("Current gate not found");
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
                    if (requestedGate.GateAddress == currentGate.GateAddress)
                    {
                        Console.WriteLine("Gate is trying to dial itself!!!");
                        Send("CSValidCheck:403");
                        break;
                    }

                    //check if destination gate is busy
                    if (requestedGate.GateStatus != "IDLE")
                    {
                        Console.WriteLine("Gate is busy");
                        Console.WriteLine("Gate status: " + requestedGate.GateStatus);
                        Send("CSValidCheck:403");
                        break;
                    }

                    //find chev count to send to requested gate
                    string currentGateCode = currentGate.GateCode;
                    string requestedGateCode = requestedGate.GateCode;
                    int chevCount = 0;

                    switch (requestedAddressFull.Length)
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
                            if (requestedGateCode[..1] ==
                                requestedAddressFull.Substring(6, 1) &&
                                requestedGateCode[..1] != "U" &&
                                currentGateCode[..1] != "U")
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
                            if (requestedGate.GateCode == requestedAddressFull.Substring(6, 2))
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
                    if (requestedGate.ActiveUsers >= requestedGate.MaxUsers)
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

                    string requestedAddressFull = message.gate_address;
                    if (requestedAddressFull.Length < 6)
                    {
                        Console.WriteLine("Address is too short");
                        Send("CSDialCheck:404");
                        break;
                    }

                    string requestedAddress = requestedAddressFull[..6];

                    //query database for requested gate
                    Stargate? requestedGate = await this._db.FindGateByAddress(requestedAddress);
                    Stargate? currentGate = await this._db.FindGateById(ID);

                    if (requestedGate == null)
                    {
                        Console.WriteLine("Requested gate not found");
                        Send("CSValidCheck:404");
                        break;
                    }

                    if (currentGate == null)
                    {
                        Console.WriteLine("Current gate not found");
                        Send("CSValidCheck:404");
                        break;
                    }

                    //check if gate is trying to dial itself
                    if (requestedGate.GateAddress == currentGate.GateAddress)
                    {
                        Console.WriteLine("Gate is trying to dial itself!!!");
                        Send("CSDialCheck:403");
                        break;
                    }

                    //check if destination gate is busy
                    if (requestedGate.GateStatus != "IDLE")
                    {
                        Console.WriteLine("Gate is busy");
                        Console.WriteLine("Gate status: " + requestedGate.GateStatus);
                        Send("CSValidCheck:403");
                        break;
                    }

                    //if gate is persistent make sure the world is up and if not start it and wait for it to fully load //TODO
                    if (requestedGate.IsPersistent)
                    {
                        /*
                        if (!(requestedGate.world_record == //function that returns true if the world is already up))
                        {
                            Console.WriteLine("Requested gate is in closed world. starting...")
                            //function that starts world
                            //waits for world to start

                            //updates gate info
                            requestedGate = await db.FindGateByAddress(requestedAddress, db);
                        }
                        */
                    }


                    //find chev count to send to requested gate
                    string currentGateCode = currentGate.GateCode;
                    string requestedGateCode = requestedGate.GateCode;
                    int chevCount = 0;

                    switch (requestedAddressFull.Length)
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
                            if (requestedGateCode[..1] ==
                                requestedAddressFull.Substring(6, 1) &&
                                requestedGateCode[..1] != "U" &&
                                currentGateCode[..1] != "U")
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
                            if (requestedGate.GateCode == requestedAddressFull.Substring(6, 2))
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
                    if (requestedGate.ActiveUsers >= requestedGate.MaxUsers)
                    {
                        Console.WriteLine("Max users reached on requested session!");
                        Send("CSDialCheck:403");
                        break;
                    }

                    //update gate states on database
                    requestedGate.GateStatus = "INCOMING";

                    currentGate.GateStatus = "OPEN";
                    currentGate.DialedGateId = requestedGate.Id;

                    await this._db.SaveChangesAsync();

                    //dial gate
                    Send("CSDialCheck:200");
                    Send("CSDialedSessionURL:" + requestedGate.SessionUrl);

                    switch (chevCount)
                    {
                        case 6:
                        {
                            Sessions.SendTo("Impulse:OpenIncoming:7", requestedGate.Id);
                            break;
                        }

                        case 7:
                        {
                            Sessions.SendTo("Impulse:OpenIncoming:8", requestedGate.Id);
                            break;
                        }

                        case 8:
                        {
                            Sessions.SendTo("Impulse:OpenIncoming:9", requestedGate.Id);
                            break;
                        }
                    }

                    Console.Write("Stargate open!");

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
                    Stargate? currentGate = await this._db.FindGateById(ID);

                    if (currentGate == null)
                    {
                        Console.WriteLine("Current gate not found");
                        Send("CSValidCheck:404");
                        break;
                    }

                    //close remote gate
                    Console.WriteLine("Closing wormhole: " + currentGate.DialedGateId);
                    Sessions.SendTo("Impulse:CloseWormhole", currentGate.DialedGateId);

                    currentGate.DialedGateId = "";
                    currentGate.GateStatus = "IDLE";
                    await this._db.SaveChangesAsync();

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
                    Stargate? gate = await this._db.FindGateById(ID);

                    if (gate == null)
                    {
                        Console.WriteLine("No stargate found for update. Is it registered?");
                        break;
                    }

                    gate.ActiveUsers = message.currentUsers;
                    gate.MaxUsers = message.maxUsers;
                    gate.GateStatus = message.gate_status;
                    gate.UpdateDate = UnixTimestamp;
                    await this._db.SaveChangesAsync();

                    Console.WriteLine("Updated record");

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

                    //set iris state in database // 
                    Stargate? gate = await this._db.FindGateById(ID);

                    if (gate == null)
                    {
                        Console.WriteLine("Current gate not found");
                        Send("CSValidCheck:404");
                        break;
                    }

                    gate.IrisState = message.iris_state;
                    await this._db.SaveChangesAsync();

                    Console.WriteLine("Sending iris state to dialing gate");
                    Stargate? incomingGate = await this._db.FindGateByDialedId(gate.Id);

                    if (incomingGate == null)
                    {
                        Console.WriteLine("Incoming gate not found");
                        Send("CSValidCheck:404");
                        break;
                    }

                    Sessions.SendTo("IrisUpdate:" + gate.IrisState, incomingGate.Id);

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
                    Stargate? gate = await this._db.FindGateById(ID);

                    if (gate == null)
                    {
                        Console.WriteLine("Current gate not found");
                        Send("CSValidCheck:404");
                        break;
                    }

                    gate.UpdateDate = UnixTimestamp;
                    await this._db.SaveChangesAsync();

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

    private static long UnixTimestamp => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    public void Dispose()
    {
        _db.Dispose();
        GC.SuppressFinalize(this);
    }
}