using Newtonsoft.Json;
using System.IO;

namespace POBC.RPG
{
	public class RPGConfig
	{
		public bool Display = true;
        public int DisplayInterval = 180; // should larger than max player count

		public RPGC[] RPGList = new RPGC[0];

		public RPGConfig Write(string file)
		{
			File.WriteAllText(file, JsonConvert.SerializeObject(this, Formatting.Indented));
			return this;
		}

		public static RPGConfig Read(string file)
		{
			if (!File.Exists(file))
			{
				WriteExample(file);
			}
			return JsonConvert.DeserializeObject<RPGConfig>(File.ReadAllText(file));
		}

		public static void WriteExample(string file)
		{
				
			var Ex = new RPGC()
			{
				Group = "default",
				GoGroup = "LV1",
				Co = new string[]
				{
					"/BC 这是服务器公告",
				},
				C = 1
			};
			var Conf = new RPGConfig()
			{
			Display = true,
			RPGList = new RPGC[] { Ex }
			};
			Conf.Write(file);
		}
	}

	public class RPGC
	{
		public string Group = string.Empty;
		public string GoGroup = string.Empty;
		public string[] Co = new string[0];
		public int C = 0;
	}
}
