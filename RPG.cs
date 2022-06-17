using Newtonsoft.Json;
using POBC.RPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using POBC2;
using Terraria;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Bank
{
	[ApiVersion(2, 1)]
	public class POBCRPG : TerrariaPlugin
	{

		#region Info
		public override string Name => "PBOC.RPG";

		public override string Author => "欲情 trbbs.cc ";

		public override string Description => "POBC 系列插件-RPG 插件.";

		public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;
		public string ConfigPath { get { return Path.Combine(TShock.SavePath, "POBC-RPG.json"); } }
		public RPGConfig RPGconfig = new RPGConfig();
		#endregion

		#region Initialize
		public override void Initialize()
		{
			File();
			ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.GamePostUpdate.Register(this, OnPostUpdate);

		}
		#endregion

		#region Dispose
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                ServerApi.Hooks.GameInitialize.Deregister(this, OnPostUpdate);

			}
			base.Dispose(disposing);
		}
		#endregion

		public POBCRPG(Main game)
			: base(game)
		{
			Order = 10;
		}

		void OnInitialize(EventArgs args) //添加命令
		{

			Commands.ChatCommands.Add(new Command("pobc.rank", rank, "rank", "升级")
			{
				HelpText = "使用命令消耗货币进行等级提升."
			});

		}

        private int frameCounter;
        private readonly Random random = new Random();


        public void OnPostUpdate(EventArgs _)
        {
            if (!RPGconfig.Display) return;
            ++frameCounter;
            if (frameCounter == RPGconfig.DisplayInterval) frameCounter = 0;
            if (frameCounter >= TShock.Players.Length) return;
            var player = TShock.Players[frameCounter];
            if (player?.Account == null) return;

            var text = "[" + player.Group.Name + "][" + Db.QueryCurrency(player.Name) + "/" + Data(player) + "]";
            NetMessage.SendData(number: (int)random.Next(1 << 24), msgType: 119, remoteClient: -1,
                ignoreClient: -1, text: NetworkText.FromLiteral(text), number2: player.X,
                number3: player.Y + 50f);
        }

        private string Data(TSPlayer player)
        {
            List<string> list = new List<string>(Gdata(player.Group.Name).ToArray());
            List<int> list2 = new List<int>(Cdata(player.Group.Name).ToArray());
            if (list.Count == 0)
            {
                return "MAX";
            }
            if (list.Count > 1)
            {
                return "Null?";
            }
            return list2[0].ToString();
        }

		private void rank(CommandArgs args)
		{
			if (!args.Player.Group.HasPermission("pobc.rank"))
			{
				args.Player.SendWarningMessage("你没有相关命令权限");
				return;
			}
			if (args.Player.Group.Name == "superadmin")
			{
				args.Player.SendWarningMessage("超管升什么鸡毛等级");
				return;
			}
			if (args.Parameters.Count < 1) 
			{
				List<string> GroupList = new List<string>(Gdata(args.Player.Group.Name).ToArray());
				List<int> Clist = new List<int>(Cdata(args.Player.Group.Name).ToArray());
				if (GroupList.Count == 0)
				{
					args.Player.SendWarningMessage("您已达到最高等级！");
					return;

				}
				if (GroupList.Count > 1)
				{
					args.Player.SendWarningMessage(" 你面临职业专精选择 :", 255, 255, 255);
					for (int i = 0; i < GroupList.Count; i++)
					{
						args.Player.SendWarningMessage("  请输入 /rank " + GroupList[i] + " 进行等级提升", 255, 255, 255);
					}
					return;
				}
				else
				{
					if (POBC2.Db.QueryCurrency(args.Player.Name) >= Clist[0])
					{
						TShock.UserAccounts.SetUserGroup(args.Player.Account, GroupList[0]);
						POBC2.Db.DownC(args.Player.Name, Clist[0],"提升等级消耗");
						args.Player.SendWarningMessage("您的等级成功提升，当前等级" + GroupList[0]);

						for (int i = 0; i < Co(GroupList[0]).Count; i++)
						{
							var C = Co(GroupList[0])[i].Replace("{name}", args.Player.Name);
							Commands.HandleCommand(TSPlayer.Server, C);
						}
                    }
                    else
                    {
						args.Player.SendWarningMessage("您的货币不足，您当前货币" + POBC2.Db.QueryCurrency(args.Player.Name) + "升级需要货币：" + Clist[0]);
					}


				}

			}
			else
			{
				List<string> GroupList = new List<string>(Gdata(args.Player.Group.Name).ToArray());
				List<int> Clist = new List<int>(Cdata(args.Player.Group.Name).ToArray());
				if (args.Parameters.Count != 1)
				{
					args.Player.SendWarningMessage("命令错误，正确语法：  /rank [等级名]");
					return;
				}
				if (!GroupList.Contains(args.Parameters[0]))
				{
					args.Player.SendWarningMessage("专精职业输入错误，正确语法：  /rank [等级名]");
					return;
				}
				string com = args.Parameters[0];
				int coordinate = GroupList.IndexOf(com);
				if (Clist[coordinate] <= POBC2.Db.QueryCurrency(args.Player.Name))
				{
					TShock.UserAccounts.SetUserGroup(args.Player.Account, args.Parameters[0]);
					args.Player.SendWarningMessage("您的等级成功提升，当前等级" + args.Player.Group.Name);
					POBC2.Db.DownC(args.Player.Name, Clist[coordinate],"提升等级消耗");
					for (int i = 0; i < Co(GroupList[coordinate]).Count; i++)
					{
						var C = Co(GroupList[coordinate])[i].Replace("{name}", args.Player.Name);
						Commands.HandleCommand(TSPlayer.Server, C);
					}

				}
				else
				{
					args.Player.SendWarningMessage("您的货币不足，您当前货币" + POBC2.Db.QueryCurrency(args.Player.Name) + "升级需要货币：" + Clist[coordinate]);
				}
			}




		}


		public List<string> Gdata(string group)  //获取可以升级的Group
		{
			List<string> GroupList = new List<string>();

			for (int i = 0; i < RPGconfig.RPGList.Length; i++)
			{
				if (group == RPGconfig.RPGList[i].Group)
				{
					GroupList.Add(RPGconfig.RPGList[i].GoGroup);

				}
			}
			return GroupList;

		}
		public List<int> Cdata(string group)  //获取升级需要的货币值
		{

			List<int> Clist = new List<int>();

			for (int i = 0; i < RPGconfig.RPGList.Length; i++)
			{
				if (group == RPGconfig.RPGList[i].Group)
				{
					Clist.Add(RPGconfig.RPGList[i].C);

				}
			}
			return Clist;

		}

		
		public List<string> Co(string Gogroup) //将Co存进Lsit
		{
			List<string> Co = new List<string>();

			for (int i = 0; i < RPGconfig.RPGList.Length; i++)
			{
				if (Gogroup == RPGconfig.RPGList[i].GoGroup)
				{
					for (int i2 = 0; i2 < RPGconfig.RPGList[i].Co.Length; i2++)
					{
						Co.Add(RPGconfig.RPGList[i].Co[i2]);
					}

				}
			}
			return Co;

		}

		public void File()
		{
			try
			{
				RPGconfig = RPGConfig.Read(ConfigPath).Write(ConfigPath);
			}
			catch (Exception ex)
			{
				RPGconfig = new RPGConfig();
				TShock.Log.ConsoleError("[POBC] 读取配置文件发生错误!\n{0}".SFormat(ex.ToString()));
			}





		}




	}



}

