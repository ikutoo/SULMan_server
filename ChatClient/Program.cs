using System.Collections.Generic;
using System.Text;
using UnityNetwork;

namespace ChatClient
{
    class Program
    {
        static void Main(string[] args)
        {
            ushort id=10;
            byte n =0;
            bool b = false;
            int vint = 100;
            uint vint2 = 999;
            short vshort = 10;
            ushort vshort2 = 101;
            float number = 0.8f;
            string data = "我是中国人";

            NetBitStream stream = new NetBitStream();
            stream.BeginWrite(id);
            stream.WriteByte(n);
            stream.WriteBool(b);
            stream.WriteInt(vint);
            stream.WriteUInt(vint2);
            stream.WriteShort(vshort);
            stream.WriteUShort(vshort2);
            stream.WriteFloat(number);
            stream.WriteString(data);
            stream.EncodeHeader();

            NetPacket packet = new NetPacket();
            packet.CopyBytes(stream);
          

            NetBitStream stream2 = new NetBitStream();
            stream2.BeginRead( packet,out id);
            stream2.ReadByte(out n);
            stream2.ReadBool(out b);
            stream2.ReadInt(out vint);
            stream2.ReadUInt(out vint2);
            stream2.ReadShort(out vshort);
            stream2.ReadUShort(out vshort2);
            stream2.ReadFloat(out number);
            stream2.ReadString(out data);

            System.Console.WriteLine(id);
            System.Console.WriteLine(n);
            System.Console.WriteLine(b);
            System.Console.WriteLine(vint);
            System.Console.WriteLine(vint2);
            System.Console.WriteLine(vshort);
            System.Console.WriteLine(vshort2);
            System.Console.WriteLine(number);
            System.Console.WriteLine(data);

            NetStructManager.TestStruct cif;
            cif.header = 10;
            cif.msgid = 5;
            cif.m = 0.9f;
            cif.n = 10;
            cif.str = "hello";
      

            byte[] bs = NetStructManager.getBytes(cif);
            NetStructManager.EncoderHeader(ref bs);

            NetStructManager.TestStruct cif2;
            System.Type type = typeof(NetStructManager.TestStruct);

            cif2 = (NetStructManager.TestStruct)NetStructManager.fromBytes(bs, type);

            System.Console.WriteLine(":"+bs.Length);
            System.Console.WriteLine(":"+cif2.header);
            System.Console.WriteLine(cif2.msgid);
            System.Console.WriteLine(cif2.m);
            System.Console.WriteLine(cif2.n);
            System.Console.WriteLine(cif2.str);
            

            ChatClient client = new ChatClient();
            client.Start();
            client.Update();
        }

        public class ChatClient :NetworkManager
        {
            System.Threading.Thread NetThread;

            NetTCPClient client=null;

            public void Start()
            {
                System.Console.WriteLine("启动聊天客户端");

                client = new NetTCPClient();
                client.Connect("127.0.0.1", 8089);
            }

            public override void Update()
            {
                NetPacket packet = null;
                while (true)
                {

                    for (packet = GetPacket(); packet != null; )
                    {
                        ushort msgid = 0;
                        packet.TOID(out msgid);

                        switch (msgid)
                        {
                            case (ushort)MessageIdentifiers.ID.CONNECTION_REQUEST_ACCEPTED:
                                {
                                    System.Console.WriteLine("连接到服务器");
                                    NetThread = new System.Threading.Thread(new System.Threading.ThreadStart(Input));
                                    NetThread.Start();
                                    break;
                                }
                            case (ushort)MessageIdentifiers.ID.CONNECTION_ATTEMPT_FAILED:
                                {
                                    System.Console.WriteLine("连接服务器失败,请按任意键退出");
                                    System.Console.Read();
                                    return;
                                }
                            case (ushort)MessageIdentifiers.ID.CONNECTION_LOST:
                                {
                                    System.Console.WriteLine("丢失与服务器的连接,请按任意键退出");
                                    System.Console.Read();
                                    return;
                                }
                            case (ushort)MessageIdentifiers.ID.ID_CHAT:
                                {
                                    string chatdata="";

                                    NetBitStream stream = new NetBitStream();
                                    stream.BeginRead2(packet);
                                    stream.ReadString(out chatdata);

                                    System.Console.WriteLine("收到消息:"+chatdata);
                                    break;
                                }
                            case (ushort)MessageIdentifiers.ID.ID_CHAT2:
                                {
                                    NetStructManager.TestStruct chatstr;

                                    chatstr=(NetStructManager.TestStruct)NetStructManager.fromBytes(packet._bytes,typeof(NetStructManager.TestStruct));

                                    System.Console.WriteLine("收到消息:" + chatstr.str);
                                    break;
                                }
                            default:
                                {
                                    // 错误
                                    break;
                                }
                        }

                        packet = null;

                    }// end fore
                }// end while

            }

            public void Input()
            {
                while (true)
                {
                    string str=System.Console.ReadLine();
                    if (str.CompareTo("quit") == 0)
                    {

                        client.Disconnect(0);
                        NetThread.Abort();
                        break;
                    }
                    else
                    {
                        /*
                        NetBitStream stream = new NetBitStream();
                        stream.BeginWrite((ushort)MessageIdentifiers.ID.ID_CHAT);
                        stream.WriteString(str);
                        stream.EncodeHeader();
  
                        client.Send(stream);
                        */

                        NetStructManager.TestStruct chatstr;
                        chatstr.header = 0;
                        chatstr.msgid = (ushort)MessageIdentifiers.ID.ID_CHAT2;
                        chatstr.m = 0.1f;
                        chatstr.n = 100;
                        chatstr.str = str;
                       
                        byte[] bs=NetStructManager.getBytes(chatstr);
                        //NetStructManager.EncoderHeader(ref bs);

                        NetBitStream stream = new NetBitStream();
                        stream.CopyBytes(bs);

                        client.Send(stream);
                    }
                }
            }
        }



    }
}
