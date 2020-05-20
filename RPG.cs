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

		public override string Author => "欲情 trbbs.cc";

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
			PlayerHooks.PlayerPostLogin += Login;
			PlayerHooks.PlayerLogout += UserOut;

		}
		public List<int> Idlist = new List<int>();
		private void Login(PlayerPostLoginEventArgs e)
		{
			Idlist.Add(e.Player.Index);
			if (!RPGconfig.Display)
			{
				return;
			}
			if (Idlist.Count>0)
			{
			t_task();
			}
			

		}

		private void UserOut(PlayerLogoutEventArgs e)
		{
			Idlist.Remove(e.Player.Index);

		}

		public  async Task t_task()
		{
			for (int i = 0; Idlist.Count>0; i++ )
			{

				for (int i2 = 0; i2 < Idlist.Count; i2++)
				{
					if (Idlist.Count < 1)
					{
						return;
					}
					msg(Idlist[i2]);

				}
				await Task.Delay(2000);
			}
	
		
		}
		#endregion

		#region Dispose
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
				PlayerHooks.PlayerPostLogin -= Login;
				PlayerHooks.PlayerLogout -= UserOut;

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

		private string Data(int id)
		{
			List<string> GroupList = new List<string>(Gdata(TShock.Players[id].Group.Name).ToArray());
			List<int> Clist = new List<int>(Cdata(TShock.Players[id].Group.Name).ToArray());
			string data;
			if (GroupList.Count ==0)
			{
				data = "MAX";
				return data;
			}
			if (GroupList.Count >1)
			{
				data = "Null?";
				return data;
			}
			else
			{
				data = Clist[0].ToString();
				return data;

			}
			
		}

		public void msg(int id)
		{
			Random rd = new Random();
			int r = rd.Next(0, 255);
			int g = rd.Next(0, 255);
			int b = rd.Next(0, 255);
			string message = "[" + TShock.Players[id].Group.Name + "]" + "["+pobcc.Db.QueryCurrency(TShock.Players[id].Name) + "/" +Data(id)+ "]";
			Microsoft.Xna.Framework.Color c = new Microsoft.Xna.Framework.Color(r, g, b);
			NetMessage.SendData(119,
	-1, -1, NetworkText.FromLiteral(message), (int)c.PackedValue, TShock.Players[id].X, TShock.Players[id].Y + 50);



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
					if (pobcc.Db.QueryCurrency(args.Player.Name) > Clist[0])
					{
						TShock.UserAccounts.SetUserGroup(args.Player.Account, GroupList[0]);
						args.Player.SendWarningMessage("您的等级成功提升，当前等级" + GroupList[0]);

						for (int i = 0; i < Co(GroupList[0]).Count; i++)
						{
							var C = Co(GroupList[0])[i].Replace("{name}", args.Player.Name);
							Commands.HandleCommand(TSPlayer.Server, C);
						}
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
				if (Clist[coordinate] < pobcc.Db.QueryCurrency(args.Player.Name))
				{
					TShock.UserAccounts.SetUserGroup(args.Player.Account, args.Parameters[0]);
					args.Player.SendWarningMessage("您的等级成功提升，当前等级" + args.Player.Group.Name);
					for (int i = 0; i < Co(GroupList[0]).Count; i++)
					{
						var C = Co(GroupList[0])[i].Replace("{name}", args.Player.Name);
						Commands.HandleCommand(TSPlayer.Server, C);
					}

				}
				else
				{
					args.Player.SendWarningMessage("您的货币不足，您当前货币" + pobcc.Db.QueryCurrency(args.Player.Name) + "升级需要货币：" + Clist[coordinate]);
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

